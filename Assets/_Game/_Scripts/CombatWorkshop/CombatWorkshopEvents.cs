using System.Collections.Generic;

public class Command_OpenCombatWorkshopUI : IGameEvent
{
}

public class Command_CloseCombatWorkshopUI : IGameEvent
{
}

public class Command_UnlockCharacter : IGameEvent
{
    public string CharacterId;

    public Command_UnlockCharacter(string characterId)
    {
        CharacterId = characterId;
    }
}

public class Command_EquipCharacter : IGameEvent
{
    public string CharacterId;

    public Command_EquipCharacter(string characterId)
    {
        CharacterId = characterId;
    }
}

public class Command_UnequipCharacter : IGameEvent
{
    public string CharacterId;

    public Command_UnequipCharacter(string characterId)
    {
        CharacterId = characterId;
    }
}

public class Command_RequestWorkshopState : IGameEvent
{
}

public class Command_OpenGateLoadoutUI : IGameEvent
{
}

public class Command_CloseGateLoadoutUI : IGameEvent
{
}

public class Event_WorkshopStateUpdated : IGameEvent
{
    public List<string> UnlockedIds;
    public List<string> EquippedIds;
    public int PlayerLevel;

    public Event_WorkshopStateUpdated(List<string> unlockedIds, List<string> equippedIds, int playerLevel)
    {
        UnlockedIds = unlockedIds;
        EquippedIds = equippedIds;
        PlayerLevel = playerLevel;
    }
}

public class Event_CharacterUnlocked : IGameEvent
{
    public string CharacterId;

    public Event_CharacterUnlocked(string characterId)
    {
        CharacterId = characterId;
    }
}

public class Event_LoadoutChanged : IGameEvent
{
    public List<string> EquippedIds;

    public Event_LoadoutChanged(List<string> equippedIds)
    {
        EquippedIds = equippedIds;
    }
}