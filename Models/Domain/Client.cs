using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.Models.Domain
{
    public class Client
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MotDePasse { get; set; } = string.Empty;

        public DateTime DateInscription { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Role { get; set; } = "Client";

        // Navigation properties
        public virtual ICollection<Adresse> Adresses { get; set; } = new List<Adresse>();
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
    }
}
