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
    [SerializeField] private GameObject blackImage;       // dark overlay behind inventory panel
    [SerializeField] private GameObject[] slots;          // bag slots (SlotType = None)
    [SerializeField] private GameObject[] equipmentSlots; // equipment slots (SlotType = Shield/Weapon)
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
    [SerializeField] private Image questNpcImage;            // NPC portrait
    [SerializeField] private TextMeshProUGUI questNpcNameText; // NPC name
    [SerializeField] private TextMeshProUGUI questDialogText;
    [SerializeField] private Button questAcceptBtn;
    [SerializeField] private Button questRejectBtn;
    [SerializeField] private Button questNextBtn;
    [SerializeField] private Button questFinishBtn;
    [SerializeField] private Button questNotFinishBtn;
    // W13: Party Invite — reuse the same DialoguePanel (matches manual 42.8)
    [SerializeField] private Button questJoinPartyBtn;
    [SerializeField] private Button questNotJoinPartyBtn;
    // Button labels — show the player's response from QuestData
    [SerializeField] private TextMeshProUGUI questAcceptBtnText;
    [SerializeField] private TextMeshProUGUI questRejectBtnText;
    [SerializeField] private TextMeshProUGUI questNextBtnText;
    [SerializeField] private TextMeshProUGUI questFinishBtnText;
    [SerializeField] private TextMeshProUGUI questNotFinishBtnText;
    [SerializeField] private string joinPartyMessage = "Do you want me to join your party?";
    private Quest activeQuest;
    private Character activeQuestHero;
    private Npc activeQuestNpc;
    private int questDialogStep = 0;

    // ── Party Reform (W11) ───────────────────────────────────────────────────
    [SerializeField] private GameObject partyPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Toggle[] toggleRemove;
    [SerializeField] private Button removeButton;
    private int removeIndex = -1;

    // ── Party Invite (W13) — shares the quest DialoguePanel ──────────────────
    private Hero pendingInviteHero;
    private Character invitingHero;

    // ── Shop (W12) ────────────────────────────────────────────────────────────
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopScrollContent;   // left scroll content
    [SerializeField] private Transform playerScrollContent; // right scroll content (player inventory)
    [SerializeField] private GameObject itemInShopPrefab;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI moneyText;     // party gold
    [SerializeField] private TextMeshProUGUI npcMoneyText;  // shop gold
    private Npc curShopNpc;
    private List<ItemInShop> shopCards = new List<ItemInShop>();
    private List<ItemInShop> partyCards = new List<ItemInShop>();

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
            InventorySlot slot = slots[i] != null ? slots[i].GetComponent<InventorySlot>() : null;
            if (slot != null)
            {
                slot.SlotId = i;
                slot.SlotType = ItemType.None;   // bag slots accept any item
            }
        }
        // Equipment slots keep the SlotType (Shield/Weapon) set in the Inspector — don't touch them here.
    }

    public void ShowInventory(bool show)
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(show);
        if (blackImage != null) blackImage.SetActive(show);
        if (show) RefreshInventoryUI();
    }

    // เปิด/ปิดสลับกันด้วยปุ่มเดียว
    public void ToggleInventoryPanel()
    {
        if (inventoryPanel == null) return;
        if (!inventoryPanel.activeInHierarchy)
        {
            inventoryPanel.SetActive(true);
            if (blackImage != null) blackImage.SetActive(true);
            RefreshInventoryUI();
        }
        else
        {
            inventoryPanel.SetActive(false);
            if (blackImage != null) blackImage.SetActive(false);
        }
    }

    public void RefreshInventoryUI()
    {
        if (itemUIPrefab == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];

        // Bag slots
        if (slots != null)
            for (int i = 0; i < slots.Length && i < InventoryManager.MAXSLOT; i++)
            {
                Item it = (hero.InventoryItems != null && i < hero.InventoryItems.Length)
                    ? hero.InventoryItems[i] : null;
                FillSlot(slots[i], it);
            }

        // Equipment slots — look up the right item by the slot's SlotType
        if (equipmentSlots != null)
            foreach (GameObject go in equipmentSlots)
            {
                InventorySlot es = go != null ? go.GetComponent<InventorySlot>() : null;
                if (es == null) continue;
                int ei = InventoryManager.EquipIndexOf(es.SlotType);
                Item it = (ei >= 0 && hero.EquipmentItems != null && ei < hero.EquipmentItems.Length)
                    ? hero.EquipmentItems[ei] : null;
                FillSlot(go, it);
            }
    }

    // Clear a slot's current icon and (re)build one for the given item
    private void FillSlot(GameObject slotGo, Item item)
    {
        if (slotGo == null) return;
        foreach (Transform child in slotGo.transform)
            Destroy(child.gameObject);
        if (item == null) return;

        GameObject iconObj = Instantiate(itemUIPrefab, slotGo.transform);
        iconObj.transform.localPosition = Vector3.zero;
        ItemDrag drag = iconObj.GetComponent<ItemDrag>();
        if (drag != null) drag.Init(item);
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
        foreach (Transform child in slots[slotId].transform)
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
        activeQuestNpc = npc;
        questDialogStep = 0;

        // W10: show NPC portrait + name
        if (questNpcImage != null && npc != null && npc.ProfileSprite != null)
            questNpcImage.sprite = npc.ProfileSprite;
        if (questNpcNameText != null && npc != null)
            questNpcNameText.text = npc.CharName != "" ? npc.CharName : npc.name;

        if (questDialogPanel != null) questDialogPanel.SetActive(true);
        Time.timeScale = 0f;   // pause the game while talking
        AdvanceQuestDialogue();
    }

    public void AdvanceQuestDialogue()
    {
        if (activeQuest == null || questDialogText == null) return;

        // Static button labels — what the player will say if they click
        SetLabel(questAcceptBtnText,    activeQuest.answerAccept);
        SetLabel(questRejectBtnText,    activeQuest.answerReject);
        SetLabel(questFinishBtnText,    activeQuest.answerFinish);
        SetLabel(questNotFinishBtnText, activeQuest.answerNotFinish);

        // Hide party-invite buttons while showing a quest (panel is shared)
        if (questJoinPartyBtn != null)    questJoinPartyBtn.gameObject.SetActive(false);
        if (questNotJoinPartyBtn != null) questNotJoinPartyBtn.gameObject.SetActive(false);

        if (activeQuest.questStatus == QuestStatus.New)
        {
            if (questDialogStep < activeQuest.questDialogue.Length)
            {
                int currentStep = questDialogStep;
                questDialogText.text = activeQuest.questDialogue[currentStep];

                // Next button label = the player's response to the current NPC line
                if (activeQuest.answerNext != null && currentStep < activeQuest.answerNext.Length)
                    SetLabel(questNextBtnText, activeQuest.answerNext[currentStep]);

                questDialogStep++;
                bool isLast = questDialogStep >= activeQuest.questDialogue.Length;
                if (questAcceptBtn != null)    questAcceptBtn.gameObject.SetActive(isLast);
                if (questRejectBtn != null)    questRejectBtn.gameObject.SetActive(isLast);
                if (questNextBtn != null)      questNextBtn.gameObject.SetActive(!isLast);
                if (questFinishBtn != null)    questFinishBtn.gameObject.SetActive(false);
                if (questNotFinishBtn != null) questNotFinishBtn.gameObject.SetActive(false);
            }
        }
        else if (activeQuest.questStatus == QuestStatus.InProgress)
        {
            // W10: NPC asks the in-progress question; player picks Finish/NotFinish
            questDialogText.text = activeQuest.questionInProgress;

            // Type-aware completion check (the OR with killCount was always true for Delivery quests)
            bool canFinish = false;
            if (activeQuest.questType == QuestType.Delivery)
                canFinish = QuestManager.instance.CheckDelivery(activeQuestHero, activeQuest);
            else if (activeQuest.questType == QuestType.KillCount)
                canFinish = activeQuest.curKillCount >= activeQuest.killCount;
            if (questAcceptBtn != null)    questAcceptBtn.gameObject.SetActive(false);
            if (questRejectBtn != null)    questRejectBtn.gameObject.SetActive(false);
            if (questNextBtn != null)      questNextBtn.gameObject.SetActive(false);
            if (questFinishBtn != null)    questFinishBtn.gameObject.SetActive(canFinish);
            if (questNotFinishBtn != null) questNotFinishBtn.gameObject.SetActive(!canFinish);
        }
    }

    private void SetLabel(TextMeshProUGUI label, string text)
    {
        if (label != null) label.text = text;
    }

    public void AcceptQuest()
    {
        if (activeQuest == null) return;
        if (activeQuest.questStatus == QuestStatus.New)
        {
            activeQuest.questStatus = QuestStatus.InProgress;
            // W10: add to party's accepted quest list (matches manual 33.7)
            if (PartyManager.instance != null && !PartyManager.instance.QuestList.Contains(activeQuest))
                PartyManager.instance.QuestList.Add(activeQuest);
        }
        else if (activeQuest.questStatus == QuestStatus.InProgress)
        {
            QuestManager.instance.CompleteQuest(activeQuest, activeQuestHero);
        }
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
        Time.timeScale = 1f;   // resume the game
        activeQuest = null;
        activeQuestHero = null;
        activeQuestNpc = null;
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

    // W11: hide CharPanel without deselecting the hero (so player can move/click freely)
    public void CloseCharPanel()
    {
        if (charPanel != null) charPanel.SetActive(false);
    }

    // Open/close toggle
    public void ToggleCharPanel()
    {
        if (charPanel == null) return;
        bool show = !charPanel.activeInHierarchy;
        charPanel.SetActive(show);
        if (show) UpdateCharPanel();
    }

    public void UpdateCharPanel()
    {
        if (charPanel == null) return;
        // W11: don't auto-open — only refresh if the player already opened the panel
        if (!charPanel.activeInHierarchy) return;
        if (PartyManager.instance.SelectChars.Count <= 0)
        {
            charPanel.SetActive(false);
            return;
        }

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

    // Refresh which avatar toggles are visible after party changes
    public void RefreshPartyAvatars()
    {
        if (toggleAvatar == null) return;
        for (int i = 0; i < toggleAvatar.Length; i++)
        {
            if (toggleAvatar[i] == null) continue;
            bool hasHero = i < PartyManager.instance.Members.Count;
            toggleAvatar[i].gameObject.SetActive(hasHero);
            if (!hasHero) toggleAvatar[i].isOn = false;
        }
        UpdateCharPanel();
    }

    // ── Party Reform (W11) ───────────────────────────────────────────────────

    public void TogglePartyPanel()
    {
        if (partyPanel == null) return;
        bool show = !partyPanel.activeInHierarchy;
        partyPanel.SetActive(show);
        if (show) MapToggleRemove();
    }

    private void MapToggleRemove()
    {
        if (toggleRemove == null) return;
        for (int i = 0; i < toggleRemove.Length; i++)
        {
            int idx = i;
            if (toggleRemove[i] == null) continue;
            toggleRemove[i].onValueChanged.RemoveAllListeners();
            toggleRemove[i].onValueChanged.AddListener((isOn) => SelectToRemove(idx, isOn));
            bool hasHero = idx < PartyManager.instance.Members.Count;
            toggleRemove[i].gameObject.SetActive(hasHero);
            toggleRemove[i].isOn = false;
        }
        removeIndex = -1;
        CheckRemoveButton();
    }

    public void CheckRemoveButton()
    {
        if (removeButton != null) removeButton.interactable = removeIndex >= 0;
    }

    public void SelectToRemove(int i, bool isOn)
    {
        removeIndex = isOn ? i : -1;
        // Radio-style: turn off all others
        if (isOn && toggleRemove != null)
        {
            for (int j = 0; j < toggleRemove.Length; j++)
                if (j != i && toggleRemove[j] != null) toggleRemove[j].isOn = false;
        }
        CheckRemoveButton();
    }

    public void ToggleConfirmPanel(bool show)
    {
        if (confirmPanel != null) confirmPanel.SetActive(show);
    }

    public void RemoveMemberFromParty()
    {
        if (removeIndex < 0) return;
        PartyManager.instance.RemoveHeroFromParty(removeIndex);
        removeIndex = -1;
        ToggleConfirmPanel(false);
        MapToggleRemove();
        UpdateCharPanel();
    }

    // ── Party Invite (W13) ───────────────────────────────────────────────────

    public void ShowJoinPartyDialogue(Hero targetHero, Character inviter)
    {
        pendingInviteHero = targetHero;
        invitingHero = inviter;

        // Target hero is the speaker — show their portrait + name
        if (questNpcImage != null && targetHero.ProfileSprite != null)
            questNpcImage.sprite = targetHero.ProfileSprite;
        if (questNpcNameText != null)
            questNpcNameText.text = targetHero.CharName != "" ? targetHero.CharName : targetHero.name;
        if (questDialogText != null) questDialogText.text = joinPartyMessage;

        // Only the join-party buttons are visible in this mode
        if (questNextBtn != null)         questNextBtn.gameObject.SetActive(false);
        if (questAcceptBtn != null)       questAcceptBtn.gameObject.SetActive(false);
        if (questRejectBtn != null)       questRejectBtn.gameObject.SetActive(false);
        if (questFinishBtn != null)       questFinishBtn.gameObject.SetActive(false);
        if (questNotFinishBtn != null)    questNotFinishBtn.gameObject.SetActive(false);
        if (questJoinPartyBtn != null)    questJoinPartyBtn.gameObject.SetActive(true);
        if (questNotJoinPartyBtn != null) questNotJoinPartyBtn.gameObject.SetActive(true);

        if (questDialogPanel != null) questDialogPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void AnswerJoinParty()
    {
        if (pendingInviteHero != null)
            PartyManager.instance.AddHeroToParty(pendingInviteHero);
        CloseJoinPartyDialogue();
    }

    public void AnswerNotJoinParty()
    {
        CloseJoinPartyDialogue();
    }

    private void CloseJoinPartyDialogue()
    {
        if (questDialogPanel != null) questDialogPanel.SetActive(false);
        if (questJoinPartyBtn != null)    questJoinPartyBtn.gameObject.SetActive(false);
        if (questNotJoinPartyBtn != null) questNotJoinPartyBtn.gameObject.SetActive(false);
        Time.timeScale = 1f;
        pendingInviteHero = null;
        invitingHero = null;
    }

    // ── Shop (W12) ────────────────────────────────────────────────────────────

    public void OpenShop(Npc npc)
    {
        curShopNpc = npc;
        if (shopPanel != null) shopPanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = npc.CharName != "" ? npc.CharName : npc.name;
        RefreshShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        curShopNpc = null;
    }

    // Rebuild both sides + money labels — call after every buy/sell
    private void RefreshShopUI()
    {
        if (curShopNpc == null) return;
        PopulateShopItems(curShopNpc);
        SetupPartyItems();
        if (moneyText != null) moneyText.text = PartyManager.instance.PartyGold + "g";
        if (npcMoneyText != null) npcMoneyText.text = curShopNpc.NpcGold + "g";
    }

    // Left panel — populate from the shop NPC's own inventory
    private void PopulateShopItems(Npc npc)
    {
        if (shopScrollContent == null || itemInShopPrefab == null) return;
        foreach (Transform child in shopScrollContent) Destroy(child.gameObject);
        shopCards.Clear();
        if (npc.InventoryItems == null) return;

        for (int i = 0; i < npc.InventoryItems.Length; i++)
        {
            Item item = npc.InventoryItems[i];
            if (item == null) continue;
            ItemData data = LookupItemData(item.ID);
            if (data == null) continue;

            ItemInShop card = SpawnCard(shopScrollContent);
            if (card != null) { card.InitAsShopItem(data, item.ID); shopCards.Add(card); }
        }
    }

    // Right panel — populate from the selected hero's bag
    private void SetupPartyItems()
    {
        if (playerScrollContent == null || itemInShopPrefab == null) return;
        foreach (Transform child in playerScrollContent) Destroy(child.gameObject);
        partyCards.Clear();
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character hero = PartyManager.instance.SelectChars[0];
        if (hero.InventoryItems == null) return;

        for (int i = 0; i < hero.InventoryItems.Length; i++)
        {
            Item item = hero.InventoryItems[i];
            if (item == null) continue;
            ItemData data = LookupItemData(item.ID);
            if (data == null) continue;

            ItemInShop card = SpawnCard(playerScrollContent);
            if (card != null) { card.InitAsPartyItem(data, i); partyCards.Add(card); }
        }
    }

    public void BuySelectedItem()
    {
        if (curShopNpc == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character buyer = PartyManager.instance.SelectChars[0];

        foreach (ItemInShop card in shopCards)
        {
            if (!card.IsSelected() || !card.IsShop) continue;
            int price = card.GetItemData().normalPrice;
            if (PartyManager.instance.PartyGold < price) continue;

            int npcSlot = FindSlotByItemId(curShopNpc, card.GetDataId());
            if (npcSlot < 0) continue;
            int playerSlot = FindEmptySlot(buyer);
            if (playerSlot < 0) continue;   // buyer's bag full

            // Move item NPC → buyer + transfer money
            Item bought = curShopNpc.InventoryItems[npcSlot];
            InventoryManager.instance.RemoveItemFromSlot(curShopNpc, npcSlot);
            InventoryManager.instance.SaveItem(buyer, playerSlot, bought);
            PartyManager.instance.PartyGold -= price;
            curShopNpc.NpcGold += price;
        }
        RefreshShopUI();
        RefreshInventoryUI();
    }

    public void SellSelectedItem()
    {
        if (curShopNpc == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        Character seller = PartyManager.instance.SelectChars[0];

        foreach (ItemInShop card in partyCards)
        {
            if (!card.IsSelected() || card.IsShop) continue;
            int slot = card.GetSlotId();
            if (slot < 0 || slot >= seller.InventoryItems.Length) continue;
            Item item = seller.InventoryItems[slot];
            if (item == null) continue;

            int sellPrice = Mathf.Max(1, item.NormalPrice / 2);
            if (curShopNpc.NpcGold < sellPrice) continue;   // shop can't afford
            int npcSlot = FindEmptySlot(curShopNpc);
            if (npcSlot < 0) continue;                       // shop is full

            // Move item seller → NPC + transfer money
            InventoryManager.instance.RemoveItemFromSlot(seller, slot);
            InventoryManager.instance.SaveItem(curShopNpc, npcSlot, item);
            PartyManager.instance.PartyGold += sellPrice;
            curShopNpc.NpcGold -= sellPrice;
        }
        RefreshShopUI();
        RefreshInventoryUI();
    }

    // ── Shop helpers ──────────────────────────────────────────────────────────
    private ItemData LookupItemData(int id)
    {
        if (InventoryManager.instance == null || InventoryManager.instance.ItemDataArr == null) return null;
        if (id < 0 || id >= InventoryManager.instance.ItemDataArr.Length) return null;
        return InventoryManager.instance.ItemDataArr[id];
    }

    private ItemInShop SpawnCard(Transform parent)
    {
        GameObject go = Instantiate(itemInShopPrefab, parent);
        return go.GetComponent<ItemInShop>();
    }

    private static int FindSlotByItemId(Character c, int itemId)
    {
        if (c == null || c.InventoryItems == null) return -1;
        for (int i = 0; i < c.InventoryItems.Length; i++)
            if (c.InventoryItems[i] != null && c.InventoryItems[i].ID == itemId) return i;
        return -1;
    }

    private static int FindEmptySlot(Character c)
    {
        if (c == null || c.InventoryItems == null) return -1;
        for (int i = 0; i < c.InventoryItems.Length; i++)
            if (c.InventoryItems[i] == null) return i;
        return -1;
    }
}
