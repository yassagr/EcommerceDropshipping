namespace EcommerceDropshipping.ViewModels.Panier
{
    public class PanierViewModel
    {
        public List<PanierItemViewModel> Items { get; set; } = new();
        
        public int NombreArticles => Items.Sum(i => i.Quantite);
        public decimal SousTotal => Items.Sum(i => i.Total);
        public decimal FraisLivraison => SousTotal >= 50 ? 0 : 4.99m;
        public decimal Total => SousTotal + FraisLivraison;
        
        public bool EstVide => !Items.Any();
        public bool LivraisonGratuite => SousTotal >= 50;
        public decimal MontantPourLivraisonGratuite => 50 - SousTotal;
    }
}
