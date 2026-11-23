using OndaS.Gameplay.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSMainMenuSetup
{
    const string MenuPath = "Assets/OndaS/Art/Scenes/OndaS_MenuPrincipal.unity";
    const string GamePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    const string ImagePath = "Assets/OndaS/Art/Textures/Menu/Fondo_Edificio.png";
    [MenuItem("OndaS/Crear menú principal")]
    public static void Create()
    {
        if (EditorApplication.isPlaying || AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath) != null)
        { Debug.LogError("Sal de Play y comprueba que el menú no exista ya."); return; }
        var image = AssetDatabase.LoadAssetAtPath<Texture2D>(ImagePath);
        if (image == null) { Debug.LogError("Falta el fondo del menú."); return; }
        var importer = (TextureImporter)AssetImporter.GetAtPath(ImagePath);
        importer.filterMode = FilterMode.Point; importer.maxTextureSize = 512; importer.mipmapEnabled = false; importer.SaveAndReimport();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var root = new GameObject("OndaS - Menú principal"); SceneManager.MoveGameObjectToScene(root, scene);
        var menu = root.AddComponent<OndaSMainMenu>();
        var serialized = new SerializedObject(menu); serialized.FindProperty("background").objectReferenceValue=image; serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, MenuPath); EditorSceneManager.CloseScene(scene, true);
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(MenuPath, true));
        foreach (var existing in EditorBuildSettings.scenes)
            if (existing.path != MenuPath) scenes.Add(existing.path == GamePath ? new EditorBuildSettingsScene(GamePath,true) : existing);
        if (!scenes.Exists(item => item.path == GamePath)) scenes.Add(new EditorBuildSettingsScene(GamePath,true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("OndaS: menú creado y configurado como primera escena.");
    }
}
