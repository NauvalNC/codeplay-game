using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ChapterEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtSubtitle;
    [SerializeField] private Image imgBanner;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject requiredStarsObj;
    [SerializeField] private TMP_Text txtRequiredStars;
    [SerializeField] private TMP_Text txtLockMessage;

    #region Helpers
    private bool isLocked = true;
    public bool IsLocked { get { return isLocked; } }

    private Button btnEntry;
    public delegate void OnChapterEntryClicked();
    #endregion

    private void Awake()
    {
        btnEntry = GetComponent<Button>();
        ToggleComplete(false);
    }

    public void InitData(Chapter chapter, string subtitle, OnChapterEntryClicked onClicked)
    {
        ToggleComplete(chapter.is_solved != 0);
        InitData(chapter.title, subtitle, chapter.banner, onClicked);
    }

    public void InitData(string title, string subtitle, string banner, OnChapterEntryClicked onClicked)
    {
        txtTitle.text = title;
        txtSubtitle.text = subtitle;

        Texture2D tTexture = Resources.Load<Texture2D>(banner);
        if (tTexture)
        {
            Sprite tImage = Sprite.Create(tTexture, new Rect(0, 0, tTexture.width, tTexture.height), Vector2.zero);
            imgBanner.color = Color.white;
            imgBanner.sprite = tImage;
            imgBanner.preserveAspect = true;
            imgBanner.SetNativeSize();
        }

        btnEntry.onClick.RemoveAllListeners();
        btnEntry.onClick.AddListener(() =>
        {
            if (!isLocked) onClicked.Invoke();
        });
    }

    public void SetStarsRequirement(int minStars)
    {
        requiredStarsObj.SetActive(true);
        txtRequiredStars.text = $"{minStars}";
        txtLockMessage.text = $"Bintang Diperlukan";
    }

    public void SetDefaultRequirement()
    {
        requiredStarsObj.SetActive(false);
        txtLockMessage.text = $"Selesaikan\nWorld Sebelumnya";
    }

    public void ToggleLock(bool isLocked)
    {
        this.isLocked = isLocked;
        lockIcon.gameObject.SetActive(isLocked);
    }

    public void ToggleComplete(bool isComplete)
    {
        ToggleLock(!isComplete);
    }
}
