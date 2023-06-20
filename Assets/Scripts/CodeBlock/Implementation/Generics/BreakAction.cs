using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakAction : CodeAction
{
    public override IEnumerator Execute()
    {
        yield return base.Execute();
        CodeManager.Instance.isBreakRequested = true;
    }
}
