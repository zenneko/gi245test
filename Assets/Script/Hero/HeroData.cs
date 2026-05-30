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
    public int[] equipmentItemIds = new int[InventoryManager.EQUIP_COUNT];

    public void SaveFrom(Hero hero)
    {
        prefabId = hero.PrefabID;
        curHP = hero.CurHP;
        maxHP = hero.MaxHP;
        level = hero.Level;
        exp = hero.Exp;
        nextExp = hero.NextExp;
        attackDamage = hero.AttackDamage;   // includes equipped weapon bonus
        defensePower = hero.DefensePower;   // includes equipped shield bonus

        SaveIds(hero.InventoryItems, ref inventoryItemIds, InventoryManager.MAXSLOT);
        SaveIds(hero.EquipmentItems, ref equipmentItemIds, InventoryManager.EQUIP_COUNT);
    }

    public void LoadTo(Hero hero)
    {
        hero.CurHP = curHP;
        hero.MaxHP = maxHP;
        hero.Level = level;
        hero.Exp = exp;
        hero.NextExp = nextExp;
        hero.AttackDamage = attackDamage;   // already includes equip bonuses
        hero.DefensePower = defensePower;

        hero.InventoryItems = LoadIds(inventoryItemIds, InventoryManager.MAXSLOT);
        hero.EquipmentItems = LoadIds(equipmentItemIds, InventoryManager.EQUIP_COUNT);

        // Stats were restored above (with bonuses), so only rebuild the 3D models here
        hero.RefreshEquipVisuals();
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private static void SaveIds(Item[] items, ref int[] ids, int length)
    {
        if (ids == null || ids.Length != length) ids = new int[length];
        for (int i = 0; i < length; i++)
            ids[i] = (items != null && i < items.Length && items[i] != null) ? items[i].ID : -1;
    }

    private static Item[] LoadIds(int[] ids, int length)
    {
        Item[] items = new Item[length];
        if (ids == null || InventoryManager.instance == null) return items;
        for (int i = 0; i < length && i < ids.Length; i++)
            items[i] = ids[i] >= 0 ? InventoryManager.instance.CreateItem(ids[i]) : null;
        return items;
    }
}
