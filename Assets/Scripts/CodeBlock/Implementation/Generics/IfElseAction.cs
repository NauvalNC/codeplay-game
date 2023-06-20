using System.Collections;
using UnityEngine;

public class IfElseAction : CodeAction
{
    private enum EIfElseType
    {
        IF_STATEMENT,
        IF_ELSE_STATEMENT
    }

    [Header("Generics")]
    [SerializeField] private EIfElseType statementType = EIfElseType.IF_ELSE_STATEMENT;
    [SerializeField] private bool overrideStatement = false;
    [SerializeField] private bool overrideTrue = true;

    [Header("User Interfaces")]
    [SerializeField] private GameObject[] elseStatementObj;

    private void OnValidate()
    {
        foreach (GameObject tObj in elseStatementObj)
        {
            tObj.SetActive(statementType == EIfElseType.IF_ELSE_STATEMENT);
        }
    }

    public override IEnumerator Execute()
    {
        yield return base.Execute();

        // Initially, the condition is always false.
        int tPortIndexToExecute = 1;

        // Check condition based on slot variable.
        VariableBlock tValue = GetOwner().slotPorts[0].GetSlotData();
        if (tValue)
        {
            tPortIndexToExecute = tValue.variableValue.GetBooleanValue() ? 0 : 1;
        }

        // Check override (only for debugging).
        if (overrideStatement)
        {
            tPortIndexToExecute = overrideTrue ? 0 : 1;
        }

        // If no else statement and the condition is false, then cannot execute code.
        if (statementType == EIfElseType.IF_STATEMENT && tPortIndexToExecute != 0)
        {
            yield return null;
        }
        // If there is else statement, execute code based on condition.
        else
        {
            RectTransform tContainer = GetOwner().innerPorts[tPortIndexToExecute].containerRect;
            int tChildCount = tContainer.childCount;
            for (int j = 0; j < tChildCount; j++)
            {
                CodeAction tCode = tContainer.GetChild(j).GetComponent<CodeAction>();
                yield return tCode.Execute();
            }
        }
    }
}
