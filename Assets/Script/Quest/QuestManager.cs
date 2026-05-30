using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestData[] questDataArr;

    private List<Quest> quests = new List<Quest>();
    public List<Quest> Quests { get { return quests; } }

    public static QuestManager instance;

    void Awake()
    {
        // W14: persist quest state across scenes
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        foreach (QuestData data in questDataArr)
            quests.Add(new Quest(data));
        // NPCs self-init in their own Start (W14) — supports per-scene NPCs
    }

    public Quest GetQuestById(int id)
    {
        return quests.Find(q => q.questId == id);
    }

    public bool CheckDelivery(Character hero, Quest quest)
    {
        if (hero.InventoryItems == null) return false;
        foreach (Item item in hero.InventoryItems)
            if (item != null && item.ID == quest.questItemId) return true;
        return false;
    }

    public void CompleteQuest(Quest quest, Character hero)
    {
        quest.questStatus = QuestStatus.Finish;

        // W10: hand over the quest item (remove it from the bag) for Delivery quests
        if (quest.questType == QuestType.Delivery)
            DeliverItem(hero, quest);

        NPCGiveReward(quest, hero);

        if (UiManager.instance != null) UiManager.instance.RefreshInventoryUI();
    }

    // Remove the quest item from the hero's bag (the "delivery")
    public void DeliverItem(Character hero, Quest quest)
    {
        if (hero.InventoryItems == null) return;
        for (int i = 0; i < hero.InventoryItems.Length; i++)
        {
            if (hero.InventoryItems[i] != null && hero.InventoryItems[i].ID == quest.questItemId)
            {
                InventoryManager.instance.RemoveItemFromSlot(hero, i);
                break;
            }
        }
    }

    // NPC gives reward item + EXP
    public void NPCGiveReward(Quest quest, Character hero)
    {
        if (PartyManager.instance != null) PartyManager.instance.DistributeExp(quest.rewardExp);
        if (quest.rewardItemId >= 0 && hero.InventoryItems != null)
        {
            Item reward = InventoryManager.instance.CreateItem(quest.rewardItemId);
            for (int i = 0; i < hero.InventoryItems.Length; i++)
            {
                if (hero.InventoryItems[i] == null)
                {
                    InventoryManager.instance.SaveItem(hero, i, reward);
                    break;
                }
            }
        }
    }

    // W13: called when enemy dies
    public void OnEnemyKilled(string enemyTag)
    {
        foreach (Quest q in quests)
        {
            if (q.questType == QuestType.KillCount && q.questStatus == QuestStatus.InProgress)
            {
                q.curKillCount++;
                if (q.curKillCount >= q.killCount)
                    q.questStatus = QuestStatus.Finish;
            }
        }
    }
}
