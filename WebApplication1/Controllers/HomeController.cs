using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;

        // Объединённый конструктор
        public HomeController(ILogger<HomeController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
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

        public IActionResult deriviate_test()
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

            // Регистрация пользователя
            var user = await _authService.RegisterAsync(model);

            // Если регистрация успешна, редирект на страницу Index
            if (user != null)
            {
                return RedirectToAction("Index");
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
    }
}
