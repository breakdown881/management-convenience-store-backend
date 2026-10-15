using ConvenienceStore.Application.Common.Interfaces;

namespace ConvenienceStore.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Sử dụng WorkFactor = 12 (chuẩn an toàn chống Brute-Force)
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
