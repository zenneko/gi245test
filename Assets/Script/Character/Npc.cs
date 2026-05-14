using UnityEngine;

public class Npc : Character
{
    [SerializeField] private int[] questIds;
    // W12: this NPC sells items (data IDs)
    [SerializeField] private int[] shopItemIds;
    public int[] ShopItemIds { get { return shopItemIds; } }

    void Update()
    {
        // NPC is stationary — no autonomous movement
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
        else if (shopItemIds != null && shopItemIds.Length > 0)
            ShopManager.instance?.OpenShop(this);
    }
}
