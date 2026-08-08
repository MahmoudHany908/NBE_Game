using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{


    [Tooltip("If true, child GameObjects are deactivated after combining.")]
    public bool deactivateChildren = true;


    [Button]
    public void CombineChildren()
    {
        // Gather all MeshFilters in children (excludes self)
        MeshFilter[] childFilters = GetComponentsInChildren<MeshFilter>();

        // Each entry = one sub-mesh list (one per material slot across all children)
        // materialKey → list of CombineInstances that share that material
        Dictionary<Material, List<CombineInstance>> materialMap =
            new Dictionary<Material, List<CombineInstance>>();

        List<GameObject> childObjects = new List<GameObject>();

        foreach (MeshFilter mf in childFilters)
        {
            if (mf.gameObject == gameObject) continue;

            if (mf.sharedMesh == null)
            {
                Debug.LogWarning($"[MeshCombiner] '{mf.gameObject.name}' has no mesh — skipping.", mf.gameObject);
                continue;
            }

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            Material[] mats = mr != null ? mr.sharedMaterials : new Material[1];
            Matrix4x4 localToParent = transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;

            // A mesh can have multiple sub-meshes, each mapped to one material slot
            for (int sub = 0; sub < mf.sharedMesh.subMeshCount; sub++)
            {
                Material mat = (mats != null && sub < mats.Length) ? mats[sub] : null;

                if (mat == null)
                {
                    Debug.LogWarning($"[MeshCombiner] '{mf.gameObject.name}' submesh {sub} has no material (or no MeshRenderer). Skipping.", mf.gameObject);
                    continue;
                }

                if (!materialMap.ContainsKey(mat))
                    materialMap[mat] = new List<CombineInstance>();

                materialMap[mat].Add(new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    subMeshIndex = sub,
                    transform = localToParent
                });
            }

            if (!childObjects.Contains(mf.gameObject))
                childObjects.Add(mf.gameObject);
        }

        if (materialMap.Count == 0)
        {
            Debug.LogWarning("[MeshCombiner] No valid child meshes found to combine.");
            return;
        }

        // Step 1 — merge each material group into its own sub-mesh
        List<CombineInstance> finalCombines = new List<CombineInstance>();
        List<Material> finalMaterials = new List<Material>();

        foreach (var kvp in materialMap)
        {
            Mesh groupMesh = new Mesh();
            groupMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            // mergeSubMeshes:true collapses this group into one sub-mesh
            groupMesh.CombineMeshes(kvp.Value.ToArray(), mergeSubMeshes: true, useMatrices: true);

            finalCombines.Add(new CombineInstance { mesh = groupMesh, transform = Matrix4x4.identity });
            finalMaterials.Add(kvp.Key);
        }

        // Step 2 — merge all groups into one mesh, keeping one sub-mesh per material
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedMesh";
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // mergeSubMeshes:false preserves one sub-mesh per material group
        combinedMesh.CombineMeshes(finalCombines.ToArray(), mergeSubMeshes: false, useMatrices: true);
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateTangents();

        // Assign mesh and the full material array
        GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        GetComponent<MeshRenderer>().sharedMaterials = finalMaterials.ToArray();

        // Deactivate children
        if (deactivateChildren)
        {
            foreach (GameObject child in childObjects)
                child.SetActive(false);
        }

        Debug.Log($"[MeshCombiner] Combined into '{combinedMesh.name}' — " +
                  $"{combinedMesh.vertexCount} verts, {combinedMesh.triangles.Length / 3} tris, " +
                  $"{finalMaterials.Count} material(s).");
    }
}