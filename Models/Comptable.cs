using System.ComponentModel.DataAnnotations;

namespace GestionBudget_V2.Models
{
    public class Comptable
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Nom { get; set; } = string.Empty;
        
        [Required]
        public string Prenom { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string MotDePasse { get; set; } = string.Empty;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}