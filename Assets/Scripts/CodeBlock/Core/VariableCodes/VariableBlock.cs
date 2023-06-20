using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableBlock : CodeBlock
{
    [Header("Variable Settings")]
    public VariableValue variableValue;

    public override void AssignNextCode(CodeBlock newCode, CodePort parentPort)
    {
        // NOTE: Code blocks cannot be assigned to variable block. Variable block is independent.
        return;
    }

    public override void DetachFromSiblings()
    {
        // NOTE: Variable block simply detach from parent, it doesn't need to update its parent hierarchy.
        RectTransform tParent = containerPort ? containerPort.containerRect : null;
        transform.SetParent(originTransform);
        containerPort = null;
    }

    public override void TryReassign()
    {
        // NOTE: Variable block simply try to reassign, it doesn't need to update its parent hierarchy.

        // Try to reassign.
        if (lastConfig.port)
        {
            containerPort = lastConfig.port;
            transform.SetParent(containerPort.containerRect);
            transform.SetSiblingIndex(lastConfig.siblingIndex);
        }
        // If failed to reassign, then destroy.
        else if (deleteOnFailedReassign)
        {
            Delete();
        }
    }

    public override void Delete(bool destroyObject = true)
    {
        // NOTE: Variable block simply need to be deleted, it doesn't need to recurse delete to its parent or siblings.

        // If code block was assigned, then delete its history from code manager.
        if (lastConfig.port || containerPort)
        {
            CodeManager.Instance.OnCodeDeleted();
        }

        Destroy(gameObject);
    }
}
