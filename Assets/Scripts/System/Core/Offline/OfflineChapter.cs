using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Offline Chapter", menuName = "CodePlay/Offline/New Offline Chapter", order = 1)]
public class OfflineChapter : ScriptableObject
{
    [Header("Chapter Information")]
    public Chapter chapterInfo;

    [Header("Chapter's Stages")]
    public List<Stage> stages = new List<Stage>();
}

#if UNITY_EDITOR
[CustomEditor(typeof(OfflineChapter))]
public class BuilderEditor : Editor
{
    private OfflineChapter offlineChapter = null;

    private void OnEnable()
    {
        offlineChapter = (OfflineChapter)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset Chapter"))
        {
            foreach (Stage tStage in offlineChapter.stages)
            {
                tStage.is_solved = 0;
                tStage.fastest_time = -1;
            }

            offlineChapter.chapterInfo.is_solved = 0;
        }

        if (GUILayout.Button("Set Complete"))
        {
            foreach (Stage tStage in offlineChapter.stages)
            {
                tStage.is_solved = 1;
                tStage.fastest_time = 0;
            }

            offlineChapter.chapterInfo.is_solved = 1;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif