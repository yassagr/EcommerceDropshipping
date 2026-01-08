using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDropshipping.Models.Domain
{
    public class LigneCommande
    {
        public Guid Id { get; set; }

        public Guid CommandeId { get; set; }

        public Guid ProduitId { get; set; }

        [Required]
        public int Quantite { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixUnitaire { get; set; }

        // Computed property for total line price
        [NotMapped]
        public decimal Total => Quantite * PrixUnitaire;

        // Navigation properties
        public virtual Commande Commande { get; set; } = null!;
        public virtual Produit Produit { get; set; } = null!;
    }
}
