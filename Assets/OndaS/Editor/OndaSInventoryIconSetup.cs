using System.Collections.Generic;
using System.IO;
using OndaS.Gameplay.Items;
using UnityEditor;
using UnityEngine;

public static class OndaSInventoryIconSetup
{
    private static readonly Queue<string> pending = new Queue<string>();
    private static int attempts, generated, skipped;
    private const string Folder = "Assets/OndaS/Items/InventoryIcons";

    [MenuItem("OndaS/Generar iconos de inventario")]
    private static void Begin()
    {
        if (EditorApplication.isPlaying) return;
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/OndaS/Items", "InventoryIcons");
        pending.Clear(); generated = skipped = attempts = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:OndaSItemDefinition", new[] { "Assets/OndaS/Items/Definitions" })) pending.Enqueue(AssetDatabase.GUIDToAssetPath(guid));
        pending.Enqueue("Assets/OndaS/Items/Good Items/Backpack/Prefabs/Backpack.prefab");
        EditorApplication.update -= Process;
        EditorApplication.update += Process;
    }

    private static void Process()
    {
        if (pending.Count == 0)
        {
            EditorApplication.update -= Process; AssetDatabase.SaveAssets();
            Debug.Log("Iconos de inventario: " + generated + " generados, " + skipped + " sin vista previa."); return;
        }
        string path = pending.Peek();
        var item = AssetDatabase.LoadAssetAtPath<OndaSItemDefinition>(path);
        var asset = item != null ? item.WorldPrefab : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { skipped++; pending.Dequeue(); attempts = 0; return; }
        var preview = AssetPreview.GetAssetPreview(asset);
        if (preview == null)
        {
            if (++attempts < 1500) return;
            skipped++; pending.Dequeue(); attempts = 0; return;
        }
        string output = Folder + "/" + asset.name + ".png";
        if (!File.Exists(output)) File.WriteAllBytes(output, preview.EncodeToPNG());
        AssetDatabase.ImportAsset(output, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(output);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(output);
        if (item != null)
        {
            var serialized = new SerializedObject(item);
            if (serialized.FindProperty("icon").objectReferenceValue == null)
            { serialized.FindProperty("icon").objectReferenceValue = sprite; serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(item); }
        }
        else
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var serialized = new SerializedObject(root.GetComponent<OndaSBackpack>());
                if (serialized.FindProperty("icon").objectReferenceValue == null)
                { serialized.FindProperty("icon").objectReferenceValue = sprite; serialized.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, path); }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        generated++; pending.Dequeue(); attempts = 0;
    }
}
