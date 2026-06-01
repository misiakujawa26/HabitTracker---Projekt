using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Controllers
{
    //Użytkownik musi być zalogowany
    [Authorize] 
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
    //Konstruktor - wstrzykiwanie bazy danych
    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
         _context = context;
         _userManager = userManager;
    }

    // GET: Dashboard
    public async Task<IActionResult> Index()
    {
      // Pobranie  Id aktualnie zalogowanego użytkownika
      var userId = _userManager.GetUserId(User);
      //Licznik aktywnych nawyków
      var activeHabitsCount = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .CountAsync();
      // liczba wszystkich kliknięć "Zrobione"
      var totalCompletions = await _context.HabitLogs
                .Include(l => l.Habit)
                .Where(l => l.Habit!.UserId == userId)
                .CountAsync();
      //Statystyki kategorii
      var habitsByCategory = await _context.Habits
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.Category!.Name)
                .Select(g => new CategoryStatsViewModel
                {
                    CategoryName = g.Key ?? "Brak kategorii",
                    Count = g.Count()
                })
                .ToListAsync();
      //Ostatnia aktywność
      var recentActivities = await _context.HabitLogs
                .Include(l => l.Habit)
                .Where(l => l.Habit!.UserId == userId)
                .OrderByDescending(l => l.DateCompleted)
                .Take(5)
                .ToListAsync();
      //Lista wszystkich aktualnych, aktywnych nawyków
      var userHabits = await _context.Habits
                .Include(h => h.Category)
                .Where(h => h.UserId == userId && h.IsActive)
                .ToListAsync();

            ViewBag.ActiveHabitsCount = activeHabitsCount;
            ViewBag.TotalCompletions = totalCompletions;
            ViewBag.HabitsByCategory = habitsByCategory;
            ViewBag.RecentActivities = recentActivities;
            ViewBag.UserHabits = userHabits; 

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteHabit(int habitId)
        {
             var userId = _userManager.GetUserId(User);
            // Bezpieczne pobranie nawyku
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId);
             if (habit == null)
            {
                return NotFound();
            }
            // Blokada wielokrotnego klikania
            var today = DateTime.Today;
            var alreadyCompletedToday = await _context.HabitLogs
                .AnyAsync(l => l.HabitId == habitId && l.DateCompleted.Date == today);
             if (!alreadyCompletedToday)
            {
                var log = new HabitLog
                {
                    HabitId = habitId,
                    DateCompleted = DateTime.Now
                   
                };

                _context.HabitLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // Pomocniczy model danych
    public class CategoryStatsViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }



}