using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    // ── Selection ──────────────────────────────────────────────────────────────
    [SerializeField] private RectTransform selectionBox;
    public RectTransform SelectionBox { get { return selectionBox; } }

    // ── Pause ──────────────────────────────────────────────────────────────────
    [SerializeField] private Toggle togglePauseUnpause;

    // ── Magic Toggles (W7: now show icons) ────────────────────────────────────
    [SerializeField] private Toggle[] toggleMagic;
    public Toggle[] ToggleMagic { get { return toggleMagic; } }
    [SerializeField] private int curToggleMagicID = -1;

    // ── Inventory (W8) ────────────────────────────────────────────────────────
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform[] slots;           // all InventorySlot transforms
    [SerializeField] private GameObject itemUIPrefab;     // ItemUI prefab (has ItemDrag)

    // ── Item Dialog (W9) ──────────────────────────────────────────────────────
    [SerializeField] private GameObject grayImage;
    [SerializeField] private GameObject itemDialog;
    private ItemDrag curItemDrag;
    private int curSlotId = -1;

    // ── Party UI / Char Panel (W11) ───────────────────────────────────────────
    [SerializeField] private Toggle[] toggleAvatar;       // 6 avatar toggles top-right
    public Toggle[] ToggleAvatar { get { return toggleAvatar; } }
    [SerializeField] private GameObject charPanel;
    [SerializeField] private TextMeshProUGUI charNameText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private Image charImage;

    // ── Quest Dialogue (W10) ──────────────────────────────────────────────────
    [SerializeField] private GameObject questDialogPanel;
    [SerializeField] private TextMeshProUGUI questDialogText;
    [SerializeField] private Button questAcceptBtn;
    [SerializeField] private Button questRejectBtn;
    [SerializeField] private Button questNextBtn;
    private Quest activeQuest;
    private Character activeQuestHero;
    private int questDialogStep = 0;

    // ── Shop (W12) ────────────────────────────────────────────────────────────
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopScrollContent;   // left scroll content
    [SerializeField] private Transform playerScrollContent; // right scroll content (player inventory)
    [SerializeField] private GameObject itemInShopPrefab;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI moneyText;
    private Npc curShopNpc;
    private List<ItemInShop> shopCards = new List<ItemInShop>();

    public static UiManager instance;

    // ──────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // W11: map toggleAvatar → SelectHeroByAvatar
        MapToggleAvatar();
        // W8: assign slot IDs
        SetSlotIds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && togglePauseUnpause != null)
            togglePauseUnpause.isOn = !togglePauseUnpause.isOn;
    }

    // ── General ───────────────────────────────────────────────────────────────

    public void ToggleAI(bool isOn)
    {
        foreach (Character member in PartyManager.instance.Members)
        {
            AttackAI ai = member.GetComponent<AttackAI>();
            if (ai != null) ai.enabled = isOn;
        }
    }

    public void SelectAll()
    {
        PartyManager.instance.SelectChars.Clear();
        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.CurHP > 0)
            {
                member.ToggleRingSelection(true);
                PartyManager.instance.SelectChars.Add(member);
            }
        }
    }

    public void PauseUnpause(bool isOn)
    {
        Time.timeScale = isOn ? 0 : 1;
    }

    // ── Magic Toggles (W7: icon support) ──────────────────────────────────────

    public void ShowMagicToggles()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];

        for (int i = 0; i < toggleMagic.Length; i++)
        {
            if (i < hero.MagicSkills.Count)
            {
                toggleMagic[i].interactable = true;
                toggleMagic[i].isOn = false;
                // W7: show icon if available
                Image img = toggleMagic[i].GetComponent<Image>();
                if (img != null && hero.MagicSkills[i].Icon != null)
                    img.sprite = hero.MagicSkills[i].Icon;
                TextMeshProUGUI tmp = toggleMagic[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = hero.MagicSkills[i].Name;
                // Legacy Text support
                Text legacyTxt = toggleMagic[i].GetComponentInChildren<Text>();
                if (legacyTxt != null) legacyTxt.text = hero.MagicSkills[i].Name;
            }
            else
            {
                toggleMagic[i].interactable = false;
            }
        }
    }

    public void SelectMagicSkill(int i)
    {
        curToggleMagicID = i;
        PartyManager.instance.HeroSelectMagicSkill(i);
    }

    public void IsOnCurToggleMagic(bool flag)
    {
        if (curToggleMagicID >= 0 && curToggleMagicID < toggleMagic.Length)
            toggleMagic[curToggleMagicID].isOn = flag;
    }

    // ── Inventory (W8) ────────────────────────────────────────────────────────

    private void SetSlotIds()
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i]?.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.SlotId = i;
                // last slot (index 16) is shield slot
                if (i == InventoryManager.MAXSLOT - 1)
                    slot.SlotType = ItemType.Shield;
            }
        }
    }

    public void ShowInventory(bool show)
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(show);
        if (show) RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (slots == null || itemUIPrefab == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];

        for (int i = 0; i < slots.Length && i < InventoryManager.MAXSLOT; i++)
        {
            // clear existing children
            foreach (Transform child in slots[i])
                Destroy(child.gameObject);

            if (hero.InventoryItems == null || hero.InventoryItems[i] == null) continue;

            GameObject iconObj = Instantiate(itemUIPrefab, slots[i]);
            iconObj.transform.localPosition = Vector3.zero;
            ItemDrag drag = iconObj.GetComponent<ItemDrag>();
            if (drag != null)
            {
                drag.Init(hero.InventoryItems[i]);
            }
        }
    }

    // ── Item Use Dialog (W9) ──────────────────────────────────────────────────

    public void SetCurItemInUse(ItemDrag drag, int slotId)
    {
        curItemDrag = drag;
        curSlotId = slotId;
    }

    public void ToggleItemDialog(bool show)
    {
        if (grayImage != null) grayImage.SetActive(show);
        if (itemDialog != null) itemDialog.SetActive(show);
    }

    public void DeleteItemIcon(int slotId)
    {
        if (slots == null || slotId < 0 || slotId >= slots.Length) return;
        foreach (Transform child in slots[slotId])
            Destroy(child.gameObject);
    }

    public void ClickDrinkConsumable()
    {
        if (curItemDrag == null || curItemDrag.item == null) return;
        InventoryManager.instance.DrinkPotion(curItemDrag.item, curSlotId);
        ToggleItemDialog(false);
        curItemDrag = null;
        curSlotId = -1;
    }

    // ── Quest Dialogue (W10) ──────────────────────────────────────────────────

    public void ShowQuestDialogue(Npc npc, Quest quest, Character hero)
    {
        activeQuest = quest;
        activeQuestHero = hero;
        questDialogStep = 0;

        if (questDialogPanel != null) questDialogPanel.SetActive(true);
        AdvanceQuestDialogue();
    }

    public void AdvanceQuestDialogue()
    {
        if (activeQuest == null || questDialogText == null) return;

        if (activeQuest.questStatus == QuestStatus.New)
        {
            if (questDialogStep < activeQuest.questDialogue.Length)
            {
                questDialogText.text = activeQuest.questDialogue[questDialogStep];
                questDialogStep++;
                // Show Accept/Reject on last dialogue line
                bool isLast = questDialogStep >= activeQuest.questDialogue.Length;
                if (questAcceptBtn != null) questAcceptBtn.gameObject.SetActive(isLast);
                if (questRejectBtn != null) questRejectBtn.gameObject.SetActive(isLast);
                if (questNextBtn != null) questNextBtn.gameObject.SetActive(!isLast);
            }
        }
        else if (activeQuest.questStatus == QuestStatus.InProgress)
        {
            bool canFinish = QuestManager.instance.CheckDelivery(activeQuestHero, activeQuest)
                             || activeQuest.curKillCount >= activeQuest.killCount;
            questDialogText.text = canFinish ? activeQuest.answerFinish : activeQuest.answerNotFinish;
            if (questAcceptBtn != null) questAcceptBtn.gameObject.SetActive(canFinish);
            if (questRejectBtn != null) questRejectBtn.gameObject.SetActive(false);
            if (questNextBtn != null) questNextBtn.gameObject.SetActive(!canFinish);
        }
    }

    public void AcceptQuest()
    {
        if (activeQuest == null) return;
        if (activeQuest.questStatus == QuestStatus.New)
            activeQuest.questStatus = QuestStatus.InProgress;
        else if (activeQuest.questStatus == QuestStatus.InProgress)
            QuestManager.instance.CompleteQuest(activeQuest, activeQuestHero);
        CloseQuestDialogue();
    }

    public void RejectQuest()
    {
        if (activeQuest != null) activeQuest.questStatus = QuestStatus.Reject;
        CloseQuestDialogue();
    }

    public void CloseQuestDialogue()
    {
        if (questDialogPanel != null) questDialogPanel.SetActive(false);
        activeQuest = null;
        activeQuestHero = null;
    }

    // ── Party UI / Avatar Toggles (W11) ───────────────────────────────────────

    private void MapToggleAvatar()
    {
        if (toggleAvatar == null) return;
        for (int i = 0; i < toggleAvatar.Length; i++)
        {
            int idx = i;
            if (toggleAvatar[i] == null) continue;
            toggleAvatar[i].onValueChanged.RemoveAllListeners();
            toggleAvatar[i].onValueChanged.AddListener((isOn) => SelectHeroByAvatar(idx, isOn));

            // Show avatar toggle only if member exists
            bool hasHero = idx < PartyManager.instance.Members.Count;
            toggleAvatar[i].gameObject.SetActive(hasHero);
        }
    }

    public void SelectHeroByAvatar(int i, bool isOn)
    {
        if (isOn)
            PartyManager.instance.SelectSingleHeroByToggle(i);
        else
            PartyManager.instance.UnSelectSingleHeroByToggle(i);

        UpdateCharPanel();
    }

    public void UpdateCharPanel()
    {
        if (charPanel == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0)
        {
            charPanel.SetActive(false);
            return;
        }

        charPanel.SetActive(true);
        Hero hero = PartyManager.instance.SelectChars[0] as Hero;
        if (hero == null) return;

        if (charNameText != null) charNameText.text = hero.CharName != "" ? hero.CharName : hero.name;
        if (charImage != null && hero.ProfileSprite != null) charImage.sprite = hero.ProfileSprite;
        if (statText != null)
            statText.text = $"HP: {hero.CurHP}/{hero.MaxHP}\nLv: {hero.Level}\nEXP: {hero.Exp}/{hero.NextExp}\nATK: {hero.AttackDamage}  DEF: {hero.DefensePower}";
        if (abilityText != null)
        {
            string skills = "";
            foreach (Magic m in hero.MagicSkills) skills += m.Name + "\n";
            abilityText.text = skills;
        }
    }

    // ── Shop (W12) ────────────────────────────────────────────────────────────

    public void OpenShop(Npc npc)
    {
        curShopNpc = npc;
        if (shopPanel != null) shopPanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = npc.CharName != "" ? npc.CharName : npc.name;
        if (moneyText != null) moneyText.text = PartyManager.instance.PartyGold + "g";
        PopulateShopItems(npc);
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        curShopNpc = null;
    }

    private void PopulateShopItems(Npc npc)
    {
        if (shopScrollContent == null || itemInShopPrefab == null) return;
        foreach (Transform child in shopScrollContent) Destroy(child.gameObject);
        shopCards.Clear();

        foreach (int dataId in npc.ShopItemIds)
        {
            if (dataId < 0 || dataId >= InventoryManager.instance.ItemDataArr.Length) continue;
            GameObject card = Instantiate(itemInShopPrefab, shopScrollContent);
            ItemInShop cardScript = card.GetComponent<ItemInShop>();
            if (cardScript != null)
            {
                cardScript.Init(InventoryManager.instance.ItemDataArr[dataId], dataId);
                shopCards.Add(cardScript);
            }
        }
    }

    public void BuySelectedItem()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character buyer = PartyManager.instance.SelectChars[0];

        foreach (ItemInShop card in shopCards)
        {
            if (!card.IsSelected()) continue;
            int price = card.GetItemData().normalPrice;
            if (PartyManager.instance.PartyGold < price) continue;
            if (InventoryManager.instance.BuyItem(buyer, card.GetDataId()))
            {
                PartyManager.instance.PartyGold -= price;
                if (moneyText != null) moneyText.text = PartyManager.instance.PartyGold + "g";
            }
        }
        RefreshInventoryUI();
    }

    public void SellSelectedItem()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character seller = PartyManager.instance.SelectChars[0];

        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (seller.InventoryItems == null || seller.InventoryItems[i] == null) continue;
            // simple: sell first occupied slot — in a full UI you'd check which card is selected
            int sellPrice;
            if (InventoryManager.instance.SellItem(seller, i, out sellPrice))
            {
                PartyManager.instance.PartyGold += sellPrice;
                if (moneyText != null) moneyText.text = PartyManager.instance.PartyGold + "g";
                break;
            }
        }
        RefreshInventoryUI();
    }
}
