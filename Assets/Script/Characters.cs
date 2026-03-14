using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CharState
{
    Idle, Walk, WalkToEnemy, Attack,WalkToMagicCast,MagicCast, Hit, Die
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
    public int CurHP { get { return curHP; } }

    [SerializeField] protected Character curCharTarget;
    public Character CurCharTarget { get { return curCharTarget; } set { curCharTarget = value; } }

    [SerializeField] protected float attackRange = 2f;
    public float AttackRange
    {
        get { return attackRange; }
    }
    [SerializeField] protected int attackDamage = 3;
    [SerializeField] protected float attackCoolDown = 2f;
    [SerializeField] protected float attackTimer = 0f;
    [SerializeField] protected float findingRange = 20f;
    public float FindingRange { get { return findingRange; } }
    [SerializeField] protected List<Magic> magicSkills = new List<Magic>();
    public List<Magic> MagicSkills { get { return magicSkills; } set { magicSkills = value; }}
    [SerializeField] protected Magic curMagicCast = null;
    public Magic CurMagicCast {get { return curMagicCast; } set { curMagicCast = value; }}
    
    [SerializeField] protected bool isMagicMode = false;
    public bool IsMagicMode {get { return isMagicMode; } set { isMagicMode = value; }}

    protected VFXManager vfxManager;
    protected UiManager uiManager;

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
        if(ringSelection != null) ringSelection.SetActive(flag);
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
        SetState(CharState.WalkToEnemy);

        if (isMagicMode)
        {
            SetState(CharState.WalkToMagicCast);
        }
        else
        {
            SetState(CharState.WalkToEnemy);
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
        curHP -= damage;
        if (curHP <= 0) { curHP = 0; Die(); }
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

    public void charInit(VFXManager vfxM, UiManager uiM)
    {
        vfxManager = vfxM;
        uiManager = uiM;
    }

    public void ReceiveDamge(int damage)
    {
        if (curHP <= 0 || state == CharState.Die)
        {
         return;           
        }
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            Die();
        }
    
    }

    protected void MagicCastLogic(Magic magic)
    {
        Character target = curCharTarget.GetComponent<Character>();
        if (target != null)
        {
            target.ReceiveDamage(magic.Power);
        }

    }

    private IEnumerator ShootMagicCast(Magic curMagicCast)
    {
        if (vfxManager != null)
        {
            vfxManager.ShootMagic(curMagicCast.ShootId,transform.position,curCharTarget.transform.position,curMagicCast.ShootTime);

            yield return new WaitForSeconds(curMagicCast.ShootTime);
            
            MagicCastLogic(curMagicCast);
            isMagicMode = false;
            
            SetState(CharState.Idle);
        }
    }

    private IEnumerator LoadMagicCast(Magic curMagicCasts)
    {
        if (vfxManager != null)
        {
            vfxManager.LoadMagic(curMagicCasts.LoadId,transform.position,curMagicCasts.LoadTime);

            yield return new WaitForSeconds(curMagicCasts.LoadTime);

            StartCoroutine(ShootMagicCast(curMagicCasts));
        }
    }

    private void MagicCast(Magic curMagicCast)
    {
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("MagicCast");   
        StartCoroutine(LoadMagicCast(curMagicCast));
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
            MagicCast(curMagicCast);
        }
    }
    
    

}