using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using OndaS.Visual;

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
    [MenuItem("OndaS/Crear alumbrado exterior")]
    public static void CreateStreetLights()
    {
        var scene = SceneManager.GetActiveScene();
        var street = GameObject.Find(StreetName);
        if (scene.path != ScenePath || EditorApplication.isPlaying || street == null || street.transform.Find("Postes de luz") != null)
        { Debug.LogError("Abre la escena principal y crea la carretera antes del alumbrado."); return; }
        var metal = Material("Calle_Poste_Metal", new Color(.16f,.17f,.16f));
        var lamp = Material("Calle_Farola", new Color(.72f,.66f,.49f));
        ConfigureLampMaterial(lamp);
        var posts = new GameObject("Postes de luz").transform;
        posts.SetParent(street.transform, false);
        foreach (float z in new[] {-30f, -15f, 10f, 25f, 40f}) CreatePost(posts, 40.05f, z, 1f, metal, lamp);
        foreach (float z in new[] {-23f, -7f, 1f, 17f, 33f}) CreatePost(posts, 51.95f, z, -1f, metal, lamp);
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: postes de luz exteriores creados.", posts);
    }
    static void CreatePost(Transform parent, float x, float z, float direction, Material metal, Material lamp)
    {
        var post = new GameObject("Poste de luz").transform;
        post.SetParent(parent, false); post.position = new Vector3(x, .15f, z);
        post.gameObject.AddComponent<OndaSStreetLight>();
        Block(post, "Base", post.position + new Vector3(0,.12f,0), new Vector3(.4f,.24f,.4f), metal, true);
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name="Fuste"; pole.transform.SetParent(post,false); pole.transform.localPosition=new Vector3(0,2.8f,0);
        pole.transform.localScale=new Vector3(.16f,2.8f,.16f); pole.GetComponent<Renderer>().sharedMaterial=metal;
        Block(post,"Brazo",post.position+new Vector3(direction*.62f,5.5f,0),new Vector3(1.28f,.12f,.12f),metal,false);
        Block(post,"Carcasa",post.position+new Vector3(direction*1.2f,5.44f,0),new Vector3(.60f,.17f,.38f),metal,false);
        Block(post,"Difusor",post.position+new Vector3(direction*1.2f,5.34f,0),new Vector3(.50f,.05f,.30f),lamp,false);
        var bulb = new GameObject("Luz cálida",typeof(Light)); bulb.transform.SetParent(post,false);
        bulb.transform.localPosition=new Vector3(direction*1.2f,5.29f,0); bulb.transform.localRotation=Quaternion.Euler(90,0,0);
        var light=bulb.GetComponent<Light>(); light.type=LightType.Spot; light.range=22; light.spotAngle=120;
        light.color=new Color(1f,.72f,.43f); light.intensity=240f; light.shadows=LightShadows.None;
    }

    [MenuItem("OndaS/Conectar luces exteriores")]
    public static void ConnectStreetLights()
    {
        var street = GameObject.Find(StreetName);
        if (EditorApplication.isPlaying || street == null) { Debug.LogError("Falta la calle o Unity está en Play."); return; }
        foreach (var light in street.GetComponentsInChildren<Light>(true)) {
            var old = light.GetComponent<OndaSStreetLight>();
            if (old != null) Object.DestroyImmediate(old);
            if (light.GetComponentInParent<OndaSStreetLight>() == null) light.transform.parent.gameObject.AddComponent<OndaSStreetLight>();
            light.intensity = 240f;
            light.range = 22f;
        }
        ConfigureLampMaterial(Material("Calle_Farola", new Color(.72f,.66f,.49f)));
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("OndaS: alumbrado exterior independiente del apagón.");
    }

    [MenuItem("OndaS/Completar alumbrado frente a la salida")]
    public static void AddExitLight()
    {
        var scene = SceneManager.GetActiveScene();
        var street = GameObject.Find(StreetName);
        var posts = street != null ? street.transform.Find("Postes de luz") : null;
        if (EditorApplication.isPlaying || posts == null) { Debug.LogError("Faltan los postes exteriores."); return; }
        foreach (Transform post in posts) if (Vector3.Distance(post.position, new Vector3(51.95f,.15f,1f)) < .1f) return;
        CreatePost(posts, 51.95f, 1f, -1f, Material("Calle_Poste_Metal", new Color(.16f,.17f,.16f)), Material("Calle_Farola", new Color(.72f,.66f,.49f)));
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    static void ConfigureLampMaterial(Material material)
    {
        if (!material.HasProperty("_EmissionColor")) return;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", new Color(1f,.64f,.29f) * 1.5f);
        EditorUtility.SetDirty(material);
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
