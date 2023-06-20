public class CheckCanGoLeft : VariableValue
{
    public override bool GetBooleanValue()
    {
        return CodeManager.Instance.player.CanGoLeft;
    }
}