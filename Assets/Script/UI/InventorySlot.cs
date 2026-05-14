using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int slotId;
    public int SlotId { get { return slotId; } set { slotId = value; } }

    // W9: None = accepts any item type; set to Shield for equipment slot
    [SerializeField] private ItemType slotType = ItemType.None;
    public ItemType SlotType { get { return slotType; } set { slotType = value; } }

    public void OnDrop(PointerEventData eventData)
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];

        ItemDrag dragged = eventData.pointerDrag?.GetComponent<ItemDrag>();
        if (dragged == null) return;

        // Reject if slot type doesn't match (e.g. shield slot only accepts Shield)
        if (slotType != ItemType.None && dragged.item != null && dragged.item.Type != slotType) return;

        int fromSlotId = dragged.FindIndexOfSlotParent();
        Item itemA = dragged.item;
        Item itemB = (hero.InventoryItems != null && slotId < hero.InventoryItems.Length)
            ? hero.InventoryItems[slotId] : null;

        // Remove A from its original slot
        if (fromSlotId >= 0)
            InventoryManager.instance.RemoveItemFromSlot(hero, fromSlotId);

        // If slot B had an item, move it to slot A (swap)
        if (itemB != null && fromSlotId >= 0)
            InventoryManager.instance.SaveItem(hero, fromSlotId, itemB);

        // Place A in this slot
        InventoryManager.instance.SaveItem(hero, slotId, itemA);

        // Re-parent the dragged icon
        dragged.transform.SetParent(transform);
        dragged.transform.localPosition = Vector3.zero;

        UiManager.instance.RefreshInventoryUI();
    }
}
