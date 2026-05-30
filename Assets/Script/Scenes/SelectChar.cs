using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SelectChar : MonoBehaviour
{
    [SerializeField] private GameObject[] heroPrefabs;     // drag hero prefabs here
    [SerializeField] private Image charImage;              // profile portrait
    [SerializeField] private TextMeshProUGUI charNameText; // hero name

    // ── Stat ──────────────────────────────────────────────────────────────────
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI dexterityText;
    [SerializeField] private TextMeshProUGUI constitutionText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI wisdomText;
    [SerializeField] private TextMeshProUGUI charismaText;

    [SerializeField] private int curId = 0;

    void Start()
    {
        ShowHero(curId);
    }

    private void ShowHero(int index)
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0) return;
        index = Mathf.Clamp(index, 0, heroPrefabs.Length - 1);
        GameObject prefab = heroPrefabs[index];
        if (prefab == null) return;

        Hero hero = prefab.GetComponent<Hero>();
        if (hero == null) return;

        if (charNameText != null)
            charNameText.text = hero.CharName != "" ? hero.CharName : prefab.name;
        if (charImage != null && hero.ProfileSprite != null)
            charImage.sprite = hero.ProfileSprite;

        SetStat(strengthText,     "STR", hero.Strength);
        SetStat(dexterityText,    "DEX", hero.Dexterity);
        SetStat(constitutionText, "CON", hero.Constitution);
        SetStat(intelligenceText, "INT", hero.Intelligence);
        SetStat(wisdomText,       "WIS", hero.Wisdom);
        SetStat(charismaText,     "CHA", hero.Charisma);
    }

    private static void SetStat(TextMeshProUGUI label, string name, int value)
    {
        if (label != null) label.text = $"{name}: {value}";
    }

    public void SelectNextChar()
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0) return;
        curId = (curId + 1) % heroPrefabs.Length;
        ShowHero(curId);
    }

    public void SelectPreviousChar()
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0) return;
        curId = (curId - 1 + heroPrefabs.Length) % heroPrefabs.Length;
        ShowHero(curId);
    }

    public void BeginGame()
    {
        Settings.selectedHeroIndex = curId;
        Settings.isWarp = false;
        SceneManager.LoadScene(Settings.VILLAGE_SCENE);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(Settings.MAIN_MENU_SCENE);
    }
}
