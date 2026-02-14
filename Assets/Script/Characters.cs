using UnityEngine;
using UnityEngine.AI;

public enum CharState
{
    Idle,
    Walk,
    Attack,
    Hit,
    Die,
    Cast
    
}   

public abstract class Characters : MonoBehaviour
{
    protected NavMeshAgent navMeshAgent;

    protected Animator anim;

    public Animator Anim { get { return anim; } }
    [SerializeField] protected CharState state;
    public CharState State
    {
        get { return state; }
    }
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    public void SetState(CharState s)
    {
        state = s;
    }
}




