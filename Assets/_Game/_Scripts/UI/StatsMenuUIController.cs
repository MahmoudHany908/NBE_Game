using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using VaultSurvivors.Core.Production;

public class StatsMenuUIController : MonoBehaviour
{
    [Header("UI References")]
    //[SerializeField] private Button _upgradeHackBtn;
    //[SerializeField] private Button _infiniteMoneyGlitchBtn;
    [SerializeField] private TextMeshProUGUI _playerLevelTxt;
    [SerializeField] private Button _closeBtn;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;
    private ISaveSystem _saveSystem;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, ISaveSystem saveSystem)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _saveSystem = saveSystem;
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
        ApplyStartupCheats();
    }

    // ── Startup cheats ───────────────────────────────────────────────────────
    private void ApplyStartupCheats()
    {
        if (_saveSystem == null) return;

        // Set player level to 30
        GameData data = _saveSystem.Load();
        data.player.level = 30;
        _saveSystem.Save(data);

        // Grant all resources
        ApplyInfiniteMoneyGlitch();

        UpdateUI();
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnEnable()
    {
        //if (_upgradeHackBtn != null) _upgradeHackBtn.onClick.AddListener(OnUpgradeHackClicked);
        //if (_infiniteMoneyGlitchBtn != null) _infiniteMoneyGlitchBtn.onClick.AddListener(OnInfiniteMoneyGlitchClicked);
        if (_closeBtn != null) _closeBtn.onClick.AddListener(CloseMenu);
        UpdateUI();
    }

    private void OnDisable()
    {
        //if (_upgradeHackBtn != null) _upgradeHackBtn.onClick.RemoveListener(OnUpgradeHackClicked);
        //if (_infiniteMoneyGlitchBtn != null) _infiniteMoneyGlitchBtn.onClick.RemoveListener(OnInfiniteMoneyGlitchClicked);
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(CloseMenu);
    }

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        CloseMenu();
    }

    public void OpenMenu()
    {
        if (gameObject.activeSelf) return;

        _eventPublisher?.Publish(new Command_CloseAllMenus());
        gameObject.SetActive(true);
        _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

        UpdateUI();
    }

    public void CloseMenu()
    {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = false });
    }

    private void OnUpgradeHackClicked() => ApplyUpgradeHack();

    private void ApplyUpgradeHack()
    {
        if (_saveSystem == null) return;
        GameData data = _saveSystem.Load();
        data.player.level++;
        _saveSystem.Save(data);
        UpdateUI();
    }

    private void OnInfiniteMoneyGlitchClicked() => ApplyInfiniteMoneyGlitch();

    private void ApplyInfiniteMoneyGlitch()
    {
        if (_saveSystem == null) return;
        GameData data = _saveSystem.Load();
        data.resources.gold += 50000;
        data.resources.food += 50000;
        data.resources.energy += 50000;
        data.resources.electronics += 50000;
        _saveSystem.Save(data);
        UpdateUI();
        _eventPublisher?.Publish(new Command_RequestFactoryState());
    }

    private void UpdateUI()
    {
        if (_saveSystem != null && _playerLevelTxt != null)
        {
            GameData data = _saveSystem.Load();
            _playerLevelTxt.text = $"Level: {data.player.level}";
        }
    }
}
