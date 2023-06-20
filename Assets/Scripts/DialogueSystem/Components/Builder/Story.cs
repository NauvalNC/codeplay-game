using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem { 
    [System.Serializable]
    public class Story
    {
        [SerializeField] private int ID = -1;
        public List<Dialogue> dialogues = new List<Dialogue>();
        public int mergePointID = -1;

        public Story(int ID)
        {
            SetID(ID);
        }

        public void SetID(int ID) { this.ID = ID; }

        public int GetID() { return this.ID; }

        public bool IsEmpty() { return dialogues.Count <= 0; }
    }
}