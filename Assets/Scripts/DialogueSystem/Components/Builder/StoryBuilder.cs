#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem { 
    public abstract class StoryBuilder : Builder
    {
        public Story MAIN_STORY;

        public StoryBuilder()
        {
            MAIN_STORY = new Story(GetNextID());
        }

        /// <summary>
        /// Build your story here.
        /// </summary>
        public abstract void BuildStory();

        /// <summary>
        /// Generate JSON file of the Story
        /// </summary>
        /// <param name="filePath"></param>
        public override void GenerateJSON(string filePath)
        {
            BuildStory();
            string jsonString = JsonUtility.ToJson(MAIN_STORY, true);
            System.IO.File.WriteAllText(filePath, jsonString);
        }
    }
}

#endif