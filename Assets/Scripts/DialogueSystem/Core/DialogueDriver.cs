using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DialogueSystem;

namespace DialogueSystem
{
    public class DialogueDriver : MonoBehaviour
    {
        public delegate void OnDialogueEnds(StoryPackage storyPackage);
        public OnDialogueEnds onDialogueEndsDelegate;

        private static DialogueDriver instance;
        public static DialogueDriver Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<DialogueDriver>();
                }
                return instance;
            }
        }

        [Header("User Interface")]
        [SerializeField] private GameObject canvas;
        [SerializeField] private TMP_Text txtSpeaker;
        [SerializeField] private TMP_Text txtMessage;
        [SerializeField] private Transform[] btnBranches;
        [SerializeField] private Button btnSpeed;
        [SerializeField] private Button btnAuto;
        [SerializeField] private Button btnSkip;
        [SerializeField] private Image imgBackground;

        [Header("Controls")]
        [SerializeField] private Color charDisabledColor;
        [SerializeField] private float charImageHeight = 600f;
        [SerializeField] private float printingSpeed = 0.01f;
        [SerializeField] private float readBreakTime = 1f;

        [Header("Avatar Pool")]
        [SerializeField] private int maximumDisplayedAvatar = 2;
        [SerializeField] private Transform avatarPoolCont;
        [SerializeField] private Transform avatarDisplayCont;
        private Dictionary<string, CharacterAvatar> avatarPoolDict = new Dictionary<string, CharacterAvatar>();

        private const string DOUBLE_SPEED = "DIALOGUE_DOUBLE_SPEED";
        private const string AUTO_MODE = "DIALOGUE_AUTO_MODE";

        private Animator canvasAC;
        private int chosenBranchIndex = 0;

        public StoryPackage storyPackage;
        private StoryUnpacker story;
        private Dialogue currDialogue;
        private bool allowExecNext = true;

        public bool isAutoMode = false;
        private bool isDoubleSpeed = false;
        private bool isPlaying = false;
        private bool interuptAuto = false;

        private void Awake()
        {
            canvasAC = canvas.GetComponent<Animator>();
            btnSpeed.onClick.AddListener(ToggleDoubleSpeed);
            btnAuto.onClick.AddListener(ToggleAutoMode);
            btnSkip.onClick.AddListener(OnStoryEnds);
        }

        void SetupDriver()
        {
            // Reset attributtes
            UpdatePlayerPrefs();

            StopAllCoroutines();
            chosenBranchIndex = 0;
            allowExecNext = true;
            ClearAvatarPool();

            // Unpack story
            storyPackage.Refresh();
            story = new StoryUnpacker(storyPackage.GetJSONFile());

            // Setup branch option interfaces
            imgBackground.sprite = storyPackage.GetBGImage();
            ClearBranchOptions();
            RepaintUI();

            TryReadCurrentDialogue();
        }

        void RepaintUI()
        {
            if (btnSpeed != null)
            {
                btnSpeed.transform.GetChild(1).GetComponent<TMP_Text>().text = isDoubleSpeed ? "2X" : "1X";
            }

            if (btnAuto != null)
            {
                btnAuto.transform.GetChild(1).GetComponent<TMP_Text>().text = isAutoMode ? "Auto" : "Manual";
            }
        }

        /// <summary>
        /// Try read next current dialogue
        /// </summary>
        void TryReadCurrentDialogue()
        {
            if (allowExecNext == false) return;

            if (story.HasDialogueToFetch()) StartCoroutine(IEPrintDialogue());
            else OnStoryEnds();
        }

        /// <summary>
        /// Call this to exec next dialogue manually only.
        /// This call will interrupt dialogue reading if it still playing.
        /// </summary>
        public void NextDialogue()
        {
            if (isPlaying) interuptAuto = true;
            else if (allowExecNext) TryReadCurrentDialogue();
        }

        /// <summary>
        /// Print sentence with typing animation typing
        /// </summary>
        /// <returns></returns>
        IEnumerator IEPrintDialogue()
        {
            allowExecNext = false;

            currDialogue = story.GetCurrDialogue();

            // Assign Avatar to avatar pool and get the current speaking avatar based on current dialogue
            foreach (string speaker in currDialogue.GetSpeakerGroup())
            {
                if (avatarPoolDict.ContainsKey(speaker) == false)
                {
                    AssignAvatarToPool(storyPackage.FindCharByAddressKey(speaker));
                }
            }
            CharacterAvatar currAvatar = avatarPoolDict[currDialogue.charAddressKey];

            // Display avatars
            DisplayCharacterImages(currDialogue);

            // Display dialogue
            txtSpeaker.gameObject.SetActive(!string.IsNullOrEmpty(currAvatar.GetCharName()));
            txtSpeaker.text = currAvatar.GetCharName();
            
            string sentence = currDialogue.sentence;
            int sentenceLen = sentence.Length;
            txtMessage.text = "";
            isPlaying = true;
            for (int i = 0; i < sentenceLen; i++)
            {
                if (interuptAuto)
                {
                    txtMessage.text = sentence;
                    break;
                }
                txtMessage.text += sentence[i];
                yield return new WaitForSeconds(printingSpeed);
            }
            isPlaying = false;
            interuptAuto = false;

            // Display branch if any
            if (currDialogue.HasBranch())
            {
                yield return new WaitForSeconds(readBreakTime / 2f);
                DisplayBranchOptions();
            }
            else
            {
                story.ShiftToNextDialogue();
                ExecAutoMode();
            }
        }

        void OnStoryEnds()
        {
            Time.timeScale = 1f;
            StartCoroutine(IFadeOutAnim(null));
        }

        void DisplayBranchOptions()
        {
            int len = currDialogue.branches.Count;

            if (len > btnBranches.Length)
            {
                throw new System.InvalidOperationException("Branches out of bound. Can't display the interfaces");
            }

            for (int i = 0; i < len; i++)
            {
                btnBranches[i].gameObject.SetActive(true);
                btnBranches[i].GetChild(0).GetComponent<TMP_Text>().text = currDialogue.branches[i].sentence;

                int index = i;
                btnBranches[i].GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(ChooseBranch(index)); });
            }
        }

        void DisplayCharacterImages(Dialogue dialogue)
        {
            // Clear display container
            while(avatarDisplayCont.childCount > 0)
            {
                avatarDisplayCont.GetChild(0).SetParent(avatarPoolCont);
            }

            List<string> group = dialogue.GetSpeakerGroup();
            int len = group.Count;

            // Check if out of bound
            if (len > maximumDisplayedAvatar)
            {
                Debug.LogWarning("Warning: available container image is less than number of images to set");
                return;
            }

            // Set images
            CharacterAvatar avatar;
            for (int i = 0; i < len; i++)
            {
                avatar = avatarPoolDict[group[i]];

                if (avatar.GetAddressKey() == "me") continue;

                avatar.transform.SetParent(avatarDisplayCont);
                avatar.transform.localScale = Vector3.one;

                if (dialogue.charAddressKey == avatar.GetAddressKey())
                {
                    avatar.TintImage(Color.white);

                    // Play animation
                    avatar.PlayAnim(currDialogue.speakerAnim);
                    avatar.PlayExpression(currDialogue.speakerExp);
                }
                else avatar.TintImage(charDisabledColor);
            }
        }

        void ExecAutoMode()
        {
            if (isAutoMode == false)
            {
                allowExecNext = true;
                return;
            }
            StartCoroutine(IExecAutoMode());
        }

        IEnumerator IExecAutoMode()
        {
            yield return new WaitForSeconds(readBreakTime);
            allowExecNext = true;

            TryReadCurrentDialogue();
        }

        public void ToggleDoubleSpeed()
        {
            // Toggle Speed
            isDoubleSpeed = !isDoubleSpeed;
            PlayerPrefs.SetInt(DOUBLE_SPEED, isDoubleSpeed ? 1 : 0);

            UpdatePlayerPrefs();
            RepaintUI();
        }

        void UpdatePlayerPrefs()
        {
            isAutoMode = PlayerPrefs.GetInt(AUTO_MODE, 0) == 0 ? false : true;
            isDoubleSpeed = PlayerPrefs.GetInt(DOUBLE_SPEED, 0) == 0 ? false : true;

            Time.timeScale = isDoubleSpeed ? 2f : 1f;
        }

        void ResetPlayerPrefs()
        {
            Time.timeScale = 1f;
        }

        public void ToggleAutoMode()
        {
            // Toggle Auto
            isAutoMode = !isAutoMode;
            PlayerPrefs.SetInt(AUTO_MODE, isAutoMode ? 1 : 0);

            if (isAutoMode && allowExecNext) TryReadCurrentDialogue();

            RepaintUI();
        }

        void AssignAvatarToPool(CharacterAvatar avatar)
        {
            GameObject obj = Instantiate(avatar.gameObject, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(avatarPoolCont);

            avatarPoolDict.Add(avatar.GetAddressKey(), obj.GetComponent<CharacterAvatar>());
        }

        void ClearAvatarPool()
        {
            foreach(Transform obj in avatarDisplayCont) Destroy(obj.gameObject);

            foreach(Transform obj in avatarPoolCont) Destroy(obj.gameObject);

            avatarPoolDict.Clear();
        }

        IEnumerator ChooseBranch(int index)
        {
            foreach(Transform btn in btnBranches)
            {
                if (btn.gameObject.activeInHierarchy)
                {
                    btn.GetComponent<Animator>().Play("branch_out");
                }
            }

            yield return new WaitForSeconds(0.3f);
            ClearBranchOptions();

            int tChosen = story.ChooseBranch(index).chosenBranchIndex;

            // Set chosen branch index if there any branch chosen
            if (tChosen != -1) chosenBranchIndex = tChosen; 

            story.ShiftToNextDialogue();

            allowExecNext = true;

            TryReadCurrentDialogue();
        }

        void ClearBranchOptions()
        {
            foreach (Transform btn in btnBranches)
            {
                btn.gameObject.SetActive(false);
                btn.transform.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }

        public void StartDriver()
        {
            SetupDriver();
            StartCoroutine(IFadeInAnim(null));
        }

        public void StopDriver()
        {
            ResetPlayerPrefs();

            allowExecNext = false;
            StopAllCoroutines();
        }

        public void ResumeDriver()
        {
            UpdatePlayerPrefs();

            allowExecNext = true;

            ClearBranchOptions();
            TryReadCurrentDialogue();

            StartCoroutine(IFadeInAnim(null));
        }

        public IEnumerator IFadeInAnim(object param)
        {
            canvas.gameObject.SetActive(true);
            canvasAC.Play("fade_in", -1, 0f);
            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator IFadeOutAnim(object param)
        {
            canvasAC.Play("fade_out", -1, 0f);
            yield return new WaitForSeconds(0.5f);

            currDialogue = null;
            canvas.gameObject.SetActive(false);

            StopDriver();

            onDialogueEndsDelegate?.Invoke(storyPackage);
            storyPackage = null;

            Debug.Log("Story ends!");
        }
    }
}