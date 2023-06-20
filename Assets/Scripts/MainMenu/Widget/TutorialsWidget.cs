using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialsWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private ChapterEntry entry;
    [SerializeField] private EntryLayoutElement entryContainer;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        entryContainer.ClearEntries();

        int tIndex = 0;
        foreach (Stage level in OfflineGameplaySubsystem.Instance.tutorialChapter.stages)
        {
            tIndex++;

            ChapterEntry newEntry = Instantiate(entry);
            newEntry.InitData(level.title, $"Tutorial #{tIndex}", level.banner, () =>
            {
                OpenTutorial(level);
            });
            newEntry.ToggleLock(false);

            entryContainer.AddEntry(newEntry.gameObject);
        }
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        entryContainer.ClearEntries();
    }

    private void OpenTutorial(Stage tutorialLevel)
    {
        Debug.Log($"Open Tutorial: {tutorialLevel.stage_id}");

        GameplayStatics.currentChapter = OfflineGameplaySubsystem.Instance.tutorialChapter.chapterInfo;
        GameplayStatics.currentStage = tutorialLevel;

        MenuManager.Instance.SwitchWidget(MenuManager.MenuType.TUTORIAL_DETAILS);
    }
}