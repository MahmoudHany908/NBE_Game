using System;
using System.Collections.Generic;
using System.Linq;
using VaultSurvivors.Core.Production;
using VContainer.Unity;

public class CombatWorkshopManager : ICombatWorkshopManager, IInitializable, IDisposable
{
    private readonly SupportCharacterRegistrySO _registry;
    private readonly IEventPublisher _publisher;
    private readonly IEventSubscriber _subscriber;
    private readonly ISaveSystem _saveSystem;

    private readonly HashSet<string> _unlockedIds = new HashSet<string>();
    private readonly List<string> _equippedIds = new List<string>();

    private int _cachedPlayerLevel = 1;

    // Flag to suppress the OnSaveDataLoaded event loop during internal writes
    private bool _isInternalLoad = false;

    private Action<SaveDataLoadedEvent> _onSaveDataLoaded;
    private Action<Command_UnlockCharacter> _onUnlockCharacter;
    private Action<Command_EquipCharacter> _onEquipCharacter;
    private Action<Command_UnequipCharacter> _onUnequipCharacter;
    private Action<Command_RequestWorkshopState> _onRequestWorkshopState;

    public CombatWorkshopManager(
        SupportCharacterRegistrySO registry,
        IEventPublisher publisher,
        IEventSubscriber subscriber,
        ISaveSystem saveSystem)
    {
        _registry = registry;
        _publisher = publisher;
        _subscriber = subscriber;
        _saveSystem = saveSystem;
    }
    
    public void Initialize()
    {
        _onSaveDataLoaded          = OnSaveDataLoaded;
        _onUnlockCharacter         = evt => TryUnlock(evt.CharacterId);
        _onEquipCharacter          = evt => TryEquip(evt.CharacterId);
        _onUnequipCharacter        = evt => TryUnequip(evt.CharacterId);
        _onRequestWorkshopState    = _ => PublishStateUpdated();

        _subscriber.Subscribe(_onSaveDataLoaded);
        _subscriber.Subscribe(_onUnlockCharacter);
        _subscriber.Subscribe(_onEquipCharacter);
        _subscriber.Subscribe(_onUnequipCharacter);
        _subscriber.Subscribe(_onRequestWorkshopState);

        LoadState();
    }

    public void Dispose()
    {
        _subscriber.Unsubscribe(_onSaveDataLoaded);
        _subscriber.Unsubscribe(_onUnlockCharacter);
        _subscriber.Unsubscribe(_onEquipCharacter);
        _subscriber.Unsubscribe(_onUnequipCharacter);
        _subscriber.Unsubscribe(_onRequestWorkshopState);
    }
    
    private void LoadState()
    {
        if (_saveSystem.HasSaveFile())
        {
            GameData gameData = LoadGameDataInternal();
            var data = gameData.workshop;

            _unlockedIds.Clear();
            foreach (var id in data.unlockedCharacterIds) _unlockedIds.Add(id);

            _equippedIds.Clear();
            _equippedIds.AddRange(data.equippedCharacterIds);

            _cachedPlayerLevel = gameData.player.level;
            
            PublishStateUpdated();
        }
    }

    private GameData LoadGameDataInternal()
    {
        _isInternalLoad = true;
        try
        {
            return _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
        }
        finally
        {
            _isInternalLoad = false;
        }
    }

    private void OnSaveDataLoaded(SaveDataLoadedEvent evt)
    {
        if (_isInternalLoad) return;

        var data = evt.Data.workshop;

        _unlockedIds.Clear();
        foreach (var id in data.unlockedCharacterIds) _unlockedIds.Add(id);

        _equippedIds.Clear();
        _equippedIds.AddRange(data.equippedCharacterIds);

        _cachedPlayerLevel = evt.Data.player.level;

        PublishStateUpdated();
    }

    public bool IsUnlocked(string characterId) => _unlockedIds.Contains(characterId);

    public bool CanUnlock(string characterId)
    {
        var character = FindCharacter(characterId);
        if (character == null || IsUnlocked(characterId)) return false;

        if (_cachedPlayerLevel < character.RequiredHideoutLevel) return false;

        if (character.RequiresMission)
        {
            // Mission system not wired yet — placeholder always passes.
        }

        return true;
    }

    public bool TryUnlock(string characterId)
    {
        var character = FindCharacter(characterId);
        if (character == null || !CanUnlock(characterId)) return false;

        GameData gameData = LoadGameDataInternal();
        if (!CanAfford(gameData, character.UnlockCost)) return false;

        DeductCost(gameData, character.UnlockCost);
        _unlockedIds.Add(characterId);

        PersistState(gameData);
        _publisher.Publish(new Event_CharacterUnlocked(characterId));
        PublishStateUpdated();
        return true;
    }

    public bool IsEquipped(string characterId) => _equippedIds.Contains(characterId);

    public bool TryEquip(string characterId)
    {
        if (!IsUnlocked(characterId) || IsEquipped(characterId)) return false;
        if (_equippedIds.Count >= _registry.MaxLoadoutSlots) return false;

        _equippedIds.Add(characterId);
        
        GameData gameData = LoadGameDataInternal();
        PersistState(gameData);
        
        _publisher.Publish(new Event_LoadoutChanged(new List<string>(_equippedIds)));
        PublishStateUpdated();
        return true;
    }

    public bool TryUnequip(string characterId)
    {
        if (!_equippedIds.Remove(characterId)) return false;

        GameData gameData = LoadGameDataInternal();
        PersistState(gameData);
        
        _publisher.Publish(new Event_LoadoutChanged(new List<string>(_equippedIds)));
        PublishStateUpdated();
        return true;
    }

    public List<SupportCharacterSO> GetUnlockedCharacters()
        => _registry.Characters.Where(c => _unlockedIds.Contains(c.CharacterId)).ToList();

    public List<SupportCharacterSO> GetEquippedCharacters()
        => _registry.Characters.Where(c => _equippedIds.Contains(c.CharacterId)).ToList();


    private bool CanAfford(GameData gameData, List<ResourceCost> cost)
    {
        foreach (var entry in cost)
        {
            float wallet = entry.Type switch
            {
                ResourceType.Gold        => gameData.resources.gold,
                ResourceType.Food        => gameData.resources.food,
                ResourceType.Energy      => gameData.resources.energy,
                ResourceType.Electronics => gameData.resources.electronics,
                _                        => 0f
            };
            if (wallet < entry.Amount) return false;
        }
        return true;
    }

    private void DeductCost(GameData gameData, List<ResourceCost> cost)
    {
        foreach (var entry in cost)
        {
            switch (entry.Type)
            {
                case ResourceType.Gold:        gameData.resources.gold        -= entry.Amount; break;
                case ResourceType.Food:        gameData.resources.food        -= entry.Amount; break;
                case ResourceType.Energy:      gameData.resources.energy      -= entry.Amount; break;
                case ResourceType.Electronics: gameData.resources.electronics -= entry.Amount; break;
            }
        }
    }

    private void PersistState(GameData gameData)
    {
        gameData.workshop.unlockedCharacterIds = _unlockedIds.ToList();
        gameData.workshop.equippedCharacterIds = new List<string>(_equippedIds);
        
        // Save without triggering the OnSaveDataLoaded event internally
        _isInternalLoad = true;
        try
        {
            _saveSystem.Save(gameData);
        }
        finally
        {
            _isInternalLoad = false;
        }
    }

    private void PublishStateUpdated()
    {
        _publisher.Publish(new Event_WorkshopStateUpdated(
            _unlockedIds.ToList(),
            new List<string>(_equippedIds),
            _cachedPlayerLevel));
    }

    private SupportCharacterSO FindCharacter(string characterId)
        => _registry.Characters.FirstOrDefault(c => c.CharacterId == characterId);
}