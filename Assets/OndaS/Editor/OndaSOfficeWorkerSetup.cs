using System;
using System.Linq;
using OndaS.Gameplay;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSOfficeWorkerSetup
{
    private const string Root = "Assets/OndaS/Characters/OfficeWorker";
    private const string ModelPath = Root + "/fbx/office_worker.fbx";
    private const string AtlasPath = Root + "/textures/atlas.png";
    private const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity";

    [MenuItem("OndaS/Configurar personaje principal")]
    public static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || scene.isDirty) { Debug.LogError("Abre la escena OndaS_RetroLookDev y guarda los cambios antes de configurar el personaje."); return; }
        var player = GameObject.Find("Base Scene/First Person Player");
        var controller = player != null ? player.GetComponent<CharacterController>() : null;
        if (controller == null) { Debug.LogError("No se encontró el controlador del jugador."); return; }
        var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        var textureImporter = AssetImporter.GetAtPath(AtlasPath) as TextureImporter;
        if (importer == null || textureImporter == null) { Debug.LogError("Faltan el modelo o su textura."); return; }
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.mipmapEnabled = false;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.SaveAndReimport();
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips)
            clip.loopTime = Array.IndexOf(new[] { "idle", "walk", "run", "talk", "wave", "sit", "cower" }, clip.name.ToLowerInvariant()) >= 0;
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
        var animations = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
        var idle = animations.FirstOrDefault(c => c.name.Equals("idle", StringComparison.OrdinalIgnoreCase));
        var walk = animations.FirstOrDefault(c => c.name.Equals("walk", StringComparison.OrdinalIgnoreCase));
        var run = animations.FirstOrDefault(c => c.name.Equals("run", StringComparison.OrdinalIgnoreCase));
        var shader = Shader.Find("OndaS/Character Vertex Color");
        if (model == null || atlas == null || idle == null || walk == null || run == null || shader == null)
        { Debug.LogError("Faltan recursos del personaje. Clips disponibles: " + string.Join(", ", animations.Select(a => a.name))); return; }

        var matPath = Root + "/OfficeWorker.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, matPath); }
        material.shader = shader; material.SetTexture("_BaseMap", atlas); EditorUtility.SetDirty(material);
        var controllerPath = Root + "/OfficeWorker.controller";
        var animationController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (animationController == null)
        {
            animationController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var sm = animationController.layers[0].stateMachine;
            var idleState = sm.AddState("Idle"); idleState.motion = idle;
            var walkState = sm.AddState("Walk"); walkState.motion = walk;
            sm.defaultState = idleState;
        }
        EnsureRunState(animationController, run);
        var existing = player.transform.Find("Office Worker");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
        visual.name = "Office Worker";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = controller.center - Vector3.up * (controller.height * 0.5f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterial = material;
        var animator = visual.GetComponent<Animator>();
        if (animator == null) animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = animationController;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var oldCapsule = player.transform.Find("Capsule");
        if (oldCapsule != null) oldCapsule.gameObject.SetActive(false);
        var visualDriver = player.GetComponent<OndaSPlayerVisual>();
        if (visualDriver == null) visualDriver = player.AddComponent<OndaSPlayerVisual>();
        var serialized = new SerializedObject(visualDriver);
        serialized.FindProperty("animator").objectReferenceValue = animator;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = visual;
        Debug.Log("OndaS: personaje principal configurado. Clips: " + string.Join(", ", animations.Select(a => a.name)));
    }

    [MenuItem("OndaS/Actualizar animación de carrera")]
    public static void UpdateRunAnimation()
    {
        var animationController = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "/OfficeWorker.controller");
        var run = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>()
            .FirstOrDefault(c => c.name.Equals("run", StringComparison.OrdinalIgnoreCase));
        if (animationController == null || run == null) { Debug.LogError("No se encontró el controlador o el clip run."); return; }
        EnsureRunState(animationController, run);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureRunState(AnimatorController controller, AnimationClip run)
    {
        var sm = controller.layers[0].stateMachine;
        var state = sm.states.Select(child => child.state).FirstOrDefault(s => s.name == "Run");
        if (state == null) state = sm.AddState("Run");
        state.motion = run;
        EditorUtility.SetDirty(state);
    }
}
