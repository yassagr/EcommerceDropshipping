namespace EcommerceDropshipping.ViewModels.Admin
{
    public class DashboardViewModel
    {
        // Stats
        public int TotalCommandes { get; set; }
        public int CommandesEnAttente { get; set; }
        public decimal RevenusTotal { get; set; }
        public decimal RevenusMois { get; set; }
        public int TotalProduits { get; set; }
        public int ProduitsActifs { get; set; }
        public int TotalClients { get; set; }
        public int TotalFournisseurs { get; set; }
        public int ProduitsStockFaible { get; set; }
        
        // Recent orders
        public List<CommandeRecenteViewModel> CommandesRecentes { get; set; } = new();
        
        // Top products
        public List<ProduitPopulaireViewModel> ProduitsPopulaires { get; set; } = new();
    }
    
    public class CommandeRecenteViewModel
    {
        public Guid Id { get; set; }
        public string ClientNom { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Montant { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string StatutClass { get; set; } = string.Empty;
    }
    
    public class ProduitPopulaireViewModel
    {
        public Guid Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int VentesTotal { get; set; }
        public decimal RevenusTotal { get; set; }
    }
}
