using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaptersWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private TMP_Text txtTotalStars;
    [SerializeField] private ChapterEntry entry;
    [SerializeField] private EntryLayoutElement entryContainer;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        entryContainer.ClearEntries();

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.GetChapters(
            GameplayStatics.currentPlayer.player_id,
            (bool isSuccessful, string message, GetChaptersResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    int tIndex = 0;
                    int tTotalExpectedStars = 0;
                    int tTotalStars = 0;
                    bool tCheckpointFound = false;
                    bool tLastChapterIsSolved = true;
                    foreach (Chapter tChapter in result.chapters)
                    {
                        tIndex++;

                        ChapterEntry tNewEntry = Instantiate(entry);
                        tNewEntry.InitData(tChapter, $"Dunia #{tIndex}", () =>
                        {
                            OpenChapter(tChapter);
                        });

                        // Required stars to enter the new chapter is 75% of total last chapters expected stars.
                        int tMinRequiredStars = tTotalExpectedStars * 75 / 100;
                        tTotalExpectedStars += (tChapter.total_stages * 3);

                        // Add total stars the player currently has.
                        tTotalStars += tChapter.total_stars;

                        // Set new entry unlock requirements messages.
                        if (tLastChapterIsSolved)
                        {
                            tNewEntry.SetStarsRequirement(tMinRequiredStars);
                        } 
                        else
                        {
                            tNewEntry.SetDefaultRequirement();
                        }
                        tLastChapterIsSolved = tChapter.is_solved != 0;

                        // Unlock the new entry if the checkpoint chapter is found.
                        if (!tCheckpointFound && tChapter.is_solved == 0)
                        {
                            tCheckpointFound = true;
                            tNewEntry.ToggleLock(tTotalStars < tMinRequiredStars);
                        }

                        entryContainer.AddEntry(tNewEntry.gameObject);
                    }

                    txtTotalStars.text = $"{tTotalStars}";
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

    private void OpenChapter(Chapter chapter)
    {
        Debug.Log($"Open Chapter: {chapter.chapter_id}");

        GameplayStatics.currentChapter = chapter;
        MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LEVELS);
    }
}