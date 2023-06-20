using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem { 
    [CreateAssetMenu(fileName = "New Story Package", menuName = "Dialogue System/New Story Package", order = 1)]
    public class StoryPackage : ScriptableObject
    {
        [SerializeField] private string storyAddressKey = "story_address_key";
        [SerializeField] private string storyTitle = "story_title";
        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private Sprite backgroundImg;
        [SerializeField] private List<CharacterAvatar> charAvatars = new List<CharacterAvatar>();
        private Dictionary<string, CharacterAvatar> charDict = null;

        public string GetStoryTitle() { return storyTitle; }

        public Sprite GetBGImage() { return backgroundImg; }

        /// <summary>
        /// Get story dialogue JSON file
        /// </summary>
        /// <returns></returns>
        public TextAsset GetJSONFile() { return jsonFile; }

        /// <summary>
        /// Refresh data contained in the package
        /// </summary>
        public void Refresh()
        {
            CreateCharDictionary();
        }

        public string GetAddressKey() { return storyAddressKey; }

        void CreateCharDictionary()
        {
            charDict = null;
            charDict = new Dictionary<string, CharacterAvatar>();
            foreach(CharacterAvatar cd in charAvatars)
            {
                if (charDict.ContainsKey(cd.GetAddressKey()) == false)
                    charDict.Add(cd.GetAddressKey(), cd);
            }
        }

        /// <summary>
        /// Return character filtered by address key.
        /// </summary>
        /// <param name="addressKey"></param>
        /// <returns></returns>
        public CharacterAvatar FindCharByAddressKey(string addressKey)
        {
            return charDict[addressKey];
        }
    }
}