using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupNeonAndAudioEditor : Editor
{
    [MenuItem("Tools/Setup Neon UI and Audio")]
    public static void SetupNeonUI()
    {
        // 1. Create or Find Material
        string matPath = "Assets/_Game/Art/Materials/NeonButtonMat.mat";
        Material neonMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (neonMat == null)
        {
            Shader shader = Shader.Find("Custom/URP/NeonUnlitHDR");
            if (shader == null)
            {
                Debug.LogError("Could not find shader Custom/URP/NeonUnlitHDR. Make sure the shader is imported.");
                return;
            }
            neonMat = new Material(shader);
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Art/Materials"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Art"))
                    AssetDatabase.CreateFolder("Assets/_Game", "Art");
                AssetDatabase.CreateFolder("Assets/_Game/Art", "Materials");
            }
            AssetDatabase.CreateAsset(neonMat, matPath);
        }

        // 2. Find Factory Upgrade Button
        var factoryUIs = Object.FindObjectsByType<FactoryUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var factoryUI in factoryUIs)
        {
            SerializedObject so = new SerializedObject(factoryUI);
            SerializedProperty upgradeBtnProp = so.FindProperty("_upgradeBtn");
            if (upgradeBtnProp != null && upgradeBtnProp.objectReferenceValue != null)
            {
                ApplyNeonToButton((Button)upgradeBtnProp.objectReferenceValue, neonMat, SpecialButtonType.Upgrade);
            }
        }

        // 3. Find Combat Workshop Unlock Button (Remove Neon, keep Audio Tag)
        var combatUIs = Object.FindObjectsByType<CombatWorkshopUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var combatUI in combatUIs)
        {
            SerializedObject so = new SerializedObject(combatUI);
            SerializedProperty unlockBtnProp = so.FindProperty("_unlockBtn");
            if (unlockBtnProp != null && unlockBtnProp.objectReferenceValue != null)
            {
                Button btn = (Button)unlockBtnProp.objectReferenceValue;
                RemoveNeonFromButton(btn);
                
                // Still apply the audio tag since they want custom SFX
                SpecialButtonAudioTag tag = btn.GetComponent<SpecialButtonAudioTag>();
                if (tag == null) tag = Undo.AddComponent<SpecialButtonAudioTag>(btn.gameObject);
                SerializedObject tagSO = new SerializedObject(tag);
                tagSO.FindProperty("buttonType").enumValueIndex = (int)SpecialButtonType.Unlock;
                tagSO.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(tag);
            }
        }

        // 4. Find Market Sell Buttons (Remove Neon, remove audio tag if exists)
        var marketUIs = Object.FindObjectsByType<VaultSurvivors.UI.MarketUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var marketUI in marketUIs)
        {
            SerializedObject so = new SerializedObject(marketUI);
            RemoveNeonFromMarketRow(so, "foodUI");
            RemoveNeonFromMarketRow(so, "energyUI");
            RemoveNeonFromMarketRow(so, "electronicsUI");

            // Find confirm modal and add audio tag to its confirm button
            SerializedProperty confirmModalProp = so.FindProperty("confirmModal");
            if (confirmModalProp != null && confirmModalProp.objectReferenceValue != null)
            {
                var confirmModal = (VaultSurvivors.UI.MarketConfirmModal)confirmModalProp.objectReferenceValue;
                SerializedObject modalSo = new SerializedObject(confirmModal);
                SerializedProperty confirmBtnProp = modalSo.FindProperty("confirmButton");
                if (confirmBtnProp != null && confirmBtnProp.objectReferenceValue != null)
                {
                    Button confirmBtn = (Button)confirmBtnProp.objectReferenceValue;
                    SpecialButtonAudioTag tag = confirmBtn.GetComponent<SpecialButtonAudioTag>();
                    if (tag == null)
                    {
                        tag = Undo.AddComponent<SpecialButtonAudioTag>(confirmBtn.gameObject);
                    }
                    SerializedObject tagSO = new SerializedObject(tag);
                    tagSO.FindProperty("buttonType").enumValueIndex = (int)SpecialButtonType.Sell;
                    tagSO.ApplyModifiedProperties();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tag);
                    EditorUtility.SetDirty(confirmBtn.gameObject);
                }
            }
        }

        // 5. Setup GlobalUIButtonAudio on HubLifetimeScope
        var hubScope = Object.FindAnyObjectByType<HubLifetimeScope>(FindObjectsInactive.Include);
        if (hubScope != null)
        {
            GlobalUIButtonAudio audioSetup = hubScope.GetComponent<GlobalUIButtonAudio>();
            if (audioSetup == null)
            {
                audioSetup = Undo.AddComponent<GlobalUIButtonAudio>(hubScope.gameObject);
                EditorUtility.SetDirty(hubScope.gameObject);
                ApplyPrefabRoot(hubScope.gameObject);
                Debug.Log("Attached GlobalUIButtonAudio to HubLifetimeScope!");
            }
        }

        Debug.Log("Neon UI Setup Complete! Check your buttons.");
    }

    private static void RemoveNeonFromMarketRow(SerializedObject so, string rowName)
    {
        SerializedProperty rowProp = so.FindProperty(rowName);
        if (rowProp != null)
        {
            SerializedProperty sellBtnProp = rowProp.FindPropertyRelative("sellButton");
            if (sellBtnProp != null && sellBtnProp.objectReferenceValue != null)
            {
                Button btn = (Button)sellBtnProp.objectReferenceValue;
                
                // Remove audio tag if it exists on the market row button
                SpecialButtonAudioTag tag = btn.GetComponent<SpecialButtonAudioTag>();
                if (tag != null)
                {
                    Undo.DestroyObjectImmediate(tag);
                }

                RemoveNeonFromButton(btn);
            }
        }
    }

    private static void RemoveNeonFromButton(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null && img.material != null && img.material.name.Contains("Neon"))
        {
            Undo.RecordObject(img, "Remove Neon Material");
            img.material = null; // Reverts to default UI material
            PrefabUtility.RecordPrefabInstancePropertyModifications(img);
        }

        NeonImagePopup popup = btn.GetComponent<NeonImagePopup>();
        if (popup != null)
        {
            Undo.DestroyObjectImmediate(popup);
        }
        
        EditorUtility.SetDirty(btn.gameObject);
    }

    private static void ApplyNeonToButton(Button btn, Material mat, SpecialButtonType? type = null)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Apply Neon Material");
            img.material = mat;
            PrefabUtility.RecordPrefabInstancePropertyModifications(img);
        }

        NeonImagePopup popup = btn.GetComponent<NeonImagePopup>();
        if (popup == null)
        {
            popup = Undo.AddComponent<NeonImagePopup>(btn.gameObject);
        }

        SerializedObject popupSO = new SerializedObject(popup);
        popupSO.FindProperty("triggerButton").objectReferenceValue = btn;
        popupSO.FindProperty("targetImage").objectReferenceValue = img;
        popupSO.FindProperty("neonColor").colorValue = new Color(0f, 3f, 3f, 1f); // Cyan HDR
        popupSO.ApplyModifiedProperties();
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(popup);
        
        if (type.HasValue)
        {
            SpecialButtonAudioTag tag = btn.GetComponent<SpecialButtonAudioTag>();
            if (tag == null)
            {
                tag = Undo.AddComponent<SpecialButtonAudioTag>(btn.gameObject);
            }
            
            SerializedObject tagSO = new SerializedObject(tag);
            tagSO.FindProperty("buttonType").enumValueIndex = (int)type.Value;
            tagSO.ApplyModifiedProperties();

            PrefabUtility.RecordPrefabInstancePropertyModifications(tag);
        }

        EditorUtility.SetDirty(btn.gameObject);
        ApplyPrefabRoot(btn.gameObject);
    }
    
    private static void ApplyPrefabRoot(GameObject go)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(go))
        {
            if (PrefabUtility.HasPrefabInstanceAnyOverrides(go, false))
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (root != null)
                {
                    PrefabUtility.ApplyPrefabInstance(root, InteractionMode.AutomatedAction);
                    Debug.Log($"Applied prefab overrides to: {root.name}");
                }
            }
        }
    }
}
