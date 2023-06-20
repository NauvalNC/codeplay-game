using UnityEngine;
using UnityEngine.UI;

public class BaseWidget : MonoBehaviour
{
    [Header("Back Behavior")]
    public bool isBackHandler = false;
    [SerializeField] protected Button btnBack;
    [SerializeField] MenuManager.MenuType backDestination;

    private void Awake()
    {
        SetupWidget();
    }

    private void OnEnable()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        OnWidgetActivated();
    }

    protected void OnDisable()
    {
        if (gameObject.activeInHierarchy)
        {
            return;
        }

        OnWidgetDeactivated();
    }

    protected virtual void OnWidgetActivated()
    {

    }

    protected virtual void OnWidgetDeactivated()
    {

    }

    protected virtual void SetupWidget()
    {
        if (isBackHandler && btnBack)
        {
            btnBack.onClick.AddListener(() =>
            {
                MenuManager.Instance.SwitchWidget(backDestination);
            });
        }
    }

    protected virtual void ResetWidget()
    {

    }

    public void Activate()
    {
        enabled = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        enabled = false;
    }
}
