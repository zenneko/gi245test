using UnityEngine;

public enum ItemType
{
    None,
    Consumable,
    Weapon,
    Shield,
    Equipment
}

[System.Serializable]
public class Item
{
    [SerializeField] private int id;
    public int ID { get { return id; } }
    [SerializeField] private string itemName;
    public string ItemName { get { return itemName; } }
    [SerializeField] private ItemType itemType;
    public ItemType Type { get { return itemType; } }
    [SerializeField] private Sprite icon;
    public Sprite Icon { get { return icon; } }
    // W9
    [SerializeField] private int prefabID;
    public int PrefabID { get { return prefabID; } }
    [SerializeField] private int normalPrice;
    public int NormalPrice { get { return normalPrice; } }

    public Item(ItemData data)
    {
        this.id = data.id;
        this.itemName = data.itemName;
        this.itemType = data.itemType;
        this.icon = data.icon;
        this.prefabID = data.prefabID;
        this.normalPrice = data.normalPrice;
    }
}
