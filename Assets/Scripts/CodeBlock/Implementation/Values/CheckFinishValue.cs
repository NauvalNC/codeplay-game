using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFinishValue : VariableValue
{
    public override bool GetBooleanValue()
    {
        return !GameManager.Instance.isGameOver;
    }
}
