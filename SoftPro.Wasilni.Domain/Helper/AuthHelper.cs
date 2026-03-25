using Microsoft.IdentityModel.Tokens;
using SoftPro.Wasilni.Domain.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SoftPro.Wasilni.Domain.Helper;

public class AuthHelper()
{
    public static DateTime GetExpirationDate(JwtOption jwtOption)
        => DateTime.UtcNow.AddHours(3).AddDays(jwtOption.DurationExpiredInDayJWT);

    public static bool Equals(byte[] first, byte[] second)
        => Encoding.UTF8.GetString(first) == Encoding.UTF8.GetString(second);

    public static (string token, DateTime expirationDate) GenerateToken(List<Claim> claim, JwtOption jwtOption)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtOption.keyJwt);
        DateTime expirationDate = DateTime.UtcNow.AddHours(3).AddDays(jwtOption.DurationExpiredInDayJWT);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claim),
            Expires = expirationDate,
            Issuer = jwtOption.IssuerJwt,
            Audience = jwtOption.AudienceJwt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expirationDate);
    }

    public static string GenerateCode(int codeLength = 6)
    {
        StringBuilder sb = new();
        for (int i = 0; i < codeLength; i++)
        {
            sb.Append($"{Random.Shared.Next(0, 10)}");
        }
        return sb.ToString();
    }

    public static byte[] GenerateSalt(int size = 16)
    {
        byte[] salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    public static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[passwordBytes.Length + salt.Length];

            Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);

            return sha256.ComputeHash(combined);
        }
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
