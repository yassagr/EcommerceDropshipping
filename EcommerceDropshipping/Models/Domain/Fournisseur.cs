using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.Models.Domain
{
    public class Fournisseur
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telephone { get; set; }

        // Navigation property
        public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
    }
}
