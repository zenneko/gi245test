using UnityEngine;

public class Enemy : Character
{
    // W13: exp given on death
    [SerializeField] private int expDrop = 10;
    public int ExpDrop { get { return expDrop; } set { expDrop = value; } }

    void Update()
    {
        switch (state)
        {
            case CharState.Walk:
                WalkUpdate();
                break;
            case CharState.WalkToEnemy:
                WalkToEnemyUpdate();
                break;
            case CharState.Attack:
                AttackUpdate();
                break;
        }
    }

    protected override void Die()
    {
        // W13: distribute exp to party before destroy
        if (partyManager != null)
            partyManager.DistributeExp(expDrop);

        // W10: notify quest manager
        if (QuestManager.instance != null)
            QuestManager.instance.OnEnemyKilled(gameObject.tag);

        // W8: drop inventory items
        if (InventoryManager.instance != null && inventoryItems != null)
            InventoryManager.instance.SpawnDropInventory(inventoryItems, transform.position);

        base.Die();
    }
}
