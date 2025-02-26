using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DataLayer
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }  // Имя
        public string LastName { get; set; }   // Фамилия
        public string MiddleName { get; set; } // Отчество
        public string ProfileImagePath { get; set; } // Путь к изображению профиля
        public ICollection<TestAttempt> TestAttempts { get; set; }
    }
}