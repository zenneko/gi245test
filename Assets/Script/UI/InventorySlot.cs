using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int slotId;                       // bag index (used when SlotType == None)
    public int SlotId { get { return slotId; } set { slotId = value; } }

    // W9: None = bag slot; Shield/Weapon = equipment slot (only accepts that type)
    [SerializeField] private ItemType slotType = ItemType.None;
    public ItemType SlotType { get { return slotType; } set { slotType = value; } }

    public bool IsEquipment { get { return slotType != ItemType.None; } }

    public void OnDrop(PointerEventData eventData)
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];

        ItemDrag dragged = eventData.pointerDrag?.GetComponent<ItemDrag>();
        if (dragged == null || dragged.OriginSlot == null || dragged.item == null) return;

        InventorySlot fromSlot = dragged.OriginSlot;
        Item itemA = dragged.item;

        // This (target) is an equipment slot → only accept a matching item type
        if (IsEquipment && itemA.Type != slotType) return;

        Item itemB = InventoryManager.instance.GetItemAt(hero, this);

        // Swapping would push itemB back to the origin; if origin is equipment it must match
        if (fromSlot.IsEquipment && itemB != null && itemB.Type != fromSlot.SlotType) return;

        // Dropping back onto the same slot — nothing to do
        if (fromSlot == this)
        {
            dragged.transform.SetParent(transform);
            dragged.transform.localPosition = Vector3.zero;
            return;
        }

        // Move B into the origin slot (or clear it), then place A in this slot
        InventoryManager.instance.SetItemAt(hero, fromSlot, itemB);
        InventoryManager.instance.SetItemAt(hero, this, itemA);

        // Re-parent the dragged icon (RefreshInventoryUI rebuilds everything anyway)
        dragged.transform.SetParent(transform);
        dragged.transform.localPosition = Vector3.zero;

        UiManager.instance.RefreshInventoryUI();
    }
}
