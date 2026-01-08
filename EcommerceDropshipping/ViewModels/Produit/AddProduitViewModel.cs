using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.ViewModels.Produit
{
    public class AddProduitViewModel
    {
        [Required(ErrorMessage = "Le titre est requis")]
        [StringLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères")]
        [Display(Name = "Titre")]
        public string Titre { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La description ne peut pas dépasser 2000 caractères")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le prix est requis")]
        [Range(0.01, 999999.99, ErrorMessage = "Le prix doit être entre 0.01 et 999999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Prix (€)")]
        public decimal Prix { get; set; }

        [Required(ErrorMessage = "Le stock est requis")]
        [Range(0, 9999999, ErrorMessage = "Le stock doit être positif")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "URL invalide")]
        [Display(Name = "URL de l'image")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Le fournisseur est requis")]
        [Display(Name = "Fournisseur")]
        public Guid FournisseurId { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; } = true;
    }
}
