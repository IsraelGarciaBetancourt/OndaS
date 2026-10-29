using System;
using UnityEngine;

namespace OndaS.Gameplay.Items
{
    public sealed class OndaSInventory : MonoBehaviour
    {
        public const int Capacity = 2;
        public const int BackpackSlot = 2;
        private readonly OndaSCollectible[] slots = new OndaSCollectible[Capacity];
        public OndaSBackpack Backpack { get; private set; }
        public event Action Changed;
        public OndaSCollectible GetItem(int slot) => slot >= 0 && slot < Capacity ? slots[slot] : null;
        public bool HasFreeSlot => slots[0] == null || slots[1] == null;
        public bool TryAdd(OndaSCollectible item)
        {
            if (item == null || item.Item == null || item.transform.IsChildOf(transform)) return false;
            for (int i = 0; i < Capacity; i++)
            {
                if (slots[i] != null) continue;
                slots[i] = item;
                item.Store(transform);
                Changed?.Invoke();
                return true;
            }
            return false;
        }
        public OndaSCollectible Remove(int slot)
        {
            var item = GetItem(slot);
            if (item == null) return null;
            slots[slot] = null;
            Changed?.Invoke();
            return item;
        }
        public bool EquipBackpack(OndaSBackpack backpack)
        {
            if (Backpack != null || backpack == null || backpack.IsEquipped) return false;
            Backpack = backpack;
            backpack.Equip(transform);
            Changed?.Invoke();
            return true;
        }
        public OndaSBackpack DetachBackpack()
        {
            var backpack = Backpack;
            if (backpack == null) return null;
            Backpack = null;
            Changed?.Invoke();
            return backpack;
        }
        // Swap a selected quick slot with a specific backpack slot, or retrieve into a free quick slot.
        public bool Transfer(OndaSBackpack backpack, int bagSlot, int selected, out string feedback)
        {
            feedback = "";
            if (backpack == null || backpack.IsEquipped || bagSlot < 0 || bagSlot >= OndaSBackpack.Capacity)
            { feedback = "Mochila no disponible"; return false; }
            var stored = backpack.GetItem(bagSlot);
            int target = selected >= 0 && selected < Capacity ? selected : -1;
            var held = target >= 0 ? slots[target] : null;
            if (held == null && stored != null)
            {
                if (target < 0 || slots[target] != null)
                    target = slots[0] == null ? 0 : slots[1] == null ? 1 : -1;
                if (target < 0) { feedback = "Selecciona un objeto para intercambiar"; return false; }
            }
            if (held == null && stored == null) { feedback = "Espacio vacío"; return false; }
            if (target < 0) { feedback = "Selecciona un objeto"; return false; }
            slots[target] = stored;
            backpack.SetItem(bagSlot, held);
            if (stored != null) stored.Store(transform);
            if (held != null) held.Store(backpack.transform);
            feedback = held != null && stored != null ? "Objetos intercambiados" : held != null ? "Objeto guardado" : "Objeto retirado";
            Changed?.Invoke();
            return true;
        }
    }
}
