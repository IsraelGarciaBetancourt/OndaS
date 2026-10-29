using System;
using UnityEngine;

namespace OndaS.Gameplay.Items
{
    public enum OndaSAddResult { Added, Full, TooHeavy, Invalid }

    public sealed class OndaSInventory : MonoBehaviour
    {
        public const int Capacity = 5;
        public const float MaxWeightKg = 5f;
        private readonly OndaSItemDefinition[] slots = new OndaSItemDefinition[Capacity];
        public event Action Changed;
        public float TotalWeightKg { get; private set; }
        public OndaSItemDefinition GetItem(int slot) => slot >= 0 && slot < Capacity ? slots[slot] : null;

        public OndaSAddResult CanAdd(OndaSItemDefinition item)
        {
            if (item == null) return OndaSAddResult.Invalid;
            bool hasSpace = false;
            for (int i = 0; i < Capacity; i++) if (slots[i] == null) { hasSpace = true; break; }
            if (!hasSpace) return OndaSAddResult.Full;
            if (TotalWeightKg + item.WeightKg > MaxWeightKg + 0.0001f) return OndaSAddResult.TooHeavy;
            return OndaSAddResult.Added;
        }

        public OndaSAddResult TryAdd(OndaSItemDefinition item)
        {
            var result = CanAdd(item);
            if (result != OndaSAddResult.Added) return result;
            for (int i = 0; i < Capacity; i++)
            {
                if (slots[i] != null) continue;
                slots[i] = item;
                TotalWeightKg += item.WeightKg;
                Changed?.Invoke();
                break;
            }
            return result;
        }

        public OndaSItemDefinition Remove(int slot)
        {
            var item = GetItem(slot);
            if (item == null) return null;
            slots[slot] = null;
            TotalWeightKg = Mathf.Max(0, TotalWeightKg - item.WeightKg);
            Changed?.Invoke();
            return item;
        }
    }
}
