using UnityEngine;

public class Npc : Character
{
    [SerializeField] private int[] questIds;

    // W12: shop
    [SerializeField] private bool isShopKeeper = false;
    public bool IsShopKeeper { get { return isShopKeeper; } }
    [SerializeField] private int[] shopItems;          // ItemData IDs that the shop starts with
    public int[] ShopItemIds { get { return shopItems; } }
    [SerializeField] private int npcGold = 1000;
    public int NpcGold { get { return npcGold; } set { npcGold = value; } }

    void Start()
    {
        // W14: each NPC self-inits so it works in any scene
        // (without relying on QuestManager.Start, which only runs once with DontDestroyOnLoad)
        CharInit(VFXManager.instance, UiManager.instance, PartyManager.instance, InventoryManager.instance);
        InitShopInventory();
    }

    void Update()
    {
        // NPC is stationary — no autonomous movement
    }

    // W12: populate this shop's inventory from the initial shopItems (call after CharInit)
    public void InitShopInventory()
    {
        if (!isShopKeeper) return;
        if (inventoryItems == null || shopItems == null) return;
        if (InventoryManager.instance == null) return;
        for (int i = 0; i < shopItems.Length && i < inventoryItems.Length; i++)
            inventoryItems[i] = InventoryManager.instance.CreateItem(shopItems[i]);
    }

    public Quest GetQuestByStatus(QuestStatus status)
    {
        if (QuestManager.instance == null) return null;
        foreach (int id in questIds)
        {
            Quest q = QuestManager.instance.GetQuestById(id);
            if (q != null && q.questStatus == status) return q;
        }
        return null;
    }

    public void TryStartDialogue(Character hero)
    {
        transform.LookAt(new Vector3(hero.transform.position.x, transform.position.y, hero.transform.position.z));

        Quest inProgress = GetQuestByStatus(QuestStatus.InProgress);
        Quest newQuest = GetQuestByStatus(QuestStatus.New);

        if (UiManager.instance == null) return;

        if (inProgress != null)
            UiManager.instance.ShowQuestDialogue(this, inProgress, hero);
        else if (newQuest != null)
            UiManager.instance.ShowQuestDialogue(this, newQuest, hero);
        else if (isShopKeeper)
            UiManager.instance.OpenShop(this);   // direct call — no more ShopManager wrapper
    }
}
