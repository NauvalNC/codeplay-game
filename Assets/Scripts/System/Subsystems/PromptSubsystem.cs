using UnityEngine;
using UnityEngine.UI;

public class PromptSubsystem : MonoBehaviour
{
    private static PromptSubsystem instance;
    public static PromptSubsystem Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<PromptSubsystem>();
            }
            return instance;
        }
    }

    [Header("User Interface")]
    [SerializeField] private LoadingWidget loadingWidget;
    [SerializeField] private PopUpWidget popUpWidget;
    [SerializeField] private GameObject creditWidget;
    [SerializeField] private Button btnCreditOk;

    private void Awake()
    {
        btnCreditOk.onClick.AddListener(() =>
        {
            HideCredit();
        });
    }

    public void ShowLoading(string message = "Memuat...")
    {
        loadingWidget.Activate();
        loadingWidget.SetLoadingMessage(message);
    }

    public void HideLoading()
    {
        loadingWidget.Deactivate();
    }

    public void ShowPopUp(string title, string subtitle, PopUpWidget.PopUpType type, PopUpWidget.OnPopUpResult onResult = null)
    {
        popUpWidget.Activate();
        popUpWidget.SetPopUp(title, subtitle, type, onResult);
    }

    public void ShowPopUp(string title, string subtitle, string imgPath, PopUpWidget.PopUpType type, PopUpWidget.OnPopUpResult onResult = null)
    {
        ShowPopUp(title, subtitle, type, onResult);
        popUpWidget.SetImage(imgPath);
    }

    public void ShowCredit()
    {
        creditWidget.SetActive(true);
    }

    public void HideCredit()
    {
        creditWidget.SetActive(false);
    }
}