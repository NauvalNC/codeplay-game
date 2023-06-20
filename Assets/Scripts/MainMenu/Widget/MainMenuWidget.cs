using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnTutorial;
    [SerializeField] private TMP_Text txtUsername;
    [SerializeField] private Button btnEditProfile;
    [SerializeField] private Button btnEditSkin;
    [SerializeField] private Button btnExit;
    [SerializeField] private Button btnLogout;
    [SerializeField] private ProfileWindow profileWindow;
    
    [SerializeField] private Button btnCredit;

    [SerializeField] private Button btnSound;
    [SerializeField] private Image btnSoundImg;
    [SerializeField] private Sprite btnSoundOn;
    [SerializeField] private Sprite btnSoundOff;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();
        UpdateProfileData();
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        btnPlay.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.CHAPTERS);
        });

        btnTutorial.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.TUTORIALS);
        });

        btnEditProfile.onClick.AddListener(() =>
        {
            if (GameplayStatics.IsOnline)
            {
                profileWindow.gameObject.SetActive(true);
            } 
            else
            {
                Debug.Log("Edit profile is not available on offline mode.");
            }
        });

        btnEditSkin.onClick.AddListener(() =>
        {
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.SKINS);
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

        btnLogout.onClick.AddListener(() =>
        {
            PromptSubsystem.Instance.ShowPopUp(
                "Logout", 
                "Kamu yakin logout dari akun sekarang?", 
                PopUpWidget.PopUpType.YES_NO, 
                (PopUpWidget.PopUpResult result) =>
                {
                    if (result == PopUpWidget.PopUpResult.CONFIRMED)
                    {
                        Logout();
                    }
                }
            );
        });

        btnSoundImg.sprite = SoundManager.Instance.IsMuted() ? btnSoundOff : btnSoundOn;
        btnSound.onClick.AddListener(() =>
        {
            SoundManager.Instance.ToggleMute();
            btnSoundImg.sprite = SoundManager.Instance.IsMuted() ? btnSoundOff : btnSoundOn;
        });

        btnCredit.onClick.AddListener(() =>
        {
            PromptSubsystem.Instance.ShowCredit();
        });

        profileWindow.ownerMainMenuWidget = this;
        btnLogout.gameObject.SetActive(!GameplayStatics.FORCE_OFFLINE);
    }

    public void UpdateProfileData()
    {
        NetworkingSubsystem.Instance.GetPlayerInfo(
            GameplayStatics.FORCE_OFFLINE ? null : AWSAuthSubsystem.Instance.GetCurrentUserId(),
            (bool isSuccessful, string message, GetPlayerInfoResponse result) =>
            {
                if (isSuccessful)
                {
                    GameplayStatics.currentPlayer = result.player;
                    txtUsername.text = GameplayStatics.currentPlayer.username;
                }
                else
                {
                    Debug.LogWarning(message);
                }
            }
        );
    }

    private void Logout()
    {
        // Immediately logout if the account is offline.
        if (!GameplayStatics.IsOnline)
        {
            GameplayStatics.SetGameplayOnlineMode(true);
            MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LOGIN);
            return;
        }

        PromptSubsystem.Instance.ShowLoading("Sedang Logout...");

        // Logout from AWS Cognito.
        AWSAuthSubsystem.Instance.Logout((bool isSuccessful, string message) =>
        {
            PromptSubsystem.Instance.HideLoading();

            if (isSuccessful)
            {
                MenuManager.Instance.SwitchWidget(MenuManager.MenuType.LOGIN);
            }
            else
            {
                PromptSubsystem.Instance.ShowPopUp("Gagal Logout", message, PopUpWidget.PopUpType.OK);
            }
        });
    }
}