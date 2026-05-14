using UnityEngine;

public class Hero : Character
{
    // W13: RPG stat fields
    [SerializeField] private int strength = 10;
    public int Strength { get { return strength; } set { strength = value; } }
    [SerializeField] private int dexterity = 10;
    public int Dexterity { get { return dexterity; } set { dexterity = value; } }
    [SerializeField] private int constitution = 10;
    public int Constitution { get { return constitution; } set { constitution = value; } }
    [SerializeField] private int intelligence = 10;
    public int Intelligence { get { return intelligence; } set { intelligence = value; } }
    [SerializeField] private int wisdom = 10;
    public int Wisdom { get { return wisdom; } set { wisdom = value; } }
    [SerializeField] private int charisma = 10;
    public int Charisma { get { return charisma; } set { charisma = value; } }

    [SerializeField] private int level = 1;
    public int Level { get { return level; } set { level = value; } }

    [SerializeField] private int exp = 0;
    public int Exp { get { return exp; } set { exp = value; } }

    [SerializeField] private int nextExp = 30;
    public int NextExp { get { return nextExp; } set { nextExp = value; } }

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
            case CharState.WalkToMagicCast:
                WalkToMagicCastUpdate();
                break;
            case CharState.WalkToNPC:
                WalkToNpcUpdate();
                break;
        }
    }

    // W13
    public void ReceiveExp(int amount)
    {
        exp += amount;
        CheckLevel();
    }

    private void CheckLevel()
    {
        while (exp >= nextExp)
        {
            exp -= nextExp;
            level++;
            nextExp = Mathf.RoundToInt(nextExp * 1.5f);
            UpdateStat();
        }
    }

    private void UpdateStat()
    {
        attackDamage += strength / 5;
        defensePower += constitution / 10;
        maxHP += constitution * 2;
        curHP = maxHP;
    }
}
