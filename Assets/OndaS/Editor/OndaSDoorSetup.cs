using OndaS.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSDoorSetup
{
    [MenuItem("OndaS/Configurar interacción con puertas")]
    public static void Configure()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity")
        { Debug.LogError("Abre la escena OndaS_RetroLookDev para configurar sus puertas."); return; }
        if (EditorApplication.isPlaying) { Debug.LogError("Sal de Play antes de configurar las puertas."); return; }
        var player = GameObject.Find("Base Scene/First Person Player");
        var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;
        if (player == null || camera == null) { Debug.LogError("No se encontró el jugador o su cámara."); return; }

        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var old in root.GetComponentsInChildren<SojaExiles.opencloseDoor>(true))
                if (Install(old, old.openandclose, "Opening", "Closing")) count++;
            foreach (var old in root.GetComponentsInChildren<SojaExiles.opencloseDoor1>(true))
                if (Install(old, old.openandclose1, "Opening 1", "Closing 1")) count++;
            foreach (var old in root.GetComponentsInChildren<SojaExiles.opencloseStallDoor>(true))
                if (Install(old, old.openandclose, "OpeningStall", "ClosingStall")) count++;
            foreach (var old in root.GetComponentsInChildren<SojaExiles.ClosetopencloseDoor>(true))
                if (Install(old, old.Closetopenandclose, "ClosetOpening", "ClosetClosing")) count++;
            foreach (var old in root.GetComponentsInChildren<SojaExiles.BRGlassDoor>(true))
                if (Install(old, old.openandclose, "BRGlassDoorOpen", "BRGlassDoorClose")) count++;
        }
        var interactor = player.GetComponent<OndaSDoorInteractor>();
        if (interactor == null) interactor = player.AddComponent<OndaSDoorInteractor>();
        var serialized = new SerializedObject(interactor);
        serialized.FindProperty("viewCamera").objectReferenceValue = camera;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: interacción E configurada en " + count + " puertas de " + scene.name + ".");
    }

    private static bool Install(Behaviour old, Animator animator, string opening, string closing)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        { Debug.LogWarning("Puerta sin Animator configurado: " + old.gameObject.name, old); return false; }
        var door = old.GetComponent<OndaSDoor>();
        if (door == null) door = old.gameObject.AddComponent<OndaSDoor>();
        var serialized = new SerializedObject(door);
        serialized.FindProperty("doorAnimator").objectReferenceValue = animator;
        serialized.FindProperty("openingState").stringValue = opening;
        serialized.FindProperty("closingState").stringValue = closing;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        old.enabled = false;
        return true;
    }
}
