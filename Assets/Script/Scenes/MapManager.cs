using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    // EnterPoints: where heroes spawn when arriving from another scene
    [SerializeField] private Transform[] enterPoints;

    public static MapManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (Settings.isWarp && PartyManager.instance != null)
        {
            PartyManager.instance.LoadAllHeroData();
            PlaceHeroesAtEnterPoint(Settings.enterPointId);
        }
    }

    private void PlaceHeroesAtEnterPoint(int pointId)
    {
        if (enterPoints == null || pointId >= enterPoints.Length) return;
        Transform spawnPoint = enterPoints[pointId];
        for (int i = 0; i < PartyManager.instance.Members.Count; i++)
        {
            Vector3 offset = new Vector3(i * 1.5f, 0, 0);
            PartyManager.instance.Members[i].transform.position = spawnPoint.position + offset;
        }
    }

    public static void WarpTo(string sceneName, int enterPointId = 0)
    {
        if (PartyManager.instance != null)
            PartyManager.instance.SaveAllHeroData();
        Settings.isWarp = true;
        Settings.enterPointId = enterPointId;
        SceneManager.LoadScene(sceneName);
    }
}
