using OndaS.Gameplay.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSBackpackSetup
{
    private const string PrefabPath = "Assets/OndaS/Items/Good Items/Backpack/Prefabs/Backpack.prefab";

    [MenuItem("OndaS/Preparar mochila en escena")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("Sal de Play antes de preparar la mochila."); return; }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) { Debug.LogError("Falta el prefab de la mochila."); return; }
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var filter = root.GetComponentInChildren<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) { Debug.LogError("La mochila no tiene malla."); return; }
            var box = root.GetComponent<BoxCollider>();
            if (box == null) box = root.AddComponent<BoxCollider>();
            var bounds = filter.sharedMesh.bounds;
            box.center = filter.transform.localPosition + bounds.center;
            box.size = bounds.size;
            var body = root.GetComponent<Rigidbody>();
            if (body == null) body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            if (root.GetComponent<OndaSBackpack>() == null) root.AddComponent<OndaSBackpack>();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity") { Debug.Log("Prefab preparado; abre OndaS_RetroLookDev para colocarlo."); return; }
        GameObject existing = GameObject.Find("Mochila en la pared");
        var player = GameObject.Find("Base Scene/First Person Player");
        var camera = player != null ? player.GetComponentInChildren<Camera>() : null;
        if (camera == null) { Debug.LogError("No se encontró la cámara del jugador."); return; }
        Physics.SyncTransforms();
        RaycastHit best = default; float bestDistance = float.PositiveInfinity;
        for (int i = 0; i < 24; i++)
        {
            var direction = Quaternion.Euler(0, i * 15, 0) * Vector3.forward;
            if (!Physics.Raycast(camera.transform.position, direction, out var hit, 6f)) continue;
            if (hit.collider.transform.IsChildOf(player.transform) || hit.collider.GetComponentInParent<OndaSBackpack>() != null || Mathf.Abs(hit.normal.y) > 0.25f || hit.distance < 1f || hit.distance >= bestDistance) continue;
            string n = hit.collider.name.ToLowerInvariant();
            if (n.Contains("shelf") || n.Contains("mirror")) continue;
            if (!n.Contains("wall") && !n.Contains("pared")) continue;
            best = hit; bestDistance = hit.distance;
        }
        if (float.IsPositiveInfinity(bestDistance)) { Debug.LogError("No se encontró una pared cercana; coloca el prefab manualmente."); return; }
        var instance = existing != null ? existing : (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        if (existing == null) Undo.RegisterCreatedObjectUndo(instance, "Colocar mochila");
        else Undo.RecordObject(instance.transform, "Ubicar mochila");
        instance.name = "Mochila en la pared";
        instance.transform.position = best.point + best.normal * 0.14f + Vector3.down * 0.25f;
        instance.transform.rotation = Quaternion.LookRotation(-best.normal, Vector3.up);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Mochila colocada en pared sin guardar la escena: " + instance.transform.position + ", pared=" + best.collider.name);
    }
}
