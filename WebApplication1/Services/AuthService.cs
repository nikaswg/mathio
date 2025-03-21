﻿using DataLayer; // Убедитесь, что это ваше пространство имен
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger; // Внедряем ILogger
        }

        public string GenerateJwtToken(string email, int id)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Используйте Guid для уникальности
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, "user")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetJwtKey()
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            return key;
        }

        public async Task<User> RegisterAsync(RegisterModel registerModel)
        {
            // Проверка на существование пользователя с таким же именем или почтой
            if (await _context.Users.AnyAsync(u => u.UserName == registerModel.Username))
            {
                throw new ArgumentException("Имя пользователя уже занято.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerModel.Email))
            {
                throw new ArgumentException("Электронная почта уже используется.");
            }

            var user = new User
            {
                UserName = registerModel.Username,
                Email = registerModel.Email,
                PasswordHash = HashPassword(registerModel.Password),
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                MiddleName = registerModel.MiddleName,
                Role = registerModel.Username.Equals("nikaswg", StringComparison.OrdinalIgnoreCase) ? "Developer" : "User" // Устанавливаем роль
            };

            // Логируем информацию о пользователе перед добавлением в базу данных
            _logger.LogInformation("Добавление пользователя в базу данных: {@User}", user);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Сохраняем пользователя

            return user; // Возвращаем зарегистрированного пользователя
        }

        public async Task<User> LoginAsync(LoginModel loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            // Ищем пользователя по email или нику
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Username || u.UserName == loginDto.Username);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Неверный логин или пароль.");
            }

            return user; // Возвращаем пользователя при успешном входе
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<TestAttempt> SubmitQuizResultsAsync(string email, int score, int totalQuestions, string userAnswers, string name_test)
        {
            _logger.LogInformation("Получены результаты теста от пользователя: {Email}, Очки: {Score}, Всего вопросов: {TotalQuestions}.", email, score, totalQuestions);

            var attempt = new TestAttempt
            {
                UserId = email, // Используем email как UserId
                Score = score,
                TotalQuestions = totalQuestions,
                UserAnswers = userAnswers,
                AttemptDate = DateTime.UtcNow, // Устанавливаем текущее время
                name_test = name_test
            };

            // Логируем информацию о попытке
            _logger.LogInformation("Добавление попытки теста в базу данных: {@Attempt}", attempt);

            _context.TestAttempts.Add(attempt); // Добавляем попытку в контекст
            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных

            _logger.LogInformation("Попытка теста успешно добавлена в базу данных для пользователя: {Email}.", email);
            return attempt; // Возвращаем созданный объект попытки
        }


        public List<TestAttempt> GetUserAttempts(string email)
        {
            _logger.LogInformation("Запрос попыток для: {Email}", email);
            try
            {
                return _context.TestAttempts
                    .Where(a => a.UserId == email)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении попыток");
                return new List<TestAttempt>();
            }
        }
    }
}