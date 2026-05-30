using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInShop : MonoBehaviour
{
    [SerializeField] private Toggle toggleIcon;
    [SerializeField] private Image iconImage;       // the item icon shown inside the toggle
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ItemData itemData;
    private int dataId;
    private int slotId = -1;      // bag slot when this card lives on the party side
    private bool isShop = true;   // which panel this card belongs to

    // Shop side card — full normal price, dataId used to look item up
    public void InitAsShopItem(ItemData data, int dataId)
    {
        Setup(data, dataId, slotId: -1, isShop: true, price: data.normalPrice);
    }

    // Party side card — half price (sell), slotId tracks where it came from
    public void InitAsPartyItem(ItemData data, int slotId)
    {
        Setup(data, data.id, slotId: slotId, isShop: false, price: Mathf.Max(1, data.normalPrice / 2));
    }

    private void Setup(ItemData data, int dataId, int slotId, bool isShop, int price)
    {
        this.itemData = data;
        this.dataId = dataId;
        this.slotId = slotId;
        this.isShop = isShop;
        if (itemText != null)  itemText.text = data.itemName;
        if (priceText != null) priceText.text = price + "g";
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
        if (toggleIcon != null) toggleIcon.isOn = false;
    }

    public ItemData GetItemData() { return itemData; }
    public int GetDataId() { return dataId; }
    public int GetSlotId() { return slotId; }
    public bool IsShop { get { return isShop; } }
    public bool IsSelected() { return toggleIcon != null && toggleIcon.isOn; }
}
