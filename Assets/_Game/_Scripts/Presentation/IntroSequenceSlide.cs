using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaultSurvivors.Presentation
{
    public class IntroSequenceSlide : PresentationSlide
    {
        [Header("Step 1: Player Idle")]
        public GameObject playerIdleGroup;
        public Light playerSpotlight;

        [Header("Step 2: Enemies")]
        public GameObject meleeEnemyGroup;
        public Light meleeSpotlight;
        public GameObject droneGroup;
        public Light droneSpotlight;

        [Header("Step 3: Logo")]
        public CanvasGroup logoCanvasGroup;
        
        [Header("Step 4: Characters")]
        public GameObject charactersCanvas;
        public List<GameObject> characterImages = new List<GameObject>();
        public List<Light> characterSpotlights = new List<Light>();
        
        [Tooltip("The time (in seconds) to wait after the first character appears before showing the next one.")]
        public float dominoStartDelay = 0.8f;
        
        [Tooltip("How much the delay shrinks each time. 0.5 means each wait is half as long as the previous (accelerating domino effect). 1.0 means the delay stays exactly the same every time.")]
        public float dominoAccelerationMultiplier = 0.5f;

        [Header("Audio")]
        public AudioSource sfxSource;
        [Tooltip("The general SFX played for character/enemy reveals")]
        public AudioClip sfxClip;
        [Tooltip("The SFX played during the logo electricity flicker")]
        public AudioClip electricitySfxClip;

        private int currentStep = 0;
        private bool isAnimating = false;
        private Coroutine currentAnimCoroutine;

        public override void EnterSlide()
        {
            base.EnterSlide();
            
            // Reset state
            currentStep = 0;
            isAnimating = false;

            // Hide everything initially
            if (playerIdleGroup != null) playerIdleGroup.SetActive(false);
            if (playerSpotlight != null) playerSpotlight.enabled = false;
            
            if (meleeEnemyGroup != null) meleeEnemyGroup.SetActive(false);
            if (meleeSpotlight != null) meleeSpotlight.enabled = false;
            
            if (droneGroup != null) droneGroup.SetActive(false);
            if (droneSpotlight != null) droneSpotlight.enabled = false;
            
            if (logoCanvasGroup != null)
            {
                logoCanvasGroup.gameObject.SetActive(false);
                logoCanvasGroup.alpha = 0f;
            }
            
            if (charactersCanvas != null) charactersCanvas.SetActive(false);
            
            for (int i = 0; i < characterImages.Count; i++)
            {
                if (characterImages[i] != null) characterImages[i].SetActive(false);
                if (characterSpotlights != null && i < characterSpotlights.Count && characterSpotlights[i] != null) 
                    characterSpotlights[i].enabled = false;
            }
        }

        public override void ExitSlide()
        {
            if (currentAnimCoroutine != null)
            {
                StopCoroutine(currentAnimCoroutine);
            }
            base.ExitSlide();
        }

        public override bool TryNextInnerStep()
        {
            if (isAnimating) return true; // Block input while an animation is playing so we don't glitch

            if (currentStep == 0)
            {
                currentStep = 1;
                PlayStep1();
                return true;
            }
            else if (currentStep == 1)
            {
                currentStep = 2;
                PlayStep2();
                return true;
            }
            else if (currentStep == 2)
            {
                currentStep = 3;
                PlayStep3();
                return true;
            }
            else if (currentStep == 3)
            {
                currentStep = 4;
                PlayStep4();
                return true;
            }
            
            // If currentStep >= 4, we are done with all inner steps!
            // Return false to let PresentationManager move to the next slide (which will fade to black).
            return false;
        }
        
        public override bool TryPreviousInnerStep()
        {
            // Reset the entire slide back to Step 0 if we hit previous
            if (!isAnimating && currentStep > 0)
            {
                if (currentAnimCoroutine != null) StopCoroutine(currentAnimCoroutine);
                EnterSlide();
                return true; 
            }
            
            // If we are already at step 0, let the manager go to the previous slide
            return false;
        }

        private void PlaySFX()
        {
            if (sfxSource != null && sfxClip != null)
            {
                sfxSource.PlayOneShot(sfxClip);
            }
        }

        private void PlayStep1()
        {
            if (playerIdleGroup != null) playerIdleGroup.SetActive(true);
            if (playerSpotlight != null) playerSpotlight.enabled = true;
            PlaySFX();
        }

        private void PlayStep2()
        {
            // Melee Enemy
            if (meleeEnemyGroup != null) meleeEnemyGroup.SetActive(true);
            if (meleeSpotlight != null) meleeSpotlight.enabled = true;
            PlaySFX();

            // Drone
            if (droneGroup != null) droneGroup.SetActive(true);
            if (droneSpotlight != null) droneSpotlight.enabled = true;
        }

        private void PlayStep3()
        {
            currentAnimCoroutine = StartCoroutine(Step3Routine());
        }

        private IEnumerator Step3Routine()
        {
            isAnimating = true;

            // 1. Fade out melee enemy spotlight then disable it
            yield return StartCoroutine(FadeOutSpotlightRoutine(meleeEnemyGroup, meleeSpotlight, 0.4f));
            
            // 2. Fade out drone spotlight then disable it
            yield return StartCoroutine(FadeOutSpotlightRoutine(droneGroup, droneSpotlight, 0.4f));

            // 3. Logo canvas keeps flickering like unstable electricity (for 1.8 secs)
            if (logoCanvasGroup != null)
            {
                if (sfxSource != null && electricitySfxClip != null)
                {
                    sfxSource.PlayOneShot(electricitySfxClip);
                }

                logoCanvasGroup.gameObject.SetActive(true);
                float timer = 0f;
                float flickerDuration = 1.8f;
                
                while (timer < flickerDuration)
                {
                    timer += 0.05f; // rapidly flicker every 50ms
                    // Random alpha between 0 and 1, weighted to look broken
                    logoCanvasGroup.alpha = Random.value > 0.6f ? Random.Range(0.7f, 1f) : Random.Range(0f, 0.3f);
                    yield return new WaitForSeconds(0.05f);
                }
                
                // 4. Turns on and stabilizes
                logoCanvasGroup.alpha = 1f;
            }

            isAnimating = false;
        }

        private void PlayStep4()
        {
            currentAnimCoroutine = StartCoroutine(Step4Routine());
        }

        private IEnumerator Step4Routine()
        {
            isAnimating = true;

            if (charactersCanvas != null) charactersCanvas.SetActive(true);

            float currentDelay = dominoStartDelay; 
            
            for (int i = 0; i < characterImages.Count; i++)
            {
                // Show Image
                if (characterImages[i] != null) characterImages[i].SetActive(true);
                
                // Turn on spotlight
                if (characterSpotlights != null && i < characterSpotlights.Count && characterSpotlights[i] != null)
                {
                    characterSpotlights[i].enabled = true;
                }
                
                // Play SFX
                PlaySFX();
                
                // Wait for domino delay
                yield return new WaitForSeconds(currentDelay);
                
                // Shrink delay for the next character to create the domino acceleration
                currentDelay = Mathf.Max(0.01f, currentDelay * dominoAccelerationMultiplier);
            }

            isAnimating = false;
        }
        
        // Fades out a spotlight smoothly then disables the associated GameObject
        private IEnumerator FadeOutSpotlightRoutine(GameObject groupToDisable, Light spot, float duration)
        {
            float startIntensity = spot != null ? spot.intensity : 0f;
            
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                
                if (spot != null)
                {
                    spot.intensity = Mathf.Lerp(startIntensity, 0f, t);
                }
                
                yield return null;
            }
            
            if (groupToDisable != null) groupToDisable.SetActive(false);
            if (spot != null) spot.intensity = startIntensity; // Reset intensity for the next time it's played
        }
    }
}
