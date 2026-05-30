using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    // W9: power = heal amount (Consumable), attack boost (Weapon), defense boost (Shield)
    public int power;
    public int prefabID;
    public int normalPrice;
    // Scale to apply when this item drops on the floor (independent of the prefab's hand-held scale).
    // Leave at (1,1,1) to use the prefab's own scale.
    public Vector3 dropScale = Vector3.one;
}
