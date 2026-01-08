using EcommerceDropshipping.ViewModels.Adresse;
using EcommerceDropshipping.ViewModels.Panier;
using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.ViewModels.Commande
{
    public class CheckoutViewModel
    {
        public PanierViewModel Panier { get; set; } = new();
        
        public List<AdresseListItemViewModel> AdressesDisponibles { get; set; } = new();
        
        [Required(ErrorMessage = "Veuillez sélectionner une adresse de livraison")]
        [Display(Name = "Adresse de livraison")]
        public Guid? AdresseLivraisonId { get; set; }
        
        // New address form if no address exists
        public bool CreerNouvelleAdresse { get; set; }
        
        public AddAdresseViewModel? NouvelleAdresse { get; set; }
    }
}
