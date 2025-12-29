namespace EcommerceDropshipping.ViewModels.Commande
{
    public class ConfirmationViewModel
    {
        public Guid CommandeId { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal MontantTotal { get; set; }
        public string AdresseLivraison { get; set; } = string.Empty;
        public int NombreArticles { get; set; }
    }
}
