using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.ViewModels.Adresse
{
    public class AddAdresseViewModel
    {
        [Required(ErrorMessage = "La rue est requise")]
        [StringLength(200, ErrorMessage = "La rue ne peut pas dépasser 200 caractères")]
        [Display(Name = "Rue")]
        public string Rue { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville est requise")]
        [StringLength(100, ErrorMessage = "La ville ne peut pas dépasser 100 caractères")]
        [Display(Name = "Ville")]
        public string Ville { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code postal est requis")]
        [StringLength(20, ErrorMessage = "Le code postal ne peut pas dépasser 20 caractères")]
        [Display(Name = "Code Postal")]
        public string CodePostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le pays est requis")]
        [StringLength(100, ErrorMessage = "Le pays ne peut pas dépasser 100 caractères")]
        [Display(Name = "Pays")]
        public string Pays { get; set; } = "France";

        [Display(Name = "Adresse principale")]
        public bool EstPrincipale { get; set; }
    }
}
