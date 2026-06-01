using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nazwa kategorii jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa nie może przekraczać {1} znaków.")]
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
        // Relacja "jeden do wielu" 
        public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    }
}