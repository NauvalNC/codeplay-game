using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelsWidget : BaseWidget
{
    private static LevelsWidget instance;
    public static LevelsWidget Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<LevelsWidget>();
            }
            return instance;
        }
    }

    [Header("User Interface")]
    [SerializeField] private TMP_Text txtTotalStars;
    [SerializeField] private LevelEntry entry;
    [SerializeField] private EntryLayoutElement entryContainer;
    [SerializeField] private LevelDetailWindow levelDetailWindow;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        entryContainer.ClearEntries();
        txtTotalStars.text = "0/0";

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.GetStages(
            GameplayStatics.currentPlayer.player_id,
            GameplayStatics.currentChapter.chapter_id,
            (bool isSuccessful, string message, GetStagesResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    int tTotalStars = 0;

                    int tIndex = 0;
                    bool tCheckpointFound = false;
                    foreach(Stage stage in result.stages)
                    {
                        int tLocalIndex = ++tIndex;
                        tTotalStars += stage.stars;

                        LevelEntry newEntry = Instantiate(entry);
                        newEntry.InitData(stage, tLocalIndex.ToString(), (Stage level) =>
                        {
                            DisplayLevelDetail(level, tLocalIndex.ToString());
                        });

                        // Enable the checkpoint of uncompleted level.
                        if (!tCheckpointFound && stage.is_solved == 0)
                        {
                            tCheckpointFound = true;
                            newEntry.ToggleLock(false);
                        }

                        entryContainer.AddEntry(newEntry.gameObject);
                    }

                    txtTotalStars.text = $"{tTotalStars}/{result.stages.Count * 3}";
                } 
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.LogWarning(message);
                }
            }
        );
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        entryContainer.ClearEntries();
    }

    public void DisplayLevelDetail(Stage level, string levelDisplayName)
    {
        Debug.Log($"Open Level: {level.stage_id}");

        GameplayStatics.currentStage = level;
        PlayerPrefs.SetString(GameplayStatics.LEVEL_DISPLAY_INDEX, levelDisplayName);

        levelDetailWindow.gameObject.SetActive(true);
    }
}
