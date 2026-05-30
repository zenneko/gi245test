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
        if (!Settings.isWarp || PartyManager.instance == null) return;

        // W14: heroes carry state via DontDestroyOnLoad — refresh per-scene refs + drop stale targets
        foreach (Character m in PartyManager.instance.Members)
        {
            if (m == null) continue;
            m.RefreshManagers(VFXManager.instance, UiManager.instance,
                              PartyManager.instance, InventoryManager.instance);
            // Drop any references to objects from the previous scene
            m.CurCharTarget = null;
            m.CurNpcTarget = null;
            m.CurHeroInvite = null;
            m.SetState(CharState.Idle);
        }
        PlaceHeroesAtEnterPoint(Settings.enterPointId);
        // Clear party selection too (avatars / SelectChars may reference destroyed UI)
        PartyManager.instance.SelectChars.Clear();
    }

    private void PlaceHeroesAtEnterPoint(int pointId)
    {
        if (enterPoints == null || pointId >= enterPoints.Length) return;
        Transform spawnPoint = enterPoints[pointId];
        for (int i = 0; i < PartyManager.instance.Members.Count; i++)
        {
            Character hero = PartyManager.instance.Members[i];
            if (hero == null) continue;
            Vector3 offset = new Vector3(i * 1.5f, 0, 0);

            // NavMeshAgent needs to be warped (Disable→Move→Enable, or use agent.Warp)
            UnityEngine.AI.NavMeshAgent agent = hero.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(spawnPoint.position + offset);
            else hero.transform.position = spawnPoint.position + offset;
        }
    }

    public static void WarpTo(string sceneName, int enterPointId = 0, int bgmId = -1)
    {
        Settings.isWarp = true;
        Settings.enterPointId = enterPointId;
        // Switch BGM at warp time (-1 = keep current). AudioManager persists across scenes.
        if (bgmId >= 0 && AudioManager.instance != null)
            AudioManager.instance.PlayBGM(bgmId);
        SceneManager.LoadScene(sceneName);
    }
}
