using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public const int MAXSLOT = 16;     // bag slots only (equipment lives in a separate array)
    public const int EQUIP_COUNT = 2;  // equipment slots: Shield, Weapon (extend here)

    [SerializeField] private ItemData[] itemDataArr;
    public ItemData[] ItemDataArr { get { return itemDataArr; } }

    // W8: prefab objects to drop on floor
    [SerializeField] private GameObject[] itemPrefabs;
    public GameObject[] ItemPrefabs { get { return itemPrefabs; } }

    public static InventoryManager instance;

    void Awake()
    {
        instance = this;
    }

    // Maps an equippable ItemType to its index in Character.EquipmentItems. -1 = not equippable.
    public static int EquipIndexOf(ItemType type)
    {
        switch (type)
        {
            case ItemType.Shield: return 0;
            case ItemType.Weapon: return 1;
            // case ItemType.Equipment: return 2;  // helmet / armor later
            default: return -1;
        }
    }

    public Item CreateItem(int dataId)
    {
        if (dataId < 0 || dataId >= itemDataArr.Length || itemDataArr[dataId] == null) return null;
        return new Item(itemDataArr[dataId]);
    }

    // ── Container-aware access (used by drag/drop) ────────────────────────────
    // Reads/writes the right store based on whether the slot is a bag or equipment slot.
    public Item GetItemAt(Character c, InventorySlot slot)
    {
        if (c == null || slot == null) return null;
        if (slot.IsEquipment)
        {
            int ei = EquipIndexOf(slot.SlotType);
            return (ei >= 0 && c.EquipmentItems != null && ei < c.EquipmentItems.Length)
                ? c.EquipmentItems[ei] : null;
        }
        int id = slot.SlotId;
        return (c.InventoryItems != null && id >= 0 && id < c.InventoryItems.Length)
            ? c.InventoryItems[id] : null;
    }

    public void SetItemAt(Character c, InventorySlot slot, Item item)
    {
        if (c == null || slot == null) return;
        if (slot.IsEquipment)
        {
            if (item != null) c.EquipItem(item);     // EquipItem stores it + applies effects
            else c.UnequipItem(slot.SlotType);
        }
        else
        {
            int id = slot.SlotId;
            if (c.InventoryItems != null && id >= 0 && id < c.InventoryItems.Length)
                c.InventoryItems[id] = item;
        }
    }

    // ── Bag-only helpers (buy/sell/drink/quest reward) ────────────────────────
    public void SaveItem(Character character, int slotId, Item item)
    {
        if (character.InventoryItems == null) return;
        if (slotId < 0 || slotId >= character.InventoryItems.Length) return;
        character.InventoryItems[slotId] = item;
    }

    public void RemoveItemFromSlot(Character character, int slotId)
    {
        if (character.InventoryItems == null) return;
        if (slotId < 0 || slotId >= character.InventoryItems.Length) return;
        character.InventoryItems[slotId] = null;
    }

    // W9
    public void DrinkPotion(Item item, int slotId)
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];
        if (item == null || item.Type != ItemType.Consumable) return;
        hero.Recover(item.Power);
        RemoveItemFromSlot(hero, slotId);
        UiManager.instance.DeleteItemIcon(slotId);
    }

    // W8: spawn dropped item on ground
    public void SpawnDropItem(Item item, Vector3 position)
    {
        if (item == null) return;
        int pid = item.PrefabID;
        if (pid < 0 || pid >= itemPrefabs.Length || itemPrefabs[pid] == null) return;
        GameObject obj = Instantiate(itemPrefabs[pid], position, Quaternion.identity);
        // W14: apply per-item drop scale (any non-zero axis overrides the prefab's scale)
        Vector3 s = item.DropScale;
        if (s.x > 0f && s.y > 0f && s.z > 0f)
            obj.transform.localScale = s;
        ItemPick pick = obj.GetComponent<ItemPick>();
        if (pick == null) pick = obj.AddComponent<ItemPick>();
        pick.Init(item);
    }

    public void SpawnDropInventory(Item[] inventory, Vector3 position)
    {
        if (inventory == null) return;
        foreach (Item item in inventory)
        {
            if (item != null)
            {
                Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                SpawnDropItem(item, position + offset);
            }
        }
    }

}
