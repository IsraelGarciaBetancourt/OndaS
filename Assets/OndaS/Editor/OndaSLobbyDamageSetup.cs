using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSLobbyDamageSetup
{
    const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    const string DamagePath = "Structure_03/Interior/OndaS - Derrumbes pisos 8 y 7/Daños visibles tras el sismo";
    const string Folder = "Assets/OndaS/Art/Aftermath/";
    static Material concrete, metal;
    static Mesh[] chunks;

    [MenuItem("OndaS/Destrozar estructura del lobby")]
    public static void StructuralDamage()
    {
        var damage = Prepare("Lobby - daños estructurales");
        if (damage == null) return;
        var root = Group(damage, "Lobby - daños estructurales");
        Collapse(root, "Derrumbe junto a las escaleras", new Vector3(16.8f,.1f,8.7f), 1);
        Collapse(root, "Sección derrumbada lateral", new Vector3(23f,.1f,-5.8f), -1);
        Collapse(root, "Viga caída junto a la entrada", new Vector3(28.3f,.1f,7f), 1);
        Save("estructura del lobby");
    }

    [MenuItem("OndaS/Desgastar techo y suelo del lobby")]
    public static void SurfaceDamage()
    {
        var damage = Prepare("Lobby - techo y restos del sismo");
        if (damage == null) return;
        var root = Group(damage, "Lobby - techo y restos del sismo");
        DamagedCeiling(root, new Vector3(16.8f,3.02f,8.7f), 1);
        DamagedCeiling(root, new Vector3(23f,3.02f,-5.8f), -1);
        DamagedCeiling(root, new Vector3(28.3f,3.02f,7f), 1);
        DamagedCeiling(root, new Vector3(21f,3.02f,1f), -1);
        var scatter = Group(root,"Restos repartidos por el vestíbulo");
        var points = new[] {
            new Vector3(13.8f,.1f,7.5f), new Vector3(15.1f,.1f,5.8f), new Vector3(18f,.1f,6.9f),
            new Vector3(18.6f,.1f,-1.3f), new Vector3(20.4f,.1f,-3.2f), new Vector3(22.2f,.1f,2.4f),
            new Vector3(24.8f,.1f,-3.6f), new Vector3(25f,.1f,5.3f), new Vector3(27f,.1f,3.4f),
            new Vector3(30.3f,.1f,-2.5f), new Vector3(31.5f,.1f,5.5f)
        };
        for(int i=0;i<points.Length;i++) {
            var p=points[i];
            Piece(scatter,"Trozo de yeso y hormigón",p+new Vector3(0,.07f,0),new Vector3(.42f,.14f,.32f),new Vector3(3,i*39,4),i%4);
            Piece(scatter,"Astilla del techo",p+new Vector3(.27f,.04f,.19f),new Vector3(.24f,.07f,.16f),new Vector3(0,i*67,0),(i+1)%4);
        }
        for(int i=0;i<5;i++) {
            var p=new Vector3(17.7f+i*1.7f,.112f,-2.0f+(i%2)*.4f);
            Bar(root,p,new Vector3(.68f,.016f,.028f),new Vector3(0,20+i*16,0));
        }
        Save("techo y restos del lobby");
    }

    static void DamagedCeiling(Transform parent,Vector3 p,int side)
    {
        var g=Group(parent,"Techo roto y varillas colgantes");
        // Dark irregular backing suggests a missing section, while the fragments hang below it.
        Piece(g,"Hueco entre placas rotas",p,new Vector3(2.5f,.035f,1.7f),new Vector3(0,side*11,0),2,metal);
        Piece(g,"Placa de techo quebrada",p+new Vector3(-.55f,-.16f,-.22f),new Vector3(1.05f,.11f,.85f),new Vector3(side*9,side*15,side*12),0);
        Piece(g,"Placa de techo desprendida",p+new Vector3(.65f,-.28f,.2f),new Vector3(1.1f,.12f,.8f),new Vector3(side*-10,side*-8,side*-20),1);
        for(int i=0;i<4;i++)
            Bar(g,p+new Vector3(-.85f+i*.56f,-.38f,(i%2==0?-.35f:.35f)),new Vector3(.025f,.7f+i*.1f,.025f),new Vector3(side*(8+i*4),0,side*(15+i*7)));
    }

    static Transform Prepare(string groupName)
    {
        var scene = SceneManager.GetActiveScene();
        var root = GameObject.Find(DamagePath);
        if (scene.path != ScenePath || EditorApplication.isPlaying || root == null || root.transform.Find(groupName) != null)
        { Debug.LogError("Abre la escena principal y comprueba que el grupo no exista."); return null; }
        concrete = AssetDatabase.LoadAssetAtPath<Material>("Assets/Brick Project Studio/_BPS Basic Assets/Common/Materials/Basic Assets/BA_Concrete_01.mat");
        metal = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Aftermath_Metal.mat");
        chunks = new Mesh[4];
        for(int i=0;i<chunks.Length;i++) chunks[i]=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"Concrete_Fragment_"+(i+1)+".asset");
        if(concrete==null || metal==null || System.Array.Exists(chunks, mesh=>mesh==null))
        { Debug.LogError("Faltan los materiales o fragmentos del derrumbe."); return null; }
        return root.transform;
    }
    static void Save(string label)
    {
        var scene=SceneManager.GetActiveScene(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: completada " + label + ".");
    }
    static Transform Group(Transform parent,string name)
    {
        var obj=new GameObject(name); obj.transform.SetParent(parent,false); return obj.transform;
    }
    static void Piece(Transform parent,string name,Vector3 position,Vector3 scale,Vector3 rotation,int variant,Material mat=null)
    {
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); obj.transform.SetParent(parent,false);
        obj.transform.position=position; obj.transform.localScale=scale; obj.transform.rotation=Quaternion.Euler(rotation);
        obj.GetComponent<MeshFilter>().sharedMesh=chunks[variant%chunks.Length];
        obj.GetComponent<MeshRenderer>().sharedMaterial=mat!=null?mat:concrete; obj.isStatic=true;
    }
    static void Box(Transform parent,string name,Vector3 position,Vector3 size)
    {
        var obj=new GameObject(name,typeof(BoxCollider)); obj.transform.SetParent(parent,false);
        obj.transform.position=position; obj.GetComponent<BoxCollider>().size=size; obj.isStatic=true;
    }
    static void Bar(Transform parent,Vector3 position,Vector3 scale,Vector3 rotation)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name="Varilla de acero expuesta"; obj.transform.SetParent(parent,false);
        obj.transform.position=position; obj.transform.localScale=scale; obj.transform.rotation=Quaternion.Euler(rotation);
        obj.GetComponent<Renderer>().sharedMaterial=metal; Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.isStatic=true;
    }
    static void Collapse(Transform parent,string name,Vector3 p,int side)
    {
        var g=Group(parent,name);
        Piece(g,"Losa principal fracturada",p+new Vector3(0,.48f,0),new Vector3(2.6f,.55f,1.5f),new Vector3(5,side*14,side*13),0);
        Piece(g,"Panel de concreto inclinado",p+new Vector3(.45f,.91f,.2f),new Vector3(1.55f,.32f,1.25f),new Vector3(8,side*-21,side*-27),1);
        Piece(g,"Viga rota",p+new Vector3(-.65f,.33f,-.35f),new Vector3(1.5f,.46f,.65f),new Vector3(-9,side*38,side*9),2);
        Box(g,"Colisión de la losa caída",p+new Vector3(0,.44f,0),new Vector3(2.3f,.85f,1.18f));
        for(int i=0;i<5;i++) {
            float dx=(i-2)*.49f, dz=(i%2==0?-.84f:.8f);
            Piece(g,"Piedra desprendida",p+new Vector3(dx,.12f,dz),new Vector3(.42f,.24f,.35f),new Vector3(4,i*36,side*7),i%4);
        }
        for(int i=0;i<3;i++) Bar(g,p+new Vector3(-.8f+i*.7f,1.03f,.05f),new Vector3(.035f,.8f,.035f),new Vector3(side*16,0,side*(20+i*9)));
    }
}
