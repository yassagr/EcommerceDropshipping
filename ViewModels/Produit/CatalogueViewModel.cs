namespace EcommerceDropshipping.ViewModels.Produit
{
    public class CatalogueViewModel
    {
        public List<ProduitListItemViewModel> Produits { get; set; } = new();
        public string? SearchQuery { get; set; }
        public decimal? PrixMin { get; set; }
        public decimal? PrixMax { get; set; }
        public Guid? FournisseurId { get; set; }
        public string? TriPar { get; set; } // "prix_asc", "prix_desc", "nouveau", "nom"
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        
        // Filters data
        public List<FournisseurFilterViewModel> Fournisseurs { get; set; } = new();
    }
    
    public class FournisseurFilterViewModel
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int NombreProduits { get; set; }
    }
}
