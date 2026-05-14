using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<Character> enemies = new List<Character>();
    public List<Character> Enemies { get { return enemies; } }

    public static EnemyManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        foreach (Character e in enemies)
        {
            e.CharInit(VFXManager.instance, UiManager.instance, PartyManager.instance, InventoryManager.instance);
            // Give first enemy starter items (index 0=Potion, 1=Sword, 2=Shield)
            if (e.InventoryItems != null && InventoryManager.instance != null)
            {
                e.InventoryItems[0] = InventoryManager.instance.CreateItem(0);
                e.InventoryItems[1] = InventoryManager.instance.CreateItem(1);
                e.InventoryItems[2] = InventoryManager.instance.CreateItem(2);
            }
        }
    }
}
