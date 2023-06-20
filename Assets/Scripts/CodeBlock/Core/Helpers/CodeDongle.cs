using UnityEngine;

/* Class to attach codeblock to detected port. */
public class CodeDongle : MonoBehaviour
{
    [SerializeField] private CodeBlock codeOwner;
    [SerializeField] private CodeBlock olderSiblingCode;
    private CodeBlock lastSiblingCode = null;
    private CodePort targetPort;
    private CodeTrashBin trashBin;
    private bool isCodeOwnerVariable = false;

    private void Start()
    {
        isCodeOwnerVariable = codeOwner is VariableBlock;

        // When begin drag, always reset codeblock relationship.
        codeOwner.onBeginDragDelegate += () =>
        {
            codeOwner.DetachFromSiblings();
            ClearDongleData();
        };

        codeOwner.onDragDelegate += () =>
        {
            lastSiblingCode?.OnPortExitHover();
            lastSiblingCode = olderSiblingCode;

            olderSiblingCode?.OnPortHovered();
        };

        codeOwner.onEndDragDelegate += () =>
        {
            lastSiblingCode?.OnPortExitHover();

            // Delete the code if it was dragged to trash bin.
            if (trashBin)
            {
                trashBin.ToggleHover(false);
                codeOwner.Delete();
            } 
            // Assign code to older sibling code if any.
            else if (olderSiblingCode)
            {
                olderSiblingCode.OnPortExitHover();
                olderSiblingCode.AssignNextCode(codeOwner, targetPort);
            }
            // Fallback, try to reassign code the the original sibling code.
            else
            {
                codeOwner.TryReassign();
            }
        };
    }

    public void SetOwner(CodeBlock owner) { codeOwner = owner; }

    public CodeBlock GetOwner() { return codeOwner; }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!codeOwner.IsDragging)
        {
            ClearDongleData();
            return;
        }

        if (collision.GetComponent<CodeTrashBin>())
        {
            ClearDongleData();
            trashBin = collision.GetComponent<CodeTrashBin>();
            trashBin.ToggleHover(true);
            return;
        }

        CodePort tPort = collision.GetComponent<CodePort>();
        if (tPort && tPort.GetOwner().isAssignable)
        {
            if ((tPort.isSlotPort && !isCodeOwnerVariable) || (!tPort.isSlotPort && isCodeOwnerVariable))
            {
                return;
            }

            ClearDongleData();
            olderSiblingCode = tPort.GetOwner();
            targetPort = tPort;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!codeOwner.IsDragging)
        {
            return;
        }

        if (trashBin)
        {
            trashBin.ToggleHover(false);
        }

        ClearDongleData();
    }

    private void ClearDongleData()
    {
        trashBin = null;
        olderSiblingCode = null;
        targetPort = null;
    }

    private void OnDestroy()
    {
        olderSiblingCode?.OnPortExitHover();
    }
}