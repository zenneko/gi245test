using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInShop : MonoBehaviour
{
    [SerializeField] private Toggle toggleIcon;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ItemData itemData;
    private int dataId;

    public void Init(ItemData data, int id)
    {
        itemData = data;
        dataId = id;
        if (itemText != null) itemText.text = data.itemName;
        if (priceText != null) priceText.text = data.normalPrice + "g";
        if (toggleIcon != null) toggleIcon.isOn = false;
    }

    public ItemData GetItemData() { return itemData; }
    public int GetDataId() { return dataId; }
    public bool IsSelected() { return toggleIcon != null && toggleIcon.isOn; }
}
