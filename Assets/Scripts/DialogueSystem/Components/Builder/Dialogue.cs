using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem 
{
    [System.Serializable]
    public class Dialogue
    {
        public string charAddressKey;
        public AvatarExp speakerExp = AvatarExp.NORMAL;
        public AvatarAnim speakerAnim = AvatarAnim.NORMAL;
        public string sentence;
        public List<Branch> branches = new List<Branch>();

        [SerializeField] private List<string> group = new List<string>();
        private HashSet<string> speakerGroup = new HashSet<string>();

        public Dialogue(string adressKey, string sentence, AvatarExp exp)
        {
            AddSpeakerGroup(new string[] { adressKey });

            this.sentence = sentence;
            speakerExp = exp;
            charAddressKey = adressKey;
        }

        public Dialogue(string adressKey, string sentence, AvatarExp exp, AvatarAnim anim)
        {
            AddSpeakerGroup(new string[] { adressKey });

            this.sentence = sentence;
            speakerExp = exp;
            charAddressKey = adressKey;
            speakerAnim = anim;
        }

        public Dialogue(string addressKey, string sentence, AvatarExp exp, string[] group)
        {
            AddSpeakerGroup(group);
            AddSpeakerGroup(new string[] { addressKey });

            this.sentence = sentence;
            speakerExp = exp;
            charAddressKey = addressKey;
        }

        public Dialogue(string addressKey, string sentence, AvatarExp exp, AvatarAnim anim, string[] group)
        {
            AddSpeakerGroup(group);
            AddSpeakerGroup(new string[] { addressKey });

            this.sentence = sentence;
            speakerExp = exp;
            speakerAnim = anim;
            charAddressKey = addressKey;
        }

        public bool HasBranch()
        {
            return branches.Count > 0;
        }

        public void AddSpeakerGroup(string[] group)
        {
            foreach(string g in group)
            {
                speakerGroup.Add(g);
            }

            SerializeSpeakerGroup();
        }

        public void RemoveSpeakerGroup(string[] group)
        {
            foreach(string g in group)
            {
                if (g == charAddressKey) continue;
                speakerGroup.Remove(g);
            }

            SerializeSpeakerGroup();
        }

        public List<string> GetSpeakerGroup()
        {
            return group;
        }

        void SerializeSpeakerGroup()
        {
            group.Clear();
            foreach(string st in speakerGroup) group.Add(st);
        }
    }

    [System.Serializable]
    public class Branch
    {
        public string sentence;
        public Story nextStory;
        public int chosenBranchIndex = -1;

        public Branch() { }

        public Branch(string sentence)
        {
            this.sentence = sentence;
        }

        public Branch(string sentence, int chosenBranchIndex)
        {
            this.sentence = sentence;
            this.chosenBranchIndex = chosenBranchIndex;
        }

        public Branch(string sentence, Story nextStory)
        {
            this.sentence = sentence;
            this.nextStory = nextStory;
        }

        public Branch(string sentence, Story nextStory, int chosenBranchIndex)
        {
            this.sentence = sentence;
            this.nextStory = nextStory;
            this.chosenBranchIndex = chosenBranchIndex;
        }
    }

    [System.Serializable]
    public enum AvatarExp
    {
        NORMAL,
        HAPPY,
        SAD,
        ANGRY,
        SHOCKED,
        QUESTION,
        BLUSH
    }

    [System.Serializable]
    public enum AvatarAnim
    {
        NORMAL,
        BOUNCE,
        DOWN
    }
}
