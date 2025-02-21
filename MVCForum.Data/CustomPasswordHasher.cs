using Microsoft.AspNetCore.Identity;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SnitzCore.Data
{
    public class CustomPasswordHasher : PasswordHasher<IdentityUser>
{
    public override PasswordVerificationResult VerifyHashedPassword(IdentityUser user, string hashedPassword,
        string providedPassword)
    {
        var isValidPasswordWithLegacyHash = VerifyHashedPassword(hashedPassword, providedPassword);
        return isValidPasswordWithLegacyHash
            ? PasswordVerificationResult.SuccessRehashNeeded
            : base.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    private const int _pbkdf2IterCount = 1000;
    private const int _pbkdf2SubkeyLength = 256 / 8;
    private const int _saltSize = 128 / 8;

    public static bool VerifyHashedPassword(string hashedPassword, string password)
    {
        //Checks password using legacy hashing from System.Web.Helpers.Crypto
        var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
        if (hashedPasswordBytes.Length != (1 + _saltSize + _pbkdf2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
        {
            return false;
        }
        var salt = new byte[_saltSize];
        Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, _saltSize);
        var storedSubkey = new byte[_pbkdf2SubkeyLength];
        Buffer.BlockCopy(hashedPasswordBytes, 1 + _saltSize, storedSubkey, 0, _pbkdf2SubkeyLength);
        byte[] generatedSubkey;
        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, _pbkdf2IterCount,HashAlgorithmName.SHA1))
        {
            generatedSubkey = deriveBytes.GetBytes(_pbkdf2SubkeyLength);
        }
        return ByteArraysEqual(storedSubkey, generatedSubkey);
    }

    internal static string BinaryToHex(byte[] data)
    {
        var hex = new char[data.Length * 2];
        for (var iter = 0; iter < data.Length; iter++)
        {
            var hexChar = (byte) (data[iter] >> 4);
            hex[iter * 2] = (char) (hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
            hexChar = (byte) (data[iter] & 0xF);
            hex[iter * 2 + 1] = (char) (hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
        }
        return new string(hex);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static bool ByteArraysEqual(byte[] a, byte[] b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }
        var areSame = true;
        for (var i = 0; i < a.Length; i++)
        {
            areSame &= (a[i] == b[i]);
        }
        return areSame;
    }
}
}
