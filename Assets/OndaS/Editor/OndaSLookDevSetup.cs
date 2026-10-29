using System.Collections.Generic;
using System.IO;
using OndaS.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class OndaSLookDevSetup
{
    private const string Root = "Assets/OndaS/Art";
    private const string ScenePath = Root + "/Scenes/OndaS_RetroLookDev.unity";

    [MenuItem("OndaS/Crear prueba visual del cuarto piso")]
    public static void CreateLookDev()
    {
        var current = SceneManager.GetActiveScene();
        if (current.path != "Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_02.unity")
        {
            Debug.LogError("Abre Scene_02 antes de crear la prueba visual de OndaS.");
            return;
        }
        if (current.isDirty)
        {
            Debug.LogError("Scene_02 tiene cambios sin guardar. Guarda o descarta esos cambios antes de crear la copia.");
            return;
        }
        if (File.Exists(ScenePath))
        {
            Debug.LogError("La escena de prueba ya existe. Ábrela directamente para conservar sus ajustes.");
            return;
        }

        EnsureFolder(Root + "/Scenes");
        EnsureFolder(Root + "/Materials");
        EnsureFolder(Root + "/Textures");
        if (!EditorSceneManager.SaveScene(current, ScenePath, true))
        {
            Debug.LogError("No se pudo crear la copia de la escena.");
            return;
        }
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var floor = GameObject.Find("Structure_03/Interior/4th Floor");
        var player = GameObject.Find("Base Scene/First Person Player");
        if (floor == null || player == null)
        {
            Debug.LogError("No se encontró el cuarto piso o el jugador en la copia.");
            return;
        }

        var materialCache = new Dictionary<Material, Material>();
        var textureCache = new Dictionary<Texture, Texture>();
        int renderersChanged = 0;
        foreach (var renderer in floor.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                var source = materials[i];
                if (source == null || source.shader == null || source.shader.name != "Universal Render Pipeline/Lit") continue;
                if (!materialCache.TryGetValue(source, out var variant))
                {
                    var sourcePath = AssetDatabase.GetAssetPath(source);
                    var id = AssetDatabase.AssetPathToGUID(sourcePath);
                    if (string.IsNullOrEmpty(id)) continue;
                    var path = Root + "/Materials/Retro_" + id + ".mat";
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
                        var tex = variant.HasProperty("_BaseMap") ? variant.GetTexture("_BaseMap") : null;
                        if (tex != null)
                        {
                            if (!textureCache.TryGetValue(tex, out var retroTexture))
                            {
                                retroTexture = MakeTextureVariant(tex);
                                textureCache[tex] = retroTexture;
                            }
                            if (retroTexture != null) variant.SetTexture("_BaseMap", retroTexture);
                        }
                        AssetDatabase.CreateAsset(variant, path);
                    }
                    materialCache[source] = variant;
                }
                materials[i] = variant;
                changed = true;
            }
            if (changed) { renderer.sharedMaterials = materials; renderersChanged++; }
        }

        foreach (var light in floor.GetComponentsInChildren<Light>(true))
        {
            light.color = new Color(1f, 0.83f, 0.64f);
            light.intensity *= 0.8f;
            light.shadows = LightShadows.None;
        }
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.44f, 0.46f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.30f, 0.31f, 0.29f);
        RenderSettings.ambientGroundColor = new Color(0.20f, 0.19f, 0.17f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.31f, 0.34f, 0.32f);
        RenderSettings.fogDensity = 0.012f;

        var compositeShader = Shader.Find("OndaS/RetroComposite");
        var dustShader = Shader.Find("OndaS/Dust");
        if (compositeShader == null || dustShader == null) { Debug.LogError("Faltan shaders de OndaS; espera a que Unity termine de importarlos."); return; }
        var composite = GetOrCreateMaterial(Root + "/Materials/RetroComposite.mat", compositeShader);
        var dust = GetOrCreateMaterial(Root + "/Materials/RetroDust.mat", dustShader);
        var camera = player.GetComponentInChildren<Camera>(true);
        var retro = camera.gameObject.AddComponent<OndaSRetroCamera>();
        var serialized = new SerializedObject(retro);
        serialized.FindProperty("compositeMaterial").objectReferenceValue = composite;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        camera.allowMSAA = false;
        ConfigureWalkthrough(player, camera);

        // Place the player above a real floor tile, using its world coordinates.
        var tiles = floor.transform.Find("Apartment_01/Floors");
        var tile = tiles.GetChild(1);
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.transform.position = tile.position + Vector3.up * 1.55f;
        player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        if (controller != null) controller.enabled = true;

        var localLights = floor.transform.Find("Apartment_01/Lighting").GetComponentsInChildren<Light>();
        if (localLights.Length > 0) CreateDust(floor.transform, localLights[0].transform.position + Vector3.down * 1.5f, dust);
        if (localLights.Length > 2) CreateDust(floor.transform, localLights[2].transform.position + Vector3.down * 1.5f, dust);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = floor;
        Debug.Log("OndaS: prueba visual creada. Renderers: " + renderersChanged + ", materiales: " + materialCache.Count + ", texturas: " + textureCache.Count + ". Escena: " + ScenePath);
    }

    [MenuItem("OndaS/Actualizar controles de la prueba visual")]
    public static void UpdateWalkthrough()
    {
        if (SceneManager.GetActiveScene().path != ScenePath) { Debug.LogError("Abre OndaS_RetroLookDev antes de actualizar los controles."); return; }
        var player = GameObject.Find("Base Scene/First Person Player");
        if (player == null) { Debug.LogError("No se encontró el jugador de prueba."); return; }
        ConfigureWalkthrough(player, player.GetComponentInChildren<Camera>(true));
        RenderSettings.ambientSkyColor = new Color(0.44f, 0.46f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.30f, 0.31f, 0.29f);
        RenderSettings.ambientGroundColor = new Color(0.20f, 0.19f, 0.17f);
        var composite = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/RetroComposite.mat");
        if (composite != null) { composite.SetFloat("_Dither", 0.08f); composite.SetFloat("_Vignette", 0.08f); EditorUtility.SetDirty(composite); AssetDatabase.SaveAssets(); }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("OndaS: controles de recorrido preparados para el Input System.");
    }

    [MenuItem("OndaS/Alternar apagón de prueba")]
    public static void ToggleBlackout()
    {
        if (!EditorApplication.isPlaying) { Debug.Log("Entra en Play para previsualizar el apagón."); return; }
        var player = GameObject.Find("Base Scene/First Person Player");
        var walkthrough = player != null ? player.GetComponent<OndaSLookDevWalkthrough>() : null;
        if (walkthrough != null) walkthrough.ToggleBlackoutPreview();
    }

    private static void ConfigureWalkthrough(GameObject player, Camera camera)
    {
        foreach (var old in player.GetComponentsInChildren<SojaExiles.PlayerMovement>(true)) old.enabled = false;
        foreach (var old in player.GetComponentsInChildren<SojaExiles.MouseLook>(true)) old.enabled = false;
        var walkthrough = player.GetComponent<OndaSLookDevWalkthrough>();
        if (walkthrough == null) walkthrough = player.AddComponent<OndaSLookDevWalkthrough>();
        var serialized = new SerializedObject(walkthrough);
        serialized.FindProperty("viewCamera").objectReferenceValue = camera;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Texture MakeTextureVariant(Texture source)
    {
        string path = AssetDatabase.GetAssetPath(source);
        string ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".tga") return source;
        string guid = AssetDatabase.AssetPathToGUID(path);
        string target = Root + "/Textures/Retro_" + guid + ext;
        if (!File.Exists(target) && !AssetDatabase.CopyAsset(path, target)) return source;
        var importer = AssetImporter.GetAtPath(target) as TextureImporter;
        if (importer != null)
        {
            importer.filterMode = FilterMode.Point;
            importer.anisoLevel = 0;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = (source.width >= 2048 || source.height >= 2048) ? 1024 : 256;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture>(target);
    }

    private static Material GetOrCreateMaterial(string path, Shader shader)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(shader);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreateDust(Transform parent, Vector3 position, Material material)
    {
        var go = new GameObject("OndaS - Polvo ambiental");
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        var particles = go.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = true; main.playOnAwake = true; main.startLifetime = 9f;
        main.startSpeed = 0.04f; main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.045f);
        main.startColor = new Color(0.80f, 0.77f, 0.68f, 0.30f); main.maxParticles = 90;
        var emission = particles.emission; emission.rateOverTime = 7;
        var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(5f, 2f, 5f);
        var psRenderer = go.GetComponent<ParticleSystemRenderer>();
        psRenderer.sharedMaterial = material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
