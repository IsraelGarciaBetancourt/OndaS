using UnityEditor;
using UnityEngine;

public static class OndaSCloudDustSetup
{
    const string TexturePath = "Assets/OndaS/Art/Textures/PolvoNubePSX.png";
    const string MaterialPath = "Assets/OndaS/Art/Materials/PolvoNubePSX.mat";

    [MenuItem("OndaS/Preparar textura de polvo")]
    public static void PrepareMaterial()
    {
        var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null) { Debug.LogError("Falta la textura de polvo."); return; }
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 512;
        importer.SaveAndReimport();
        var shader = Shader.Find("OndaS/CloudDust");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (shader == null || texture == null) { Debug.LogError("Falta el shader o la textura de polvo."); return; }
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null) { material = new Material(shader) { name = "PolvoNubePSX" }; AssetDatabase.CreateAsset(material, MaterialPath); }
        material.shader = shader; material.SetTexture("_MainTex", texture); material.SetColor("_Color", Color.white);
        EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
        Debug.Log("OndaS: textura de polvo preparada.");
    }
}
