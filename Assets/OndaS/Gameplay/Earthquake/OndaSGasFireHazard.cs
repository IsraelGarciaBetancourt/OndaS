using System;
using System.Collections;
using System.Collections.Generic;
using OndaS.Gameplay.Gas;
using OndaS.Visual;
using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    // Runtime-only consequence of leaving the eighth-floor gas valve open.
    [DefaultExecutionOrder(100)]
    public sealed class OndaSGasFireHazard : MonoBehaviour
    {
        private const int ApartmentSites = 18;
        private const int HallwaySites = 12;
        private OndaSGasValve valve;
        private OndaSFirstEarthquake earthquake;
        private OndaSLookDevWalkthrough movement;
        private OndaSDoorInteractor interactor;
        private Camera playerCamera;
        private Vector3 savedCameraPosition, cutscenePosition;
        private Quaternion savedCameraRotation, cutsceneRotation;
        private float savedFieldOfView;
        private bool movementWasEnabled, interactorWasEnabled, inCutscene;
        private GameObject effectsRoot, rock, cutsceneLight;
        private Material flameMaterial, smokeMaterial, rockMaterial;
        private Mesh rockMesh;
        private ParticleSystem smokeFront;
        private Vector3 frontPosition;
        private int spreadSites;
        private float nextLightUpdate;
        private readonly List<Light> fireLights = new List<Light>();
        private readonly List<Transform> floors = new List<Transform>();

        public void Begin(OndaSGasValve source, OndaSLookDevWalkthrough player, OndaSDoorInteractor playerInteractor, Camera camera)
        {
            if (source == null || player == null || camera == null) { Destroy(this); return; }
            valve = source;
            earthquake = GetComponent<OndaSFirstEarthquake>();
            movement = player;
            interactor = playerInteractor;
            playerCamera = camera;
            effectsRoot = new GameObject("OndaS - Incendio por gas");
            effectsRoot.transform.SetParent(transform, false);
            var shader = Shader.Find("OndaS/Dust");
            if (shader != null)
            {
                // Loading from Resources keeps the procedural flame shader in builds.
                var flameShader = Resources.Load<Shader>("OndaS/Flame");
                flameMaterial = new Material(flameShader != null ? flameShader : shader);
                flameMaterial.SetColor("_Color", Color.white);
                smokeMaterial = new Material(shader);
                smokeMaterial.SetColor("_Color", new Color(0.26f, 0.23f, 0.21f, 0.34f));
            }
            GatherFloors();
            StartCoroutine(ImpactSequence());
        }

        private void GatherFloors()
        {
            var interior = transform.parent;
            if (interior == null) return;
            foreach (Transform floor in interior)
                if (floor.name.Contains("Floor") && floor.name.Length > 0 && char.IsDigit(floor.name[0])) floors.Add(floor);
            floors.Sort((a, b) => b.position.y.CompareTo(a.position.y));
        }

        private IEnumerator ImpactSequence()
        {
            // Interrupt an inspection before disabling controls so its camera pose is restored.
            valve.InterruptInspection();
            movementWasEnabled = movement.enabled;
            interactorWasEnabled = interactor != null && interactor.enabled;
            if (interactor != null) interactor.enabled = false;
            movement.enabled = false;
            savedCameraPosition = playerCamera.transform.localPosition;
            savedCameraRotation = playerCamera.transform.localRotation;
            savedFieldOfView = playerCamera.fieldOfView;
            inCutscene = true;

            Bounds bounds = new Bounds(valve.transform.position, Vector3.one * 0.5f);
            foreach (var renderer in valve.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
            Vector3 target = bounds.center + Vector3.up * 0.1f;
            Vector3 direction = Vector3.ProjectOnPlane(playerCamera.transform.position - target, Vector3.up).normalized;
            if (direction.sqrMagnitude < 0.01f) direction = -valve.transform.forward;
            float distance = 2.2f;
            if (Physics.Raycast(target + Vector3.up * 0.25f, direction, out var wall, distance, ~0, QueryTriggerInteraction.Ignore))
                distance = Mathf.Clamp(wall.distance - 0.15f, 0.65f, distance);
            cutscenePosition = target + direction * distance + Vector3.up * 0.65f;
            cutsceneRotation = Quaternion.LookRotation(target - cutscenePosition, Vector3.up);
            playerCamera.fieldOfView = 55f;
            playerCamera.transform.SetPositionAndRotation(cutscenePosition, cutsceneRotation);
            cutsceneLight = new GameObject("OndaS - Luz tenue del impacto", typeof(Light));
            cutsceneLight.transform.SetParent(effectsRoot.transform, true);
            cutsceneLight.transform.position = cutscenePosition + Vector3.up * 0.4f;
            var fill = cutsceneLight.GetComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 4f;
            fill.intensity = 1.2f;
            fill.color = new Color(0.65f, 0.7f, 0.8f);
            fill.shadows = LightShadows.None;

            rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "OndaS - Fragmento de techo";
            rock.transform.SetParent(effectsRoot.transform, true);
            rock.transform.localScale = new Vector3(0.48f, 0.27f, 0.38f);
            rock.transform.rotation = Quaternion.Euler(18f, 25f, 12f);
            var filter = rock.GetComponent<MeshFilter>();
            if (filter.sharedMesh != null && filter.sharedMesh.isReadable)
            {
                rockMesh = Instantiate(filter.sharedMesh);
                var vertices = rockMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = vertices[i];
                    float roughness = Mathf.PerlinNoise(v.x * 5f + 2f, v.y * 5f + v.z * 3f + 3f);
                    vertices[i] = v * Mathf.Lerp(0.75f, 1.2f, roughness);
                }
                rockMesh.vertices = vertices;
                rockMesh.RecalculateNormals();
                rockMesh.RecalculateBounds();
                filter.sharedMesh = rockMesh;
            }
            var collider = rock.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null)
            {
                rockMaterial = new Material(lit);
                rockMaterial.color = new Color(0.31f, 0.29f, 0.25f);
                rock.GetComponent<Renderer>().sharedMaterial = rockMaterial;
            }
            Vector3 impact = new Vector3(bounds.center.x, bounds.max.y + 0.08f, bounds.center.z);
            Vector3 start = impact + Vector3.up * 1.8f;
            rock.transform.position = start;
            float elapsed = 0f;
            bool ignited = false;
            while (elapsed < 4f && inCutscene)
            {
                elapsed += Time.deltaTime;
                float fall = Mathf.Clamp01((elapsed - 0.55f) / 0.65f);
                if (rock != null) rock.transform.position = Vector3.Lerp(start, impact, fall * fall);
                if (!ignited && fall >= 1f)
                {
                    ignited = true;
                    frontPosition = new Vector3(bounds.center.x, Mathf.Max(bounds.min.y + 0.05f, transform.position.y + 0.12f), bounds.center.z);
                    CreateFire(frontPosition, 1.3f);
                    CreateFire(frontPosition + new Vector3(0.65f, 0f, 0.35f), 1f, false);
                    CreateFire(frontPosition + new Vector3(-0.55f, 0f, 0.4f), 1f, false);
                    CreateSmokeCloud(frontPosition);
                    smokeFront = CreateSmokeCloud(frontPosition, true);
                    StartCoroutine(SpreadFire());
                }
                yield return null;
            }
            if (inCutscene) CancelCutscene(true);
        }

        private void LateUpdate()
        {
            // The earthquake moves the camera's parent; keep the cinematic view fixed.
            if (inCutscene && playerCamera != null)
                playerCamera.transform.SetPositionAndRotation(cutscenePosition, cutsceneRotation);
            if (Time.time < nextLightUpdate) return;
            nextLightUpdate = Time.time + 0.12f;
            Vector3 viewer = playerCamera != null ? playerCamera.transform.position : transform.position;
            for (int i = 0; i < fireLights.Count; i++)
            {
                var fireLight = fireLights[i];
                if (fireLight == null) continue;
                fireLight.enabled = (fireLight.transform.position - viewer).sqrMagnitude < 484f;
                if (fireLight.enabled) fireLight.intensity = 2.8f + Mathf.PerlinNoise(Time.time * 4f, i * 0.73f) * 1.4f;
            }
        }

        private IEnumerator SpreadFire()
        {
            for (int floorIndex = 0; floorIndex < floors.Count; floorIndex++)
            {
                Transform floor = floors[floorIndex];
                if (floor == null) continue;
                var apartments = GetAreaSites(floor, false);
                var hallways = GetAreaSites(floor, true);
                if (floorIndex == 0)
                {
                    yield return SpreadArea(apartments, ApartmentSites);
                    yield return SpreadArea(hallways, HallwaySites);
                }
                else
                {
                    // The smoke enters the next floor from the stairwell.
                    yield return SpreadArea(hallways, HallwaySites);
                    yield return SpreadArea(apartments, ApartmentSites);
                }
                if (floorIndex + 1 < floors.Count && floors[floorIndex + 1] != null)
                    yield return SpreadDownStairs(floor, floors[floorIndex + 1]);
            }
            if (smokeFront != null) smokeFront.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private IEnumerator SpreadArea(List<Vector3> sites, int limit)
        {
            sites = SelectDistributedSites(sites, limit, frontPosition);
            int count = Mathf.Min(limit, sites.Count);
            for (int i = 0; i < count; i++)
            {
                int nearest = 0;
                float best = float.PositiveInfinity;
                for (int j = 0; j < sites.Count; j++)
                {
                    float distance = (sites[j] - frontPosition).sqrMagnitude;
                    if (distance < best) { best = distance; nearest = j; }
                }
                Vector3 destination = sites[nearest];
                sites.RemoveAt(nearest);
                Vector3 start = frontPosition;
                float distanceToTarget = Vector3.Distance(start, destination);
                float traveled = 0f;
                while (traveled < distanceToTarget)
                {
                    float speed = earthquake != null ? earthquake.VelocidadIncendio : 0.7f;
                    traveled += speed * Time.deltaTime;
                    if (smokeFront != null) smokeFront.transform.position = Vector3.Lerp(start, destination, traveled / distanceToTarget) + Vector3.up;
                    yield return null;
                }
                frontPosition = destination;
                CreateSmokeCloud(destination);
                CreateFire(destination, 1.1f, ++spreadSites % 2 == 0);
            }
        }

        private static List<Vector3> SelectDistributedSites(List<Vector3> candidates, int limit, Vector3 start)
        {
            if (candidates.Count <= limit) return candidates;
            var remaining = new List<Vector3>(candidates);
            var selected = new List<Vector3>();
            while (selected.Count < limit && remaining.Count > 0)
            {
                int bestIndex = 0;
                float bestDistance = selected.Count == 0 ? float.PositiveInfinity : -1f;
                for (int i = 0; i < remaining.Count; i++)
                {
                    float distance = (remaining[i] - start).sqrMagnitude;
                    if (selected.Count > 0)
                        foreach (var chosen in selected) distance = Mathf.Min(distance, (remaining[i] - chosen).sqrMagnitude);
                    if ((selected.Count == 0 && distance < bestDistance) || (selected.Count > 0 && distance > bestDistance))
                    { bestDistance = distance; bestIndex = i; }
                }
                selected.Add(remaining[bestIndex]);
                remaining.RemoveAt(bestIndex);
            }
            return selected;
        }

        private IEnumerator SpreadDownStairs(Transform upper, Transform lower)
        {
            var stairCenters = new List<Vector3>();
            foreach (Transform stairs in upper.parent)
            {
                if (!stairs.name.StartsWith("Staricases", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var renderer in stairs.GetComponentsInChildren<Renderer>())
                {
                    Bounds b = renderer.bounds;
                    if (renderer.name.IndexOf("Staircase", StringComparison.OrdinalIgnoreCase) < 0 || b.size.y < 2.5f || b.size.y > 5.5f) continue;
                    if (Mathf.Abs(b.center.y - (upper.position.y + lower.position.y) * 0.5f) > 0.8f) continue;
                    Vector3 center = b.center;
                    bool duplicate = false;
                    foreach (var previous in stairCenters) if (Vector3.Distance(previous, center) < 2f) { duplicate = true; break; }
                    if (!duplicate) stairCenters.Add(center);
                }
            }
            if (stairCenters.Count == 0)
            {
                for (int i = 0; i < 4; i++) yield return WaitForStairStep();
                yield break;
            }
            stairCenters.Sort((a, b) => (a - frontPosition).sqrMagnitude.CompareTo((b - frontPosition).sqrMagnitude));
            // Both stairwells fill together, descending from the upper landing.
            for (int step = 0; step < 4; step++)
            {
                float height = Mathf.Lerp(upper.position.y + 0.1f, lower.position.y + 0.1f, (step + 1f) / 4f);
                foreach (var center in stairCenters)
                {
                    Vector3 point = new Vector3(center.x, height, center.z);
                    CreateSmokeCloud(point);
                    CreateFire(point, 0.9f, step % 2 == 0);
                }
                var primary = stairCenters[0];
                frontPosition = new Vector3(primary.x, height, primary.z);
                if (smokeFront != null) smokeFront.transform.position = frontPosition + Vector3.up;
                yield return WaitForStairStep();
            }
        }

        private IEnumerator WaitForStairStep()
        {
            float elapsed = 0f;
            while (elapsed < (earthquake != null ? earthquake.SegundosPorTramoEscalera : 4f))
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private List<Vector3> GetAreaSites(Transform floor, bool hallway)
        {
            var candidates = new List<Vector3>();
            foreach (Transform room in floor)
            {
                bool isHallway = room.name.IndexOf("Hallway", StringComparison.OrdinalIgnoreCase) >= 0;
                if (hallway != isHallway) continue;
                Transform tiles = room.Find("Floors");
                if (tiles == null) continue;
                foreach (var renderer in tiles.GetComponentsInChildren<Renderer>())
                {
                    Bounds b = renderer.bounds;
                    if (b.size.x < 0.35f || b.size.z < 0.35f || Mathf.Abs(b.max.y - floor.position.y) > 1.2f) continue;
                    Vector3 p = new Vector3(b.center.x, b.max.y + 0.03f, b.center.z);
                    bool near = false;
                    foreach (var existing in candidates) if ((existing - p).sqrMagnitude < 5.5f) { near = true; break; }
                    if (!near) candidates.Add(p);
                }
            }
            return candidates;
        }

        private ParticleSystem CreateSmokeCloud(Vector3 position, bool moving = false)
        {
            if (smokeMaterial == null) return null;
            var go = new GameObject(moving ? "OndaS - Frente de humo" : "OndaS - Niebla de incendio", typeof(ParticleSystem));
            go.transform.SetParent(effectsRoot.transform, true);
            go.transform.position = position + Vector3.up;
            var smoke = go.GetComponent<ParticleSystem>();
            var main = smoke.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.3f, 3.4f);
            main.maxParticles = moving ? 24 : 30;
            var emission = smoke.emission;
            emission.rateOverTime = moving ? 3f : 3.5f;
            var shape = smoke.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.8f, 0.8f, 1.8f);
            var colors = smoke.colorOverLifetime;
            colors.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.48f, 0.2f), new GradientAlphaKey(0.36f, 0.7f), new GradientAlphaKey(0f, 1f) });
            colors.color = gradient;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = smokeMaterial;
            smoke.Play();
            return smoke;
        }

        private void CreateFire(Vector3 position, float size, bool addLight = true)
        {
            var root = new GameObject("OndaS - Foco de incendio");
            root.transform.SetParent(effectsRoot.transform, true);
            root.transform.position = position;
            if (flameMaterial != null)
            {
                var fire = MakeParticles(root.transform, "Llamas", flameMaterial, size, false);
                var colors = fire.colorOverLifetime;
                colors.enabled = true;
                var gradient = new Gradient();
                gradient.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0f), new GradientColorKey(new Color(1f, 0.2f, 0.015f), 0.55f), new GradientColorKey(new Color(0.28f, 0.035f, 0.01f), 1f) },
                    new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.75f, 0.55f), new GradientAlphaKey(0f, 1f) });
                colors.color = gradient;
                MakeParticles(root.transform, "Humo", smokeMaterial, size, true);
            }
            if (!addLight) return;
            var lightObject = new GameObject("Resplandor", typeof(Light));
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = Vector3.up * 0.65f;
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.31f, 0.08f);
            light.range = 7f * size;
            light.intensity = 3.5f;
            light.shadows = LightShadows.None;
            fireLights.Add(light);
        }

        private static ParticleSystem MakeParticles(Transform parent, string name, Material material, float size, bool smoke)
        {
            var go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            var particles = go.GetComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = smoke ? new ParticleSystem.MinMaxCurve(1.2f, 2.0f) : new ParticleSystem.MinMaxCurve(0.8f, 1.35f);
            main.startSpeed = smoke ? new ParticleSystem.MinMaxCurve(0.5f, 1.0f) : new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            main.startSize = smoke ? new ParticleSystem.MinMaxCurve(0.45f * size, 0.8f * size) : new ParticleSystem.MinMaxCurve(0.65f * size, 1.25f * size);
            main.maxParticles = smoke ? 30 : 55;
            var emission = particles.emission;
            emission.rateOverTime = smoke ? 7f : 30f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = smoke ? 14f : 8f;
            shape.radius = (smoke ? 0.22f : 0.55f) * size;
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            particles.Play();
            return particles;
        }

        public void CancelCutscene(bool restoreControls)
        {
            if (!inCutscene) return;
            inCutscene = false;
            if (cutsceneLight != null) { Destroy(cutsceneLight); cutsceneLight = null; }
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = savedCameraPosition;
                playerCamera.transform.localRotation = savedCameraRotation;
                playerCamera.fieldOfView = savedFieldOfView;
            }
            if (restoreControls)
            {
                if (movement != null) movement.enabled = movementWasEnabled;
                if (interactor != null) interactor.enabled = interactorWasEnabled;
            }
        }

        private void OnDestroy()
        {
            CancelCutscene(true);
            if (effectsRoot != null) Destroy(effectsRoot);
            if (flameMaterial != null) Destroy(flameMaterial);
            if (smokeMaterial != null) Destroy(smokeMaterial);
            if (rockMaterial != null) Destroy(rockMaterial);
            if (rockMesh != null) Destroy(rockMesh);
        }
    }
}
