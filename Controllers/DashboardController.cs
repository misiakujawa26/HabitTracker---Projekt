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
      var today = DateTime.Today; // Data na dzisiaj
    
      //id nawyków ukonczonych dzisiaj
      var completedHabitIds = await _context.HabitLogs
        .Where(l => l.DateCompleted.Date == today && l.Habit!.UserId == userId)
        .Select(l => l.HabitId)
        .ToListAsync();
     //przekazanie do widoku
     ViewBag.CompletedHabitIds = completedHabitIds;


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

        //wyciąganie danych z bazy, grupowanie ich i liczenie
        [HttpGet]
        public async Task<IActionResult> GetActivityData()
        {
            var userId = _userManager.GetUserId(User);
            var sevenDaysAgo = DateTime.Today.AddDays(-6);

            // pobranie logow z ostatnich 7 dni dla zalog usera
            var logs = await _context.HabitLogs
                .Where(l => l.Habit!.UserId == userId && l.DateCompleted >= sevenDaysAgo)
                .GroupBy(l => l.DateCompleted.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // lista z 7 dni (etykiety, wartości)
            var labels = new List<string>();
            var data = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                labels.Add(date.ToString("dd.MM")); // np. "06.09"
                data.Add(logs.FirstOrDefault(l => l.Date == date)?.Count ?? 0);
            }

            return Json(new { labels, data });
        }
    }

    // Pomocniczy model danych
    public class CategoryStatsViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }



}