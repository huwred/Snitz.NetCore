using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SnitzCore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public string GeneratePassword()
    {
        var opts = new PasswordOptions()
        {
            RequiredLength = _options.Password.RequiredLength,
            RequiredUniqueChars = _options.Password.RequiredUniqueChars,
            RequireDigit = _options.Password.RequireDigit,
            RequireLowercase = _options.Password.RequireLowercase,
            RequireNonAlphanumeric = _options.Password.RequireNonAlphanumeric,
            RequireUppercase = _options.Password.RequireUppercase
        };

        string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-^"                        // non-alphanumeric
        };

        Random rand = new Random(Environment.TickCount);
        List<char> chars = new List<char>();

        if (opts.RequireUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (opts.RequireLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (opts.RequireDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (opts.RequireNonAlphanumeric)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

        for (int i = chars.Count; i < opts.RequiredLength
                                    || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
        {
            string rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }

}
