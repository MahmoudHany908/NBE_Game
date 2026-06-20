using System;
using UnityEngine;

public class Testing : MonoBehaviour {
    void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not initialized!");
            return;
        }
        
        GameData dataSave = new GameData();
        // dataSave.player.level = 5;
        // dataSave.player.xp = 110;
        //
        // dataSave.upgrades.productionFactoryLevel = 2;
        // dataSave.upgrades.marketLevel = 1;
        //
        // dataSave.production.SelectedResource = "Gold";
        // dataSave.production.Amount = 150.5f;
        // dataSave.production.isProducing = true;
        //
        // dataSave.resources.GoldAmount = 100;
        // dataSave.resources.FoodAmount = 30;
        
        // simulate a 10 minute upgrade timer starting now
        DateTime upgradeEndTime = DateTime.Now.AddMinutes(10);
        dataSave.timers.upgradeTimerEndTime = SaveManager.Instance.DateTimeToLong(upgradeEndTime);

        // simulate a 5 minute production timer
        DateTime productionEndTime = DateTime.Now.AddMinutes(5);
        
        dataSave.production.factories.Add(new Factory
        {
            factoryId = 1,
            level = 1,
            selectedResource = "Gold",
            isProducing = true
        });
        
        // ========= SAVING AND LOADING =========
        SaveManager.Instance.Save(dataSave);
        GameData loadData = SaveManager.Instance.Load();
        // ========= ========= ========= =========
        
        
        float upgradeRemaining = SaveManager.Instance.GetRemainingSeconds(loadData.timers.upgradeTimerEndTime);
        Debug.Log("Upgrade Timer Remaining: " + upgradeRemaining + " seconds");

        Factory factory = dataSave.production.factories.Find(f => f.factoryId == 1);
        
        // Debug.Log("Player Level: " + loadData.player.level);
        // Debug.Log("Player xp: " + loadData.player.xp);
        // Debug.Log("Factory Level: " + loadData.upgrades.productionFactoryLevel);
        // Debug.Log("Market Level: " + loadData.upgrades.marketLevel);
    }
}
