namespace ServicingSystem.ViewModels
{
    public class ServiceView
    {
        public int StandardJobID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal StandardHours { get; set; }
        public decimal ExtPrice { get; set; }
        public int ServiceID { get; set; }
        public DateTime? StartDate { get; set; }
        public string Comment { get; set; }
    }
}

