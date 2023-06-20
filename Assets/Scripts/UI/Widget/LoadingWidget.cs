using UnityEngine;
using TMPro;

public class LoadingWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private TMP_Text txtLoading;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        SetLoadingMessage("Loading...");
    }

    public void SetLoadingMessage(string message)
    {
        txtLoading.text = message;
    }
}
