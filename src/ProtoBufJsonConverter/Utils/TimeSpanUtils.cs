namespace ProtoBufJsonConverter.Utils;

internal static class TimeSpanUtils
{
    internal static (long Seconds, int Nanos) ToDurationValue(TimeSpan timeSpan)
    {
        var seconds = (long)timeSpan.TotalSeconds;

        // Extract nanoseconds
        var ticks = timeSpan.Ticks % TimeSpan.TicksPerSecond; // Fractional ticks in the current second
        var nanos = (int)ticks * 100; // Convert ticks to nanoseconds

        return (seconds, nanos);
    }

    internal static TimeSpan FromDurationValue(long seconds, int nanos)
    {
        var ticks = seconds * TimeSpan.TicksPerSecond + nanos / 100; // Convert seconds and nanoseconds to ticks
        return TimeSpan.FromTicks(ticks);
    }
}