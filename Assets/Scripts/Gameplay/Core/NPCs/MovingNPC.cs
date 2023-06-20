using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pawn))]
public class MovingNPC : MonoBehaviour
{
    [SerializeField] private MovementAction.EMovementType[] movementSequence;
    private Pawn pawn;
    private int sequenceIndex;

    private void Awake()
    {
        Debug.Log("NANI!");

        pawn = GetComponent<Pawn>();
        CodeManager.Instance.onCodeExecutionStopDelegate += ResetNPC;
        CodeManager.Instance.onCurrentCodeExecutedDelegate += MoveNPC;
    }

    private void MoveNPC(CodeAction codeAction)
    {
        Debug.Log("NANI 2!");

        if (sequenceIndex >= movementSequence.Length || !(codeAction is MovementAction) //|| 
            /*((MovementAction)codeAction).movementType != MovementAction.EMovementType.MOVE_FORWARD*/)
        {
            return;
        }

        MovementAction.EMovementType tMovement = movementSequence[sequenceIndex++];
        switch(tMovement)
        {
            case MovementAction.EMovementType.MOVE_FORWARD:
                pawn.MoveForward();
                break;
            case MovementAction.EMovementType.ROTATE_LEFT:
                pawn.RotateLeft();
                break;
            case MovementAction.EMovementType.ROTATE_RIGHT:
                pawn.RotateRight();
                break;
        }
    }

    private void ResetNPC()
    {
        Debug.Log("NANI 3!");

        sequenceIndex = 0;
        pawn.ResetPawn();
    }
}
