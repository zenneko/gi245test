using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private List<Character> members = new List<Character>();
    public List<Character> Members { get { return members; } }

    [SerializeField] private List<Character> selectChars = new List<Character>();
    public List<Character> SelectChars { get { return selectChars; } }

    // W12: party gold
    [SerializeField] private int partyGold = 100;
    public int PartyGold { get { return partyGold; } set { partyGold = value; } }

    // W14: cross-scene hero persistence
    [SerializeField] private HeroData[] heroData;
    public HeroData[] HeroDataArr { get { return heroData; } }

    public static PartyManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (!Settings.isWarp)
        {
            foreach (Character c in members)
            {
                c.CharInit(VFXManager.instance, UiManager.instance, this, InventoryManager.instance);
                // W7: use MagicData SO if available, else hardcode Fireball
                if (VFXManager.instance != null && VFXManager.instance.MagicDataArr != null
                    && VFXManager.instance.MagicDataArr.Length > 0)
                    c.MagicSkills.Add(new Magic(VFXManager.instance.MagicDataArr[0]));
                else
                    c.MagicSkills.Add(new Magic(0, "Fireball", 10f, 30, 3f, 1f, 0, 1));
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (selectChars.Count > 0)
            {
                selectChars[0].IsMagicMode = true;
                selectChars[0].CurMagicCast = selectChars[0].MagicSkills[0];
            }
        }
    }

    public void SelectSingleHero(int i)
    {
        foreach (Character c in selectChars) c.ToggleRingSelection(false);
        selectChars.Clear();
        if (i >= 0 && i < members.Count)
        {
            selectChars.Add(members[i]);
            members[i].ToggleRingSelection(true);
        }
    }

    public void HeroSelectMagicSkill(int i)
    {
        if (selectChars.Count <= 0) return;
        selectChars[0].IsMagicMode = true;
        selectChars[0].CurMagicCast = selectChars[0].MagicSkills[i];
    }

    // W11
    public int FindIndexFromClass(Character c)
    {
        return members.IndexOf(c);
    }

    public void SelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count) return;
        Character hero = members[i];
        if (!selectChars.Contains(hero))
        {
            selectChars.Add(hero);
            hero.ToggleRingSelection(true);
        }
    }

    public void UnSelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count) return;
        Character hero = members[i];
        selectChars.Remove(hero);
        hero.ToggleRingSelection(false);
    }

    // W13
    public void DistributeExp(int totalExp)
    {
        if (members.Count <= 0) return;
        int expEach = Mathf.Max(1, totalExp / members.Count);
        foreach (Character c in members)
        {
            Hero hero = c as Hero;
            if (hero != null && hero.CurHP > 0)
                hero.ReceiveExp(expEach);
        }
    }

    // W14
    public void SaveAllHeroData()
    {
        if (heroData == null) return;
        for (int i = 0; i < members.Count && i < heroData.Length; i++)
        {
            Hero hero = members[i] as Hero;
            if (hero != null && heroData[i] != null)
                heroData[i].SaveFrom(hero);
        }
    }

    public void LoadAllHeroData()
    {
        if (heroData == null) return;
        for (int i = 0; i < members.Count && i < heroData.Length; i++)
        {
            Hero hero = members[i] as Hero;
            if (hero == null || heroData[i] == null) continue;

            hero.CharInit(VFXManager.instance, UiManager.instance, this, InventoryManager.instance);
            heroData[i].LoadTo(hero);

            if (VFXManager.instance != null && VFXManager.instance.MagicDataArr != null
                && VFXManager.instance.MagicDataArr.Length > 0)
                hero.MagicSkills.Add(new Magic(VFXManager.instance.MagicDataArr[0]));
            else
                hero.MagicSkills.Add(new Magic(0, "Fireball", 10f, 30, 3f, 1f, 0, 1));
        }
    }
}
