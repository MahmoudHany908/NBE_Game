using UnityEngine;

public class BudgetCalculator : MonoBehaviour
{
    [SerializeField] private int baseBudget = 10;
    [SerializeField] private int budgetIncrement = 5;

    public int GetBudgetForWave(int wave)
    {
        return baseBudget + (wave - 1) * budgetIncrement;
    }
}