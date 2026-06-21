namespace ServiceCity.Services.Interfaces;

public interface IPhoneValidationService
{
    (bool IsValid, string? Normalized, string? Error) Validate(string input);
}
