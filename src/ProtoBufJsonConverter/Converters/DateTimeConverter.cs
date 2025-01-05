namespace ProtoBufJsonConverter.Converters;

internal static class DateTimeConverter
{
    internal static (long Seconds, int Nanos) ToTimestampValue(DateTime dateTime)
    {
        var seconds = new DateTimeOffset(dateTime).ToUnixTimeSeconds();

        // Extract nanoseconds
        var ticks = dateTime.Ticks % TimeSpan.TicksPerSecond; // Fractional ticks in the current second
        var nanos = (int)ticks * 100; // Convert ticks to nanoseconds

        return (seconds, nanos);
    }

    internal static DateTime FromTimestampValue(long seconds, int nanos)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
        return dateTimeOffset.DateTime.AddTicks(nanos / 100); // Convert nanoseconds back to ticks and add to DateTime
    }
}