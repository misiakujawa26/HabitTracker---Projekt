using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Controllers
{
 [Authorize]
   // Kontroler zarządzający księgą nawyków
    public class HabitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
   //// Konstruktor - wstrzyknięcie bazy danych
        public HabitsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
     {
        _context = context;
        _userManager = userManager;
     }
        ///// Wyświetlenie listy wszystkich nawyków
        // GET: Habits
        public async Task<IActionResult> Index()
      {
       var userId = _userManager.GetUserId(User);
       var habits = _context.Habits
                .Include(h => h.Category)
                .Include(h => h.User)
                .Where(h => h.UserId == userId);

            return View(await habits.ToListAsync());
       }

        // GET: Habits/Details/5
        ///// Pokazanie szczegółowych informacji o wybranym nawyku
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

        var userId = _userManager.GetUserId(User);
        var habit = await _context.Habits
                .Include(h => h.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
         if (habit == null)
         {
            return NotFound();
          }
            return View(habit);
        }
        // GET: Habits/Create
        //// Wyświetlenie formularza tworzenia nowego nawyku
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }
        // POST: Habits/Create
        /// Zapisanie nowo utworzonego nawyku w bazie danych
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,Frequency,Target,IsActive,CategoryId")] Habit habit)
        {
            var userId = _userManager.GetUserId(User);
            habit.UserId = userId ?? string.Empty;
            ModelState.Remove("UserId");
            ModelState.Remove("User");
             if (ModelState.IsValid)
            {
                _context.Add(habit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", habit.CategoryId);
            return View(habit);
        }
        // GET: Habits/Edit/5
        //// Wyświetlenie formularza edycji istniejącego nawyku
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
             var userId = _userManager.GetUserId(User);
             var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
             if (habit == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", habit.CategoryId);
            return View(habit);
        }

        // POST: Habits/Edit/5
        // Zapisanie wprowadzonych zmian w nawyku
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,Frequency,Target,IsActive,CategoryId")] Habit habit)
        {
            if (id != habit.Id)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            habit.UserId = userId ?? string.Empty;
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    var existsAndOwned = _context.Habits.Any(h => h.Id == id && h.UserId == userId);
                    if (!existsAndOwned)
                    {
                        return NotFound();
                    }
                    _context.Update(habit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HabitExists(habit.Id, userId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", habit.CategoryId);
            return View(habit);
        }
        // Wyświetlenie strony z pytaniem o potwierdzenie usunięcia nawyku
        // GET: Habits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            var habit = await _context.Habits
                .Include(h => h.Category)
                .Include(h => h.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        // POST: Habits/Delete/5
        // Ostateczne usunięcie wybranego nawyku
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit != null)
            {
                _context.Habits.Remove(habit);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //przełączanie statusu nawyku
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit != null)
            {
                habit.IsActive = !habit.IsActive;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        //// Metoda pomocnicza weryfikująca istnienie nawyku
        private bool HabitExists(int id, string userId)
        {
            return _context.Habits.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}