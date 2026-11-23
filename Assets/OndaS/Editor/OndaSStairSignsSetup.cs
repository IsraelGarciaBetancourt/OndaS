using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Generates low-resolution, weathered signs as scene geometry, without gameplay colliders.
public static class OndaSStairSignsSetup
{
    const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
    const string Folder = "Assets/OndaS/Art/Textures/SeñalesPisos";
    const string RootName = "OndaS - Señales de pisos en escaleras";
    static readonly string[] Digits = {
        "111101101101111", "010110010010111", "111001111100111", "111001111001111",
        "101101111001001", "111100111001111", "111100111101111", "111001010010010",
        "111101111101111"
    };
    static readonly string[] Letters = { "110101110100100", "111010010010111", "111100111001111", "111101101101111" }; // P I S O

    [MenuItem("OndaS/Crear señales de pisos en escaleras")]
    public static void Create()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || EditorApplication.isPlaying || GameObject.Find(RootName) != null)
        { Debug.LogError("Abre la escena principal fuera de Play y comprueba que no existan las señales."); return; }
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/OndaS/Art/Textures", "SeñalesPisos");
        var root = new GameObject(RootName).transform;
        for (int floor = 1; floor <= 8; floor++)
        {
            Material material = MakeMaterial(floor);
            foreach (float z in new[] { -18f, 18f })
            {
                var sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
                sign.name = "Piso " + floor + (z < 0 ? " - escalera izquierda" : " - escalera derecha");
                sign.transform.SetParent(root, false);
                sign.transform.position = new Vector3(13.63f, (floor - 1) * 4f + 1.72f, z);
                sign.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                sign.transform.localScale = new Vector3(.72f, .92f, 1f);
                Object.DestroyImmediate(sign.GetComponent<Collider>());
                sign.GetComponent<MeshRenderer>().sharedMaterial = material;
            }
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: 16 señales de pisos creadas en ambas escaleras.", root);
    }

    static Material MakeMaterial(int floor)
    {
        var tex = new Texture2D(96, 128, TextureFormat.RGB24, false);
        var random = new System.Random(812 + floor);
        for (int y = 0; y < tex.height; y++)
        for (int x = 0; x < tex.width; x++)
        {
            float noise = (float)random.NextDouble() * .09f;
            bool edge = x < 4 || x >= 92 || y < 4 || y >= 124;
            bool border = (x >= 7 && x < 9 || x >= 87 && x < 89) && y >= 7 && y < 121 || (y >= 7 && y < 9 || y >= 119 && y < 121) && x >= 7 && x < 89;
            var c = edge ? new Color(.15f, .14f, .12f) : border ? new Color(.57f, .52f, .40f) : new Color(.20f + noise, .21f + noise, .19f + noise * .8f);
            tex.SetPixel(x, y, c);
        }
        var ink = new Color(.77f, .73f, .60f);
        for (int i = 0; i < 4; i++) DrawGlyph(tex, Letters[i], 16 + i * 17, 101, 4, ink, random);
        DrawGlyph(tex, Digits[floor], 28, 24, 13, ink, random);
        tex.Apply();
        string png = Folder + "/Piso_" + floor + ".png";
        File.WriteAllBytes(png, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(png, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(png);
        importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.SaveAndReimport();
        string matPath = Folder + "/Piso_" + floor + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, matPath); }
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(png);
        mat.SetFloat("_Smoothness", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void DrawGlyph(Texture2D tex, string glyph, int left, int bottom, int scale, Color ink, System.Random random)
    {
        for (int row = 0; row < 5; row++) for (int col = 0; col < 3; col++)
        {
            if (glyph[row * 3 + col] != '1') continue;
            for (int y = 0; y < scale; y++) for (int x = 0; x < scale; x++)
            {
                // Sparse, dark scratches keep the lettering worn but readable.
                if (random.Next(100) < 4) continue;
                tex.SetPixel(left + col * scale + x, bottom + (4 - row) * scale + y, ink);
            }
        }
    }
}
