public static class Settings
{
    public const string VILLAGE_SCENE = "VillageScene";
    public const string DUNGEON_SCENE = "DungeonScene";
    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string SELECT_CHAR_SCENE = "SelectChar";

    // W13: index of hero chosen in SelectChar scene
    public static int selectedHeroIndex = 0;

    // W14: set to true when warping between scenes
    public static bool isWarp = false;
    public static int enterPointId = 0;
}
