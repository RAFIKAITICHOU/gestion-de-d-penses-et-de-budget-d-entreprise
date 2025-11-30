using System.ComponentModel.DataAnnotations;

namespace GestionBudget_V2.Models
{
    public class Budget
    {
        [Key]
        public int Id_budget { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Montant_alloue { get; set; }
        
        [Required]
        public DateTime Date_alloue { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}