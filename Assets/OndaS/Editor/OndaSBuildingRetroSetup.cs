using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Extends the existing look-dev treatment to the whole building without editing kit assets.
public static class OndaSBuildingRetroSetup
{
    private const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity";
    private const string ExtendedScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    private const string MaterialFolder = "Assets/OndaS/Art/Materials/";
    private const string ResultPath = "Temp/OndaSApplyBuildingRetro.result";
    private static readonly Color RetroLight = new Color(1f, 0.83f, 0.64f);

    [MenuItem("OndaS/Extender estilo retro al edificio")]
    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || (scene.path != ScenePath && scene.path != ExtendedScenePath))
        {
            string message = "Abre OndaS_RetroLookDev u OndaS_RetroEdificio en modo edición antes de aplicar el estilo al edificio.";
            File.WriteAllText(ResultPath, "ERROR: " + message);
            Debug.LogError(message);
            return;
        }
        var building = GameObject.Find("Structure_03");
        var originalFloor = building != null ? building.transform.Find("Interior/8th Floor") : null;
        if (building == null || originalFloor == null)
            throw new InvalidOperationException("No se encontró el edificio o el piso 8; no se aplicaron cambios.");

        // Save the current editor state first, including any user changes, then retain a
        // separate pre-conversion copy outside Assets for recovery.
        if (!EditorSceneManager.SaveScene(scene))
            throw new IOException("No se pudo guardar la escena antes de aplicar el estilo.");
        File.Copy(scene.path, "Temp/OndaS_RetroLookDev_before_building_retro.unity", true);

        var materials = new Dictionary<Material, Material>();
        var textures = new Dictionary<Texture, Texture>();
        int renderersChanged = 0, slotsChanged = 0, lightsChanged = 0, created = 0;
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Extender estilo retro al edificio");

        foreach (var renderer in building.GetComponentsInChildren<Renderer>(true))
        {
            var slots = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < slots.Length; i++)
            {
                var source = slots[i];
                if (source == null || source.shader == null || source.shader.name != "Universal Render Pipeline/Lit") continue;
                string sourcePath = AssetDatabase.GetAssetPath(source);
                if (sourcePath.StartsWith(MaterialFolder + "Retro_", StringComparison.Ordinal)) continue;
                if (!materials.TryGetValue(source, out var variant))
                {
                    string guid = AssetDatabase.AssetPathToGUID(sourcePath);
                    if (string.IsNullOrEmpty(guid)) continue;
                    string path = MaterialFolder + "Retro_" + guid + ".mat";
                    variant = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (variant == null)
                    {
                        variant = new Material(source) { name = source.name + " - OndaS Retro" };
                        bool transparent = variant.HasProperty("_Surface") && variant.GetFloat("_Surface") > 0.5f;
                        if (!transparent && variant.HasProperty("_Smoothness")) variant.SetFloat("_Smoothness", Mathf.Min(variant.GetFloat("_Smoothness"), 0.25f));
                        if (!transparent && variant.HasProperty("_BumpScale")) variant.SetFloat("_BumpScale", variant.GetFloat("_BumpScale") * 0.35f);
                        if (!transparent && variant.HasProperty("_BaseColor"))
                        {
                            var color = variant.GetColor("_BaseColor");
                            color.r *= 0.90f; color.g *= 0.89f; color.b *= 0.84f;
                            variant.SetColor("_BaseColor", color);
                        }
                        var texture = variant.HasProperty("_BaseMap") ? variant.GetTexture("_BaseMap") : null;
                        if (texture != null)
                        {
                            if (!textures.TryGetValue(texture, out var retroTexture))
                            {
                                retroTexture = OndaSLookDevSetup.MakeTextureVariant(texture);
                                textures[texture] = retroTexture;
                            }
                            if (retroTexture != null) variant.SetTexture("_BaseMap", retroTexture);
                        }
                        AssetDatabase.CreateAsset(variant, path);
                        created++;
                    }
                    materials[source] = variant;
                }
                if (variant == null || variant == source) continue;
                slots[i] = variant;
                changed = true;
                slotsChanged++;
            }
            if (!changed) continue;
            Undo.RecordObject(renderer, "Aplicar materiales retro");
            renderer.sharedMaterials = slots;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            renderersChanged++;
        }

        foreach (var light in building.GetComponentsInChildren<Light>(true))
        {
            // The original look-dev floor has already received this treatment.
            if (light.transform.IsChildOf(originalFloor)) continue;
            // This also makes repeat use safe for lights already converted.
            if (light.color == RetroLight && light.shadows == LightShadows.None) continue;
            Undo.RecordObject(light, "Aplicar iluminación retro");
            light.color = RetroLight;
            light.intensity *= 0.8f;
            light.shadows = LightShadows.None;
            PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            lightsChanged++;
        }

        Undo.CollapseUndoOperations(undoGroup);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new IOException("El estilo se aplicó en memoria, pero no se pudo guardar la escena.");
        string result = $"OK: {renderersChanged} renderers, {slotsChanged} materiales asignados, {created} variantes nuevas, {lightsChanged} luces. Copia previa: Temp/OndaS_RetroLookDev_before_building_retro.unity";
        File.WriteAllText(ResultPath, result);
        Debug.Log("OndaS: " + result);
    }

    // Entry point for running the conversion in a separate project copy.
    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Apply();
    }

    public static void ValidateBatch()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var baselineBuilding = GameObject.Find("Structure_03");
        if (baselineBuilding == null) throw new InvalidOperationException("No se encontró el edificio original.");
        var baselineMissing = FindMissingMaterials(baselineBuilding);
        EditorSceneManager.OpenScene(ExtendedScenePath, OpenSceneMode.Single);
        var building = GameObject.Find("Structure_03");
        if (building == null) throw new InvalidOperationException("No se encontró el edificio.");
        int renderers = 0, retroSlots = 0;
        foreach (var renderer in building.GetComponentsInChildren<Renderer>(true))
        {
            renderers++;
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null && AssetDatabase.GetAssetPath(material).StartsWith(MaterialFolder + "Retro_", StringComparison.Ordinal)) retroSlots++;
            }
        }
        int warmLights = 0;
        foreach (var light in building.GetComponentsInChildren<Light>(true))
            if (light.color == RetroLight && light.shadows == LightShadows.None) warmLights++;
        var missing = FindMissingMaterials(building);
        missing.ExceptWith(baselineMissing);
        Debug.Log($"OndaS validación: {renderers} renderers, {retroSlots} materiales retro, {baselineMissing.Count} materiales faltantes preexistentes, {missing.Count} nuevos, {warmLights} luces retro.");
        if (missing.Count != 0) throw new InvalidOperationException("La conversión introdujo materiales o shaders faltantes: " + string.Join(", ", missing));
    }

    private static HashSet<string> FindMissingMaterials(GameObject root)
    {
        var result = new HashSet<string>();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                if (materials[i] == null || materials[i].shader == null || materials[i].shader.name == "Hidden/InternalErrorShader")
                    result.Add(GetPath(renderer.transform) + ":" + renderer.GetType().Name + ":" + i);
        }
        return result;
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null) { transform = transform.parent; path = transform.name + "/" + path; }
        return path;
    }
}
