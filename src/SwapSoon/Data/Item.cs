namespace SwapSoon.Data;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int IntervalDays { get; set; }

    public DateOnly LastReplacedOn { get; set; }

    public List<Replacement> Replacements { get; set; } = [];
}
