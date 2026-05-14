using UnityEngine;

public enum QuestType { Delivery, KillCount }
public enum QuestStatus { New, InProgress, Finish, Reject }

[System.Serializable]
public class Quest
{
    public int questId;
    public QuestType questType;
    public QuestStatus questStatus;

    public int questItemId;   // item ID NPC wants (Delivery)
    public int killCount;     // kills required (KillCount)
    public int curKillCount;

    public string[] questDialogue;
    public string[] answerNext;
    public string answerAccept;
    public string answerReject;
    public string questionInProgress;
    public string answerFinish;
    public string answerNotFinish;

    public int rewardItemId = -1;
    public int rewardExp;

    public Quest(QuestData data)
    {
        questId = data.questId;
        questType = data.questType;
        questStatus = QuestStatus.New;
        questItemId = data.questItemId;
        killCount = data.killCount;
        curKillCount = 0;
        questDialogue = data.questDialogue;
        answerNext = data.answerNext;
        answerAccept = data.answerAccept;
        answerReject = data.answerReject;
        questionInProgress = data.questionInProgress;
        answerFinish = data.answerFinish;
        answerNotFinish = data.answerNotFinish;
        rewardItemId = data.rewardItemId;
        rewardExp = data.rewardExp;
    }
}
