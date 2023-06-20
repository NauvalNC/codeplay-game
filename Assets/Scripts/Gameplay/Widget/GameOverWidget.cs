using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameOverWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private Button btnReplay;
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnNextLevel;
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtTimeElapsed;
    [SerializeField] private LeaderboardWindow leaderboardWindow;
    [SerializeField] private Image[] stars;
    [SerializeField] private Color activeStarColor;
    [SerializeField] private Color disabledStarColor;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        SoundManager.Instance.PlaySuccessSound();

        // Display Stars
        int tStarsScore = GameManager.Instance.gameplayConfig.starsRequirement.GetTotalStars(GameManager.Instance.BlockCount);
        foreach (Image tStar in stars) tStar.color = disabledStarColor;
        for (int i = 0; i < tStarsScore; i++) stars[i].color = activeStarColor;

        // Display fastest time.
        bool tNewRecord = tStarsScore > GameplayStatics.currentStage.stars;
        if (!tNewRecord)
        {
            if (GameplayStatics.currentStage.fastest_time <= -1) tNewRecord = true;
            else tNewRecord = (int)GameManager.Instance.elapsedSeconds < GameplayStatics.currentStage.fastest_time;
        }
        txtTitle.text = tNewRecord ? "Rekor Baru!" : "Level Selesai!";
        txtTimeElapsed.text = UtilitySubsystem.FormatTime(GameManager.Instance.elapsedSeconds);

        if (GameManager.Instance.gameplayConfig.isTutorialGameplay || !GameplayStatics.IsOnline)
        {
            leaderboardWindow.gameObject.SetActive(false);
        } else
        {
            leaderboardWindow.gameObject.SetActive(true);
        }

        // No statistic or reward to be updated in tutorial level.
        if (GameManager.Instance.gameplayConfig.isTutorialGameplay)
        {
            return;
        }

        // Update level statistic.
        GameplayStatics.currentStage.fastest_time = (int)GameManager.Instance.elapsedSeconds;
        GameplayStatics.currentStage.is_solved = 1;
        GameplayStatics.currentStage.total_codes = GameManager.Instance.BlockCount;
        GameplayStatics.currentStage.stars = tStarsScore;

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.SetStageStatistic(
            GameplayStatics.currentPlayer.player_id,
            GameplayStatics.currentStage, 
            (bool isSuccessful, string message, SetStageStatisticResponse result) =>
            {
                if (isSuccessful)
                {
                    UnityAction tAction = () => UnlockReward();
                    leaderboardWindow.InitLeaderboard(GameplayStatics.currentStage.stage_id, tAction);
                }
                else
                {
                    PromptSubsystem.Instance.HideLoading();
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.LogWarning(message);
                }
            }
        );
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnReplay.onClick.AddListener(Replay);
        btnHome.onClick.AddListener(LoadHome);
        btnNextLevel.onClick.AddListener(LoadNextLevel);
        btnNextLevel.gameObject.SetActive(!string.IsNullOrEmpty(GameManager.Instance.gameplayConfig.nextLevelId));
    }

    private void Replay()
    {
        SceneLoader.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadNextLevel()
    {
        PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.GetStage(
            GameplayStatics.currentPlayer.player_id,
            GameManager.Instance.gameplayConfig.nextLevelId,
            (bool isSuccessful, string message, GetStageResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    GameplayStatics.currentStage = result.stage;
                    SceneLoader.LoadScene(result.stage.stage_id);
                } 
                else
                {
                    PromptSubsystem.Instance.ShowPopUp(
                        "Gagal Memuat Level", 
                        "Terjadi kegagalan dalam memuat level selanjutnya. Kamu akan dikembalikan ke menu utama.", 
                        PopUpWidget.PopUpType.OK, (PopUpWidget.PopUpResult result) =>
                        {
                            if (result == PopUpWidget.PopUpResult.CONFIRMED) LoadHome();
                        }
                    );
                }
            }
        );
    }

    private void LoadHome()
    {
        SceneLoader.LoadScene(GameplayStatics.MAIN_MENU_SCENE);
    }

    private void UnlockReward()
    {
        if (GameplayStatics.currentStage.stars < 3)
        {
            return;
        }

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.UnlockSkin(
            GameplayStatics.currentPlayer.player_id,
            GameplayStatics.currentStage.stage_id,
            (bool isSuccessful, string message, UnlockSkinResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    PromptSubsystem.Instance.ShowPopUp(
                        "Kostum Baru", 
                        $"Selamat! Kamu mendapatkan kostum \"{result.unlocked_skin.skin_name}\" baru!",
                        result.unlocked_skin.skin_image,
                        PopUpWidget.PopUpType.OK);
                } 
                else
                {
                    Debug.LogWarning(message);
                }
            }
        );
    }
}