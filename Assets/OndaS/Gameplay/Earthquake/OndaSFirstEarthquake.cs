using System.Collections;
using System.Collections.Generic;
using OndaS.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OndaS.Gameplay.Earthquake
{
    // This first event deliberately targets only the eighth-floor apartment and its windows.
    public sealed class OndaSFirstEarthquake : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float preparationSeconds = 60f;
        [SerializeField, Min(1f)] private float duration = 35f;
        [SerializeField] private bool allowTestKey = true;
        [SerializeField, Range(0f, 2f)] private float cameraStrength = 1.3f;
        [SerializeField] private float roomShakeDegrees = 4f;
        [SerializeField] private int brokenWindowPanels = 2;

        private sealed class Prop
        {
            public Transform transform;
            public Quaternion rest;
            public Vector3 restPosition;
            public float releaseAt;
            public float impulse;
            public float swayMultiplier;
            public float slide;
            public bool released;
            public Rigidbody body;
        }

        private readonly List<Prop> props = new List<Prop>();
        private readonly List<Transform> glassPanels = new List<Transform>();
        private readonly List<Light> lights = new List<Light>();
        private readonly List<float> lightIntensities = new List<float>();
        private readonly List<Mesh> generatedMeshes = new List<Mesh>();
        private Transform player, cameraPivot, cameraTransform, cameraOriginalParent;
        private Vector3 cameraPivotRest;
        private Quaternion cameraOriginalRotation;
        private Vector3 cameraOriginalPosition, cameraOriginalScale;
        private ParticleSystem dustTemplate;
        private AudioSource rumble;
        private AudioClip rumbleClip, impactClip, glassClip;
        private GameObject countdownCanvas;
        private TextMeshProUGUI countdownText, countdownLabel;
        private int displayedSeconds = -1;
        private float elapsed, shakeElapsed, intensity;
        private bool started, finished;
        private int brokenCount;

        private void Awake()
        {
            if (gameObject.scene.path != "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity")
            {
                enabled = false;
                return;
            }
            var playerObject = GameObject.Find("First Person Player");
            if (playerObject == null) { Debug.LogWarning("Sismo: no se encontró al jugador.", this); enabled = false; return; }
            player = playerObject.transform;
            var camera = playerObject.GetComponentInChildren<Camera>(true);
            if (camera == null) { Debug.LogWarning("Sismo: no se encontró la cámara del jugador.", this); enabled = false; return; }
            SetupCamera(camera.transform);
            GatherEighthFloor();
            SetupAudio();
            CreateCountdown();
        }

        private void SetupCamera(Transform view)
        {
            cameraTransform = view;
            cameraOriginalParent = view.parent;
            cameraOriginalPosition = view.localPosition;
            cameraOriginalRotation = view.localRotation;
            cameraOriginalScale = view.localScale;
            var pivotObject = new GameObject("OndaS - Pivote de sismo");
            cameraPivot = pivotObject.transform;
            cameraPivot.SetParent(cameraOriginalParent, false);
            cameraPivot.localPosition = cameraOriginalPosition;
            cameraPivot.localRotation = Quaternion.identity;
            cameraPivotRest = cameraPivot.localPosition;
            view.SetParent(cameraPivot, false);
            view.localPosition = Vector3.zero;
            view.localRotation = cameraOriginalRotation;
            view.localScale = cameraOriginalScale;
        }

        private void GatherEighthFloor()
        {
            var apartment = transform.Find("Apartment_01/Furniture and Props");
            if (apartment == null) { Debug.LogWarning("Sismo: no se encontró el departamento del piso 8.", this); return; }
            AddProp(apartment, "Living Room/Props/Canvas_Painting_01", 7.5f, 0.65f);
            AddProp(apartment, "Living Room/Props/Canvas_Painting_02", 15f, 0.5f);
            AddProp(apartment, "Living Room/Props/Vase_apt_01", 10f, 0.75f);
            AddProp(apartment, "Living Room/Props/Vase_Apt_01", 13f, 0.7f);
            AddProp(apartment, "Living Room/Props/Lamp_Apt_01", 11f, 1.1f, 4f);
            AddProp(apartment, "Living Room/Props/Lamp_Apt_03", 16f, 0.8f, 3f);
            AddProp(apartment, "Living Room/Props/Lamp_Apt_03 (1)", 23f, 0.8f, 3f);
            AddProp(apartment, "Living Room/Props/TV_Apt_01", 18f, 0.8f, 1.5f);
            AddProp(apartment, "Living Room/Props/Wall_Mirror_01", 17f, 0.6f, 1.6f);
            AddProp(apartment, "Living Room/Props/Monitor_Apt_01", 14f, 0.8f, 1.4f);
            AddProp(apartment, "Living Room/Props/Sculpture_apt_02_01", 8f, 0.65f, 1.4f);
            AddProp(apartment, "Living Room/Props/Sculpture_apt_02_02", 13f, 0.65f, 1.4f);
            AddProp(apartment, "Living Room/Furniture/Lamp_Floor_01", 13f, 0.8f);
            AddProp(apartment, "Living Room/Furniture/Lamp_Floor_01 (1)", 19f, 0.8f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01", 12f, 0.55f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01 (1)", 21f, 0.4f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01 (2)", 8f, 0.8f);
            AddProp(apartment, "Living Room/Furniture/Chair_Apt_01 (3)", 16f, 0.7f);
            AddProp(apartment, "Living Room/Furniture/Table_Apt_01", 14f, 0.65f, 0.65f, 0.045f);
            AddProp(apartment, "Living Room/Furniture/Coffee_Table_Apt_01", 18f, 0.7f, 0.7f, 0.045f);
            AddProp(apartment, "Living Room/Furniture/Table_Side_Apt_01", 12f, 0.7f, 0.8f, 0.04f);
            AddProp(apartment, "Living Room/Furniture/Table_Side_Apt_01 (1)", 20f, 0.65f, 0.8f, 0.04f);
            AddProp(apartment, "Living Room/Furniture/Shelf_Apt_01", 23f, 0.9f, 0.8f, 0.035f);
            AddProp(apartment, "Living Room/Furniture/WallShelf_Apt_01", 19f, 0.7f, 1.3f);
            AddProp(apartment, "Living Room/Furniture/Sofa_Apt_01", float.PositiveInfinity, 0f, 0.45f, 0.04f);
            AddProp(apartment, "Living Room/Furniture/BarCabinet_Apt_01_Filled", float.PositiveInfinity, 0f, 0.45f, 0.03f);
            AddProp(apartment, "Kitchen/Props/DrinkingGlass_01", 8.5f, 0.5f);
            AddProp(apartment, "Kitchen/Props/DrinkingGlass_02", 16f, 0.5f);
            AddProp(apartment, "Kitchen/Props/Toaster_Apt", 19f, 0.55f);
            AddProp(apartment, "Kitchen/Props/Blender_Apt_01", 15f, 0.7f);
            AddProp(apartment, "Kitchen/Props/Bowl_Apt_01_Stacked", 10f, 0.6f);
            AddProp(apartment, "Kitchen/Appliances/Fridge_01", float.PositiveInfinity, 0f, 0.35f, 0.025f);
            AddProp(apartment, "Kitchen/Cabinets/Cabinet_Wall_DD_02", float.PositiveInfinity, 0f, 0.4f);
            AddProp(apartment, "Bed Room_01/Props/Canvas_Painting_03", 18f, 0.55f);
            AddProp(apartment, "Bed Room_01/Props/Table_Lamp_Apt_01", 23f, 0.5f);
            AddProp(apartment, "Bed Room_01/Props/Table_Lamp_Apt_01 (1)", 12f, 0.65f);
            AddProp(apartment, "Bed Room_01/Furniture/Table_End_01", 17f, 0.7f, 0.7f, 0.04f);
            AddProp(apartment, "Bed Room_02/Props/Wall_Mirror_02", 21f, 0.65f);
            AddProp(apartment, "Bed Room_02/Furniture/Table_End_02", 20f, 0.7f, 0.7f, 0.04f);
            AddProp(apartment, "Bathroom/Props/ToothbrushCup_Filled", 14f, 0.6f);
            var bottles = apartment.Find("Living Room/Props/Wall Wine Rack_Apt_01_Filled/WineBottles");
            if (bottles != null)
                for (int i = 0; i < Mathf.Min(3, bottles.childCount); i++)
                    AddProp(bottles.GetChild(i), 9f + i * 3f, 0.55f);

            var lampGroup = transform.Find("Apartment_01/Lighting");
            if (lampGroup != null)
                foreach (var light in lampGroup.GetComponentsInChildren<Light>(true))
                { lights.Add(light); lightIntensities.Add(light.intensity); }
            dustTemplate = GetComponentInChildren<ParticleSystem>(true);

            var windows = GameObject.Find("Structure_03/Exterior/Walls/8th Floor/Windows");
            if (windows != null)
            {
                foreach (var renderer in windows.GetComponentsInChildren<MeshRenderer>(true))
                    if (renderer.name == "Glass.006" || renderer.name == "Glass.007") glassPanels.Add(renderer.transform);
                glassPanels.Sort((a, b) => (a.position - player.position).sqrMagnitude.CompareTo((b.position - player.position).sqrMagnitude));
            }
        }

        private void AddProp(Transform root, string path, float releaseAt, float impulse, float swayMultiplier = 1f, float slide = 0f)
        {
            var target = root.Find(path);
            if (target == null) { Debug.LogWarning("Sismo: objeto no encontrado: " + path, this); return; }
            AddProp(target, releaseAt, impulse, swayMultiplier, slide);
        }

        private void AddProp(Transform target, float releaseAt, float impulse, float swayMultiplier = 1f, float slide = 0f)
        {
            if (target.GetComponentInParent<Items.OndaSCollectible>() != null || target.GetComponentInParent<Items.OndaSBackpack>() != null) return;
            props.Add(new Prop { transform = target, rest = target.localRotation, restPosition = target.localPosition, releaseAt = releaseAt, impulse = impulse, swayMultiplier = swayMultiplier, slide = slide });
        }

        private void Update()
        {
            if (finished) return;
            if (!started)
            {
                elapsed += Time.deltaTime;
                UpdateCountdown();
                if (elapsed >= preparationSeconds || (allowTestKey && Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)) Begin();
                return;
            }
            shakeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(shakeElapsed / duration);
            float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.13f)) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / 0.2f));
            float pulses = 0.55f + 0.45f * Mathf.PerlinNoise(shakeElapsed * 0.32f, 1.7f);
            intensity = envelope * pulses;
            if (rumble != null) rumble.volume = PlayerOnFloor() ? intensity * 0.55f : 0f;
            AnimateProps();
            AnimateLights();
            while (brokenCount < Mathf.Min(brokenWindowPanels, glassPanels.Count) && shakeElapsed >= 11f + brokenCount * 6f)
            {
                BreakGlass(glassPanels[brokenCount]);
                brokenCount++;
            }
            if (shakeElapsed >= duration) Finish();
        }

        private void Begin()
        {
            if (started) return;
            started = true;
            shakeElapsed = 0f;
            if (countdownText != null) countdownText.text = "0:00";
            if (countdownLabel != null) countdownLabel.text = "SISMO";
            if (rumble != null) rumble.Play();
            Debug.Log("OndaS: comenzó el primer sismo en el piso 8.", this);
        }

        private void LateUpdate()
        {
            if (cameraPivot == null) return;
            float amount = started && !finished && PlayerOnFloor() ? intensity * cameraStrength : 0f;
            float x = (Mathf.PerlinNoise(shakeElapsed * 4.6f, 0.1f) - 0.5f) * amount;
            float y = (Mathf.PerlinNoise(0.7f, shakeElapsed * 5.2f) - 0.5f) * amount;
            float z = (Mathf.PerlinNoise(shakeElapsed * 3.4f, 9f) - 0.5f) * amount;
            cameraPivot.localPosition = cameraPivotRest + new Vector3(x * 0.12f, y * 0.085f, z * 0.025f);
            cameraPivot.localRotation = Quaternion.Euler(y * 4.5f, x * 3.2f, z * 7f);
        }

        private bool PlayerOnFloor()
        {
            return player != null && player.position.y >= transform.position.y - 0.5f && player.position.y < transform.position.y + 4.2f;
        }

        private void FixedUpdate()
        {
            if (!started || finished) return;
            Vector3 groundAcceleration = new Vector3(Mathf.Sin(shakeElapsed * 5.5f), 0f, Mathf.Sin(shakeElapsed * 4.3f + 1.2f)) * (intensity * 18f);
            foreach (var prop in props)
            {
                if (prop.body == null || prop.body.isKinematic) continue;
                prop.body.AddForce(groundAcceleration, ForceMode.Acceleration);
                prop.body.AddTorque(new Vector3(groundAcceleration.z, 0f, -groundAcceleration.x) * 0.18f, ForceMode.Acceleration);
            }
        }

        private void AnimateProps()
        {
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                if (prop.transform == null || prop.released) continue;
                if (shakeElapsed >= prop.releaseAt)
                {
                    prop.transform.localRotation = prop.rest;
                    prop.transform.localPosition = prop.restPosition;
                    Release(prop, i);
                    continue;
                }
                float wobble = Mathf.Sin(shakeElapsed * (6f + i * 0.08f) + i) * roomShakeDegrees * prop.swayMultiplier * intensity;
                float counterWobble = Mathf.Sin(shakeElapsed * 4.7f + i * 0.7f) * roomShakeDegrees * prop.swayMultiplier * intensity;
                prop.transform.localRotation = prop.rest * Quaternion.Euler(counterWobble * 0.5f, 0f, wobble);
                prop.transform.localPosition = prop.restPosition + new Vector3(Mathf.Sin(shakeElapsed * 7f + i), 0f, Mathf.Sin(shakeElapsed * 5.8f + i * 2f)) * prop.slide * intensity;
            }
        }

        private void Release(Prop prop, int index)
        {
            prop.released = true;
            var go = prop.transform.gameObject;
            PrepareDynamicColliders(go);
            var body = go.GetComponent<Rigidbody>();
            if (body == null) body = go.AddComponent<Rigidbody>();
            prop.body = body;
            body.mass = go.name.Contains("Table") || go.name.Contains("Shelf") ? 8f : go.name.Contains("Chair") || go.name.Contains("Toaster") ? 3f : 1f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.isKinematic = false;
            body.useGravity = true;
            Vector3 direction = new Vector3(Mathf.Sin(index * 2.1f), 0.15f, Mathf.Cos(index * 1.7f)).normalized;
            body.AddForce(direction * prop.impulse * 1.7f, ForceMode.VelocityChange);
            body.AddTorque(new Vector3(direction.z, 0.25f, -direction.x) * prop.impulse * 1.7f, ForceMode.VelocityChange);
            StartCoroutine(KeepOnEighthFloor(body));
            StartCoroutine(PlayImpactAfterDelay(prop.transform, 0.45f));
            if (index == 0 || index == 4 || index == 8) SpawnDust(prop.transform.position);
        }

        private IEnumerator KeepOnEighthFloor(Rigidbody body)
        {
            while (body != null)
            {
                if (body.worldCenterOfMass.y < transform.position.y)
                {
                    Destroy(body.gameObject);
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private static void PrepareDynamicColliders(GameObject go)
        {
            bool needsApproximation = true;
            bool hadNonConvexMesh = false;
            foreach (var collider in go.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.enabled || collider.isTrigger) continue;
                if (collider is MeshCollider mesh && !mesh.convex)
                {
                    mesh.enabled = false;
                    hadNonConvexMesh = true;
                    continue;
                }
                needsApproximation = false;
            }
            if (needsApproximation || hadNonConvexMesh) AddApproximateCollider(go);
        }

        private static void AddApproximateCollider(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            var collider = go.AddComponent<BoxCollider>();
            var localBounds = new Bounds(go.transform.InverseTransformPoint(bounds.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                localBounds.Encapsulate(go.transform.InverseTransformPoint(corner));
            }
            collider.center = localBounds.center;
            collider.size = localBounds.size;
        }

        private void AnimateLights()
        {
            for (int i = 0; i < lights.Count; i++)
                if (lights[i] != null) lights[i].intensity = lightIntensities[i] * (1f - intensity * 0.3f * Mathf.PerlinNoise(shakeElapsed * 12f, i + 1f));
        }

        private void BreakGlass(Transform glass)
        {
            if (glass == null) return;
            var filter = glass.GetComponent<MeshFilter>();
            var renderer = glass.GetComponent<MeshRenderer>();
            if (filter == null || filter.sharedMesh == null || renderer == null) return;
            Bounds bounds = filter.sharedMesh.bounds;
            Vector3 size = bounds.size;
            int thin = size.x <= size.y && size.x <= size.z ? 0 : size.y <= size.z ? 1 : 2;
            int axisA = (thin + 1) % 3, axisB = (thin + 2) % 3;
            renderer.enabled = false;
            var pieceColliders = new List<Collider>();
            var windowColliders = glass.parent.GetComponentsInParent<Collider>();
            const int columns = 3, rows = 2;
            for (int row = 0; row < rows; row++) for (int col = 0; col < columns; col++) for (int side = 0; side < 2; side++)
            {
                float a0 = bounds.min[axisA] + size[axisA] * col / columns;
                float a1 = bounds.min[axisA] + size[axisA] * (col + 1) / columns;
                float b0 = bounds.min[axisB] + size[axisB] * row / rows;
                float b1 = bounds.min[axisB] + size[axisB] * (row + 1) / rows;
                Vector3 p0 = bounds.center, p1 = bounds.center, p2 = bounds.center;
                p0[axisA] = a0; p0[axisB] = b0;
                p1[axisA] = side == 0 ? a1 : a0; p1[axisB] = side == 0 ? b0 : b1;
                p2[axisA] = a1; p2[axisB] = b1;
                var piece = new GameObject("OndaS - fragmento de vidrio");
                piece.transform.SetPositionAndRotation(glass.position, glass.rotation);
                piece.transform.localScale = glass.lossyScale;
                piece.transform.SetParent(transform, true);
                var mesh = new Mesh { name = "OndaS - vidrio fragmentado" };
                mesh.vertices = new[] { p0, p1, p2 };
                mesh.triangles = new[] { 0, 1, 2, 2, 1, 0 };
                mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one };
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); generatedMeshes.Add(mesh);
                piece.AddComponent<MeshFilter>().sharedMesh = mesh;
                piece.AddComponent<MeshRenderer>().sharedMaterial = renderer.sharedMaterial;
                var collider = piece.AddComponent<BoxCollider>();
                Vector3 colliderSize = mesh.bounds.size; colliderSize[thin] = Mathf.Max(0.02f, size[thin]);
                collider.center = mesh.bounds.center; collider.size = colliderSize;
                foreach (var other in pieceColliders) Physics.IgnoreCollision(collider, other);
                foreach (var other in windowColliders) if (other != null) Physics.IgnoreCollision(collider, other);
                pieceColliders.Add(collider);
                var body = piece.AddComponent<Rigidbody>();
                body.mass = 0.04f;
                body.AddForce((glass.forward * (side == 0 ? 0.3f : -0.3f) + Vector3.down * 0.2f + glass.right * (col - 1) * 0.2f), ForceMode.VelocityChange);
                body.AddTorque(new Vector3(row + 1, col - 1, side == 0 ? 1 : -1) * 0.8f, ForceMode.VelocityChange);
                StartCoroutine(SettleFragment(body));
            }
            if (glassClip != null && PlayerOnFloor()) AudioSource.PlayClipAtPoint(glassClip, glass.position, 0.55f);
            SpawnDust(glass.position);
        }

        private IEnumerator SettleFragment(Rigidbody body)
        {
            float end = Time.time + 6f;
            while (body != null && Time.time < end)
            {
                if (body.worldCenterOfMass.y < transform.position.y)
                {
                    Destroy(body.gameObject);
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
            if (body != null) body.isKinematic = true;
        }

        private void SpawnDust(Vector3 position)
        {
            if (dustTemplate == null) return;
            var dust = Instantiate(dustTemplate, new Vector3(position.x, transform.position.y + 3f, position.z), Quaternion.identity, transform);
            dust.name = "OndaS - polvo de impacto";
            dust.Play();
            Destroy(dust.gameObject, 4f);
        }

        private IEnumerator PlayImpactAfterDelay(Transform target, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (target != null && impactClip != null && PlayerOnFloor()) AudioSource.PlayClipAtPoint(impactClip, target.position, 0.35f);
        }

        private void SetupAudio()
        {
            rumbleClip = CreateNoiseClip("OndaS - vibración", 4f, 0.985f, false);
            impactClip = CreateNoiseClip("OndaS - golpe", 0.35f, 0.88f, true);
            glassClip = CreateNoiseClip("OndaS - cristales", 0.65f, 0.45f, true);
            rumble = gameObject.AddComponent<AudioSource>();
            rumble.clip = rumbleClip; rumble.loop = true; rumble.playOnAwake = false; rumble.spatialBlend = 0f; rumble.volume = 0f;
        }

        private void CreateCountdown()
        {
            countdownCanvas = new GameObject("OndaS - Temporizador", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = countdownCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = countdownCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            countdownLabel = CreateCountdownText("Estado", new Vector2(50f, -35f), new Vector2(280f, 38f), 26f);
            countdownText = CreateCountdownText("Tiempo restante", new Vector2(50f, -73f), new Vector2(280f, 64f), 58f);
            countdownLabel.text = "PREPARACIÓN";
            UpdateCountdown();
        }

        private TextMeshProUGUI CreateCountdownText(string objectName, Vector2 position, Vector2 size, float fontSize)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(countdownCanvas.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<TextMeshProUGUI>();
            OndaSTypography.Apply(text);
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            return text;
        }

        private void UpdateCountdown()
        {
            if (countdownText == null) return;
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, preparationSeconds - elapsed));
            if (seconds == displayedSeconds) return;
            displayedSeconds = seconds;
            countdownText.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
            countdownText.color = seconds <= 10 ? new Color32(255, 120, 65, 255) : new Color32(255, 218, 65, 255);
        }

        private static AudioClip CreateNoiseClip(string clipName, float seconds, float smoothing, bool decay)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(seconds * rate);
            var samples = new float[count];
            var random = new System.Random(clipName.GetHashCode());
            float value = 0f;
            for (int i = 0; i < count; i++)
            {
                value = value * smoothing + ((float)random.NextDouble() * 2f - 1f) * (1f - smoothing);
                float fade = decay ? Mathf.Exp(-6f * i / count) : Mathf.Min(1f, Mathf.Min(i, count - 1 - i) / 512f);
                samples[i] = value * fade * (decay ? 1.5f : 3f);
            }
            var clip = AudioClip.Create(clipName, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Finish()
        {
            finished = true; intensity = 0f;
            if (countdownCanvas != null) countdownCanvas.SetActive(false);
            if (rumble != null) rumble.Stop();
            for (int i = 0; i < lights.Count; i++) if (lights[i] != null) lights[i].intensity = lightIntensities[i];
            foreach (var prop in props) if (!prop.released && prop.transform != null)
            { prop.transform.localRotation = prop.rest; prop.transform.localPosition = prop.restPosition; }
            Debug.Log("OndaS: terminó el primer sismo.", this);
        }

        private void OnDestroy()
        {
            if (cameraTransform != null && cameraOriginalParent != null)
            {
                cameraTransform.SetParent(cameraOriginalParent, false);
                cameraTransform.localPosition = cameraOriginalPosition;
                cameraTransform.localRotation = cameraOriginalRotation;
                cameraTransform.localScale = cameraOriginalScale;
            }
            if (cameraPivot != null) Destroy(cameraPivot.gameObject);
            if (countdownCanvas != null) Destroy(countdownCanvas);
            foreach (var mesh in generatedMeshes) if (mesh != null) Destroy(mesh);
            if (rumbleClip != null) Destroy(rumbleClip);
            if (impactClip != null) Destroy(impactClip);
            if (glassClip != null) Destroy(glassClip);
        }
    }
}
