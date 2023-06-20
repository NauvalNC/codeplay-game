using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class CheckerTile : MonoBehaviour
{
    protected enum ECheckerType
    {
        REQUIRE,
        EXCEPT
    }

    [Header("Generics")]
    [SerializeField] protected ECheckerType checkerType;

    [Header("User Interface")]
    [SerializeField] protected Sprite icon;
    [SerializeField] protected Image imgIcon;
    [SerializeField] protected GameObject negateIcon;

    private bool isChecked = false;
    protected bool isPass = false;

    private void OnValidate()
    {
        imgIcon.sprite = icon;
        negateIcon.SetActive(checkerType == ECheckerType.EXCEPT);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController tPlayer = other.GetComponent<PlayerController>();
        if (tPlayer)
        {
            isChecked = false;
            CodeManager.Instance.onCurrentCodeExecutedDelegate += CheckOnExecuteCode;

            OnPreCheck();
        }
    }

    public virtual void OnPreCheck()
    {
        isPass = false;
    }

    public virtual void ResetCheck()
    {
        isChecked = false;
        isPass = false;
    }

    public virtual void CheckOnExecuteCode(CodeAction codeAction)
    {
        if (isChecked) return;
        isChecked = true;

        CodeManager.Instance.onCurrentCodeExecutedDelegate -= CheckOnExecuteCode;
    }
}
