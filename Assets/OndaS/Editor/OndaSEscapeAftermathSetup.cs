using System;
using OndaS.Gameplay.Earthquake;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds the first two floors of the post-earthquake escape route.
public static class OndaSEscapeAftermathSetup
{
    private const string AssetFolder = "Assets/OndaS/Art/Aftermath";
    private const string RootName = "OndaS - Derrumbes pisos 8 y 7";
    private static Mesh[] chunks;
    private static Material concrete;
    private static Material metal;

    [MenuItem("OndaS/Crear derrumbes pisos 8 y 7")]
    public static void Create()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity" || EditorApplication.isPlaying)
        { Debug.LogError("Abre OndaS_RetroEdificio y sal de Play antes de crear los derrumbes."); return; }
        var interior = GameObject.Find("Structure_03/Interior");
        var floor = GameObject.Find("Structure_03/Interior/8th Floor");
        var earthquake = floor != null ? floor.GetComponent<OndaSFirstEarthquake>() : null;
        if (interior == null || earthquake == null || interior.transform.Find(RootName) != null)
        { Debug.LogError("No se encontró el interior o el sismo, o los derrumbes ya existen."); return; }

        PrepareAssets();
        if (concrete == null || metal == null) { Debug.LogError("No se encontraron los materiales de los derrumbes."); return; }
        var root = new GameObject(RootName);
        root.transform.SetParent(interior.transform, false);
        var controller = root.AddComponent<OndaSEarthquakeAftermath>();
        var damage = Group(root.transform, "Daños visibles tras el sismo");
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("earthquake").objectReferenceValue = earthquake;
        serialized.FindProperty("aftermath").objectReferenceValue = damage.gameObject;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // Floor surfaces are at y=28.10 and y=24.10; corridors are 2.5 m wide.
        BlockedStairs(damage, "Piso 8 - escalera izquierda bloqueada", new Vector3(12.5f, 28.1f, -13.0f), false);
        BlockedStairs(damage, "Piso 7 - bajada derecha bloqueada", new Vector3(12.5f, 24.1f, 19.6f), true);
        CrawlPassage(damage);
        ScatteredPieces(damage, "Piso 8 - restos del pasadizo", 28.1f,
            new Vector3(11.45f, 0f, -4.5f), new Vector3(13.48f, 0f, 0.0f), new Vector3(11.48f, 0f, 8.0f), new Vector3(13.48f, 0f, 12.0f));
        ScatteredPieces(damage, "Piso 7 - restos del pasadizo", 24.1f,
            new Vector3(11.48f, 0f, 12.0f), new Vector3(13.48f, 0f, 3.0f), new Vector3(11.48f, 0f, -4.0f), new Vector3(13.48f, 0f, -10.0f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: derrumbes de los pisos 8 y 7 creados.", root);
    }

    [MenuItem("OndaS/Crear derrumbes pisos 6 y 5")]
    public static void Extend()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity" || EditorApplication.isPlaying)
        { Debug.LogError("Abre OndaS_RetroEdificio y sal de Play antes de crear los derrumbes."); return; }
        var root = GameObject.Find("Structure_03/Interior/" + RootName);
        var controller = root != null ? root.GetComponent<OndaSEarthquakeAftermath>() : null;
        var damage = root != null ? root.transform.Find("Daños visibles tras el sismo") : null;
        var hallwayWall = GameObject.Find("Structure_03/Interior/6nd Floor/Hallways/Walls/Int_Apt_02_Wall_01 (2)");
        var apartmentWall = GameObject.Find("Structure_03/Interior/6nd Floor/Apartment_04/Walls/Int_Apt_01_Wall_01 (24)");
        if (controller == null || damage == null || hallwayWall == null || apartmentWall == null ||
            damage.Find("Piso 6 - bajada izquierda bloqueada") != null)
        { Debug.LogError("Faltan los derrumbes anteriores o las paredes del piso 6, o el tramo ya existe."); return; }
        PrepareAssets();
        if (concrete == null || metal == null) { Debug.LogError("No se encontraron los materiales de los derrumbes."); return; }

        BlockedStairs(damage, "Piso 6 - bajada izquierda bloqueada", new Vector3(12.5f, 20.1f, -17.1f), true);
        BlockedStairs(damage, "Piso 6 - pasadizo cortado", new Vector3(12.5f, 20.1f, 5.5f), false);
        ApartmentBreach(damage);
        BlockedStairs(damage, "Piso 5 - bajada derecha bloqueada", new Vector3(12.5f, 16.1f, 19.6f), true);
        PartialRubble(damage, "Piso 5 - derrumbe junto a pared izquierda", new Vector3(11.85f, 16.1f, 1.0f), false);
        PartialRubble(damage, "Piso 5 - derrumbe junto a pared derecha", new Vector3(13.15f, 16.1f, -5.0f), true);
        ScatteredPieces(damage, "Piso 6 - restos del pasadizo", 20.1f,
            new Vector3(11.47f, 0f, -11f), new Vector3(13.48f, 0f, -5f), new Vector3(11.48f, 0f, 12f));
        ScatteredPieces(damage, "Piso 5 - restos del pasadizo", 16.1f,
            new Vector3(11.48f, 0f, 12f), new Vector3(13.48f, 0f, 7f), new Vector3(11.48f, 0f, -10f));

        var serialized = new SerializedObject(controller);
        var removed = serialized.FindProperty("removeAfterEarthquake");
        removed.arraySize = 2;
        removed.GetArrayElementAtIndex(0).objectReferenceValue = hallwayWall;
        removed.GetArrayElementAtIndex(1).objectReferenceValue = apartmentWall;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: derrumbes y desvío por el departamento del piso 6 creados.", root);
    }

    private static void PrepareAssets()
    {
        if (!AssetDatabase.IsValidFolder(AssetFolder)) AssetDatabase.CreateFolder("Assets/OndaS/Art", "Aftermath");
        var source = AssetDatabase.LoadAssetAtPath<Material>("Assets/Brick Project Studio/_BPS Basic Assets/Common/Materials/Basic Assets/BA_Concrete_01.mat");
        concrete = source;
        string metalPath = AssetFolder + "/Aftermath_Metal.mat";
        metal = AssetDatabase.LoadAssetAtPath<Material>(metalPath);
        if (metal == null && source != null)
        {
            metal = new Material(source) { name = "Aftermath_Metal" };
            if (metal.HasProperty("_BaseMap")) metal.SetTexture("_BaseMap", null);
            if (metal.HasProperty("_BumpMap")) metal.SetTexture("_BumpMap", null);
            if (metal.HasProperty("_BaseColor")) metal.SetColor("_BaseColor", new Color(0.16f, 0.14f, 0.12f));
            if (metal.HasProperty("_Metallic")) metal.SetFloat("_Metallic", 0.35f);
            AssetDatabase.CreateAsset(metal, metalPath);
        }
        chunks = new Mesh[4];
        for (int i = 0; i < chunks.Length; i++)
        {
            string path = AssetFolder + "/Concrete_Fragment_" + (i + 1) + ".asset";
            chunks[i] = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (chunks[i] != null) continue;
            chunks[i] = CreateChunk(183 + i * 71);
            chunks[i].name = "Concrete_Fragment_" + (i + 1);
            AssetDatabase.CreateAsset(chunks[i], path);
        }
        AssetDatabase.SaveAssets();
    }

    private static Mesh CreateChunk(int seed)
    {
        var random = new System.Random(seed);
        var corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            float x = (i & 1) == 0 ? -0.5f : 0.5f;
            float y = (i & 2) == 0 ? -0.5f : 0.5f;
            float z = (i & 4) == 0 ? -0.5f : 0.5f;
            corners[i] = new Vector3(x + ((float)random.NextDouble() - 0.5f) * 0.22f,
                y + ((float)random.NextDouble() - 0.5f) * 0.22f,
                z + ((float)random.NextDouble() - 0.5f) * 0.22f);
        }
        int[] faces = { 0,2,3, 0,3,1, 4,5,7, 4,7,6, 0,1,5, 0,5,4,
                        2,6,7, 2,7,3, 0,4,6, 0,6,2, 1,3,7, 1,7,5 };
        var vertices = new Vector3[faces.Length];
        var triangles = new int[faces.Length];
        var uv = new Vector2[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            vertices[i] = corners[faces[i]];
            triangles[i] = i;
            uv[i] = i % 3 == 0 ? Vector2.zero : i % 3 == 1 ? Vector2.up : Vector2.one;
        }
        var mesh = new Mesh { vertices = vertices, triangles = triangles, uv = uv };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static Transform Group(Transform parent, string name)
    {
        var group = new GameObject(name).transform;
        group.SetParent(parent, false);
        return group;
    }

    private static void Fragment(Transform parent, string name, Vector3 pos, Vector3 scale, Vector3 rotation, int variant)
    {
        var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.transform.SetParent(parent, false);
        obj.transform.position = pos; obj.transform.localScale = scale; obj.transform.rotation = Quaternion.Euler(rotation);
        obj.GetComponent<MeshFilter>().sharedMesh = chunks[variant % chunks.Length];
        obj.GetComponent<MeshRenderer>().sharedMaterial = concrete;
        obj.isStatic = true;
    }

    private static void ColliderBox(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var obj = new GameObject(name, typeof(BoxCollider));
        obj.transform.SetParent(parent, false); obj.transform.position = position;
        obj.GetComponent<BoxCollider>().size = size;
        obj.isStatic = true;
    }

    private static void Bar(Transform parent, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Fierro expuesto"; obj.transform.SetParent(parent, false);
        obj.transform.position = position; obj.transform.localScale = scale; obj.transform.rotation = Quaternion.Euler(rotation);
        obj.GetComponent<MeshRenderer>().sharedMaterial = metal;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.isStatic = true;
    }

    private static void BlockedStairs(Transform parent, string name, Vector3 p, bool lowerFlight)
    {
        var root = Group(parent, name);
        // This collider fills the collapsed opening, while the right-hand landing stays accessible.
        ColliderBox(root, "Colisión del derrumbe", p + new Vector3(0f, 1.18f, 0f), new Vector3(lowerFlight ? 2.2f : 2.7f, 2.36f, 1.15f));
        float[] xs = { -0.83f, 0f, 0.83f, -0.72f, 0.14f, 0.87f, -0.46f, 0.44f };
        float[] ys = { 0.38f, 0.38f, 0.38f, 1.05f, 1.06f, 1.04f, 1.80f, 1.79f };
        for (int i = 0; i < xs.Length; i++)
            Fragment(root, "Bloque de concreto " + (i + 1), p + new Vector3(xs[i], ys[i], (i % 2 == 0 ? -0.06f : 0.07f)),
                new Vector3(i < 6 ? 0.94f : 1.3f, i < 6 ? 0.8f : 0.8f, 1.05f),
                new Vector3((i % 3 - 1) * 4f, (i % 2 == 0 ? 7f : -6f), (i % 3 - 1) * 7f), i);
        Fragment(root, "Losa inclinada", p + new Vector3(0.13f, 2.2f, 0.07f), new Vector3(2.15f, 0.34f, 1.05f), new Vector3(3f, -5f, 9f), 2);
        for (int i = 0; i < 3; i++)
            Bar(root, p + new Vector3(-0.75f + i * 0.72f, 2.25f, -0.1f), new Vector3(0.035f, 0.95f, 0.035f), new Vector3(12f, 0f, -24f + i * 21f));
        Fragment(root, "Piedra suelta", p + new Vector3(-0.83f, 0.13f, -0.72f), new Vector3(0.45f, 0.24f, 0.39f), new Vector3(0f, 25f, 7f), 3);
        Fragment(root, "Piedra suelta", p + new Vector3(0.64f, 0.11f, -0.75f), new Vector3(0.4f, 0.22f, 0.3f), new Vector3(0f, -15f, -5f), 1);
    }

    private static void CrawlPassage(Transform parent)
    {
        var root = Group(parent, "Piso 7 - viga caída para pasar agachado");
        var p = new Vector3(12.5f, 24.1f, 7.5f);
        var raised = p + Vector3.up * 0.4f;
        // Allow for the CharacterController's step offset while still blocking standing movement.
        ColliderBox(root, "Colisión de la viga", raised + new Vector3(0f, 1.57f, 0f), new Vector3(2.6f, 0.5f, 0.9f));
        Fragment(root, "Viga de concreto caída", raised + new Vector3(0f, 1.57f, 0f), new Vector3(2.55f, 0.52f, 0.9f), Vector3.zero, 0);
        Fragment(root, "Placa de techo", raised + new Vector3(-0.46f, 1.9f, 0.06f), new Vector3(1.4f, 0.28f, 0.95f), new Vector3(1f, -6f, 6f), 1);
        Fragment(root, "Placa de techo", raised + new Vector3(0.74f, 1.91f, -0.03f), new Vector3(1.15f, 0.34f, 0.96f), new Vector3(-2f, 4f, -7f), 2);
        Bar(root, raised + new Vector3(-0.75f, 2.12f, 0.1f), new Vector3(0.035f, 0.9f, 0.035f), new Vector3(15f, 0f, 25f));
        Bar(root, raised + new Vector3(0.8f, 2.1f, -0.1f), new Vector3(0.035f, 0.85f, 0.035f), new Vector3(-12f, 0f, -28f));
        Fragment(root, "Escombro junto a la pared", p + new Vector3(-1.02f, 0.17f, -0.7f), new Vector3(0.35f, 0.32f, 0.4f), new Vector3(0f, 20f, 4f), 3);
        Fragment(root, "Escombro junto a la pared", p + new Vector3(1.03f, 0.13f, 0.72f), new Vector3(0.35f, 0.26f, 0.35f), new Vector3(2f, -18f, 5f), 2);
    }

    private static void ApartmentBreach(Transform parent)
    {
        var root = Group(parent, "Piso 6 - abertura al pasadizo");
        var p = new Vector3(11.25f, 20.1f, 10f);
        // The two original wall sections disappear at the end of the quake.
        // Side colliders leave a clear opening through the damaged wall.
        for (int side = -1; side <= 1; side += 2)
        {
            var sidePos = p + new Vector3(0f, 0.6f, side * 1.08f);
            ColliderBox(root, "Colisión del borde de pared", sidePos, new Vector3(0.65f, 1.2f, 0.42f));
            Fragment(root, "Borde de pared roto", sidePos, new Vector3(0.68f, 1.25f, 0.48f), new Vector3(4f, side * 8f, side * 8f), side + 1);
            Fragment(root, "Restos en el suelo", p + new Vector3(-0.15f, 0.12f, side * 0.93f), new Vector3(0.65f, 0.22f, 0.35f), new Vector3(0f, side * 17f, 4f), side + 2);
        }
        Fragment(root, "Dintel agrietado", p + new Vector3(0f, 2.6f, 0f), new Vector3(0.66f, 0.55f, 2.5f), new Vector3(4f, 0f, 2f), 1);
        Bar(root, p + new Vector3(0.05f, 2.3f, -0.9f), new Vector3(0.035f, 0.7f, 0.035f), new Vector3(10f, 0f, -22f));
        Bar(root, p + new Vector3(-0.05f, 2.32f, 0.8f), new Vector3(0.035f, 0.65f, 0.035f), new Vector3(-12f, 0f, 18f));
    }

    private static void PartialRubble(Transform parent, string name, Vector3 p, bool right)
    {
        var root = Group(parent, name);
        ColliderBox(root, "Colisión de escombros", p + Vector3.up * 0.45f, new Vector3(1.25f, 0.9f, 0.95f));
        Fragment(root, "Losa caída", p + new Vector3(0f, 0.43f, 0f), new Vector3(1.2f, 0.85f, 0.95f), new Vector3(3f, right ? -8f : 8f, right ? -12f : 12f), 2);
        Fragment(root, "Trozo de pared", p + new Vector3(right ? 0.18f : -0.18f, 0.91f, 0.04f), new Vector3(0.8f, 0.3f, 0.72f), new Vector3(2f, 4f, right ? 10f : -10f), 3);
    }

    private static void ScatteredPieces(Transform parent, string name, float floor, params Vector3[] positions)
    {
        var root = Group(parent, name);
        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i]; p.y = floor;
            Fragment(root, "Fragmento de concreto", p + Vector3.up * 0.08f, new Vector3(0.42f, 0.16f, 0.32f), new Vector3(0f, i * 39f, 4f), i);
            Fragment(root, "Fragmento pequeño", p + new Vector3(i % 2 == 0 ? 0.16f : -0.16f, 0.045f, 0.25f), new Vector3(0.2f, 0.09f, 0.19f), new Vector3(0f, i * 67f, 3f), i + 1);
        }
    }
}
