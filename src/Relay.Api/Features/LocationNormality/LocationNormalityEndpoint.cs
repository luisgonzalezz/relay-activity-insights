namespace Relay.Api.Features.LocationNormality;

public static class LocationNormalityEndpoint
{
    public static async Task<IResult> GetAsync(
        int accountId,
        string eventType,
        LocationNormalityService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.GetAsync(accountId, eventType, cancellationToken);
            return response is null
                ? Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Account not found",
                    detail: $"No account exists with id {accountId}.")
                : Results.Ok(response);
        }
        catch (UnsupportedEventTypeException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unsupported event type",
                detail: exception.Message);
        }
        catch (AccountHasNoActivityException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Account has no activity",
                detail: exception.Message);
        }
    }
}
