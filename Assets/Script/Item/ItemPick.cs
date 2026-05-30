using UnityEngine;

public class ItemPick : MonoBehaviour
{
    private Item item;

    public void Init(Item i)
    {
        item = i;
    }

    public void PickUpItem(Character hero)
    {
        if (hero == null || item == null || hero.InventoryItems == null) return;
        for (int i = 0; i < hero.InventoryItems.Length; i++)
        {
            if (hero.InventoryItems[i] == null)
            {
                InventoryManager.instance.SaveItem(hero, i, item);
                if (UiManager.instance != null) UiManager.instance.RefreshInventoryUI();
                Destroy(gameObject);
                return;
            }
        }
    }

    // Pickup is triggered by LeftClick.SelectItem (W14 §46) — no OnMouseDown to avoid
    // the unselect bug from the old input flow.
}
