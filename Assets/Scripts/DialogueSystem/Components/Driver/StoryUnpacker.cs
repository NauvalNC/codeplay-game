using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem 
{ 
    /// <summary>
    /// Unpack JSON Story file and provide functionality to fetch though dialogues.
    /// </summary>
    public class StoryUnpacker
    {
        TextAsset jsonFile;
        private Story story, targetStory;
        private Dictionary<int, Story> storyDict = new Dictionary<int, Story>();

        public StoryUnpacker(TextAsset jsonFile)
        {
            this.jsonFile = jsonFile;
            Setup();
        }

        /// <summary>
        /// Setup unpacker by passing JSON file.
        /// </summary>
        void Setup()
        {
            story = JsonUtility.FromJson<Story>(jsonFile.text);

            targetStory = null;
            storyDict.Clear();

            // Hash new story to story hashtable
            // (it's going to be used for merging story later)
            storyDict.Add(story.GetID(), story);
        }

        /// <summary>
        /// Is there is next available dialogue.
        /// </summary>
        /// <returns></returns>
        public bool HasDialogueToFetch() 
        {
            if (story == null) return false;
            return true;
        }

        /// <summary>
        /// Get current available dialogue.
        /// </summary>
        /// <returns></returns>
        public Dialogue GetCurrDialogue()
        {
            if (story == null || story.IsEmpty()) return null;
            return story.dialogues[0];
        }

        /// <summary>
        /// Shift current dialogue to next available dialogue.
        /// </summary>
        public void ShiftToNextDialogue()
        {
            if (story == null) return;

            // If there is new story from branch
            if (targetStory != null)
            {
                story = targetStory;
                targetStory = null;

                // If the recieved branch empty, then we need to fetch next available dialogue.
                if (story.IsEmpty()) ShiftToNextDialogue();
                return;
            }

            // The current dialogue has branch, stop fetching
            if (!story.IsEmpty() && GetCurrDialogue().HasBranch())
            {
                Debug.LogError("Error: Attempt to fetch next dialogue, but the current dialogue's branch hasn't been chosen.");
                throw new System.InvalidOperationException("Attempt to fetch next dialogue, but the current dialogue's branch hasn't been chosen.");
            }

            // Remove first dialogue (fetch next dialogue by shift it as first dialogue to fetch)
            if (!story.IsEmpty())
            {
                // The first dialogue of the story is already the "next" dialogue.
                story.dialogues.RemoveAt(0);
                if (story.IsEmpty()) ShiftToNextDialogue();
            }
            // Story dialogues ends, try fetch next story if available
            else
            {
                // Remove empty story from hash table
                storyDict.Remove(story.GetID());
                
                // If the story ends
                if (story.mergePointID == -1)
                {
                    // The story ends, do something about this.
                    story = null;
                }
                // Try merge to certain story with particular ID
                else
                {
                    // Get merge story
                    int temp = story.mergePointID;
                    story = storyDict[temp];

                    // When we branch, the dialogue that invoke branching remained.
                    // Therefore, we need to remove last dialogue.
                    story.dialogues.RemoveAt(0);

                    if (story.dialogues.Count <= 0) ShiftToNextDialogue();
                }
            }
        }

        /// <summary>
        /// Choose branch for current branch-supported dialogue.
        /// </summary>
        /// <param name="index"></param>
        public Branch ChooseBranch(int index)
        {
            Branch res = null;

            if (GetCurrDialogue().HasBranch() == false)
            {
                Debug.LogWarning("Warning: Attempt to choose branch but the current dialogue doesn't have any branches");
            } else
            {
                if (index < 0 || index > GetCurrDialogue().branches.Count - 1)
                {
                    Debug.LogError("Error: Attempt to choose branch but the chosen index is out of branches bound");
                } else
                {
                    res = GetCurrDialogue().branches[index];
                    targetStory = res.nextStory;

                    // Hash new story to story hashtable
                    // (it's going to be used for merging story later)
                    storyDict.Add(targetStory.GetID(), targetStory);
                }
            }

            return res;
        }
    }
}