using System.Collections.Generic;
using UnityEngine;
using DialogueSystem;

[CreateAssetMenu(fileName = "New Gameplay Config", menuName = "CodePlay/Code Block/New Gameplay Config", order = 1)]
public class GameplayConfig : ScriptableObject
{
    [Header("Game Configuration")]
    public bool isTutorialGameplay = false;
    public int blockCodeLimit;
    public CodeTemplate codeTemplate;
    public StarsRequirement starsRequirement;

    [Header("Story Configuration")]
    public StoryPackage startStory;
    public StoryPackage endStory;

    [Header("Connection Configuration")]
    public string nextLevelId;
}

[System.Serializable]
public struct CodeTemplate
{
    public List<GameObject> actionCodes;
    public List<GameObject> coreCodes;
    public List<GameObject> variableCodes;
};

[System.Serializable]
public class StarsRequirement
{
    public int twoStars;
    public int threeStars;

    public int GetTotalStars(int totalCodeBlocks)
    {
        if (totalCodeBlocks <= threeStars)
        {
            return 3;
        } 
        else if (totalCodeBlocks <= twoStars)
        {
            return 2;
        } 
        else
        {
            return 1;
        }
    }
};