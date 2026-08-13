using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VaultSurvivors.Core.Production;
using System.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VaultSurvivors.Core.Production
{
    public class FactoryCinematicController : MonoBehaviour
    {
        [Header("Cinematic Settings")]
        [SerializeField] private Transform cinematicCameraPos;
        [SerializeField] private float vfxDuration = 2.0f;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Visuals")]
        [SerializeField] private GameObject[] vfxPrefabs;
        [SerializeField] private GameObject level1Mesh;
        [SerializeField] private GameObject level5Mesh;
        [SerializeField] private GameObject[] barriers;

        [Header("Audio")]
        [SerializeField] private SoundData upgradeSound;

        private IEventPublisher _eventPublisher;
        private IEventSubscriber _eventSubscriber;
        private AudioManager _audioManager;
        private ISaveSystem _saveSystem;

        [Inject]
        public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, AudioManager audioManager, ISaveSystem saveSystem)
        {
            // Prevent double-injection bugs if HubLifetimeScope forces multiple injections
            if (_eventSubscriber != null) return;

            _eventPublisher = eventPublisher;
            _eventSubscriber = eventSubscriber;
            _audioManager = audioManager;
            _saveSystem = saveSystem;

            // Subscribe if already active (e.g. OnEnable fired before VContainer finished injecting)
            if (isActiveAndEnabled)
            {
                _eventSubscriber.Subscribe<Event_FactoryVisualUpgrade>(OnFactoryVisualUpgrade);
            }
        }

        private void OnEnable()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe<Event_FactoryVisualUpgrade>(OnFactoryVisualUpgrade);
            }
        }

        private void OnDisable()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe<Event_FactoryVisualUpgrade>(OnFactoryVisualUpgrade);
            }
        }

        private void Start()
        {
            if (_saveSystem != null)
            {
                GameData data = _saveSystem.Load();
                int level = data.production.globalFactoryLevel > 0 ? data.production.globalFactoryLevel : 1;
                SetInitialMesh(level);
            }
        }

        private void SetInitialMesh(int level)
        {
            bool isLevel5 = level >= 5;
            if (level1Mesh != null) level1Mesh.SetActive(!isLevel5);
            if (level5Mesh != null) level5Mesh.SetActive(isLevel5);
        }



        private void OnFactoryVisualUpgrade(Event_FactoryVisualUpgrade evt)
        {
            // Only trigger cinematic at Level 5
            if (evt.NewLevel == 5)
            {
                PlayCinematicSequence();
            }
        }

        private async void PlayCinematicSequence()
        {
            // Step 1: Block input and close all menus
            _eventPublisher.Publish(new Command_CloseAllMenus());
            PlayerInputs playerInputs = FindAnyObjectByType<PlayerInputs>();
            if (playerInputs != null) playerInputs.enabled = false;

            // Create dynamic Canvas for screen fade
            Canvas fadeCanvas = CreateFadeCanvas(out Image fadeImage);

            // Step 2: Fade out
            _ = LMotion.Create(0f, 1f, fadeDuration).BindToColorA(fadeImage);
            await Awaitable.WaitForSecondsAsync(fadeDuration);
            
            // Step 4: Cache camera, disable CinemachineBrain, and snap to cinematic position
            Camera mainCam = Camera.main;
            Vector3 originalCamPos = Vector3.zero;
            Quaternion originalCamRot = Quaternion.identity;
            MonoBehaviour cinemachineBrain = null;
            
            if (mainCam != null)
            {
                originalCamPos = mainCam.transform.position;
                originalCamRot = mainCam.transform.rotation;

                // Disable CinemachineBrain so it doesn't fight the manual position
                cinemachineBrain = mainCam.GetComponent("Unity.Cinemachine.CinemachineBrain") as MonoBehaviour;
                if (cinemachineBrain != null) cinemachineBrain.enabled = false;

                if (cinematicCameraPos != null)
                {
                    mainCam.transform.position = cinematicCameraPos.position;
                    mainCam.transform.rotation = cinematicCameraPos.rotation;
                }
            }

            // Step 5: Fade back in
            _ = LMotion.Create(1f, 0f, fadeDuration).BindToColorA(fadeImage);
            await Awaitable.WaitForSecondsAsync(fadeDuration);

            // Step 6: Activate VFX and play audio
            if (vfxPrefabs != null)
            {
                foreach (var vfx in vfxPrefabs)
                {
                    if (vfx != null) Instantiate(vfx, transform.position, transform.rotation);
                }
            }
            
            if (_audioManager != null && upgradeSound != null)
            {
                _audioManager.PlaySoundAtPosition(upgradeSound, transform.position);
            }

            // Step 7: Wait for VFX duration
            await Awaitable.WaitForSecondsAsync(vfxDuration);

            // Step 8: Swap meshes and remove barriers
            if (level1Mesh != null) level1Mesh.SetActive(false);
            if (level5Mesh != null) 
            {
                level5Mesh.SetActive(true);
                
                // Prevent the UI from immediately popping up since the player is already inside the zone
                var panels = level5Mesh.GetComponentsInChildren<FactoryControlPanel>(true);
                foreach (var panel in panels)
                {
                    panel.RequireExit();
                }
            }
            
            if (barriers != null)
            {
                foreach (var barrier in barriers)
                {
                    if (barrier != null) barrier.SetActive(false);
                }
            }

            // Step 9: Wait 1 more second to show off the mesh
            await Awaitable.WaitForSecondsAsync(1.0f);

            // Step 10: Fade out, restore, fade in, open menu
            _ = LMotion.Create(0f, 1f, fadeDuration).BindToColorA(fadeImage);
            await Awaitable.WaitForSecondsAsync(fadeDuration);

            // Restore camera
            if (mainCam != null)
            {
                mainCam.transform.position = originalCamPos;
                mainCam.transform.rotation = originalCamRot;
                if (cinemachineBrain != null) cinemachineBrain.enabled = true;
            }

            // Restore input and fade in
            _eventPublisher.Publish(new Command_CloseAllMenus()); // Prevent UI from staying open if trigger fired during mesh swap
            if (playerInputs != null) playerInputs.enabled = true;
            _ = LMotion.Create(1f, 0f, fadeDuration).BindToColorA(fadeImage);
            await Awaitable.WaitForSecondsAsync(fadeDuration);

            // Cleanup canvas
            if (fadeCanvas != null) Destroy(fadeCanvas.gameObject);

            // Removed Command_OpenFactoryUI so the panel stays hidden until player walks back into trigger
        }

        private Canvas CreateFadeCanvas(out Image img)
        {
            GameObject canvasGO = new GameObject("CinematicFadeCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGO.AddComponent<GraphicRaycaster>(); // block clicks during fade

            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform, false);
            img = imageGO.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Start transparent

            RectTransform rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return canvas;
        }
    }
}
