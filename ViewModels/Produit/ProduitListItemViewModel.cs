namespace EcommerceDropshipping.ViewModels.Produit
{
    public class ProduitListItemViewModel
    {
        public Guid Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime DateAjout { get; set; }
        public bool EstActif { get; set; }
        public string FournisseurNom { get; set; } = string.Empty;
        
        // Computed properties
        public bool EstNouveau => (DateTime.Now - DateAjout).TotalDays <= 7;
        public bool StockLimite => Stock > 0 && Stock < 10;
        public bool EnStock => Stock > 0;
        
        // Short description for cards
        public string DescriptionCourte => 
            Description != null && Description.Length > 100 
                ? Description.Substring(0, 100) + "..." 
                : Description ?? string.Empty;
    }
}
