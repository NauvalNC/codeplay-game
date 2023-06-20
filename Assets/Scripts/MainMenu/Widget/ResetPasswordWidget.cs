using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResetPasswordWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputCodeVerification;
    [SerializeField] private TMP_InputField inputNewPassword;
    [SerializeField] private Button btnResetPassword;

    private float verificationCodeCountdown = 0;
    [SerializeField] private int verificationCodeTimeout = 10;
    [SerializeField] private TMP_Text txtSendVerificationCode;
    [SerializeField] private Button btnSendVerificationCode;

    private void Update()
    {
        if (verificationCodeCountdown >= 0)
        {
            verificationCodeCountdown -= Time.deltaTime;
            txtSendVerificationCode.text = $"{(int)verificationCodeCountdown}s";
            btnSendVerificationCode.interactable = false;
        } 
        else
        {
            txtSendVerificationCode.text = "Minta Kode";
            btnSendVerificationCode.interactable = true;
        }
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnBack.onClick.AddListener(ResetWidget);
        btnSendVerificationCode.onClick.AddListener(SendVerificationCode);
        btnResetPassword.onClick.AddListener(RequestPasswordReset);
    }

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        verificationCodeCountdown = 0;
    }

    protected override void ResetWidget()
    {
        base.ResetWidget();

        inputEmail.text = string.Empty;
        inputCodeVerification.text = string.Empty;
        inputNewPassword.text = string.Empty;
        btnSendVerificationCode.interactable = true;
    }

    private void SendVerificationCode()
    {
        string tEmail = inputEmail.text;
        if (string.IsNullOrEmpty(tEmail))
        {
            PromptSubsystem.Instance.ShowPopUp("Notifikasi", "Mohon isi e-mail terlebih dahulu sebelum meminta kode verifikasi.", PopUpWidget.PopUpType.OK);
            return;
        }

        PromptSubsystem.Instance.ShowLoading("Meminta Kode Verifikasi...");

        // Send verification code.
        AWSAuthSubsystem.Instance.RequestForgotPasswordVerificationCode(tEmail, (bool isSuccessful, string message) =>
        {
            MainThreadWorker.worker.AddJob(() =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    verificationCodeCountdown = verificationCodeTimeout;
                    PromptSubsystem.Instance.ShowPopUp("Berhasil Meminta Kode", message, PopUpWidget.PopUpType.OK);
                }
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Gagal Meminta Kode", message, PopUpWidget.PopUpType.OK);
                }
            });
        });
    }

    private void RequestPasswordReset()
    {
        string tEmail = inputEmail.text;
        string tVerificationCode = inputCodeVerification.text;
        string tNewPassword = inputNewPassword.text;

        if (string.IsNullOrEmpty(tEmail) || string.IsNullOrEmpty(tVerificationCode) || string.IsNullOrEmpty(tNewPassword))
        {
            PromptSubsystem.Instance.ShowPopUp("Notifikasi", "Mohon isi semua isian untuk mengganti password.", PopUpWidget.PopUpType.OK);
            return;
        }

        PromptSubsystem.Instance.ShowLoading("Mengganti Password...");

        // Request reset password using AWS Cognito.
        AWSAuthSubsystem.Instance.ChangePassword(tEmail, tVerificationCode, tNewPassword, (bool isSuccessful, string message) =>
        {
            MainThreadWorker.worker.AddJob(() =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    ResetWidget();
                    PromptSubsystem.Instance.ShowPopUp("Berhasil Mengubah Password", message, PopUpWidget.PopUpType.OK, (PopUpWidget.PopUpResult result) =>
                    {
                        if (result == PopUpWidget.PopUpResult.CONFIRMED)
                        {
                            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LOGIN);
                        }
                    });
                }
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Gagal Mengubah Password", message, PopUpWidget.PopUpType.OK);
                }
            });
        });
    }
}
