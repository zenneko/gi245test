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
        instance = this;
    }

    void Start()
    {
        foreach (QuestData data in questDataArr)
            quests.Add(new Quest(data));

        // W13: init NPC characters
        foreach (Npc npc in FindObjectsByType<Npc>(FindObjectsSortMode.None))
            npc.CharInit(VFXManager.instance, UiManager.instance, PartyManager.instance, InventoryManager.instance);
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
        if (PartyManager.instance != null) PartyManager.instance.DistributeExp(quest.rewardExp);
        if (quest.rewardItemId >= 0 && hero.InventoryItems != null)
        {
            Item reward = InventoryManager.instance.CreateItem(quest.rewardItemId);
            for (int i = 0; i < hero.InventoryItems.Length - 1; i++)
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
