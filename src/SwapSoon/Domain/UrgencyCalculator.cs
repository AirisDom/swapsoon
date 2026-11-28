namespace SwapSoon.Domain;

public class UrgencyCalculator(TimeProvider timeProvider)
{
    public const int DueSoonWindowDays = 7;

    public DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    public static DateOnly NextDue(DateOnly lastReplaced, int intervalDays) =>
        lastReplaced.AddDays(intervalDays);

    public int DaysUntil(DateOnly due) => due.DayNumber - Today().DayNumber;

    public Urgency Classify(DateOnly due)
    {
        var days = DaysUntil(due);

        if (days < 0)
        {
            return Urgency.Overdue;
        }

        return days <= DueSoonWindowDays ? Urgency.DueThisWeek : Urgency.Fine;
    }
}
