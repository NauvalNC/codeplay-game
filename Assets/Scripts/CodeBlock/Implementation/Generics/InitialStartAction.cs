using System.Collections;
using UnityEngine;

public class InitialStartAction : CodeAction
{
    public override IEnumerator Execute()
    {
        yield return base.Execute();

        RectTransform tContainer = GetOwner().innerPorts[0].containerRect;
        int tChildCount = tContainer.childCount;

        for (int i = 0; i < tChildCount; i++)
        {
            CodeAction tCode = tContainer.GetChild(i).GetComponent<CodeAction>();
            yield return tCode.Execute();
        }

        CodeManager.Instance.StopCodeExecution();
    }
}