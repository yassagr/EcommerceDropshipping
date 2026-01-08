using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.Models.Domain
{
    public class Adresse
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        [Required]
        [StringLength(200)]
        public string Rue { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Ville { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CodePostal { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Pays { get; set; } = string.Empty;

        public bool EstPrincipale { get; set; } = false;

        // Navigation property
        public virtual Client Client { get; set; } = null!;
    }
}
