#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DialogueSystem
{
    public abstract class Builder
    {
        private int allocatedID = 0;

        public abstract void GenerateJSON(string filePath);

        public int GetNextID()
        {
            int lastAlocatedID = allocatedID;

            // Allocate new ID
            allocatedID++;

            // Return last allocated ID
            return lastAlocatedID;
        }
    }

    [CustomEditor(typeof(MonoScript))]
    public class BuilderEditor : Editor
    {
        private MonoScript script = null;
        private Builder builder = null;

        [HideInInspector] private string fileName = "";
        [HideInInspector] private string savePath = "";
        [HideInInspector] private bool overwrite = true;

        private void OnEnable()
        {
            script = (MonoScript)target;
        }

        public override void OnInspectorGUI()
        {
            if (script.GetClass().IsSubclassOf(typeof(Builder)))
            {
                DrawInspector();
            } else
            {
                DrawDefaultInspector();
            }
        }

        void DrawInspector()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Save Path based on Asset Folder", EditorStyles.boldLabel);

            fileName = EditorGUILayout.TextField("File Name", fileName);
            GUILayout.Label("Ex: filename_1", EditorStyles.miniLabel);

            savePath = EditorGUILayout.TextField("Save Path", savePath);
            GUILayout.Label("Ex: example_folder/", EditorStyles.miniLabel);

            overwrite = EditorGUILayout.Toggle("Overwrite", overwrite);
            GUILayout.Label("Allow to overwrite file or not.", EditorStyles.miniLabel);

            if (GUILayout.Button("Generate JSON File"))
            {
                builder = (Builder) System.Activator.CreateInstance(script.GetClass());

                try
                {
                    string path = Application.dataPath + "/" + savePath + fileName + ".json";
                    if (System.IO.File.Exists(path) && overwrite == false)
                    {
                        Debug.LogError("JSON file is already exists, abort at: " + path);
                    }
                    else
                    {
                        builder.GenerateJSON(path);
                        AssetDatabase.Refresh();

                        Debug.Log("JSON file generated successfuly at: " + path);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to generate JSON file: " + e.Message);
                }
            }

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

#endif