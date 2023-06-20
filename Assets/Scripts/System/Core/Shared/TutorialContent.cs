using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Tutorial Content", menuName = "CodePlay/Offline/New Tutorial Content", order = 2)]
[System.Serializable]
public class TutorialContent : ScriptableObject
{
    [Header("Tutorial Content")]
    public GameObject tutorialArticle = null;
}