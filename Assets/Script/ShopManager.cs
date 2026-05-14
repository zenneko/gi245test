using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    private Npc curShopNpc;

    void Awake()
    {
        instance = this;
    }

    public void OpenShop(Npc npc)
    {
        curShopNpc = npc;
        if (UiManager.instance != null)
            UiManager.instance.OpenShop(npc);
    }

    public void CloseShop()
    {
        curShopNpc = null;
        if (UiManager.instance != null)
            UiManager.instance.CloseShop();
    }
}
