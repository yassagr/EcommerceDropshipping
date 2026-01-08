using EcommerceDropshipping.Models.Domain.Enums;

namespace EcommerceDropshipping.ViewModels.Commande
{
    public class CommandeListItemViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public StatutCommande Statut { get; set; }
        public decimal MontantTotal { get; set; }
        public int NombreArticles { get; set; }
        
        // For admin view
        public string? ClientNom { get; set; }
        public string? ClientEmail { get; set; }
        
        public string StatutLabel => Statut switch
        {
            StatutCommande.EnAttente => "En attente",
            StatutCommande.EnCours => "En cours",
            StatutCommande.Expediee => "Expédiée",
            StatutCommande.Livree => "Livrée",
            StatutCommande.Annulee => "Annulée",
            _ => "Inconnu"
        };
        
        public string StatutClass => Statut switch
        {
            StatutCommande.EnAttente => "warning",
            StatutCommande.EnCours => "info",
            StatutCommande.Expediee => "primary",
            StatutCommande.Livree => "success",
            StatutCommande.Annulee => "danger",
            _ => "secondary"
        };
    }
}
