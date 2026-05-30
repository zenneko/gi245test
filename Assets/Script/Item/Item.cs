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
    // W9: power = heal (Consumable) / attack bonus (Weapon) / defense bonus (Shield)
    [SerializeField] private int power;
    public int Power { get { return power; } }
    [SerializeField] private int prefabID;
    public int PrefabID { get { return prefabID; } }
    [SerializeField] private int normalPrice;
    public int NormalPrice { get { return normalPrice; } }
    // W14: separate scale for the dropped (on-floor) instance
    [SerializeField] private Vector3 dropScale = Vector3.one;
    public Vector3 DropScale { get { return dropScale; } }

    public Item(ItemData data)
    {
        this.id = data.id;
        this.itemName = data.itemName;
        this.itemType = data.itemType;
        this.icon = data.icon;
        this.power = data.power;
        this.prefabID = data.prefabID;
        this.normalPrice = data.normalPrice;
        this.dropScale = data.dropScale;
    }
}
