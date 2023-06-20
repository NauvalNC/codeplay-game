using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Button btnLogin;
    [SerializeField] private Button btnRegister;

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnLogin.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LOGIN);
        });

        btnRegister.onClick.AddListener(Register);
    }

    protected override void ResetWidget()
    {
        base.ResetWidget();

        inputEmail.text = "";
        inputPassword.text = "";
    }

    private void Register()
    {
        string tEmail = inputEmail.text;
        string tPassword = inputPassword.text;

        if (string.IsNullOrEmpty(tEmail) || string.IsNullOrEmpty(tPassword))
        {
            return;
        }

        PromptSubsystem.Instance.ShowLoading("Sedang Membuat Akun...");

        // Register using AWS Cognito.
        AWSAuthSubsystem.Instance.RegisterWithEmail(tEmail, tPassword, (bool isSuccessful, string message) => 
        {
            MainThreadWorker.worker.AddJob(() =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    PromptSubsystem.Instance.ShowPopUp("Sukses Membuat Akun", message, PopUpWidget.PopUpType.OK, (PopUpWidget.PopUpResult result) =>
                    {
                        if (result == PopUpWidget.PopUpResult.CONFIRMED)
                        {
                            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LOGIN);
                        }
                    });
                }
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Gagal Membuat Akun", message, PopUpWidget.PopUpType.OK);
                }
            });
        });
    }
}