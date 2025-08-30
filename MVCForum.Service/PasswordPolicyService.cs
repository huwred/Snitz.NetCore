using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SnitzCore.Data.Interfaces;
using System.Collections.Generic;

public class PasswordPolicyService : IPasswordPolicyService
{
    private readonly IdentityOptions _options;

    public PasswordPolicyService(IOptions<IdentityOptions> options)
    {
        _options = options.Value;
    }

    public List<string> GetPasswordRequirements()
    {
        var requirements = new List<string>();

        if (_options.Password.RequireDigit)
            requirements.Add("Must contain at least one digit (0-9).");

        if (_options.Password.RequireLowercase)
            requirements.Add("Must contain at least one lowercase letter (a-z).");

        if (_options.Password.RequireUppercase)
            requirements.Add("Must contain at least one uppercase letter (A-Z).");

        if (_options.Password.RequireNonAlphanumeric)
            requirements.Add("Must contain at least one special character (e.g., !, @, #).");

        requirements.Add($"Must be at least {_options.Password.RequiredLength} characters long.");
        requirements.Add($"Must contain at least {_options.Password.RequiredUniqueChars} unique characters.");

        return requirements;
    }
}
