using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class SearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public SearchService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger; // Внедряем ILogger
        }

        public async Task<User> SearchUserByEmailOrEmail(string username)
        {
            return await _context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
        }
    }
}
