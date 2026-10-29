using System;
using OndaS.Gameplay.Items;
using UnityEditor;
using UnityEngine;

public static class OndaSItemSetup
{
    [Serializable] private sealed class Catalog { public Entry[] items; }
    [Serializable] private sealed class Entry { public string name; public string category; public string prefab; }

    [MenuItem("OndaS/Configurar ítems recolectables")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("Sal de Play antes de configurar los ítems."); return; }
        var catalogFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/OndaS/Items/Catalogo_Items.json");
        if (catalogFile == null) { Debug.LogError("No se encontró el catálogo de ítems."); return; }
        var catalog = JsonUtility.FromJson<Catalog>(catalogFile.text);
        if (catalog?.items == null) { Debug.LogError("El catálogo de ítems está vacío."); return; }
        const string definitions = "Assets/OndaS/Items/Definitions";
        if (!AssetDatabase.IsValidFolder(definitions)) AssetDatabase.CreateFolder("Assets/OndaS/Items", "Definitions");
        int configured = 0;
        foreach (var entry in catalog.items)
        {
            // The backpack is a physical container, not a normal collectible.
            if (entry.name == "Backpack") continue;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.prefab);
            if (prefab == null) { Debug.LogError("Falta el prefab: " + entry.prefab); continue; }
            string definitionPath = definitions + "/" + entry.name + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<OndaSItemDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<OndaSItemDefinition>();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("displayName").stringValue = DisplayName(entry.name);
                serialized.FindProperty("category").enumValueIndex = entry.category == "Good Items" ? 0 : 1;
                serialized.FindProperty("weightKg").floatValue = Weight(entry.name);
                serialized.FindProperty("worldPrefab").objectReferenceValue = prefab;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }
            var root = PrefabUtility.LoadPrefabContents(entry.prefab);
            try
            {
                var collectible = root.GetComponent<OndaSCollectible>();
                if (collectible == null) collectible = root.AddComponent<OndaSCollectible>();
                var serialized = new SerializedObject(collectible);
                if (serialized.FindProperty("item").objectReferenceValue == null)
                {
                    serialized.FindProperty("item").objectReferenceValue = definition;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                if (root.GetComponentInChildren<Collider>(true) == null)
                {
                    if (!TryGetBounds(root, out var bounds)) { Debug.LogError("El prefab no tiene mallas: " + entry.prefab); continue; }
                    var box = root.AddComponent<BoxCollider>();
                    box.center = bounds.center;
                    box.size = Vector3.Max(bounds.size, new Vector3(0.04f, 0.04f, 0.04f));
                }
                PrefabUtility.SaveAsPrefabAsset(root, entry.prefab);
                configured++;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("OndaS: " + configured + " prefabs recolectables configurados. Pesos no definidos en el GDD son provisionales.");
    }

    private static string DisplayName(string value)
    {
        return value.Replace('_', ' ').Replace("Instantaneos", "Instantáneos").Replace("Cafe", "Café")
            .Replace("Botiquin", "Botiquín").Replace("Pilas y Botiquin", "Pilas y Botiquín").Replace("Llave Inglesa", "Llave inglesa");
    }

    private static float Weight(string name)
    {
        if (name == "Agua") return 1f;
        if (name == "Linterna") return 0.8f;
        if (name == "Botiquin") return 1.2f;
        if (name.StartsWith("Mascarilla")) return 0.2f;
        if (name.StartsWith("Radio")) return 0.8f;
        if (name == "Llave_Inglesa") return 1f;
        if (name == "Televisor_CRT") return 4f;
        if (name == "PS1") return 2f;
        if (name.StartsWith("Libro")) return 0.6f;
        if (name.StartsWith("Manta")) return 0.8f;
        if (name == "Pilas") return 0.2f;
        return 0.5f;
    }

    private static bool TryGetBounds(GameObject root, out Bounds bounds)
    {
        bounds = new Bounds();
        bool found = false;
        foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null) continue;
            var localBounds = filter.sharedMesh.bounds;
            var matrix = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                var point = matrix.MultiplyPoint3x4(localBounds.center + Vector3.Scale(localBounds.extents, new Vector3(x, y, z)));
                if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                else bounds.Encapsulate(point);
            }
        }
        return found;
    }
}
