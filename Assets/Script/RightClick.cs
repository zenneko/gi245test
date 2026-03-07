using System.Collections.Generic;
using UnityEngine;

public class RightClick : MonoBehaviour
{
    private Camera cam;
    public LayerMask layerMask;
    public static RightClick instance;

    void Start()
    {
        instance = this;
        cam = Camera.main;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building");
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            TryCommand(Input.mousePosition);
        }
    }

    private void TryCommand(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            switch (hit.collider.tag)
            {
                case "Ground":
                    CommandToWalk(hit, PartyManager.instance.SelectChars);
                    break;
                case "Enemy":
                    CommandToAttack(hit, PartyManager.instance.SelectChars);
                    break;
            }
        }
    }

    private void CommandToWalk(RaycastHit hit, List<Character> heroes)
    {
        foreach (Character h in heroes)
        {
            if (h != null) h.WalkToPosition(hit.point);
        }
    }

    private void CommandToAttack(RaycastHit hit, List<Character> heroes)
    {
        Character target = hit.collider.GetComponent<Character>();
        foreach (Character h in heroes)
        {
            if (h != null) h.ToAttackCharacter(target);
        }
    }
}