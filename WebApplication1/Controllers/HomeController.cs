using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System.Diagnostics;
using WebApplication1.Models;
using Newtonsoft.Json;
using System.Web;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;
        private readonly SearchService _searchService;
        private readonly ApplicationDbContext _context;

        // Объединённый конструктор
        public HomeController(ILogger<HomeController> logger, AuthService authService, SearchService searchService, ApplicationDbContext context)
        {
            _logger = logger;
            _authService = authService;
            _searchService = searchService;
            _context = context; 
        }

        public IActionResult Main()
        {
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var role = HttpContext.Session.GetString("Role");

            Console.WriteLine($"Username: {username}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Role: {role}");

            // Передаем данные в ViewBag
            ViewBag.Username = username ?? "Гость";
            ViewBag.Email = email ?? "Не указано";
            ViewBag.Role = role ?? "Не указана";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult check_att_prob()
        {
            return View();
        }

        public IActionResult service()
        {
            return View();
        }

        public IActionResult dev_tools()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Гость";
            ViewBag.Email = HttpContext.Session.GetString("Email") ?? "Не указано";
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Не указана";
            return View();
        }

        public IActionResult deriviate_test()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Гость";
            ViewBag.Email = HttpContext.Session.GetString("Email") ?? "Не указано";
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Не указана";
            return View();
        }

        public IActionResult prob_test()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Гость";
            ViewBag.Email = HttpContext.Session.GetString("Email") ?? "Не указано";
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Не указана";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("Home/Register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid registration data.");
            }

            try
            {
                // Регистрация пользователя
                var user = await _authService.RegisterAsync(model);

                // Если регистрация успешна, редирект на страницу Index
                if (user != null)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Возвращаем сообщение об ошибке
            }

            return BadRequest("Registration failed.");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Логин или пароль не могут быть пустыми.";
                return View("Index");
            }

            try
            {
                var user = await _authService.LoginAsync(new LoginModel { Username = usernameOrEmail, Password = password });
                var token = _authService.GenerateJwtToken(user.Email, user.Id);

                // Сохранение информации о пользователе в сессии
                HttpContext.Session.SetString("Username", user.UserName);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Main");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SubmitQuizResults(string email, int score, int totalQuestions, string userAnswers, string name_test)
        {
            _logger.LogInformation("Получен запрос на отправку результатов теста от пользователя: {Email}.", email);
            _logger.LogInformation("Данные для сохранения: Очки: {Score}, Всего вопросов: {TotalQuestions}, Ответы: {UserAnswers}.", score, totalQuestions, userAnswers);

            var result = await _authService.SubmitQuizResultsAsync(email, score, totalQuestions, userAnswers, name_test);

            if (result != null)
            {
                _logger.LogInformation("Результаты теста успешно сохранены для пользователя: {Email}.", email);
                return RedirectToAction("Main"); // Возвращаем текст "O'key"
            }
            else
            {
                _logger.LogWarning("Не удалось сохранить результаты теста для пользователя: {Email}.", email);
                return Content("Не удалось сохранить результаты."); // Возвращаем сообщение об ошибке
            }
        }
        [HttpGet]
        public ActionResult UserAttempts(string email)
        {
            // Получаем email из параметра или сессии
            var userEmail = email ?? HttpContext.Session.GetString("Email");

            // Получаем имя пользователя из сессии
            var userName = HttpContext.Session.GetString("Username") ?? "Гость";

            // Если email не найден нигде
            if (string.IsNullOrEmpty(userEmail))
            {
                ViewBag.Error = "Пользователь не авторизован";
                return View(new List<TestAttempt>());
            }

            // Получаем попытки из сервиса
            var attempts = _authService.GetUserAttempts(userEmail);

            // Передаем имя в ViewBag
            ViewBag.Username = userName;

            return View(attempts);
        }

        [HttpGet]
        public IActionResult SearchUsers(string searchTerm)
        {
            var users = _context.Users
                .Where(u => u.Email.Contains(searchTerm)
                           || u.UserName.Contains(searchTerm))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role
                })
                .ToList();
            Console.WriteLine(users);
            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                if (user.Role == "Admin")
                    return Json(new { success = false, message = "User is already an Admin" });

                user.Role = "Admin";
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Promotion error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult ViewAttempt(int id)
        {
            var attempt = _context.TestAttempts.FirstOrDefault(a => a.Id == id);
            if (attempt == null)
            {
                return NotFound();
            }

            var userAnswers = JsonConvert.DeserializeObject<Dictionary<string, string>>(attempt.UserAnswers);
            ViewBag.UserAnswers = userAnswers;
            ViewBag.Attempt = attempt;

            return View();
        }
    }
}

