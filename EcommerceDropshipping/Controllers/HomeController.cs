using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.ViewModels.Produit;
using EcommerceDropshipping.Helpers;
using EcommerceDropshipping.ViewModels.Panier;

namespace EcommerceDropshipping.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured products (newest and active)
            var produits = await _context.Produits
                .Where(p => p.EstActif)
                .Include(p => p.Fournisseur)
                .OrderByDescending(p => p.DateAjout)
                .Take(8)
                .Select(p => new ProduitListItemViewModel
                {
                    Id = p.Id,
                    Titre = p.Titre,
                    Description = p.Description,
                    Prix = p.Prix,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    DateAjout = p.DateAjout,
                    EstActif = p.EstActif,
                    FournisseurNom = p.Fournisseur.Nom
                })
                .ToListAsync();

            ViewBag.CartCount = GetCartItemCount();
            return View(produits);
        }

        public IActionResult About()
        {
            ViewBag.CartCount = GetCartItemCount();
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.CartCount = GetCartItemCount();
            return View();
        }

        public IActionResult Privacy()
        {
            ViewBag.CartCount = GetCartItemCount();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        private int GetCartItemCount()
        {
            var panier = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>("Panier");
            return panier?.Sum(i => i.Quantite) ?? 0;
        }
    }
}
