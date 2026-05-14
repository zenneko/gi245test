using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CharState
{
    Idle, Walk, WalkToEnemy, Attack, WalkToMagicCast, MagicCast, WalkToNPC, Hit, Die
}

public abstract class Character : MonoBehaviour
{
    protected NavMeshAgent navAgent;
    protected Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField] protected CharState state;
    public CharState State { get { return state; } }

    [SerializeField] protected GameObject ringSelection;
    public GameObject RingSelection { get { return ringSelection; } }

    [SerializeField] protected int curHP = 100;
    public int CurHP { get { return curHP; } set { curHP = value; } }

    // W9
    [SerializeField] protected int maxHP = 100;
    public int MaxHP { get { return maxHP; } set { maxHP = value; } }

    [SerializeField] protected Character curCharTarget;
    public Character CurCharTarget { get { return curCharTarget; } set { curCharTarget = value; } }

    [SerializeField] protected float attackRange = 2f;
    public float AttackRange { get { return attackRange; } }

    [SerializeField] protected int attackDamage = 3;
    public int AttackDamage { get { return attackDamage; } set { attackDamage = value; } }

    [SerializeField] protected int defensePower = 0;
    public int DefensePower { get { return defensePower; } set { defensePower = value; } }

    [SerializeField] protected float attackCoolDown = 2f;
    [SerializeField] protected float attackTimer = 0f;
    [SerializeField] protected float findingRange = 20f;
    public float FindingRange { get { return findingRange; } }

    [SerializeField] protected List<Magic> magicSkills = new List<Magic>();
    public List<Magic> MagicSkills { get { return magicSkills; } set { magicSkills = value; } }

    [SerializeField] protected Magic curMagicCast = null;
    public Magic CurMagicCast { get { return curMagicCast; } set { curMagicCast = value; } }

    [SerializeField] protected bool isMagicMode = false;
    public bool IsMagicMode { get { return isMagicMode; } set { isMagicMode = value; } }

    // W7: Inventory
    protected Item[] inventoryItems;
    public Item[] InventoryItems { get { return inventoryItems; } set { inventoryItems = value; } }
    protected Item mainWeapon = null;
    public Item MainWeapon { get { return mainWeapon; } set { mainWeapon = value; } }
    protected Item shield = null;
    public Item Shield { get { return shield; } set { shield = value; } }

    // W10: NPC interaction & profile
    [SerializeField] protected string charName = "";
    public string CharName { get { return charName; } set { charName = value; } }
    [SerializeField] protected Sprite profileSprite;
    public Sprite ProfileSprite { get { return profileSprite; } }

    // W14: Prefab ID for scene persistence
    [SerializeField] protected int prefabID = -1;
    public int PrefabID { get { return prefabID; } }

    protected VFXManager vfxManager;
    protected UiManager uiManager;
    protected PartyManager partyManager;

    // W10: NPC target
    protected Npc curNpcTarget;
    public Npc CurNpcTarget { get { return curNpcTarget; } set { curNpcTarget = value; } }

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    public void SetState(CharState s)
    {
        state = s;
        if (state == CharState.Idle)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }

    public void ToggleRingSelection(bool flag)
    {
        if (ringSelection != null) ringSelection.SetActive(flag);
    }

    public void WalkToPosition(Vector3 dest)
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(dest);
            navAgent.isStopped = false;
        }
        SetState(CharState.Walk);
    }

    protected void WalkUpdate()
    {
        float distance = Vector3.Distance(transform.position, navAgent.destination);
        if (distance <= navAgent.stoppingDistance) SetState(CharState.Idle);
    }

    public void ToAttackCharacter(Character target)
    {
        if (curHP <= 0 || state == CharState.Die) return;
        curCharTarget = target;
        navAgent.SetDestination(target.transform.position);
        navAgent.isStopped = false;
        if (isMagicMode)
            SetState(CharState.WalkToMagicCast);
        else
            SetState(CharState.WalkToEnemy);
    }

    // W10
    public void WalkToNpc(Npc target)
    {
        if (curHP <= 0 || state == CharState.Die) return;
        curNpcTarget = target;
        navAgent.SetDestination(target.transform.position);
        navAgent.isStopped = false;
        SetState(CharState.WalkToNPC);
    }

    protected void WalkToNpcUpdate()
    {
        if (curNpcTarget == null) { SetState(CharState.Idle); return; }
        navAgent.SetDestination(curNpcTarget.transform.position);
        float distance = Vector3.Distance(transform.position, curNpcTarget.transform.position);
        if (distance <= attackRange * 2f)
        {
            navAgent.isStopped = true;
            SetState(CharState.Idle);
            curNpcTarget.TryStartDialogue(this);
        }
    }

    protected void WalkToEnemyUpdate()
    {
        if (curCharTarget == null) { SetState(CharState.Idle); return; }
        navAgent.SetDestination(curCharTarget.transform.position);
        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);
        if (distance <= attackRange)
        {
            SetState(CharState.Attack);
            Attack();
        }
    }

    protected void Attack()
    {
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("Attack");
        AttackLogic();
    }

    protected void AttackUpdate()
    {
        if (curCharTarget == null || curCharTarget.CurHP <= 0)
        {
            SetState(CharState.Idle);
            return;
        }
        navAgent.isStopped = true;
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCoolDown)
        {
            attackTimer = 0f;
            Attack();
        }
        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);
        if (distance > attackRange)
        {
            SetState(CharState.WalkToEnemy);
            navAgent.SetDestination(curCharTarget.transform.position);
            navAgent.isStopped = false;
        }
    }

    public void ReceiveDamage(int damage)
    {
        if (curHP <= 0 || state == CharState.Die) return;
        int actual = Mathf.Max(1, damage - defensePower);
        curHP -= actual;
        if (curHP <= 0) { curHP = 0; Die(); }
    }

    // W9
    public void Recover(int amount)
    {
        curHP = Mathf.Min(curHP + amount, maxHP);
    }

    protected void AttackLogic()
    {
        Character target = curCharTarget.GetComponent<Character>();
        if (target != null) target.ReceiveDamage(attackDamage);
    }

    protected virtual void Die()
    {
        navAgent.isStopped = true;
        SetState(CharState.Die);
        anim.SetTrigger("Die");
        StartCoroutine(DestroyObject());
    }

    protected virtual IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    public bool IsMyEnemy(string targetTag)
    {
        string myTag = gameObject.tag;
        if ((myTag == "Hero" || myTag == "Player") && targetTag == "Enemy") return true;
        if (myTag == "Enemy" && (targetTag == "Hero" || targetTag == "Player")) return true;
        return false;
    }

    // W13: CharInit with capital C — main init method
    public void CharInit(VFXManager vfxM, UiManager uiM, PartyManager pm = null, InventoryManager invM = null)
    {
        vfxManager = vfxM;
        uiManager = uiM;
        partyManager = pm;
        if (invM != null)
            inventoryItems = new Item[InventoryManager.MAXSLOT];
    }

    // Keep old lowercase for backward compat with older code
    public void charInit(VFXManager vfxM, UiManager uiM)
    {
        CharInit(vfxM, uiM);
    }

    protected void MagicCastLogic(Magic magic)
    {
        Character target = curCharTarget.GetComponent<Character>();
        if (target != null) target.ReceiveDamage(magic.Power);
    }

    private IEnumerator ShootMagicCast(Magic magic)
    {
        if (vfxManager != null)
        {
            vfxManager.ShootMagic(magic.ShootId, transform.position, curCharTarget.transform.position, magic.ShootTime);
            yield return new WaitForSeconds(magic.ShootTime);
            MagicCastLogic(magic);
            isMagicMode = false;
            SetState(CharState.Idle);
        }
    }

    private IEnumerator LoadMagicCast(Magic magic)
    {
        if (vfxManager != null)
        {
            vfxManager.LoadMagic(magic.LoadId, transform.position, magic.LoadTime);
            yield return new WaitForSeconds(magic.LoadTime);
            StartCoroutine(ShootMagicCast(magic));
        }
    }

    private void MagicCastAction(Magic magic)
    {
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("MagicCast");
        StartCoroutine(LoadMagicCast(magic));
    }

    protected void WalkToMagicCastUpdate()
    {
        if (curCharTarget == null || curMagicCast == null)
        {
            SetState(CharState.Idle);
            return;
        }
        navAgent.SetDestination(curCharTarget.transform.position);
        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);
        if (distance <= curMagicCast.Range)
        {
            navAgent.isStopped = true;
            SetState(CharState.MagicCast);
            MagicCastAction(curMagicCast);
        }
    }
}
