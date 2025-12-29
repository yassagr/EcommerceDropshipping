using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceDropshipping.Models.Domain.Enums;

namespace EcommerceDropshipping.Models.Domain
{
    public class Commande
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public StatutCommande Statut { get; set; } = StatutCommande.EnAttente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        public Guid? AdresseLivraisonId { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual Adresse? AdresseLivraison { get; set; }
        public virtual ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();
    }
}
