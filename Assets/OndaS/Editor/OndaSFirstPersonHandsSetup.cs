using System;
using System.Collections.Generic;
using System.Linq;
using OndaS.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class OndaSFirstPersonHandsSetup
{
    const string Root = "Assets/OndaS/Characters/OfficeWorker";
    const string Model = Root + "/fbx/office_worker.fbx";
    const string Scene = "Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity";

    [MenuItem("OndaS/Configurar manos en primera persona")]
    public static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != Scene || scene.isDirty) { Debug.LogError("Abre OndaS_RetroLookDev y guarda la escena antes de configurar las manos."); return; }
        var player = GameObject.Find("Base Scene/First Person Player");
        var character = player != null ? player.transform.Find("Office Worker") : null;
        var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;
        var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/OfficeWorker.mat");
        if (character == null || camera == null || material == null) { Debug.LogError("No se encontró el jugador, la cámara o el material del personaje."); return; }
        var importer = AssetImporter.GetAtPath(Model) as ModelImporter;
        if (importer == null) { Debug.LogError("No se encontró el FBX del personaje."); return; }
        if (!importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
        var renderer = character.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (renderer == null) { Debug.LogError("El personaje no tiene una malla animada."); return; }
        var leftMesh = BuildArm(renderer, character, "Left");
        var rightMesh = BuildArm(renderer, character, "Right");
        if (leftMesh == null || rightMesh == null) return;
        var existing = camera.transform.Find("OndaS First Person Hands");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        var root = new GameObject("OndaS First Person Hands");
        root.transform.SetParent(camera.transform, false);
        var left = CreateArm(root.transform, "Left Arm", leftMesh, material, new Vector3(-0.29f, -0.36f, 0.38f), new Vector3(0.22f, 0.30f, 0.93f));
        var right = CreateArm(root.transform, "Right Arm", rightMesh, material, new Vector3(0.29f, -0.36f, 0.38f), new Vector3(-0.22f, 0.30f, 0.93f));
        var bob = root.AddComponent<OndaSFirstPersonHands>();
        var serialized = new SerializedObject(bob);
        serialized.FindProperty("leftArm").objectReferenceValue = left;
        serialized.FindProperty("rightArm").objectReferenceValue = right;
        serialized.FindProperty("controller").objectReferenceValue = player.GetComponent<CharacterController>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: manos de primera persona configuradas con la malla y textura originales.");
    }

    static Transform CreateArm(Transform parent, string name, Mesh mesh, Material material, Vector3 pos, Vector3 direction)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.FromToRotation(Vector3.down, direction.normalized);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return go.transform;
    }

    static Mesh BuildArm(SkinnedMeshRenderer renderer, Transform root, string side)
    {
        var mesh = renderer.sharedMesh;
        var bones = renderer.bones;
        int forearm = Array.FindIndex(bones, b => b != null && b.name == side + "ForeArm");
        int hand = Array.FindIndex(bones, b => b != null && b.name == side + "Hand");
        if (mesh == null || forearm < 0 || hand < 0) { Debug.LogError("No se encontraron los huesos del brazo " + side); return null; }
        var weights = mesh.boneWeights;
        var baked = new Mesh(); renderer.BakeMesh(baked);
        var vertices = baked.vertices; var normals = baked.normals; var uvs = mesh.uv; var colors = mesh.colors;
        var triangles = mesh.triangles;
        var selected = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (IsArm(weights[triangles[i]], forearm, hand) && IsArm(weights[triangles[i+1]], forearm, hand) && IsArm(weights[triangles[i+2]], forearm, hand))
            { selected.Add(triangles[i]); selected.Add(triangles[i+1]); selected.Add(triangles[i+2]); }
        }
        if (selected.Count == 0) { UnityEngine.Object.DestroyImmediate(baked); Debug.LogError("No se encontró geometría para el brazo " + side); return null; }
        var pivot = root.InverseTransformPoint(bones[forearm].position);
        var matrix = root.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint3x4(vertices[i]) - pivot;
            normals[i] = matrix.MultiplyVector(normals[i]).normalized;
        }
        var result = new Mesh { name = side + " First Person Arm", vertices = vertices, normals = normals, uv = uvs, triangles = selected.ToArray() };
        if (colors.Length == vertices.Length) result.colors = colors;
        result.RecalculateBounds();
        UnityEngine.Object.DestroyImmediate(baked);
        string path = Root + "/" + side + "FirstPersonArm.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) { EditorUtility.CopySerialized(result, existing); UnityEngine.Object.DestroyImmediate(result); EditorUtility.SetDirty(existing); return existing; }
        AssetDatabase.CreateAsset(result, path);
        return result;
    }

    static bool IsArm(BoneWeight weight, int forearm, int hand)
    {
        return weight.weight0 > 0.5f && (weight.boneIndex0 == forearm || weight.boneIndex0 == hand);
    }
}
