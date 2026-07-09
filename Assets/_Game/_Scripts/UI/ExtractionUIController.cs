using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the "Extract or Continue" prompt when a milestone wave (every 5th) is cleared.
/// Pure UI — knows nothing about wave logic, just reacts to ExtractionAvailableEvent
/// and reports the player's choice back out via ExtractionDecisionEvent.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ExtractionUIController : MonoBehaviour
{


    private UIDocument document;
    private VisualElement root;
    private VisualElement panel;
    private Label waveLabel;
    private Label resourceLabel;
    private Button extractButton;
    private Button continueButton;



    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        panel = root.Q<VisualElement>("extraction-panel");
        waveLabel = root.Q<Label>("wave-label");
        resourceLabel = root.Q<Label>("resource-label");
        extractButton = root.Q<Button>("extract-button");
        continueButton = root.Q<Button>("continue-button");

        extractButton.clicked += OnExtractClicked;
        continueButton.clicked += OnContinueClicked;

        HidePanel();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ExtractionAvailableEvent>(HandleExtractionAvailable);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ExtractionAvailableEvent>(HandleExtractionAvailable);
    }

    private void OnDestroy()
    {
        extractButton.clicked -= OnExtractClicked;
        continueButton.clicked -= OnContinueClicked;
    }

    private void HandleExtractionAvailable(ExtractionAvailableEvent evt)
    {
        waveLabel.text = $"WAVE {evt.WaveNumber} CLEARED";
        resourceLabel.text = "RESOURCES EXTRACTED 10100";

        ShowPanel();
    }

    private void OnExtractClicked()
    {
        HidePanel();
        EventBus.Publish(new ExtractionDecisionEvent(didExtract: true));
        SceneManager.LoadScene("HubScene");



    }

    private void OnContinueClicked()
    {
        HidePanel();
        EventBus.Publish(new ExtractionDecisionEvent(didExtract: false));
    }

    private void ShowPanel()
    {
        panel.RemoveFromClassList("hidden");
        Time.timeScale = 0f;
    }

    private void HidePanel()
    {
        panel.AddFromClassList_Safe("hidden");
        Time.timeScale = 1f;
    }
}




internal static class VisualElementExtensions
{
    // Guards against double-adding the class throwing in older UIElements versions.
    public static void AddFromClassList_Safe(this VisualElement element, string className)
    {
        if (!element.ClassListContains(className))
            element.AddToClassList(className);
    }
}
