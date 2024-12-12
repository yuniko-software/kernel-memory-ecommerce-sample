namespace KernelMemory.Ecommerce.Sample.Api.Presentation;

public static class ApiResults
{
    private const string BadRequestRfc = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

    public static IResult Problem(string problemTitle, string problemDetail)
        => Results.Problem(
            title: problemTitle,
            detail: problemDetail,
            type: BadRequestRfc,
            statusCode: StatusCodes.Status400BadRequest);
}
