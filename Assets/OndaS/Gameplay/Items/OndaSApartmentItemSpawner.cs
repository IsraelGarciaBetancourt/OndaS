using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSApartmentItemSpawner : MonoBehaviour
    {
        [Serializable]
        private struct SpawnPoint
        {
            public Vector3 localPosition;
            [Min(0f)] public float maxFootprint;
        }

        [Serializable]
        private struct SpawnArea
        {
            public string room;
            [Min(0)] public int itemCount;
            public SpawnPoint[] points;
        }

        [SerializeField] private SpawnArea[] areas;
        [SerializeField] private OndaSCollectible[] emergencySupplies;
        [SerializeField] private OndaSCollectible[] packagedFood;
        [SerializeField] private OndaSCollectible[] distractingItems;
        [SerializeField, Min(0)] private int emergencySupplyCount = 4;
        [SerializeField, Min(0)] private int distractingItemCount = 3;

        private void Start()
        {
            var selectedPoints = new List<SpawnPoint>();
            foreach (var area in areas)
            {
                var roomPoints = new List<SpawnPoint>(area.points);
                Shuffle(roomPoints);
                selectedPoints.AddRange(roomPoints.GetRange(0, Mathf.Min(area.itemCount, roomPoints.Count)));
            }
            Shuffle(selectedPoints);

            var supplies = new List<OndaSCollectible>(emergencySupplies);
            var food = new List<OndaSCollectible>(packagedFood);
            var distractions = new List<OndaSCollectible>(distractingItems);
            Shuffle(supplies);
            Shuffle(food);
            Shuffle(distractions);

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                var pool = i < distractingItemCount ? distractions
                    : i < distractingItemCount + emergencySupplyCount ? supplies : food;
                SpawnItem(selectedPoints[i], pool);
            }
        }

        private void SpawnItem(SpawnPoint point, List<OndaSCollectible> pool)
        {
            int index = pool.FindIndex(prefab => FitsPoint(prefab, point.maxFootprint));
            if (index < 0)
            {
                Debug.LogWarning("OndaS: no hay un ítem disponible que quepa en este punto.", this);
                return;
            }

            var prefab = pool[index];
            pool.RemoveAt(index);
            Vector3 position = transform.TransformPoint(point.localPosition) + Vector3.up * 0.005f;
            var item = Instantiate(prefab, position, transform.rotation * prefab.transform.rotation, transform);
            item.name = prefab.name;
        }

        private static bool FitsPoint(OndaSCollectible prefab, float maxFootprint)
        {
            return prefab != null && prefab.Item != null && prefab.TryGetComponent<BoxCollider>(out var box)
                && Mathf.Max(box.size.x, box.size.z) <= maxFootprint;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int index = Random.Range(0, i + 1);
                (list[i], list[index]) = (list[index], list[i]);
            }
        }
    }
}
