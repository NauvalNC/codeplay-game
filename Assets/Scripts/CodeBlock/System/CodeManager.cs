using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CodeManager : MonoBehaviour
{
    public delegate void OnCurrentCodeExecuted(CodeAction codeAction);
    public delegate void OnCodeExecutionStop();

    private static CodeManager instance;
    public static CodeManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<CodeManager>();
            }
            return instance;
        }
    }

    public enum ECodeExecutionStatus
    {
        IDLE,
        EXECUTING,
        ERROR
    }

    [Header("Code Template")]
    [HideInInspector] public CodeTemplate codeTemplate;
    [SerializeField] private Transform actionCodesContainer;
    [SerializeField] private Transform coreCodesContainer;
    [SerializeField] private Transform variableCodesContainer;

    [Header("User Interface")]
    [SerializeField] private TMP_Text txtCodeName;
    [SerializeField] private TMP_Text txtCodeDescription;
    [SerializeField] private Image imgExecutionStatus;
    [SerializeField] private TMP_Text txtExecutionStatus;
    [SerializeField] private Color[] executionStatusColors;

    [Header("Components")]
    public Canvas rootCanvas;
    [SerializeField] private InitialStartAction startCode;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public bool isBreakRequested = false;

    [Header("Log")]
    [SerializeField] private bool displayLog = true;
    [SerializeField] private GameObject logController;
    [SerializeField] private TMP_Text txtLog;
    [SerializeField] private ScrollRect scrollLog;
    [SerializeField] private Button btnClearLog;

    private Coroutine codeExecCoroutine = null;
    public bool IsExecuting
    {
        get
        {
            return codeExecCoroutine != null;
        }
    }

    private CodeAction currentCodeAction = null;
    public CodeAction GetCurrentCodeAction() { return currentCodeAction; }
    public OnCurrentCodeExecuted onCurrentCodeExecutedDelegate;
    public OnCodeExecutionStop onCodeExecutionStopDelegate;

    private void Awake()
    {
        btnClearLog.onClick.AddListener(ResetLog);
        ResetLog();
        HideCodeInfo();

        onCurrentCodeExecutedDelegate += OnExecuteCurrentCodeAction;
        SetExecutionStatus(ECodeExecutionStatus.IDLE);

        player = FindObjectOfType<PlayerController>();

        SetupCodeTemplate();
    }

    private void SetupCodeTemplate()
    {
        codeTemplate = GameManager.Instance.gameplayConfig.codeTemplate;

        // Generated action code templates.
        actionCodesContainer.GetChild(0).gameObject.SetActive(codeTemplate.actionCodes.Count <= 0);
        foreach(GameObject tTemplate in codeTemplate.actionCodes) Instantiate(tTemplate, actionCodesContainer);

        // Generated core code templates.
        coreCodesContainer.GetChild(0).gameObject.SetActive(codeTemplate.coreCodes.Count <= 0);
        foreach (GameObject tTemplate in codeTemplate.coreCodes) Instantiate(tTemplate, coreCodesContainer);

        // Generated variable code templates.
        variableCodesContainer.GetChild(0).gameObject.SetActive(codeTemplate.variableCodes.Count <= 0);
        foreach (GameObject tTemplate in codeTemplate.variableCodes) Instantiate(tTemplate, variableCodesContainer);
    }

    public void StartCodeExecution()
    {
        currentCodeAction = null;

        if (startCode)
        {
            PrintLog("Code execution started.");
            SetExecutionStatus(ECodeExecutionStatus.EXECUTING);
            codeExecCoroutine = StartCoroutine(startCode.Execute());
        } else
        {
            PrintLog("Start code is not valid.");
        }
    }

    public void StopCodeExecution()
    {
        if (currentCodeAction) currentCodeAction.GetOwner().OnPortExitHover();
        currentCodeAction = null;
        StartCoroutine(StopCodeExecutionInternal());
    }

    private IEnumerator StopCodeExecutionInternal()
    {
        PrintLog("Code execution finished.");

        if (codeExecCoroutine != null)
        {
            StopCoroutine(codeExecCoroutine);
            codeExecCoroutine = null;
        }

        if (!GameManager.Instance.isGameOver)
        {
            SetExecutionStatus(ECodeExecutionStatus.ERROR);
            yield return new WaitForSeconds(0.25f);

            player.ResetPawn();

            MainThreadWorker.worker.AddJob(() =>
            {
                onCodeExecutionStopDelegate?.Invoke();
            });
        }

        SetExecutionStatus(ECodeExecutionStatus.IDLE);
    }

    public bool IsValidToAddCode()
    {
        return GameManager.Instance.BlockCount < int.MaxValue;
    }

    public void OnCodeAdded()
    {
        GameManager.Instance.BlockCount += 1;
    }

    public void OnCodeDeleted()
    {
        GameManager.Instance.BlockCount -= 1;
    }

    public void DisplayCodeInfo(CodeInfo info)
    {
        txtCodeName.text = info.codeName;
        txtCodeDescription.text = info.codeDescription;
    }

    public void HideCodeInfo()
    {
        txtCodeName.text = "Ketuk Kode Blok";
        txtCodeDescription.text = "Ketuk kode blok untuk melihat informasi aksi yang dilakukan kode tersebut.\n";
    }

    public void ResetLog()
    {
        txtLog.text = "";
        if (!displayLog)
        {
            logController.SetActive(false);
        }
    }

    public void PrintLog(string log)
    {
        txtLog.text += (log + "\n\n");
        scrollLog.verticalNormalizedPosition = 0;
        Debug.Log(log);
    }

    public void SetExecutionStatus(ECodeExecutionStatus newStatus)
    {
        imgExecutionStatus.color = executionStatusColors[(int)newStatus];

        switch (newStatus)
        {
            case ECodeExecutionStatus.IDLE:
                txtExecutionStatus.text = "Persiapan";
                break;
            case ECodeExecutionStatus.EXECUTING:
                txtExecutionStatus.text = "Berjalan";
                break;
            case ECodeExecutionStatus.ERROR:
                SoundManager.Instance.PlayErrorSound();
                txtExecutionStatus.text = "Gagal";
                break;
        }
    }

    private void OnExecuteCurrentCodeAction(CodeAction codeAction)
    {
        if (currentCodeAction != codeAction)
        {
            if (currentCodeAction) currentCodeAction.GetOwner().OnPortExitHover();
            currentCodeAction = codeAction;
            if (currentCodeAction) currentCodeAction.GetOwner().OnPortHovered();
        }
    }
}