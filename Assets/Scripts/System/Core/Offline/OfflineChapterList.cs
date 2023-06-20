using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Offline Chapter List", menuName = "CodePlay/Offline/New Offline Chapter List", order = 1)]
public class OfflineChapterList : ScriptableObject
{
    public List<OfflineChapter> chapters;
}