using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialDetailsWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private Image imgBanner;
    [SerializeField] private EntryLayoutElement articleContainer;
    [SerializeField] private ScrollRect scrollRect;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        txtTitle.text = GameplayStatics.currentStage.title;
        btnPlay.gameObject.SetActive(GameplayStatics.currentStage.stage_id != OfflineGameplaySubsystem.TUTORIAL_HOWTO_STAGE_CODENAME);

        Texture2D tTexture = Resources.Load<Texture2D>(GameplayStatics.currentStage.banner);
        if (tTexture)
        {
            Sprite tImage = Sprite.Create(tTexture, new Rect(0, 0, tTexture.width, tTexture.height), Vector2.zero);
            imgBanner.color = Color.white;
            imgBanner.sprite = tImage;
            imgBanner.preserveAspect = true;
            imgBanner.SetNativeSize();
        }

        articleContainer.ClearEntries();
        GameObject tArticle = Instantiate(GameplayStatics.currentStage.tutorialContent.tutorialArticle);
        articleContainer.AddEntry(tArticle);

        scrollRect.verticalNormalizedPosition = 1f;
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnPlay.onClick.AddListener(PlayTutorial);
    }

    private void PlayTutorial()
    {
        Debug.Log($"Play Tutorial Level {GameplayStatics.currentStage.stage_id}");
        SceneLoader.LoadScene(GameplayStatics.currentStage.stage_id);
    }
}