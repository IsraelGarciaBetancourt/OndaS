using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSStreetSetup
{
    const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    const string Folder = "Assets/OndaS/Art/Aftermath/";
    const string StreetName = "OndaS - Calle exterior";

    [MenuItem("OndaS/Crear carretera exterior")]
    public static void CreateStreet()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || EditorApplication.isPlaying || GameObject.Find(StreetName) != null)
        { Debug.LogError("Abre la escena principal fuera de Play y comprueba que no exista la calle."); return; }
        var asphalt = Material("Calle_Asfalto", new Color(.12f,.13f,.14f));
        var sidewalk = Material("Calle_Acera", new Color(.36f,.35f,.32f));
        var curb = Material("Calle_Bordillo", new Color(.48f,.46f,.40f));
        var paint = Material("Calle_Pintura", new Color(.75f,.67f,.40f));
        var root = new GameObject(StreetName).transform;
        // The building exit faces +X; the roadway runs parallel to its facade.
        Block(root, "Calzada de asfalto", new Vector3(46, .055f, 1), new Vector3(10, .10f, 92), asphalt, true);
        Block(root, "Acera del edificio", new Vector3(38.5f, .075f, 1), new Vector3(5, .14f, 92), sidewalk, true);
        Block(root, "Acera opuesta", new Vector3(53.5f, .075f, 1), new Vector3(5, .14f, 92), sidewalk, true);
        Block(root, "Bordillo junto al edificio", new Vector3(40.94f, .10f, 1), new Vector3(.12f, .18f, 92), curb, true);
        Block(root, "Bordillo opuesto", new Vector3(51.06f, .10f, 1), new Vector3(.12f, .18f, 92), curb, true);
        for (int z=-43;z<=45;z+=6)
            Block(root, "Línea central", new Vector3(46,.111f,z), new Vector3(.14f,.012f,3), paint, false);
        // Fine seams make the sidewalk readable without creating collision obstacles.
        for (int z=-44;z<=46;z+=4) {
            Block(root,"Junta acera",new Vector3(38.5f,.148f,z),new Vector3(4.85f,.006f,.025f),asphalt,false);
            Block(root,"Junta acera opuesta",new Vector3(53.5f,.148f,z),new Vector3(4.85f,.006f,.025f),asphalt,false);
        }
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: carretera exterior creada.",root);
    }
    static Material Material(string name, Color color)
    {
        var path=Folder+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        var source=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Aftermath_Metal.mat");
        mat=new Material(source) {name=name};
        if(mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap",null);
        if(mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap",null);
        if(mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",color);
        if(mat.HasProperty("_Color")) mat.SetColor("_Color",color);
        AssetDatabase.CreateAsset(mat,path); return mat;
    }
    static GameObject Block(Transform parent,string name,Vector3 position,Vector3 size,Material mat,bool collision)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(parent,false);
        obj.transform.position=position; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=mat;
        if (!collision) Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.isStatic=true; return obj;
    }
}
