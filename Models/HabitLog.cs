namespace HabitTracker.Models
{
    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public Habit? Habit { get; set; }
        //  data i godzina, zaznaczenia nawyku na aktywny
        public DateTime DateCompleted { get; set; } = DateTime.Today;
        ////  potwierdzenie pomyślnie ukończonego nawyku 
        public bool IsCompleted { get; set; } = true;
    }
}
