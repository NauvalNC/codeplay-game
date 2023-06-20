using System.Collections;
using UnityEngine;

public class WaitForSecondsAction : CodeAction
{
    [SerializeField] private int secondsToWait = 1;

    public override IEnumerator Execute()
    {
        yield return base.Execute();
        yield return new WaitForSeconds(secondsToWait);
    }
}
