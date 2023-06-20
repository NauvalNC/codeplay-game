using System.Collections;
using UnityEngine;

public class MovementAction : CodeAction
{
    public enum EMovementType
    {
        NONE,
        MOVE_FORWARD,
        ROTATE_LEFT,
        ROTATE_RIGHT
    }

    public EMovementType movementType = EMovementType.NONE;

    public override IEnumerator Execute()
    {
        yield return base.Execute();

        switch (movementType)
        {
            case EMovementType.MOVE_FORWARD:
                CodeManager.Instance.player.MoveForward();
                break;
            case EMovementType.ROTATE_LEFT:
                CodeManager.Instance.player.RotateLeft();
                break;
            case EMovementType.ROTATE_RIGHT:
                CodeManager.Instance.player.RotateRight();
                break;
            default:
                Debug.Log("There is no movement definition. Abort.");
                break;
        }

        yield return new WaitForSeconds(0.75f);
    }
}
