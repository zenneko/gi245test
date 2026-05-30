using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    [SerializeField] private string targetScene = Settings.DUNGEON_SCENE;
    [SerializeField] private int targetEnterPointId = 0;
    [SerializeField] private int destinationBgmId = -1;   // BGM to play at the destination (-1 = keep current)

    // Requires Capsule Collider (Is Trigger = true) + Rigidbody (Is Kinematic = true) on Prefab
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hero") && !other.CompareTag("Player")) return;
        // Only trigger once when any hero enters
        MapManager.WarpTo(targetScene, targetEnterPointId, destinationBgmId);
    }
}
