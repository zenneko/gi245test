using UnityEngine;

public class CharAnimation : MonoBehaviour
{
    private Character character;

    void Start()
    {
        character = GetComponent<Character>();
    }

    void Update()
    {
        ChooseAnimation(character);
    }

    private void ChooseAnimation(Character c)
    {
        c.Anim.SetBool("IsIdle", false);
        c.Anim.SetBool("IsWalk", false);

        switch (c.State)
        {
            case CharState.Idle:
                c.Anim.SetBool("IsIdle", true);
                break;
            case CharState.Walk:
            case CharState.WalkToEnemy:
            case CharState.WalkToMagicCast:
                c.Anim.SetBool("IsWalk", true);
                break;
        }
    }
}