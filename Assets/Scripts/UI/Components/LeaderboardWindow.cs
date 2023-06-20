using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class LeaderboardWindow : MonoBehaviour
{
    [System.Serializable]
    private struct LeaderboardContent
    {
        [SerializeField] public Button btnTab;
        [SerializeField] public GameObject contentPanel;
        [SerializeField] public EntryLayoutElement entryContainer;
        [SerializeField] public GameObject placeholder;
        [SerializeField] public TMP_Text txtPlaceholder;
        [SerializeField] public ScrollRect scrollRect;
    };

    [SerializeField] private LeaderboardEntry entry;
    [SerializeField] private LeaderboardContent[] leaderboardContent;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color disabledColor;

    public void InitLeaderboard(string levelId, UnityAction callback = null)
    {
        // Reset the leaderboards contents.
        for (int i = 0; i < leaderboardContent.Length; i++)
        {
            int tIndex = i;
            leaderboardContent[i].btnTab.GetComponent<Image>().color = disabledColor;
            leaderboardContent[i].btnTab.onClick.AddListener(() => { SwitchLeaderboard(tIndex); });
            leaderboardContent[i].entryContainer.ClearEntries();
            leaderboardContent[i].placeholder.SetActive(true);
            leaderboardContent[i].txtPlaceholder.text = GameplayStatics.IsOnline ? "Tidak ada data Leaderboard." : "Tidak ada Leaderboard di mode offline.";
        }

        // Switch to default leaderboard.
        SwitchLeaderboard(0);

        // Fetch leaderboard list.
        NetworkingSubsystem.Instance.GetStageLeaderboard(
            GameplayStatics.currentPlayer.player_id, levelId, 
            (bool isSuccessful, string message, GetStageLeaderboardResponse result) =>
            {
                if (isSuccessful)
                {
                    PromptSubsystem.Instance.HideLoading();

                    // Display each type of leaderboards.
                    int tLeaderboardIndex = 0;
                    leaderboardContent[tLeaderboardIndex].placeholder.SetActive(result.leaderboard.Count <= 0);

                    // Display leaderboard entries.
                    foreach (PlayerStageRank tRank in result.leaderboard)
                    {
                        bool tFound = tRank.player_id == GameplayStatics.currentPlayer.player_id;

                        LeaderboardEntry newEntry = Instantiate(entry);
                        newEntry.InitData(tRank, tFound);

                        leaderboardContent[tLeaderboardIndex].entryContainer.AddEntry(newEntry.gameObject);
                    }

                    leaderboardContent[tLeaderboardIndex].scrollRect.verticalNormalizedPosition = 1f;

                    callback?.Invoke();
                }
                else
                {
                    PromptSubsystem.Instance.HideLoading();
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.Log(message);
                }
            }
        );
    }

    private void SwitchLeaderboard(int index)
    {
        for (int i = 0; i < leaderboardContent.Length; i++)
        {
            leaderboardContent[i].btnTab.GetComponent<Image>().color = index == i ? activeColor : disabledColor;
            leaderboardContent[i].contentPanel.SetActive(index == i);
        }
    }
}
