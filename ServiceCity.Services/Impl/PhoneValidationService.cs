using PhoneNumbers;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Services.Impl;

public class PhoneValidationService : IPhoneValidationService
{
    public (bool IsValid, string? Normalized, string? Error) Validate(string input)
    {
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            var number = util.Parse(input, "MM");
            if (!util.IsValidNumber(number))
                return (false, null, "Please enter a valid Myanmar phone number.");

            var normalized = util.Format(number, PhoneNumberFormat.E164);
            return (true, normalized, null);
        }
        catch (NumberParseException)
        {
            return (false, null, "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789).");
        }
    }
}
