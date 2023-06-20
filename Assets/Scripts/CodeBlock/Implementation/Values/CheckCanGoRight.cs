public class CheckCanGoRight : VariableValue
{
    public override bool GetBooleanValue()
    {
        return CodeManager.Instance.player.CanGoRight;
    }
}