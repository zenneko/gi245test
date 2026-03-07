using TMPro;
using UnityEngine;

public class VFXManager: MonoBehaviour
{
    [SerializeField] private GameObject doubleRingMarker;
    public GameObject DoubleRingMarker { get { return doubleRingMarker; } }
    
    [SerializeField]
    private GameObject[] magicVFX;
    public GameObject[] MagicVFX { get { return magicVFX; } }
    
    public static VFXManager instance;

    void Awake() 
    { 
        instance = this; 
    }

    public void LoadMagic(int id, Vector3 posA, float time)
    {
        if (magicVFX[id] == null)
        {
            return;
        }

        GameObject objLoad = Instantiate(magicVFX[id], posA, Quaternion.identity);
        Destroy(objLoad, time);
    }
    public void ShootMagic(int id, Vector3 posA,Vector3 posB, float time)
    {
        if (magicVFX[id] == null)
        {
            return;
        }

        GameObject objShoot = Instantiate(magicVFX[id], posA, Quaternion.identity);
        objShoot.transform.position = Vector3.LerpUnclamped(posA, posB, time);
        Destroy(objShoot, time);
    }
}