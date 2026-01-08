namespace EcommerceDropshipping.ViewModels.Panier
{
    public class PanierItemViewModel
    {
        public Guid ProduitId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Prix { get; set; }
        public int Quantite { get; set; }
        public int StockDisponible { get; set; }
        
        public decimal Total => Prix * Quantite;
        public decimal SousTotal => Prix * Quantite;
    }
}
