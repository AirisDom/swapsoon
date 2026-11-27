namespace SwapSoon.Data;

public class Replacement
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public DateOnly ReplacedOn { get; set; }
}
