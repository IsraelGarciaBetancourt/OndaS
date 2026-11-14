using OndaS.Gameplay.Earthquake;
using OndaS.Gameplay.Electrical;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OndaSExposedWiresSetup
{
    private const string AssetFolder = "Assets/OndaS/Art/Aftermath";
    private const string HazardName = "Piso 5 - cables eléctricos colgantes";

    [MenuItem("OndaS/Crear cables eléctricos piso 5")]
    public static void Create()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity" || EditorApplication.isPlaying)
        { Debug.LogError("Abre OndaS_RetroEdificio y sal de Play antes de colocar los cables."); return; }
        var root = GameObject.Find("Structure_03/Interior/OndaS - Derrumbes pisos 8 y 7");
        var damage = root != null ? root.transform.Find("Daños visibles tras el sismo") : null;
        var floor = GameObject.Find("Structure_03/Interior/8th Floor");
        var quake = floor != null ? floor.GetComponent<OndaSFirstEarthquake>() : null;
        var boxObject = GameObject.Find("Structure_03/Interior/8th Floor/Apartment_01/OndaS - Caja de luz");
        var fuseBox = boxObject != null ? boxObject.GetComponent<OndaSFuseBox>() : null;
        if (damage == null || quake == null || fuseBox == null || damage.Find(HazardName) != null)
        { Debug.LogError("Falta el sismo, la caja de luz o los derrumbes; también es posible que los cables ya existan."); return; }

        var metal = AssetDatabase.LoadAssetAtPath<Material>(AssetFolder + "/Aftermath_Metal.mat");
        var dust = AssetDatabase.LoadAssetAtPath<Material>("Assets/OndaS/Art/Materials/RetroDust.mat");
        if (metal == null || dust == null) { Debug.LogError("Faltan los materiales del edificio."); return; }
        var insulated = MakeMaterial(metal, "Cable_Aislamiento", new Color(0.045f, 0.055f, 0.06f));
        var red = MakeMaterial(metal, "Cable_Rojo", new Color(0.22f, 0.035f, 0.025f));
        var blue = MakeMaterial(metal, "Cable_Azul", new Color(0.025f, 0.075f, 0.16f));
        var copper = MakeMaterial(metal, "Cable_Cobre", new Color(0.58f, 0.25f, 0.07f));
        var sparkMat = MakeMaterial(dust, "Cable_Chispas", Color.white);
        AssetDatabase.SaveAssets();

        var obj = new GameObject(HazardName);
        obj.transform.SetParent(damage, false);
        obj.transform.position = new Vector3(12.5f, 16.1f, 7f);
        var trigger = obj.AddComponent<BoxCollider>();
        trigger.center = new Vector3(0f, 2.35f, 0f);
        trigger.size = new Vector3(2.6f, 1.8f, 1.5f);
        trigger.isTrigger = true;
        var body = obj.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;

        // A 1.72 m opening keeps the crouching controller, including its step offset, clear.
        var overhead = new GameObject("Colisión alta de los cables", typeof(BoxCollider));
        overhead.transform.SetParent(obj.transform, false);
        var overheadBox = overhead.GetComponent<BoxCollider>();
        overheadBox.center = new Vector3(0f, 1.86f, 0f);
        overheadBox.size = new Vector3(2.6f, 0.28f, 0.2f);

        var colors = new[] { insulated, red, insulated, blue, insulated, red };
        for (int i = 0; i < colors.Length; i++)
        {
            float x = -1.03f + 0.412f * i;
            float z = (i % 2 == 0 ? -0.07f : 0.07f);
            float tip = 1.47f + (i % 3) * 0.06f;
            var a = new Vector3(x, 3.05f, z);
            var b = new Vector3(x + (i % 2 == 0 ? 0.06f : -0.06f), 2.64f, z + 0.04f);
            var c = new Vector3(x + (i % 2 == 0 ? 0.11f : -0.08f), 2.14f, z - 0.02f);
            var d = new Vector3(x + (i % 2 == 0 ? 0.07f : -0.03f), tip + 0.18f, z);
            var e = new Vector3(d.x + 0.02f, tip, z + 0.02f);
            Rod(obj.transform, "Cable aislado", a, b, 0.026f, colors[i]);
            Rod(obj.transform, "Cable aislado", b, c, 0.026f, colors[i]);
            Rod(obj.transform, "Cable aislado", c, d, 0.026f, colors[i]);
            Rod(obj.transform, "Cobre expuesto", d, e, 0.017f, copper);
            Rod(obj.transform, "Hebra de cobre", e, e + new Vector3(-0.04f, -0.07f, 0.015f), 0.007f, copper);
            Rod(obj.transform, "Hebra de cobre", e, e + new Vector3(0.04f, -0.06f, -0.02f), 0.007f, copper);
        }

        var sparkObject = new GameObject("Chispas de cables", typeof(ParticleSystem));
        sparkObject.transform.SetParent(obj.transform, false);
        sparkObject.transform.localPosition = new Vector3(0f, 1.58f, 0f);
        var sparks = sparkObject.GetComponent<ParticleSystem>();
        var main = sparks.main; main.playOnAwake = false; main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.06f);
        main.startColor = new Color(0.75f, 0.88f, 1f, 1f); main.maxParticles = 64;
        main.gravityModifier = 0.5f;
        var emission = sparks.emission; emission.enabled = true; emission.rateOverTime = 0f;
        var shape = sparks.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2.15f, 0.08f, 0.15f);
        sparkObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = sparkMat;

        var lightObject = new GameObject("Destello eléctrico", typeof(Light));
        lightObject.transform.SetParent(obj.transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        var light = lightObject.GetComponent<Light>(); light.type = LightType.Point;
        light.color = new Color(0.65f, 0.8f, 1f); light.range = 3.5f; light.intensity = 1.4f;
        light.shadows = LightShadows.None; light.enabled = false;
        var audioSource = obj.AddComponent<AudioSource>(); audioSource.playOnAwake = false;
        audioSource.loop = true; audioSource.spatialBlend = 1f; audioSource.volume = 0.18f;
        audioSource.minDistance = 1f; audioSource.maxDistance = 7f; audioSource.rolloffMode = AudioRolloffMode.Linear;

        var hazard = obj.AddComponent<OndaSExposedWires>();
        var serialized = new SerializedObject(hazard);
        serialized.FindProperty("fuseBox").objectReferenceValue = fuseBox;
        serialized.FindProperty("earthquake").objectReferenceValue = quake;
        serialized.FindProperty("sparks").objectReferenceValue = sparks;
        serialized.FindProperty("sparkLight").objectReferenceValue = light;
        serialized.FindProperty("buzz").objectReferenceValue = audioSource;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: cables eléctricos del piso 5 colocados.", obj);
    }

    [MenuItem("OndaS/Crear más cables pisos 5 y 2")]
    public static void CreateMore()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity" || EditorApplication.isPlaying)
        { Debug.LogError("Abre OndaS_RetroEdificio y sal de Play antes de colocar los cables."); return; }
        var root = GameObject.Find("Structure_03/Interior/OndaS - Derrumbes pisos 8 y 7");
        var damage = root != null ? root.transform.Find("Daños visibles tras el sismo") : null;
        var original = damage != null ? damage.Find(HazardName) : null;
        string[] names = {
            "Piso 5 - cables laterales A", "Piso 5 - cables laterales B",
            "Piso 2 - cables altos", "Piso 2 - cables laterales A", "Piso 2 - cables laterales B"
        };
        if (original == null || original.GetComponent<OndaSExposedWires>() == null)
        { Debug.LogError("Falta el primer grupo de cables del piso 5."); return; }
        foreach (var name in names)
            if (damage.Find(name) != null) { Debug.LogError("Los cables adicionales ya existen: " + name); return; }

        CreateVariant(original, damage, names[0], new Vector3(11.9f, 16.1f, 3.7f), true);
        CreateVariant(original, damage, names[1], new Vector3(11.9f, 16.1f, -9.5f), true);
        CreateVariant(original, damage, names[2], new Vector3(12.5f, 4.1f, -11.4f), false);
        CreateVariant(original, damage, names[3], new Vector3(11.9f, 4.1f, -3.5f), true);
        CreateVariant(original, damage, names[4], new Vector3(13.1f, 4.1f, 5.2f), true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("OndaS: cables adicionales de los pisos 5 y 2 colocados.", root);
    }

    private static void CreateVariant(Transform original, Transform parent, string name, Vector3 position, bool side)
    {
        var obj = UnityEngine.Object.Instantiate(original.gameObject, parent);
        obj.name = name;
        obj.transform.position = position;
        var audioSource = obj.GetComponent<AudioSource>();
        if (audioSource != null) audioSource.volume = 0.12f;
        if (!side) return;

        // Keep the strands attached to the ceiling and lower their exposed ends.
        var geometry = new GameObject("Cables laterales").transform;
        geometry.SetParent(obj.transform, false);
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            var child = obj.transform.GetChild(i);
            if (child.name == "Cable aislado" || child.name == "Cobre expuesto" || child.name == "Hebra de cobre")
                child.SetParent(geometry, false);
        }
        geometry.localScale = new Vector3(0.52f, 1.75f, 1f);
        geometry.localPosition = new Vector3(0f, -2.2875f, 0f);

        var trigger = obj.GetComponent<BoxCollider>();
        trigger.center = new Vector3(0f, 1.55f, 0f);
        trigger.size = new Vector3(1.35f, 3.1f, 1.5f);
        var overhead = obj.transform.Find("Colisión alta de los cables");
        overhead.name = "Colisión lateral de los cables";
        var sideBox = overhead.GetComponent<BoxCollider>();
        sideBox.center = new Vector3(0f, 1.5f, 0f);
        sideBox.size = new Vector3(1.25f, 3f, 0.2f);
        var particles = obj.GetComponentInChildren<ParticleSystem>(true);
        particles.transform.localPosition = new Vector3(0f, 0.48f, 0f);
        var shape = particles.shape;
        shape.scale = new Vector3(1.1f, 0.08f, 0.15f);
        var light = obj.GetComponentInChildren<Light>(true);
        light.transform.localPosition = new Vector3(0f, 0.58f, 0f);
    }

    private static Material MakeMaterial(Material source, string name, Color color)
    {
        string path = AssetFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(source) { name = name };
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", null);
        if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", null);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void Rod(Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material)
    {
        var piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        piece.name = name; piece.transform.SetParent(parent, false);
        var direction = end - start;
        piece.transform.localPosition = (start + end) * 0.5f;
        piece.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
        piece.transform.localScale = new Vector3(radius * 2f, direction.magnitude * 0.5f, radius * 2f);
        piece.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.isStatic = true;
    }
}
