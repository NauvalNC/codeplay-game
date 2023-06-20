using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;
    public static MenuManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<MenuManager>();
            }
            return instance;
        }
    }

    public enum MenuType
    {
        LOGIN,
        REGISTER,
        MAIN_MENU,
        CHAPTERS,
        LEVELS,
        RESET_PASSWORD,
        TUTORIALS,
        TUTORIAL_DETAILS,
        SKINS
    };

    [System.Serializable]
    public struct MenuWidget
    {
        public MenuType type;
        public BaseWidget widget;
    };

    [Header("Generic")]
    public List<MenuWidget> menuWidgets;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (GameplayStatics.FORCE_OFFLINE || !GameplayStatics.IsOnline)
        {
            GameplayStatics.SetGameplayOnlineMode(false);
            SwitchWidget(MenuType.MAIN_MENU);
            return;
        } 
        else if (GameplayStatics.isLoggedIn)
        {
            SwitchWidget(MenuType.MAIN_MENU);
            return;
        }

        // Refresh AWS Cognito session.
        if (AWSAuthSubsystem.Instance.HasCachedSession())
        {
            PromptSubsystem.Instance.ShowLoading("Menyegarkan Sesi...");
            AWSAuthSubsystem.Instance.RefreshSession((bool isSuccessful, string message) =>
            {
                MainThreadWorker.worker.AddJob(() =>
                {
                    PromptSubsystem.Instance.HideLoading();

                    if (isSuccessful)
                    {
                        Debug.Log("Refresh session success, user already logged in.");
                        NetworkingSubsystem.Instance.CheckAPIConnection();
                        SwitchWidget(MenuType.MAIN_MENU);
                    }
                    else
                    {
                        Debug.Log("Refresh session failed, user need to login again.");
                        SwitchWidget(MenuType.LOGIN);
                    }
                });
            });
        }
    }

    public void SwitchWidget(MenuType type)
    {
        foreach(MenuWidget menuWidget in menuWidgets)
        {
            if (menuWidget.type != type)
            {
                menuWidget.widget.Deactivate();
            } else
            {
                menuWidget.widget.Activate();
            }
        }
    }
}