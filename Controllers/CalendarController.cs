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
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CalendarController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var userId = _userManager.GetUserId(User);

            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            var firstDayOfMonth = new DateTime(targetYear, targetMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var logsInMonth = await _context.HabitLogs
                .Include(l => l.Habit)
                .ThenInclude(h => h!.Category)
                .Where(l => l.Habit!.UserId == userId &&
                            l.DateCompleted >= firstDayOfMonth &&
                            l.DateCompleted <= lastDayOfMonth.AddDays(1).AddTicks(-1))
                .ToListAsync();

            var daysList = new List<CalendarDayViewModel>();

            int offset = ((int)firstDayOfMonth.DayOfWeek == 0) ? 6 : (int)firstDayOfMonth.DayOfWeek - 1;
            var startDate = firstDayOfMonth.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                var currentDate = startDate.AddDays(i);

                var completedToday = logsInMonth
                    .Where(l => l.DateCompleted.Date == currentDate.Date)
                    .Select(l => new HabitLogDto
                    {
                        LogId = l.Id, 
                        HabitId = l.HabitId,
                        HabitName = l.Habit?.Name ?? "Nawyk",
                        ColorHex = l.Habit?.Category?.Color ?? "#999999"
                    })
                    .ToList();

                daysList.Add(new CalendarDayViewModel
                {
                    Date = currentDate,
                    IsCurrentMonth = currentDate.Month == targetMonth,
                    CompletedHabits = completedToday
                });
            }

            // Pobieramy aktywne nawyki, aby przekazać je do okienka Modal
            var activeHabits = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .ToListAsync();

            ViewBag.ActiveHabits = activeHabits;

            var viewModel = new CalendarViewModel
            {
                CurrentMonth = firstDayOfMonth,
                Days = daysList
            };

            return View(viewModel);
        }

        // Nowy endpoint endpoint dla AJAX
        [HttpPost]
        public async Task<IActionResult> ToggleHabit(int habitId, string dateStr)
        {
            var userId = _userManager.GetUserId(User);
            if (!DateTime.TryParse(dateStr, out DateTime targetDate))
            {
                return BadRequest("Nieprawidłowy format daty.");
            }

            // Bezpieczeństwo: sprawdź czy nawyk należy do użytkownika
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId);
            if (habit == null) return NotFound();

            // Szukamy, czy istnieje już log na ten dzień 
            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == habitId && EF.Functions.DateDiffDay(l.DateCompleted, targetDate) == 0);

            if (existingLog != null)
            {
                // Jeśli istnieje - usuwamy (odznaczamy)
                _context.HabitLogs.Remove(existingLog);
                await _context.SaveChangesAsync();
                return Json(new { success = true, action = "removed", habitId = habitId });
            }
            else
            {
                // Jeśli nie istnieje - dodajemy nowy log przypisany do wybranego dnia
                var newLog = new HabitLog
                {
                    HabitId = habitId,
                    DateCompleted = targetDate.Date.AddHours(12), 
                    IsCompleted = true
                };
                _context.HabitLogs.Add(newLog);
                await _context.SaveChangesAsync();

                // Pobieramy kolor kategorii, żeby przekazać go do skryptu JS
                var categoryColor = await _context.Habits
                    .Where(h => h.Id == habitId)
                    .Select(h => h.Category!.Color)
                    .FirstOrDefaultAsync() ?? "#999999";

                return Json(new { success = true, action = "added", habitId = habitId, habitName = habit.Name, color = categoryColor });
            }
        }
    }
}