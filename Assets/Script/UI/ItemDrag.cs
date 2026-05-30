using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image itemImage;

    private Transform originalParent;
    private CanvasGroup canvasGroup;

    // Slot captured at drag start (before the icon leaves its slot)
    public InventorySlot OriginSlot { get; private set; }

    public Item item;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(Item i)
    {
        item = i;
        if (itemImage != null && i != null) itemImage.sprite = i.Icon;
    }

    // W8: Begin drag — lift icon to canvas root
    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginSlot = GetComponentInParent<InventorySlot>();   // capture BEFORE leaving the slot
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // If not dropped on a valid slot, return to original
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }

    // Helper: find the slot index this icon currently lives in
    public int FindIndexOfSlotParent()
    {
        InventorySlot slot = GetComponentInParent<InventorySlot>();
        if (slot != null) return slot.SlotId;
        return -1;
    }

    // W9: right-click on Consumable → open use dialog
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (item == null || item.Type != ItemType.Consumable) return;
        int slotId = FindIndexOfSlotParent();
        if (UiManager.instance != null)
        {
            UiManager.instance.SetCurItemInUse(this, slotId);
            UiManager.instance.ToggleItemDialog(true);
        }
    }
}
