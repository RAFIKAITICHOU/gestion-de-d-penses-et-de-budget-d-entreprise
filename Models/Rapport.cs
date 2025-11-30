using System.ComponentModel.DataAnnotations;

namespace GestionBudget_V2.Models
{
    public class Rapport
    {
        [Key]
        public int Id_rapport { get; set; }
        
        public DateTime DateGeneration { get; set; } = DateTime.Now;
        
        public string Type_rapport { get; set; } = string.Empty;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}