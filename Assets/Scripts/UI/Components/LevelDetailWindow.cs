using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelDetailWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text txtLevel;
    [SerializeField] private Image[] stars;
    [SerializeField] private Color activeStarColor;
    [SerializeField] private Color disabledStarColor;
    [SerializeField] private Image imgLevelCard;
    [SerializeField] private TMP_Text txtIsSolved;
    [SerializeField] private Image imgIsSolvedCard;
    [SerializeField] private TMP_Text txtFastestTime;
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnClose;
    [SerializeField] private LeaderboardWindow leaderboardWindow;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color solvedColor;

    private void Awake()
    {
        btnPlay.onClick.AddListener(PlayLevel);
        btnClose.onClick.AddListener(CloseWindow);
    }

    private void OnEnable()
    {
        LoadLevelDetail();
    }

    private void LoadLevelDetail()
    {
        if (GameplayStatics.IsOnline)
        {
            PromptSubsystem.Instance.ShowLoading();
            leaderboardWindow.gameObject.SetActive(true);
        } 
        else
        {
            leaderboardWindow.gameObject.SetActive(false);
        }

        NetworkingSubsystem.Instance.GetStage(
            GameplayStatics.currentPlayer.player_id,
            GameplayStatics.currentStage.stage_id,
            (bool isSuccessful, string message, GetStageResponse result) =>
            {
                if (isSuccessful)
                {
                    SetLevelDetails(result.stage);
                    leaderboardWindow.InitLeaderboard(GameplayStatics.currentStage.stage_id);
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

    private void SetLevelDetails(Stage level)
    {
        bool tIsCompleted = level.is_solved != 0;

        txtLevel.text = PlayerPrefs.GetString(GameplayStatics.LEVEL_DISPLAY_INDEX);
        txtFastestTime.text = $"Waktu Tercepat: {(tIsCompleted ? UtilitySubsystem.FormatTime(level.fastest_time) : "--:--:--")}";
        imgIsSolvedCard.color = tIsCompleted ? solvedColor : defaultColor;
        txtIsSolved.text = tIsCompleted ? "Selesai" : "Belum Selesai";

        // Display Stars
        int tStarsScore = level.stars;
        foreach (Image tStar in stars) tStar.color = disabledStarColor;
        for (int i = 0; i < tStarsScore; i++) stars[i].color = activeStarColor;
    }

    private void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    private void PlayLevel()
    {
        Debug.Log($"Play level {GameplayStatics.currentStage.stage_id}");
        SceneLoader.LoadScene(GameplayStatics.currentStage.stage_id);
    }
}
