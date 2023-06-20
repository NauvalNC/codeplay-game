public class UtilitySubsystem
{
    public static string FormatTime(float inSeconds)
    {
        float tHour = (int)(inSeconds / 3600f);
        inSeconds -= tHour * 3600f;

        float tMinute = (int)(inSeconds / 60f);
        inSeconds -= tMinute * 60f;

        inSeconds = (int)inSeconds;

        return string.Format($"{tHour:00}:{tMinute:00}:{inSeconds:00}");
    }
}