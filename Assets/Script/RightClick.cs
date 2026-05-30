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
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "NPC");
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
            TryCommand(Input.mousePosition);
    }

    private void TryCommand(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 1000, layerMask)) return;

        switch (hit.collider.tag)
        {
            case "Ground":
                CommandToWalk(hit, PartyManager.instance.SelectChars);
                break;
            case "Enemy":
                CommandToAttack(hit, PartyManager.instance.SelectChars);
                break;
            case "NPC":
                CommandToWalkToNpc(hit, PartyManager.instance.SelectChars);
                break;
            case "Hero":
                CommandToWalkToHero(hit, PartyManager.instance.SelectChars);
                break;
        }
    }

    private void CommandToWalk(RaycastHit hit, List<Character> heroes)
    {
        // Show move marker VFX
        if (VFXManager.instance != null && VFXManager.instance.DoubleRingMarker != null)
        {
            GameObject marker = Instantiate(VFXManager.instance.DoubleRingMarker,
                hit.point, Quaternion.identity);
        }
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

    // W10
    private void CommandToWalkToNpc(RaycastHit hit, List<Character> heroes)
    {
        Npc npc = hit.collider.GetComponent<Npc>();
        if (npc == null) return;
        foreach (Character h in heroes)
        {
            if (h != null) h.WalkToNpc(npc);
        }
    }

    // W13: invite a wandering hero to join the party
    private void CommandToWalkToHero(RaycastHit hit, List<Character> heroes)
    {
        Hero targetHero = hit.collider.GetComponent<Hero>();
        if (targetHero == null) return;
        // Don't walk to a hero that's already in the party
        if (PartyManager.instance.Members.Contains(targetHero)) return;
        foreach (Character h in heroes)
        {
            if (h != null) h.WalkToHero(targetHero);
        }
    }
}
