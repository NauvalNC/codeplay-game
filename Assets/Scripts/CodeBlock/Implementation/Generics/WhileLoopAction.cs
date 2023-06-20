using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhileLoopAction : CodeAction
{
    [SerializeField] private bool shouldExecute = false;

    public override IEnumerator Execute()
    {
        yield return base.Execute();

        // Check condition based on slot variable.
        VariableBlock tValue = GetOwner().slotPorts[0].GetSlotData();
        if (tValue)
        {
            shouldExecute = tValue.variableValue.GetBooleanValue();
        }

        // Continue to execute code if condition is true.
        RectTransform tContainer = GetOwner().innerPorts[0].containerRect;
        int tIterate = 0;
        while(shouldExecute)
        {
            if (tIterate > 0) yield return base.Execute();
            else tIterate++;

            if (CodeManager.Instance.isBreakRequested) break;

            if (!shouldExecute)
            {
                yield return null;
                break;
            }

            int tChildCount = tContainer.childCount;
            for (int j = 0; j < tChildCount; j++)
            {
                if (CodeManager.Instance.isBreakRequested) break;

                CodeAction tCode = tContainer.GetChild(j).GetComponent<CodeAction>();
                yield return tCode.Execute();
            }
        }

        CodeManager.Instance.isBreakRequested = false;
    }
}
