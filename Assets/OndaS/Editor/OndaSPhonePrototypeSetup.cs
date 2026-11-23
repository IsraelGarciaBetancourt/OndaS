using System.IO;
using UnityEditor;
using UnityEngine;

// Builds a small editable PSX-style phone prefab for review.
public static class OndaSPhonePrototypeSetup
{
    const string Folder = "Assets/OndaS/Art/Models/CelularPSX";
    [MenuItem("OndaS/Crear prototipo celular PSX")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/OndaS/Art/Models", "CelularPSX");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Celular_PSX.prefab") != null)
        { Debug.LogWarning("El prototipo de celular ya existe."); return; }
        var casing = Mat("Carcasa", new Color(.18f,.19f,.18f), false);
        var trim = Mat("Marco", new Color(.055f,.06f,.055f), false);
        var metal = Mat("Botones", new Color(.36f,.35f,.31f), false);
        var lens = Mat("Camara", new Color(.06f,.10f,.12f), false);
        var screen = Mat("Pantalla_alerta", Color.white, true);
        screen.mainTexture = MakeScreen(); EditorUtility.SetDirty(screen);
        var root = new GameObject("Celular PSX - prototipo");
        var body = new GameObject("Carcasa biselada", typeof(MeshFilter), typeof(MeshRenderer));
        body.transform.SetParent(root.transform, false);
        body.GetComponent<MeshFilter>().sharedMesh = MakeBody();
        body.GetComponent<MeshRenderer>().sharedMaterial = casing;
        Box(root.transform, "Marco de pantalla", new Vector3(0,0,-.0062f), new Vector3(.068f,.139f,.0015f), trim);
        var face = GameObject.CreatePrimitive(PrimitiveType.Quad); face.name = "Pantalla - alerta sísmica";
        face.transform.SetParent(root.transform, false); face.transform.localPosition = new Vector3(0,-.002f,-.0071f);
        face.transform.localScale = new Vector3(.060f,.121f,1f); Object.DestroyImmediate(face.GetComponent<Collider>());
        face.GetComponent<MeshRenderer>().sharedMaterial = screen;
        Box(root.transform, "Altavoz superior", new Vector3(0,.066f,-.0072f), new Vector3(.016f,.0015f,.0008f), metal);
        Box(root.transform, "Cámara frontal", new Vector3(-.014f,.066f,-.0073f), new Vector3(.003f,.003f,.0009f), lens);
        Box(root.transform, "Botón lateral", new Vector3(.0378f,.027f,0), new Vector3(.0018f,.016f,.004f), metal);
        Box(root.transform, "Volumen", new Vector3(-.0378f,.031f,0), new Vector3(.0018f,.020f,.004f), metal);
        Box(root.transform, "Cámara posterior", new Vector3(-.022f,.055f,.0062f), new Vector3(.010f,.010f,.0015f), trim);
        Box(root.transform, "Lente posterior", new Vector3(-.022f,.055f,.0071f), new Vector3(.005f,.005f,.0008f), lens);
        PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Celular_PSX.prefab");
        Object.DestroyImmediate(root); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("OndaS: prototipo de celular creado en " + Folder);
    }
    static Material Mat(string name, Color color, bool unlit)
    {
        var mat = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
        mat.name = name; mat.color = color;
        if (!unlit) mat.SetFloat("_Smoothness", .12f);
        AssetDatabase.CreateAsset(mat, Folder + "/" + name + ".mat"); return mat;
    }
    static void Box(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false);
        go.transform.localPosition = pos; go.transform.localScale = size; Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
    static Mesh MakeBody()
    {
        float w=.0375f, h=.075f, c=.006f, depth=.006f;
        var outline = new [] { new Vector2(-w+c,-h), new Vector2(w-c,-h), new Vector2(w,-h+c), new Vector2(w,h-c), new Vector2(w-c,h), new Vector2(-w+c,h), new Vector2(-w,h-c), new Vector2(-w,-h+c) };
        var vertices = new Vector3[16]; for(int i=0;i<8;i++) { vertices[i]=new Vector3(outline[i].x,outline[i].y,-depth); vertices[i+8]=new Vector3(outline[i].x,outline[i].y,depth); }
        var tris = new System.Collections.Generic.List<int>();
        for(int i=1;i<7;i++) { tris.Add(0); tris.Add(i+1); tris.Add(i); tris.Add(8); tris.Add(i+8); tris.Add(i+9); }
        for(int i=0;i<8;i++) { int n=(i+1)%8; tris.Add(i); tris.Add(n); tris.Add(n+8); tris.Add(i); tris.Add(n+8); tris.Add(i+8); }
        var mesh=new Mesh { name="Carcasa biselada low poly", vertices=vertices, triangles=tris.ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Folder+"/Carcasa_mesh.asset"); return mesh;
    }
    static Texture2D MakeScreen()
    {
        var tex = new Texture2D(64,128,TextureFormat.RGB24,false); var rnd=new System.Random(77);
        var dark=new Color32(19,27,27,255); var light=new Color32(214,213,191,255); var yellow=new Color32(237,196,76,255); var red=new Color32(133,41,37,255);
        for(int y=0;y<128;y++) for(int x=0;x<64;x++) { int noise=rnd.Next(0,7); tex.SetPixel(x,y,new Color32((byte)(dark.r+noise),(byte)(dark.g+noise),(byte)(dark.b+noise),255)); }
        Rect(tex,0,114,64,14,new Color32(31,39,38,255)); Draw(tex,"06:32",3,117,1,light); Rect(tex,52,119,8,4,light); Rect(tex,60,120,1,2,light);
        Draw(tex,"ALERTA",5,96,2,yellow); Rect(tex,4,89,56,2,yellow);
        Rect(tex,4,63,56,22,red); Draw(tex,"SISMO",7,69,2,light);
        Draw(tex,"BUSCA",5,47,1,light); Draw(tex,"REFUGIO",5,35,1,light); Rect(tex,4,26,42,1,new Color32(85,95,87,255));
        for(int i=0;i<90;i++) { int x=rnd.Next(2,62),y=rnd.Next(2,126); if(rnd.Next(4)==0) tex.SetPixel(x,y,new Color32(54,56,48,255)); }
        tex.Apply(); string path=Folder+"/Pantalla_alerta.png"; File.WriteAllBytes(path,tex.EncodeToPNG()); Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport); var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.filterMode=FilterMode.Point; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.mipmapEnabled=false; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Rect(Texture2D tex,int x,int y,int w,int h,Color32 c) { for(int yy=y;yy<y+h;yy++) for(int xx=x;xx<x+w;xx++) tex.SetPixel(xx,yy,c); }
    static void Draw(Texture2D tex,string text,int x,int y,int scale,Color32 color)
    {
        string[] chars={"A:010101111101101","B:110101110101110","C:011100100100011","E:111100110100111","F:111100110100100","G:011100101101011","I:111010010010111","L:100100100100111","M:101111111101101","O:111101101101111","R:110101110101101","S:011100010001110","T:111010010010010","U:101101101101111","0:111101101101111","2:110001010100111","3:110001010001110","6:011100110101010",": :000010000010000"};
        foreach(char ch in text) { string glyph=null; foreach(string def in chars) if(def[0]==ch) { glyph=def.Substring(def.Length-15); break; } if(glyph!=null) for(int r=0;r<5;r++) for(int c=0;c<3;c++) if(glyph[r*3+c]=='1') Rect(tex,x+c*scale,y+(4-r)*scale,scale,scale,color); x+=4*scale; }
    }
}
