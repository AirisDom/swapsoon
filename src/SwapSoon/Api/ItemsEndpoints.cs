using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SwapSoon.Data;
using SwapSoon.Domain;

namespace SwapSoon.Api;

public static class ItemsEndpoints
{
    public const int MaxNameLength = 100;
    public const int MinIntervalDays = 1;
    public const int MaxIntervalDays = 3650;

    public static IEndpointRouteBuilder MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items");

        group.MapGet("/", ListItems);
        group.MapPost("/", CreateItem);
        group.MapDelete("/{id:int}", DeleteItem);

        return app;
    }

    private static async Task<Ok<List<ItemResponse>>> ListItems(
        AppDbContext db,
        UrgencyCalculator calculator,
        CancellationToken cancellationToken)
    {
        var items = await db.Items.AsNoTracking().ToListAsync(cancellationToken);

        var responses = items
            .Select(item => ToResponse(item, calculator))
            .OrderBy(r => r.Urgency)
            .ThenBy(r => r.NextDue)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return TypedResults.Ok(responses);
    }

    private static async Task<Results<Created<ItemResponse>, ValidationProblem>> CreateItem(
        CreateItemRequest request,
        AppDbContext db,
        UrgencyCalculator calculator,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var item = new Item
        {
            Name = request.Name.Trim(),
            IntervalDays = request.IntervalDays,
            LastReplacedOn = request.LastReplacedOn,
        };
        item.Replacements.Add(new Replacement { ReplacedOn = request.LastReplacedOn });

        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/items/{item.Id}", ToResponse(item, calculator));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteItem(
        int id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var deleted = await db.Items
            .Where(i => i.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0 ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static Dictionary<string, string[]> Validate(CreateItemRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(CreateItemRequest.Name)] = ["Name is required."];
        }
        else if (request.Name.Trim().Length > MaxNameLength)
        {
            errors[nameof(CreateItemRequest.Name)] =
                [$"Name must be at most {MaxNameLength} characters."];
        }

        if (request.IntervalDays is < MinIntervalDays or > MaxIntervalDays)
        {
            errors[nameof(CreateItemRequest.IntervalDays)] =
                [$"Interval must be between {MinIntervalDays} and {MaxIntervalDays} days."];
        }

        return errors;
    }

    private static ItemResponse ToResponse(Item item, UrgencyCalculator calculator)
    {
        var nextDue = UrgencyCalculator.NextDue(item.LastReplacedOn, item.IntervalDays);

        return new ItemResponse(
            item.Id,
            item.Name,
            item.IntervalDays,
            item.LastReplacedOn,
            nextDue,
            calculator.DaysUntil(nextDue),
            calculator.Classify(nextDue));
    }
}
