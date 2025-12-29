namespace EcommerceDropshipping.ViewModels.Fournisseur
{
    public class FournisseurListItemViewModel
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public int NombreProduits { get; set; }
    }
}
