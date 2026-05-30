using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeftClick : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private RectTransform boxSelection;

    private Vector2 oldAnchoredPos;
    private Vector2 startPos;
    private bool isDragging = false;

    public static LeftClick instance;

    void Start()
    {
        instance = this;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "Item");
        AcquireRefs();
    }

    // Re-acquire camera + box selection — needed if the previous refs got destroyed across a scene load
    private void AcquireRefs()
    {
        if (cam == null) cam = Camera.main;
        if (boxSelection == null && UiManager.instance != null) boxSelection = UiManager.instance.SelectionBox;
    }

    void Update()
    {
        // Guard against stale refs after warp / scene reload
        if (cam == null || boxSelection == null) { AcquireRefs(); if (cam == null) return; }

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = false;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // Only clear selection if we're NOT clicking on something that needs it
            // (heroes — to add to selection; items — to pick up using the current selection)
            if (!IsClickOnPickable(Input.mousePosition))
                ClearEverything();
        }

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // only start drawing box after a minimum drag distance
            if (Vector2.Distance(Input.mousePosition, startPos) > 5f)
            {
                isDragging = true;
                UpdateSelectionBox(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
                ReleaseSelectionBox(Input.mousePosition);
            else
                TrySelect(Input.mousePosition);

            isDragging = false;
            if (boxSelection != null) boxSelection.gameObject.SetActive(false);
        }
    }

    // W12: fix — prevent adding duplicate characters
    private void TrySelect(Vector2 screenPos)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 1000, layerMask)) return;

        switch (hit.collider.tag)
        {
            case "Player":
            case "Hero":
                SelectCharacter(hit);
                break;
            case "Item":           // W14 §46: pickup dropped item
                SelectItem(hit);
                break;
        }
    }

    // W14 §46: route click on a dropped item to its ItemPick (selection preserved)
    private void SelectItem(RaycastHit hit)
    {
        ItemPick pick = hit.collider.GetComponent<ItemPick>();
        if (pick == null) return;
        if (PartyManager.instance.SelectChars.Count <= 0) return;
        pick.PickUpItem(PartyManager.instance.SelectChars[0]);
    }

    // Used at MouseDown to decide whether to keep or clear the current selection
    private bool IsClickOnPickable(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 1000, layerMask)) return false;
        string t = hit.collider.tag;
        return t == "Hero" || t == "Player" || t == "Item";
    }

    // W12: fix — check for duplicate before adding
    private void SelectCharacter(RaycastHit hit)
    {
        Character hero = hit.collider.GetComponent<Character>();
        if (hero == null || hero.CurHP <= 0) return;
        if (!PartyManager.instance.SelectChars.Contains(hero))
        {
            PartyManager.instance.SelectChars.Add(hero);
            hero.ToggleRingSelection(true);
        }
        // W11: sync toggle avatar
        SyncToggleAvatar(hero, true);
        UiManager.instance.UpdateCharPanel();
    }

    private void ClearRingSelection()
    {
        foreach (Character h in PartyManager.instance.SelectChars)
            h.ToggleRingSelection(false);
    }

    // W12: fix — also clear avatar toggles
    private void ClearEverything()
    {
        ClearRingSelection();
        PartyManager.instance.SelectChars.Clear();
        // W11: clear all toggles silently
        if (UiManager.instance.ToggleAvatar != null)
        {
            foreach (Toggle t in UiManager.instance.ToggleAvatar)
                if (t != null) t.SetIsOnWithoutNotify(false);
        }
        UiManager.instance.UpdateCharPanel();
    }

    private void UpdateSelectionBox(Vector2 mousePos)
    {
        if (!boxSelection.gameObject.activeInHierarchy)
            boxSelection.gameObject.SetActive(true);

        float width = mousePos.x - startPos.x;
        float height = mousePos.y - startPos.y;
        boxSelection.anchoredPosition = startPos + new Vector2(width / 2f, height / 2f);
        width = Mathf.Abs(width);
        height = Mathf.Abs(height);
        boxSelection.sizeDelta = new Vector2(width, height);
        oldAnchoredPos = boxSelection.anchoredPosition;
    }

    // W12: fix — avoid duplicate adds in box selection
    private void ReleaseSelectionBox(Vector2 mousePos)
    {
        boxSelection.gameObject.SetActive(false);
        Vector2 corner1 = oldAnchoredPos - (boxSelection.sizeDelta / 2f);
        Vector2 corner2 = oldAnchoredPos + (boxSelection.sizeDelta / 2f);

        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.CurHP <= 0) continue;
            Vector2 unitPos = cam.WorldToScreenPoint(member.transform.position);
            bool inBox = unitPos.x > corner1.x && unitPos.x < corner2.x
                      && unitPos.y > corner1.y && unitPos.y < corner2.y;
            if (inBox && !PartyManager.instance.SelectChars.Contains(member))
            {
                PartyManager.instance.SelectChars.Add(member);
                member.ToggleRingSelection(true);
                SyncToggleAvatar(member, true);
            }
        }
        boxSelection.sizeDelta = Vector2.zero;
        UiManager.instance.UpdateCharPanel();
    }

    private void SyncToggleAvatar(Character hero, bool isOn)
    {
        if (UiManager.instance.ToggleAvatar == null) return;
        int idx = PartyManager.instance.FindIndexFromClass(hero);
        if (idx >= 0 && idx < UiManager.instance.ToggleAvatar.Length
            && UiManager.instance.ToggleAvatar[idx] != null)
            UiManager.instance.ToggleAvatar[idx].SetIsOnWithoutNotify(isOn);
    }
}
