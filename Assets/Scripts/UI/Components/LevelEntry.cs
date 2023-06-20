using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text txtLevel;
    [SerializeField] private GameObject levelInfoObj;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image imgEntry;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color solvedColor;
    [SerializeField] private Image[] stars;
    [SerializeField] private Color activeStarColor;
    [SerializeField] private Color disabledStarColor;

    #region Helpers
    private bool isLocked = true;
    public bool IsLocked { get { return isLocked; } }

    private Button btnEntry;
    public delegate void OnLevelEntryClicked(Stage level);
    #endregion

    private void Awake()
    {
        btnEntry = GetComponent<Button>();
        ToggleComplete(false);
    }

    public void InitData(Stage level, string levelDisplayName, OnLevelEntryClicked onClicked)
    {
        ToggleComplete(level.is_solved != 0);

        txtLevel.text = levelDisplayName;

        // Display Stars
        int tStarsScore = level.stars;
        foreach (Image tStar in stars) tStar.color = disabledStarColor;
        for (int i = 0; i < tStarsScore; i++) stars[i].color = activeStarColor;

        btnEntry.onClick.RemoveAllListeners();
        btnEntry.onClick.AddListener(() =>
        {
            if (!isLocked) onClicked.Invoke(level);
        });
    }

    public void ToggleLock(bool isLocked)
    {
        this.isLocked = isLocked;
        levelInfoObj.SetActive(!isLocked);
        lockIcon.SetActive(isLocked);
    }

    public void ToggleComplete(bool isComplete)
    {
        ToggleLock(!isComplete);
        imgEntry.color = isComplete ? solvedColor : defaultColor;
    }
}