using UnityEngine;

[CreateAssetMenu(fileName = "MagicData", menuName = "Scriptable Objects/Magic Data")]
public class MagicData : ScriptableObject
{
    public int id;
    public string magicName;
    public float range = 10f;
    public int power = 30;
    public float loadTime = 3f;
    public float shootTime = 1f;
    public int loadId = 0;
    public int shootId = 1;
    public Sprite icon;
}
