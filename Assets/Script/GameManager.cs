using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (Settings.isWarp)
        {
            // MapManager.Start() handles hero placement + LoadAllHeroData
        }
        else
        {
            GeneratePlayerHeroes();
        }
    }

    private void GeneratePlayerHeroes()
    {
        // On fresh game start, PartyManager.Start() already calls CharInit for all members
        // This method is a hook for any additional fresh-start setup
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
