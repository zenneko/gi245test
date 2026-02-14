using System;
using UnityEngine;

public class CharAnimation : MonoBehaviour
{
    private Characters character;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        character = GetComponent<Characters>();
    }

    private void ChooseAnimation(Characters c)
    {
        c.Anim.SetBool("IsIdle",false);
        c.Anim.SetBool("IsWalk", false);

        switch (c.State)
        {
            case CharState.Idle:
                { 
                    c.Anim.SetBool("IsIdle",true); 
                    break;
                }
            case CharState.Walk:
            { 
                c.Anim.SetBool("IsWalk",true); 
                break;
            }
               
        }
    }

    private void Update()
    {
        ChooseAnimation(character);
    }
}
