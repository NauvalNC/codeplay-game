using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Amazon.Extensions.CognitoAuthentication;

public class LoginWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Button btnLogin;
    [SerializeField] private Button btnOfflineMode;
    [SerializeField] private Button btnRegister;
    [SerializeField] private Button btnForgotPassword;
    [SerializeField] private Button btnExit;

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnLogin.onClick.AddListener(Login);
        btnOfflineMode.onClick.AddListener(StartOfflineMode);

        btnRegister.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.REGISTER);
        });

        btnForgotPassword.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.RESET_PASSWORD);
        });

        btnExit.onClick.AddListener(() =>
        {
            PromptSubsystem.Instance.ShowPopUp(
                "Keluar Game",
                "Kamu yakin keluar dari game?",
                PopUpWidget.PopUpType.YES_NO,
                (PopUpWidget.PopUpResult result) =>
                {
                    if (result == PopUpWidget.PopUpResult.CONFIRMED)
                    {
                        Application.Quit();
                    }
                }
            );
        });
    }

    protected override void ResetWidget()
    {
        base.ResetWidget();

        inputEmail.text = "";
        inputPassword.text = "";
    }

    private void Login()
    {
        string tEmail = inputEmail.text;
        string tPassword = inputPassword.text;

        if (string.IsNullOrEmpty(tEmail) || string.IsNullOrEmpty(tPassword))
        {
            return;
        }

        PromptSubsystem.Instance.ShowLoading("Sedang Login...");

        // Login using AWS Cognito.
        AWSAuthSubsystem.Instance.LoginWithEmail(tEmail, tPassword, (bool isSuccessful, string message, CognitoUser user) =>
        {
            MainThreadWorker.worker.AddJob(() =>
            {
                if (isSuccessful)
                {
                    NetworkingSubsystem.Instance.CheckAPIConnection();
                    SignPlayer(AWSAuthSubsystem.Instance.GetCurrentUserId(), user.UserID);
                }
                else
                {
                    PromptSubsystem.Instance.HideLoading();
                    PromptSubsystem.Instance.ShowPopUp("Login Gagal", message, PopUpWidget.PopUpType.OK);
                }
            });
        });
    }

    private void StartOfflineMode()
    {
        GameplayStatics.SetGameplayOnlineMode(false);

        ResetWidget();
        MenuManager.Instance.SwitchWidget(MenuManager.MenuType.MAIN_MENU);
    }

    private void SignPlayer(string playerId, string email)
    {
        NetworkingSubsystem.Instance.SignPlayer(playerId, email, (bool isSuccessful, string message) =>
        {
            PromptSubsystem.Instance.HideLoading();

            if (isSuccessful)
            {
                ResetWidget();
                MenuManager.Instance.SwitchWidget(MenuManager.MenuType.MAIN_MENU);
            }
            else
            {
                AWSAuthSubsystem.Instance.Logout();
                PromptSubsystem.Instance.ShowPopUp("Login Gagal", "Terjadi kesalahan atau masalah koneksi internet. Silahkan coba lagi.", PopUpWidget.PopUpType.OK);
            }
        });
    }
}
