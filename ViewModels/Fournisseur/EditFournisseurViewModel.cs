using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.ViewModels.Fournisseur
{
    public class EditFournisseurViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(200)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20)]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        public int NombreProduits { get; set; }
    }
}
