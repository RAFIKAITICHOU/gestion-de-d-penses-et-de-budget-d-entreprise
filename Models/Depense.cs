using System.ComponentModel.DataAnnotations;

namespace GestionBudget_V2.Models
{
    public class Depense
    {
        [Key]
        public int Id_depense { get; set; }
        
        [Required]
        public DateTime Date_depense { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Montant { get; set; }
        
        [Required]
        public string Categorie { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}