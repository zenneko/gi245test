using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // W13: hero prefabs to spawn when starting a new game
    [SerializeField] private GameObject[] heroPrefabs;
    [SerializeField] private Transform spawnPoint;

    // W14 §44: BGM id to play in this scene (only used on fresh start; warps set their own BGM)
    [SerializeField] private int sceneBgmId = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (Settings.isWarp)
        {
            // MapManager.Start() handles hero placement + LoadAllHeroData; BGM was set at warp time
        }
        else
        {
            GeneratePlayerHeroes();
            if (AudioManager.instance != null) AudioManager.instance.PlayBGM(sceneBgmId);
        }
    }

    private void GeneratePlayerHeroes()
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0) return;

        int idx = Mathf.Clamp(Settings.selectedHeroIndex, 0, heroPrefabs.Length - 1);
        if (heroPrefabs[idx] == null) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject heroObj = Instantiate(heroPrefabs[idx], pos, Quaternion.identity);
        DontDestroyOnLoad(heroObj);   // W14: hero survives warps
        Hero hero = heroObj.GetComponent<Hero>();
        if (hero == null) return;

        // Register with party and initialise
        PartyManager.instance.Members.Add(hero);
        hero.CharInit(VFXManager.instance, UiManager.instance,
                      PartyManager.instance, InventoryManager.instance);

        if (VFXManager.instance != null && VFXManager.instance.MagicDataArr != null
            && VFXManager.instance.MagicDataArr.Length > 0)
            hero.MagicSkills.Add(new Magic(VFXManager.instance.MagicDataArr[0]));
        else
            hero.MagicSkills.Add(new Magic(0, "Fireball", 10f, 30, 3f, 1f, 0, 1));

        // Auto-select the hero
        PartyManager.instance.SelectChars.Clear();
        PartyManager.instance.SelectChars.Add(hero);
        hero.ToggleRingSelection(true);
    }

    public void Warp(string sceneName, int enterPointId = 0)
    {
        MapManager.WarpTo(sceneName, enterPointId);
    }

    public void ReturnToMainMenu()
    {
        Settings.isWarp = false;
        SceneManager.LoadScene(Settings.MAIN_MENU_SCENE);
    }
}
