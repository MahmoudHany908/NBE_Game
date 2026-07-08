using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnvironmentTransfer : EditorWindow
{
    private GameObject oldRoot;
    private GameObject newRoot;

    [MenuItem("Tools/Transfer Environment Components")]
    public static void ShowWindow()
    {
        GetWindow<EnvironmentTransfer>("Transfer Components");
    }

    private void OnGUI()
    {
        oldRoot = (GameObject)EditorGUILayout.ObjectField(
            "Old Root", oldRoot, typeof(GameObject), true);

        newRoot = (GameObject)EditorGUILayout.ObjectField(
            "New Root", newRoot, typeof(GameObject), true);

        if (GUILayout.Button("Transfer"))
        {
            Transfer();
        }
    }

    private void Transfer()
    {
        if (oldRoot == null || newRoot == null)
        {
            Debug.LogError("Assign both roots.");
            return;
        }

        var oldObjects = new Dictionary<string, GameObject>();

        foreach (Transform t in oldRoot.GetComponentsInChildren<Transform>(true))
        {
            oldObjects[GetPath(t, oldRoot.transform)] = t.gameObject;
        }

        int transferred = 0;

        foreach (Transform newTransform in newRoot.GetComponentsInChildren<Transform>(true))
        {
            string path = GetPath(newTransform, newRoot.transform);

            if (!oldObjects.TryGetValue(path, out GameObject oldObject))
                continue;

            GameObject newObject = newTransform.gameObject;

            // Copy tag/layer/static flags
            newObject.tag = oldObject.tag;
            newObject.layer = oldObject.layer;
            GameObjectUtility.SetStaticEditorFlags(
                newObject,
                GameObjectUtility.GetStaticEditorFlags(oldObject));

            Component[] oldComponents = oldObject.GetComponents<Component>();

            foreach (Component oldComp in oldComponents)
            {
                if (oldComp == null)
                    continue;

                System.Type type = oldComp.GetType();

                // Never copy these
                if (type == typeof(Transform))
                    continue;

                if (type == typeof(MeshFilter))
                    continue;

                // Special handling for Renderer
                if (typeof(Renderer).IsAssignableFrom(type))
                {
                    Component newComp = newObject.GetComponent(type);

                    if (newComp != null)
                    {
                        EditorUtility.CopySerialized(oldComp, newComp);
                    }

                    continue;
                }

                // Special handling for colliders
                if (typeof(Collider).IsAssignableFrom(type))
                {
                    // Only make sure the same collider type exists.
                    if (newObject.GetComponent(type) == null)
                    {
                        Undo.AddComponent(newObject, type);
                    }

                    continue;
                }

                Component existing = newObject.GetComponent(type);

                if (existing != null)
                {
                    EditorUtility.CopySerialized(oldComp, existing);
                }
                else
                {
                    Component added = Undo.AddComponent(newObject, type);
                    EditorUtility.CopySerialized(oldComp, added);
                }
            }

            transferred++;
        }

        Debug.Log($"Transferred components for {transferred} objects.");
    }

    private string GetPath(Transform current, Transform root)
    {
        if (current == root)
            return "";

        string path = current.name;

        while (current.parent != null && current.parent != root)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}