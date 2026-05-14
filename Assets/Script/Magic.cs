using UnityEngine;

[System.Serializable]
public class Magic
{
    [SerializeField] private int id;
    public int ID { get { return id; } }
    [SerializeField] private string name;
    public string Name { get { return name; } }
    [SerializeField] private float range;
    public float Range { get { return range; } }
    [SerializeField] private int power;
    public int Power { get { return power; } }
    [SerializeField] private float loadTime;
    public float LoadTime { get { return loadTime; } }
    [SerializeField] private float shootTime;
    public float ShootTime { get { return shootTime; } }
    [SerializeField] private int loadId;
    public int LoadId { get { return loadId; } }
    [SerializeField] private int shootId;
    public int ShootId { get { return shootId; } }
    // W7: icon from ScriptableObject
    [SerializeField] private Sprite icon;
    public Sprite Icon { get { return icon; } }

    // Original constructor
    public Magic(int id, string name, float range, int power, float loadTime, float shootTime, int loadId, int shootId)
    {
        this.id = id;
        this.name = name;
        this.range = range;
        this.power = power;
        this.loadTime = loadTime;
        this.shootTime = shootTime;
        this.loadId = loadId;
        this.shootId = shootId;
    }

    // W7: Constructor from ScriptableObject
    public Magic(MagicData data)
    {
        this.id = data.id;
        this.name = data.magicName;
        this.range = data.range;
        this.power = data.power;
        this.loadTime = data.loadTime;
        this.shootTime = data.shootTime;
        this.loadId = data.loadId;
        this.shootId = data.shootId;
        this.icon = data.icon;
    }
}
