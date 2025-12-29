using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDropshipping.Models.Domain
{
    public class Produit
    {
        public Guid Id { get; set; }

        public Guid FournisseurId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix { get; set; }

        public int Stock { get; set; } = 0;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime DateAjout { get; set; } = DateTime.Now;

        public bool EstActif { get; set; } = true;

        // Navigation properties
        public virtual Fournisseur Fournisseur { get; set; } = null!;
        public virtual ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();
    }
}
