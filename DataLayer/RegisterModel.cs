using Microsoft.AspNetCore.Http;

namespace DataLayer
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }  // Имя
        public string LastName { get; set; }   // Фамилия
        public string MiddleName { get; set; } // Отчество
        public IFormFile ProfileImage { get; set; } // Файл изображения профиля
    }
}