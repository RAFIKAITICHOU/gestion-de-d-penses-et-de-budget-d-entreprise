using System;

namespace GestionBudget_V2.Services
{
    public static class PasswordHasher
    {
        // Hacher un mot de passe
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Vérifier un mot de passe
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}