using OndaS.Gameplay.Earthquake;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSFloor4ShelterSetup
{
    const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    const string FurniturePath = "Structure_03/Interior/4rd Floor/Apartment_02/Furniture and Props/Living Room/Furniture/";
    const string RootName = "OndaS - Refugios piso 4";

    [MenuItem("OndaS/Crear refugios del piso 4")]
    public static void Create()
    {
        var scene = SceneManager.GetActiveScene();
        var desk = GameObject.Find(FurniturePath + "Table_Computer_01");
        var table = GameObject.Find(FurniturePath + "Table_Dining_Apt_01");
        if (scene.path != ScenePath || EditorApplication.isPlaying || desk == null || table == null || GameObject.Find(RootName) != null)
        { Debug.LogError("Abre la escena principal y comprueba que existan los muebles y que aún no existan los refugios."); return; }
        var root = new GameObject(RootName).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Crear refugios del piso 4");
        // Approach the desk from the room; the destination lies between its drawers.
        AddPoint(root, "Entrada y salida - Escritorio", new Vector3(22.823f, 12.1f, 12.90f), new Vector3(22.823f, 12.1f, 12.25f));
        // The dining table has a central pedestal: shelter beside it, not through it.
        AddPoint(root, "Entrada y salida - Mesa", new Vector3(28.934f, 12.1f, 17.45f), new Vector3(28.934f, 12.1f, 18.19f));
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("OndaS: refugios del escritorio y la mesa del piso 4 creados.", root);
    }

    static void AddPoint(Transform parent, string name, Vector3 entry, Vector3 interior)
    {
        var point = new GameObject(name, typeof(OndaSShelterPoint)).transform;
        point.SetParent(parent, false);
        point.position = entry;
        var inside = new GameObject("Interior").transform;
        inside.SetParent(point, false);
        inside.position = interior;
    }
}
