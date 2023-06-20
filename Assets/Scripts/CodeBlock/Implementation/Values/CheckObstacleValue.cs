public class CheckObstacleValue : VariableValue
{
    public override bool GetBooleanValue()
    {
        return CodeManager.Instance.player.HasObstracleAhead;
    }
}