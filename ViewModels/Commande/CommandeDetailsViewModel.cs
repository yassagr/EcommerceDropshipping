using EcommerceDropshipping.Models.Domain.Enums;

namespace EcommerceDropshipping.ViewModels.Commande
{
    public class CommandeDetailsViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public StatutCommande Statut { get; set; }
        public decimal MontantTotal { get; set; }
        
        // Client info
        public string ClientNom { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        
        // Adresse livraison
        public string? AdresseLivraison { get; set; }
        
        // Order lines
        public List<LigneCommandeViewModel> Lignes { get; set; } = new();
        
        public string StatutLabel => Statut switch
        {
            StatutCommande.EnAttente => "En attente",
            StatutCommande.EnCours => "En cours de préparation",
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
    
    public class LigneCommandeViewModel
    {
        public string ProduitTitre { get; set; } = string.Empty;
        public string? ProduitImage { get; set; }
        public Guid ProduitId { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Total => Quantite * PrixUnitaire;
    }
}
