using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        // Необязательные поля для ФИО
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public IFormFile ProfileImage { get; set; }

        // Метод для получения био
        public string GetBio()
        {
            return $"{FirstName ?? "No bio yet"} {LastName ?? "No bio yet"} {MiddleName ?? "No bio yet"}".Trim();
        }
    }
}