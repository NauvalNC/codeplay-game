using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Offline Skin List", menuName = "CodePlay/Offline/New Offline Skin List", order = 2)]
public class OfflineSkinList : ScriptableObject
{
    [Header("Offline Skin Cache")]
    public string equippedSkin = "SK-0";

    [Header("Offline Skins")]
    public List<Skin> skins;
}