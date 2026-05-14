using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        Settings.isWarp = false;
        SceneManager.LoadScene(Settings.SELECT_CHAR_SCENE);
    }

    public void ContinueGame()
    {
        Settings.isWarp = false;
        SceneManager.LoadScene(Settings.VILLAGE_SCENE);
    }

    public void LoadGame()
    {
        // placeholder — could load saved data in a future iteration
        ContinueGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
