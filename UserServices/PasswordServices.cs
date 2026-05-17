using Entities;

namespace Service
{
    public class PasswordServices : IPasswordServices
    {
        public Password GetStrength(string password)
        {
            var result = Zxcvbn.Core.EvaluatePassword(password);
            Password password1 = new() { PasswordValue = password, Strength = result.Score };
            return password1;
        }

        public string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
