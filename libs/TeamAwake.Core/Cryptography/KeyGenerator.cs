using System.Text;

namespace TeamAwake.Core.Cryptography;

/// <summary>Describes a way to generate a key of type <see cref="string"/>.</summary>
public static class KeyGenerator
{
    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    private const string LettersAndNumbers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    
    /// <summary>Generate a random key of type <see cref="string"/>.</summary>
    /// <returns>A encrypted key.</returns>
    public static string GenerateKey()
    {
        var builder = new StringBuilder();

        for (var i = 0; i < 32; i++)
            builder.Append(Letters[Random.Shared.Next(Letters.Length)]);

        return builder.ToString();
    }

    public static string EncryptPassword(string key, string password)
    {
        var builder = new StringBuilder("#1");

        for (var i = 0; i < password.Length; i++)
        {
            var pPass = password[i];
            var pKey = key[i];

            var aPass = pPass / 16;
            var aKey = pPass % 16;

            builder
                .Append(LettersAndNumbers[(aPass + pKey) % LettersAndNumbers.Length])
                .Append(LettersAndNumbers[(aKey + pKey) % LettersAndNumbers.Length]);
        }

        return builder.ToString();
    }
}