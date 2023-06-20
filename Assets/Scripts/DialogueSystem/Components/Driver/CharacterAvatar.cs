using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAvatar : MonoBehaviour
    {
        [SerializeField] private string charAddressKey;
        [SerializeField] private string charName;
        [SerializeField] private Image charImage;
        [SerializeField] private GameObject baloonObj;
        [SerializeField] private Image baloonExpImg;

        private Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public string GetAddressKey() { return charAddressKey; }

        public string GetCharName() { return charName; }

        public void TintImage(Color color) { charImage.color = color; }

        public void PlayAnim(AvatarAnim animType)
        {
            // TODO: Play Character Animation
            // anim.SetInteger("animState", (int)animType);
        }

        internal void PlayExpression(AvatarExp speakerExp)
        {
            // TODO: Play Character Expression
        }
    }
}