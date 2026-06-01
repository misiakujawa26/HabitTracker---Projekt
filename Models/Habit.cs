using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace HabitTracker.Models
{
    public class Habit
    {
        public int Id { get; set; }
        // Nazwa nawyku długość (od 3 do 100 znaków)
        [Required(ErrorMessage = "Brak nazwy nawyku.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 100 znaków.")]
        [Display(Name = "Nazwa")]
        //Opis nawyku max 500 znaków
        public string Name { get; set; } = string.Empty;
        [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków.")]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Brak daty rozpoczęcia.")]
        [DataType(DataType.Date)]//Wymusza wyświetlanie samego kalendarza
        [Display(Name = "Data rozpoczęcia")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Brak częstotliwości.")]
        [Display(Name = "Częstotliwość")]
        public string Frequency { get; set; } = "Codziennie";
        [Required(ErrorMessage = "Cel jest wymagany.")]
        [Display(Name = "Cel")]
        public string Target { get; set; } = string.Empty;
        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;
        [Display(Name = "Użytkownik")]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
        [Required(ErrorMessage = "Brak wyboru kategorii.")]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }
        [Display(Name = "Kategoria")]
        public Category? Category { get; set; }
        //historia wszystkich kliknięć "Zrobione"
        public ICollection<HabitLog> HabitLogs { get; set; } = new List<HabitLog>();
    }
}
