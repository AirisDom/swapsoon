using SwapSoon.Domain;

namespace SwapSoon.Api;

public record CreateItemRequest(string Name, int IntervalDays, DateOnly LastReplacedOn);

public record ItemResponse(
    int Id,
    string Name,
    int IntervalDays,
    DateOnly LastReplacedOn,
    DateOnly NextDue,
    int DaysUntilDue,
    Urgency Urgency);
