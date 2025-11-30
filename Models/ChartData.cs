namespace GestionBudget_V2.Models
{
    public class ChartData
    {
        public string Categorie { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class MonthlyData
    {
        public string Mois { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}