using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject doubleRingMarker;
    public GameObject DoubleRingMarker { get { return doubleRingMarker; } }

    [SerializeField] private GameObject[] magicVFX;
    public GameObject[] MagicVFX { get { return magicVFX; } }

    // W7: ScriptableObject data for magic spells
    [SerializeField] private MagicData[] magicDataArr;
    public MagicData[] MagicDataArr { get { return magicDataArr; } }

    public static VFXManager instance;

    void Awake()
    {
        instance = this;
    }

    public void LoadMagic(int id, Vector3 posA, float time)
    {
        if (id < 0 || id >= magicVFX.Length || magicVFX[id] == null) return;
        GameObject obj = Instantiate(magicVFX[id], posA, Quaternion.identity);
        Destroy(obj, time);
    }

    public void ShootMagic(int id, Vector3 posA, Vector3 posB, float time)
    {
        if (id < 0 || id >= magicVFX.Length || magicVFX[id] == null) return;
        GameObject obj = Instantiate(magicVFX[id], posA, Quaternion.identity);
        obj.transform.position = Vector3.LerpUnclamped(posA, posB, time);
        Destroy(obj, time);
    }
}
