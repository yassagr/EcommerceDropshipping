using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.Models.Domain
{
    public class LignePanier
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid PanierId { get; set; }
        
        [Required]
        public Guid ProduitId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être au moins 1")]
        public int Quantite { get; set; }
        
        public DateTime DateAjout { get; set; }
        
        // Navigation properties
        public virtual Panier Panier { get; set; } = null!;
        public virtual Produit Produit { get; set; } = null!;
    }
}
