using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public const int MAXSLOT = 17; // 16 bag slots + 1 shield/equipment slot (index 16)

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

    public Item CreateItem(int dataId)
    {
        if (dataId < 0 || dataId >= itemDataArr.Length || itemDataArr[dataId] == null) return null;
        return new Item(itemDataArr[dataId]);
    }

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
        if (item.ID >= 0 && item.ID < itemDataArr.Length && itemDataArr[item.ID] != null)
            hero.Recover(itemDataArr[item.ID].healAmount);
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

    // W12: Shop buy/sell
    public bool BuyItem(Character buyer, int dataId)
    {
        if (buyer.InventoryItems == null) return false;
        for (int i = 0; i < MAXSLOT - 1; i++)
        {
            if (buyer.InventoryItems[i] == null)
            {
                SaveItem(buyer, i, CreateItem(dataId));
                return true;
            }
        }
        return false; // inventory full
    }

    public bool SellItem(Character seller, int slotId, out int sellPrice)
    {
        sellPrice = 0;
        if (seller.InventoryItems == null) return false;
        if (slotId < 0 || slotId >= seller.InventoryItems.Length) return false;
        Item item = seller.InventoryItems[slotId];
        if (item == null) return false;
        sellPrice = Mathf.Max(1, item.NormalPrice / 2);
        RemoveItemFromSlot(seller, slotId);
        return true;
    }
}
