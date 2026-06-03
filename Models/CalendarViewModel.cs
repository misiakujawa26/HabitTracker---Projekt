using System;
using System.Collections.Generic;

namespace HabitTracker.Models
{
    // Główny model przekazywany z kontrolera do widoku Index.cshtml
    public class CalendarViewModel
    {
        public DateTime CurrentMonth { get; set; }

        // Inicjalizacja listy zapobiega błędom NullReferenceException
        public List<CalendarDayViewModel> Days { get; set; } = new List<CalendarDayViewModel>();
    }

    // Model reprezentujący pojedynczy kafel dnia w siatce kalendarza
    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }

        // Lista nawyków, które użytkownik faktycznie ukończył tego konkretnego dnia
        public List<HabitLogDto> CompletedHabits { get; set; } = new List<HabitLogDto>();
    }

    // Lekki obiekt transferu danych (DTO), który odcina zbędne dane z bazy 
    // i przekazuje do widoku tylko to, co niezbędne do narysowania kafelka
    public class HabitLogDto
    {
        public int LogId { get; set; }
        public int HabitId { get; set; }
        public string HabitName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#999999";
    }
}