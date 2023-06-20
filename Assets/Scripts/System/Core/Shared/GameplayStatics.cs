using UnityEngine;

public class GameplayStatics
{
    public static readonly bool FORCE_OFFLINE = false;
    public static readonly string LEVEL_DISPLAY_INDEX = "LEVEL_DISPLAY_INDEX";
    public static readonly string ONLINE_GAMEPLAY = "ONLINE_GAMEPLAY";
    public static readonly string MAIN_MENU_SCENE = "MainMenu";

    public static bool IsOnline 
    { 
        get 
        {
            return PlayerPrefs.GetInt(ONLINE_GAMEPLAY, 1) != 0; 
        }
    }

    public static PublicPlayerInfo currentPlayer = OfflineGameplaySubsystem.offlinePlayer;

    public static Chapter currentChapter = new Chapter();
    public static Stage currentStage = new Stage();

    public static bool isLoggedIn = false;

    public static void SetGameplayOnlineMode(bool isOnline)
    {
        // Load offline player data.
        currentPlayer = OfflineGameplaySubsystem.offlinePlayer;
        currentPlayer.equiped_skin = OfflineGameplaySubsystem.Instance.offlineSkinList.equippedSkin;

        PlayerPrefs.SetInt(ONLINE_GAMEPLAY, isOnline ? 1 : 0);
    }
}