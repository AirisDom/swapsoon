using Microsoft.Extensions.Time.Testing;
using SwapSoon.Domain;

namespace SwapSoon.Tests;

public class UrgencyCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 3, 10);

    private readonly FakeTimeProvider _fake = new(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));
    private readonly UrgencyCalculator _calculator;

    public UrgencyCalculatorTests()
    {
        _calculator = new UrgencyCalculator(_fake);
    }

    [Fact]
    public void Today_UsesTimeProviderDate()
    {
        Assert.Equal(Today, _calculator.Today());
    }

    [Theory]
    [InlineData(-1, Urgency.Overdue)]
    [InlineData(0, Urgency.DueThisWeek)]
    [InlineData(7, Urgency.DueThisWeek)]
    [InlineData(8, Urgency.Fine)]
    public void Classify_ReturnsExpectedUrgency(int daysFromToday, Urgency expected)
    {
        var due = Today.AddDays(daysFromToday);

        Assert.Equal(expected, _calculator.Classify(due));
    }

    [Theory]
    [InlineData("2026-03-01", 30, "2026-03-31")]
    [InlineData("2026-01-31", 30, "2026-03-02")]
    [InlineData("2027-12-15", 90, "2028-03-14")]
    public void NextDue_AddsIntervalToLastReplaced(string lastReplaced, int intervalDays, string expected)
    {
        var next = UrgencyCalculator.NextDue(DateOnly.Parse(lastReplaced), intervalDays);

        Assert.Equal(DateOnly.Parse(expected), next);
    }

    [Fact]
    public void DaysUntil_IsNegativeForOverdueDate()
    {
        var due = new DateOnly(2026, 3, 7);

        Assert.Equal(-3, _calculator.DaysUntil(due));
    }

    [Fact]
    public void DaysUntil_IsPositiveForFutureDate()
    {
        var due = new DateOnly(2026, 3, 20);

        Assert.Equal(10, _calculator.DaysUntil(due));
    }

    [Fact]
    public void Classify_ChangesWhenTimeAdvances()
    {
        var due = new DateOnly(2026, 3, 15);
        Assert.Equal(Urgency.DueThisWeek, _calculator.Classify(due));

        _fake.Advance(TimeSpan.FromDays(10));

        Assert.Equal(Urgency.Overdue, _calculator.Classify(due));
        Assert.Equal(-5, _calculator.DaysUntil(due));
    }

    [Fact]
    public void Classify_MovesFineToDueThisWeekWhenTimeAdvances()
    {
        var due = new DateOnly(2026, 3, 25);
        Assert.Equal(Urgency.Fine, _calculator.Classify(due));

        _fake.Advance(TimeSpan.FromDays(10));

        Assert.Equal(Urgency.DueThisWeek, _calculator.Classify(due));
    }
}
