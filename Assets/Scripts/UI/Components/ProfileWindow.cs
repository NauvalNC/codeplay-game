using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputNewUsername;
    [SerializeField] private Button btnChangeUsername;
    [SerializeField] private Button btnClose;
    [HideInInspector] public MainMenuWidget ownerMainMenuWidget;

    private void Awake()
    {
        inputNewUsername.text = GameplayStatics.currentPlayer.username;

        btnChangeUsername.onClick.AddListener(ChangeUsername);
        btnClose.onClick.AddListener(() => { gameObject.SetActive(false); });
    }

    private void ChangeUsername()
    {
        string tNewUsername = inputNewUsername.text.Trim();
        if (string.IsNullOrEmpty(tNewUsername) || tNewUsername == GameplayStatics.currentPlayer.username || tNewUsername.Length <= 5)
        {
            PromptSubsystem.Instance.ShowPopUp("Gagal Ganti Username", "Username baru tidak boleh kosong, kurang dari 5 karakter, atau sama dengan username lama.", PopUpWidget.PopUpType.OK);
            return;
        }

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.SetPlayerInfo(
            GameplayStatics.currentPlayer.player_id,
            tNewUsername,
            (bool isSuccessful, string message, SetPlayerInfoResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    PromptSubsystem.Instance.ShowPopUp("Ganti Username", "Username berhasil diganti.", PopUpWidget.PopUpType.OK);
                    ownerMainMenuWidget?.UpdateProfileData();
                }
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.LogWarning(message);
                }
            }
        );
    }
}
