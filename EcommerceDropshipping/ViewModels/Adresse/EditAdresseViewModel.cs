using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.ViewModels.Adresse
{
    public class EditAdresseViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "La rue est requise")]
        [StringLength(200)]
        [Display(Name = "Rue")]
        public string Rue { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville est requise")]
        [StringLength(100)]
        [Display(Name = "Ville")]
        public string Ville { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code postal est requis")]
        [StringLength(20)]
        [Display(Name = "Code Postal")]
        public string CodePostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le pays est requis")]
        [StringLength(100)]
        [Display(Name = "Pays")]
        public string Pays { get; set; } = string.Empty;

        [Display(Name = "Adresse principale")]
        public bool EstPrincipale { get; set; }
    }
}
