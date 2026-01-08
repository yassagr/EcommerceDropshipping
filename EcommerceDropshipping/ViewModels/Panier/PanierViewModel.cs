using System.Collections.Generic;
using System.Linq;

namespace EcommerceDropshipping.ViewModels.Panier
{
    public class PanierViewModel
    {
        public List<PanierItemViewModel> Items { get; set; } = new();
        
        public int NombreArticles { get; set; }
        public decimal Total { get; set; }
        
        // Computed properties for checkout
        public decimal SousTotal => Items.Sum(i => i.Total);
        public decimal FraisLivraison => Total >= 50 ? 0 : 4.99m;
        
        public bool EstVide => !Items.Any();
        public bool LivraisonGratuite => Total >= 50;
    }
}
