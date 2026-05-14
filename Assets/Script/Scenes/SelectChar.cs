using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectChar : MonoBehaviour
{
    public void StartGame()
    {
        Settings.isWarp = false;
        SceneManager.LoadScene(Settings.VILLAGE_SCENE);
    }

    public void Back()
    {
        SceneManager.LoadScene(Settings.MAIN_MENU_SCENE);
    }
}
