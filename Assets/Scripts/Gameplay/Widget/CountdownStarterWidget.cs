using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownStarterWidget : BaseWidget
{
    [SerializeField] private TMP_Text txtCountdown;
    [SerializeField] private float countDown = 3;
    private float internalCountDown;

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();
        internalCountDown = countDown;
    }

    protected override void OnWidgetDeactivated()
    {
        base.OnWidgetDeactivated();
    }

    private void Update()
    {
        if (internalCountDown <= 0)
        {
            GameManager.Instance.OnGameStarted();
            SoundManager.Instance.PlayNotificationSound();
            Deactivate();
            return;
        }
        
        txtCountdown.text = Mathf.CeilToInt(internalCountDown).ToString();
        internalCountDown -= Time.deltaTime;
    }
}