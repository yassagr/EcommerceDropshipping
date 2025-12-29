using System.ComponentModel.DataAnnotations;
using EcommerceDropshipping.ViewModels.Adresse;

namespace EcommerceDropshipping.ViewModels.Client
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public DateTime DateInscription { get; set; }

        public int NombreCommandes { get; set; }

        public decimal TotalAchats { get; set; }

        public List<AdresseListItemViewModel> Adresses { get; set; } = new();
    }
}
