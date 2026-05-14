using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    public int questId;
    public QuestType questType;
    public int questItemId;
    public int killCount;
    [TextArea] public string[] questDialogue;
    [TextArea] public string[] answerNext;
    [TextArea] public string answerAccept;
    [TextArea] public string answerReject;
    [TextArea] public string questionInProgress;
    [TextArea] public string answerFinish;
    [TextArea] public string answerNotFinish;
    public int rewardItemId = -1;
    public int rewardExp;
}
