namespace EcommerceDropshipping.ViewModels.Adresse
{
    public class AdresseListItemViewModel
    {
        public Guid Id { get; set; }
        public string Rue { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string CodePostal { get; set; } = string.Empty;
        public string Pays { get; set; } = string.Empty;
        public bool EstPrincipale { get; set; }

        public string AdresseComplete => $"{Rue}, {CodePostal} {Ville}, {Pays}";
    }
}
