using UnityEngine;

public class CodeCheckerTile : CheckerTile
{
    [Header("Check Code")]
    [SerializeField] private string[] actionsToCheck;

    public override void CheckOnExecuteCode(CodeAction codeAction)
    {
        base.CheckOnExecuteCode(codeAction);

        isPass = false;
        bool tFound = false;
        foreach (string tAction in actionsToCheck)
        {
            Debug.Log("Check for: " + codeAction.GetType().Name);

            if (codeAction.GetType().Name == tAction)
            {
                tFound = true;
                break;
            }
        }

        // Check permission.
        switch (checkerType)
        {
            case ECheckerType.REQUIRE:
                isPass = tFound;
                break;
            case ECheckerType.EXCEPT:
                isPass = !tFound;
                break;
        }

        if (!isPass)
        {
            GameManager.Instance.ResetLevel();
        }
    }
}
