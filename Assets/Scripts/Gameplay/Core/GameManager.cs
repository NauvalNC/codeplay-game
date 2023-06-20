using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DialogueSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    [Header("Game Manager")]
    public GameplayConfig gameplayConfig;
    public float elapsedSeconds = 0f;
    public bool isGameStarted = false;
    public bool isGameOver = false;
    private int blockCount;
    public int BlockCount
    {
        get
        {
            return blockCount;
        }
        set
        {
            if (value < 0)
            {
                blockCount = 0;
            }
            else
            {
                blockCount = value;
            }
        }
    }

    [Header("User Interface")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnToggleCameraView;
    [SerializeField] private TMP_Text txtBlockCount;
    [SerializeField] private TMP_Text txtTimeElapsed;
    [SerializeField] private GameOverWidget gameOverWidget;
    [SerializeField] private GameObject codeLockerPanel;
    [SerializeField] private CountdownStarterWidget countdownWidget;

    [SerializeField] private Button btnToggleObjective;
    [SerializeField] private Animator acObjectivePanel;
    [SerializeField] private TMP_Text txtOneStarObjective;
    [SerializeField] private TMP_Text txtTwoStarObjective;
    [SerializeField] private TMP_Text txtThreeStarObjective;


    [Header("Level Look and Feel")]
    [SerializeField] private Material skybox;

    #region Helpers
    private bool isGameOverFlag = false;
    private CheckerTile[] checkerTiles;
    #endregion

    private void Awake()
    {
        RenderSettings.skybox = skybox;

        btnPlay.onClick.AddListener(StartLevelCode);
        btnRestart.onClick.AddListener(ResetLevel);
        btnQuit.onClick.AddListener(QuitLevel);
        btnToggleCameraView.onClick.AddListener(() =>
        {
            CameraSwitcher.Instance.ToggleCameraSetup();
        });

        btnToggleObjective.onClick.AddListener(() =>
        {
            if (acObjectivePanel.GetCurrentAnimatorStateInfo(0).IsName("Objective_Initialize")) return;

            if (acObjectivePanel.GetCurrentAnimatorStateInfo(0).IsName("Objective_In"))
            {
                acObjectivePanel.Play("Objective_Out");
            } else
            {
                acObjectivePanel.Play("Objective_In");
            }
        });


        DialogueDriver.Instance.onDialogueEndsDelegate += OnStoryEnds;

        // Clean up on code execution stopped.
        checkerTiles = FindObjectsOfType<CheckerTile>();
        CodeManager.Instance.onCodeExecutionStopDelegate += () =>
        {
            InventoryManager.Instance.ClearItems();
            InventoryManager.Instance.ResetLevelItems();

            foreach (CheckerTile checkerTile in checkerTiles)
            {
                checkerTile.ResetCheck();
            }
        };
    }

    private void Start()
    {
        SetupGameplayConfig();
    }

    private void Update()
    {
        codeLockerPanel.SetActive(CodeManager.Instance.IsExecuting);
        UpdateGameState();
    }

    #region Game Manager
    private void OnStartGame()
    {
        Time.timeScale = 1f;
        countdownWidget.gameObject.SetActive(true);
    }

    public void OnGameStarted()
    {
        isGameStarted = true;
        acObjectivePanel.gameObject.SetActive(true);
    }

    public void EndGame()
    {
        isGameOver = true;
    }

    public void StartLevelCode()
    {
        CodeManager.Instance.StartCodeExecution();
    }

    public void ResetLevel()
    {
        isGameOver = false;
        CodeManager.Instance.StopCodeExecution();
    }

    public void QuitLevel()
    {
        PromptSubsystem.Instance.ShowPopUp(
            "Keluar Level?", 
            "Kamu yakin keluar dari level ini?", 
            PopUpWidget.PopUpType.YES_NO, (PopUpWidget.PopUpResult result) =>
            {
                if (result == PopUpWidget.PopUpResult.CONFIRMED)
                {
                    SceneLoader.LoadScene(GameplayStatics.MAIN_MENU_SCENE);
                }
            }
        );
    }

    private void SetupGameplayConfig()
    {
        BlockCount = 0;

        // Set objective texts.
        txtOneStarObjective.text = $": Selesaikan Level";
        txtTwoStarObjective.text = $": Maksimum {gameplayConfig.starsRequirement.twoStars} Blok Kode";
        txtThreeStarObjective.text = $": Maksimum {gameplayConfig.starsRequirement.threeStars} Blok Kode";

        // If any, start the story first before the gameplay.
        if (gameplayConfig.startStory)
        {
            StartStory(gameplayConfig.startStory);
        }
        else
        {
            OnStartGame();
        }
    }

    private void UpdateGameState()
    {
        if (isGameOver)
        {
            OnGameOver();
            return;
        }

        if (isGameStarted)
        {
            // Update elapsed time.
            elapsedSeconds += Time.deltaTime;
            txtTimeElapsed.text = UtilitySubsystem.FormatTime(elapsedSeconds);
        }

        // Update game control interactability.
        bool tIsExecuting = CodeManager.Instance.IsExecuting;
        btnPlay.interactable = !tIsExecuting;
        btnRestart.interactable = tIsExecuting;
        
        // Update block count.
        txtBlockCount.text = BlockCount.ToString();
    }

    private void OnGameOver()
    {
        if (isGameOverFlag)
        {
            return;
        }
        isGameOverFlag = true;
        CodeManager.Instance.StopCodeExecution();

        // If any, start the end story first before the displaying game over state.
        if (gameplayConfig.endStory)
        {
            StartStory(gameplayConfig.endStory);
        } 
        else
        {
            StartCoroutine(OnGameOver_Numerator());
        }
    }

    private IEnumerator OnGameOver_Numerator()
    {
        yield return new WaitForSeconds(0.5f);
        gameOverWidget.Activate();
    }
    #endregion

    #region Story Manager
    private void StartStory(StoryPackage storyPackage)
    {
        DialogueDriver.Instance.storyPackage = storyPackage;
        DialogueDriver.Instance.StartDriver();
    }

    private void OnStoryEnds(StoryPackage storyPackage)
    {
        if (storyPackage == gameplayConfig.startStory)
        {
            OnStartGame();
        }
        else if (storyPackage == gameplayConfig.endStory)
        {
            StartCoroutine(OnGameOver_Numerator());
        }
    }
    #endregion
}