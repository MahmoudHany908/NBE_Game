using System.Collections.Generic;
using UnityEngine;

namespace VaultSurvivors.LoadingSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewHintDatabase", menuName = "Vault Survivors/Loading System/Hint Database")]
    public class HintDatabase : ScriptableObject
    {
        [SerializeField] private List<HintData> hints = new List<HintData>();

        /// <summary>
        /// Returns a random hint from the database. 
        /// Attempts to return a different hint than the last shown one if possible.
        /// </summary>
        public HintData GetRandomHint(HintData lastShown)
        {
            if (hints == null || hints.Count == 0)
            {
                return null;
            }

            if (hints.Count == 1)
            {
                return hints[0];
            }

            HintData selectedHint = null;
            int maxAttempts = 5; // Guard against infinite loop

            for (int i = 0; i < maxAttempts; i++)
            {
                int randomIndex = Random.Range(0, hints.Count);
                selectedHint = hints[randomIndex];

                if (selectedHint != lastShown)
                {
                    break;
                }
            }

            return selectedHint;
        }
    }
}
