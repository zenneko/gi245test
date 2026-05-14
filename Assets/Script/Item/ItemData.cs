using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    // W9
    public int prefabID;
    public int normalPrice;
    public int healAmount; // used by Consumable
}
