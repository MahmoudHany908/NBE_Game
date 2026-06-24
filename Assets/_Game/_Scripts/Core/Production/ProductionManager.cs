using UnityEngine;

namespace VaultSurvivors.Core.Production
{
    public class ProductionManager : MonoBehaviour
    {
        public static ProductionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public float CalculateYield(float currentAccumulated, float ratePerSecond, float maxCapacity, float deltaTime)
        {
            if (currentAccumulated >= maxCapacity)
            {
                return maxCapacity;
            }

            float newTotal = currentAccumulated + (ratePerSecond * deltaTime);
            return Mathf.Clamp(newTotal, 0f, maxCapacity);
        }
    }
}