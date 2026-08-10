using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class HubTutorialGuide : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the pointer rotates to face the target.")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("References")]
    [Tooltip("The Dialogue Manager game object. This will be deactivated once the tutorial finishes.")]
    [SerializeField] private GameObject dialogueManager;
    
    [Tooltip("The ordered list of dialogue triggers that act as waypoints.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    private ISaveSystem _saveSystem;
    private int _currentWaypointIndex = 0;
    private bool _isConstructed = false;

    [Inject]
    public void Construct(ISaveSystem saveSystem)
    {
        if (_isConstructed) return;
        _saveSystem = saveSystem;
        _isConstructed = true;
    }

    private bool _shouldDeactivate = false;

    private void Start()
    {
        // Fallback in case injection didn't occur
        if (_saveSystem == null)
        {
            var scope = FindAnyObjectByType<GameLifetimeScope>();
            if (scope != null && scope.Container != null)
            {
                _saveSystem = scope.Container.Resolve<ISaveSystem>();
            }
            else
            {
                Debug.LogWarning("[HubTutorialGuide] ISaveSystem was not injected and GameLifetimeScope wasn't found.");
                _shouldDeactivate = true;
                return;
            }
        }

        GameData data = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
        
        // If the player has already passed the tutorial, flag for deactivation.
        if (data.player.Passed_Tutorial)
        {
            _shouldDeactivate = true;
            return;
        }

        // Initialize by looking for the first valid waypoint
        AdvanceToNextValidWaypoint();
    }

    private void Update()
    {
        if (_shouldDeactivate)
        {
            if (dialogueManager != null) dialogueManager.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
        // If we have passed all waypoints, finish the tutorial.
        if (_currentWaypointIndex >= waypoints.Count)
        {
            FinishTutorial();
            return;
        }

        Transform target = waypoints[_currentWaypointIndex];

        // The DialogueTrigger destroys itself when it finishes.
        // If the target is null, the player has completed that dialogue.
        if (target == null)
        {
            AdvanceToNextValidWaypoint();
            return;
        }

        // Calculate direction to the target
        Vector3 direction = target.position - transform.position;
        // Ignore the Y axis difference to only rotate on the Y axis
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void AdvanceToNextValidWaypoint()
    {
        // Find the next waypoint that hasn't been destroyed
        while (_currentWaypointIndex < waypoints.Count && waypoints[_currentWaypointIndex] == null)
        {
            _currentWaypointIndex++;
        }

        if (_currentWaypointIndex >= waypoints.Count)
        {
            FinishTutorial();
        }
    }

    private void FinishTutorial()
    {
        GameData data = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
        data.player.Passed_Tutorial = true;
        _saveSystem.Save(data);

        Debug.Log("[HubTutorialGuide] Tutorial finished. Deactivating DialogueManager and self.");
        
        if (dialogueManager != null) dialogueManager.SetActive(false);
        gameObject.SetActive(false);
    }
}
