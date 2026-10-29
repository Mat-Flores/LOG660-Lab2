namespace Lab2.Infrastructure.Exceptions;

// Violation d'une règle d'affaires (quota, inventaire, ...), traduite en code HTTP par l'Api.
public class RegleAffairesException : Exception
{
    public int StatusCode { get; }

    public RegleAffairesException(string message, int statusCode = 409) : base(message)
        => StatusCode = statusCode;
}
