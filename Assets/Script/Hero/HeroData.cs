using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : ScriptableObject
{
    public int prefabId = -1;
    public int curHP = 100;
    public int maxHP = 100;
    public int level = 1;
    public int exp = 0;
    public int nextExp = 30;
    public int attackDamage = 3;
    public int defensePower = 0;
    public int[] inventoryItemIds = new int[InventoryManager.MAXSLOT];

    public void SaveFrom(Hero hero)
    {
        prefabId = hero.PrefabID;
        curHP = hero.CurHP;
        maxHP = hero.MaxHP;
        level = hero.Level;
        exp = hero.Exp;
        nextExp = hero.NextExp;
        attackDamage = hero.AttackDamage;
        defensePower = hero.DefensePower;

        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (hero.InventoryItems != null && i < hero.InventoryItems.Length && hero.InventoryItems[i] != null)
                inventoryItemIds[i] = hero.InventoryItems[i].ID;
            else
                inventoryItemIds[i] = -1;
        }
    }

    public void LoadTo(Hero hero)
    {
        hero.CurHP = curHP;
        hero.MaxHP = maxHP;
        hero.Level = level;
        hero.Exp = exp;
        hero.NextExp = nextExp;
        hero.AttackDamage = attackDamage;
        hero.DefensePower = defensePower;

        if (hero.InventoryItems == null)
            hero.InventoryItems = new Item[InventoryManager.MAXSLOT];

        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (inventoryItemIds[i] >= 0 && InventoryManager.instance != null)
                hero.InventoryItems[i] = InventoryManager.instance.CreateItem(inventoryItemIds[i]);
            else
                hero.InventoryItems[i] = null;
        }
    }
}
