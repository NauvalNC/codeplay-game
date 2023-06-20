using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text txtRank;
    [SerializeField] private TMP_Text txtUsername;
    [SerializeField] private TMP_Text txtTotalCodes;
    [SerializeField] private TMP_Text txtTime;
    [SerializeField] private Image imgEntry;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;

    public void InitData(PlayerStageRank rank, bool isHighlighted = false)
    {
        txtRank.text = rank.rank.ToString();
        txtUsername.text = rank.username;
        txtTotalCodes.text = $"| {rank.total_codes} Kode";
        txtTime.text = $"| {UtilitySubsystem.FormatTime(rank.fastest_time)}";
        imgEntry.color = isHighlighted ? highlightColor : defaultColor;
    }
}
