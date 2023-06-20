using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableValue : MonoBehaviour
{
    public virtual bool GetBooleanValue()
    {
        return true;
    }

    public virtual string GetStringValue()
    {
        return string.Empty;
    }

    public virtual int GetIntValue()
    {
        return 0;
    }

    public virtual float GetFloatValue()
    {
        return 0f;
    }
}
