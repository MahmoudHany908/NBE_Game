using TMPro;
using UnityEngine;
using VContainer;
using VaultSurvivors.Core.Production;

public class MaterialsHUDController : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button statsButton;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI electronicsText;

    private ISaveSystem _saveSystem;
    private IEventSubscriber _eventSubscriber;

    [Inject]
    public void Construct(ISaveSystem saveSystem, IEventSubscriber eventSubscriber)
    {
        _saveSystem = saveSystem;
        _eventSubscriber = eventSubscriber;
    }

    private void Start()
    {
        if (_eventSubscriber != null)
        {
            _eventSubscriber.Subscribe<Event_FactoryStateUpdated>(OnFactoryStateUpdated);
        }

        UpdateHUDFromSave();
    }

    private void OnDestroy()
    {
        if (_eventSubscriber != null)
        {
            _eventSubscriber.Unsubscribe<Event_FactoryStateUpdated>(OnFactoryStateUpdated);
        }
    }

    private void UpdateHUDFromSave()
    {
        if (_saveSystem != null && _saveSystem.HasSaveFile())
        {
            GameData data = _saveSystem.Load();
            UpdateUI(data.resources.gold, data.resources.food, data.resources.energy, data.resources.electronics);
        }
        else
        {
            UpdateUI(0, 0, 0, 0);
        }
    }

    private void OnFactoryStateUpdated(Event_FactoryStateUpdated evt)
    {
        UpdateUI(evt.WalletGold, evt.WalletFood, evt.WalletEnergy, evt.WalletElectronics);
    }

    public void UpdateUI(float gold, float food, float energy, float electronics)
    {
        if (goldText != null) goldText.text = Mathf.FloorToInt(gold).ToString();
        if (foodText != null) foodText.text = Mathf.FloorToInt(food).ToString();
        if (energyText != null) energyText.text = Mathf.FloorToInt(energy).ToString();
        if (electronicsText != null) electronicsText.text = Mathf.FloorToInt(electronics).ToString();
    }
}
