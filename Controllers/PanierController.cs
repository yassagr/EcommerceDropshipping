using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.ViewModels.Panier;
using EcommerceDropshipping.Helpers;

namespace EcommerceDropshipping.Controllers
{
    public class PanierController : Controller
    {
        private readonly AppDbContext _context;
        private const string PanierSessionKey = "Panier";

        public PanierController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Panier
        public async Task<IActionResult> Index()
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();

            // Refresh product info from database
            var produitIds = panierItems.Select(i => i.ProduitId).ToList();
            var produits = await _context.Produits
                .Where(p => produitIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Update cart with current prices and stock
            foreach (var item in panierItems.ToList())
            {
                if (produits.TryGetValue(item.ProduitId, out var produit))
                {
                    item.Titre = produit.Titre;
                    item.Prix = produit.Prix;
                    item.ImageUrl = produit.ImageUrl;
                    item.StockDisponible = produit.Stock;

                    // Adjust quantity if stock changed
                    if (item.Quantite > produit.Stock)
                    {
                        item.Quantite = produit.Stock;
                    }
                    
                    // Remove if out of stock
                    if (produit.Stock <= 0 || !produit.EstActif)
                    {
                        panierItems.Remove(item);
                    }
                }
                else
                {
                    // Product no longer exists
                    panierItems.Remove(item);
                }
            }

            // Save updated cart
            HttpContext.Session.SetObjectAsJson(PanierSessionKey, panierItems);

            var viewModel = new PanierViewModel
            {
                Items = panierItems
            };

            ViewBag.CartCount = panierItems.Sum(i => i.Quantite);
            return View(viewModel);
        }

        // POST: /Panier/Ajouter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajouter(Guid produitId, int quantite = 1)
        {
            var produit = await _context.Produits.FindAsync(produitId);

            if (produit == null || !produit.EstActif)
            {
                TempData["ErrorMessage"] = "Produit non disponible";
                return RedirectToAction("Index", "Produit");
            }

            if (produit.Stock < quantite)
            {
                TempData["ErrorMessage"] = "Stock insuffisant";
                return RedirectToAction("Details", "Produit", new { id = produitId });
            }

            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();

            var existingItem = panierItems.FirstOrDefault(i => i.ProduitId == produitId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantite + quantite;
                if (newQuantity > produit.Stock)
                {
                    TempData["ErrorMessage"] = "Quantité maximale atteinte";
                    return RedirectToAction("Details", "Produit", new { id = produitId });
                }
                existingItem.Quantite = newQuantity;
            }
            else
            {
                panierItems.Add(new PanierItemViewModel
                {
                    ProduitId = produit.Id,
                    Titre = produit.Titre,
                    Prix = produit.Prix,
                    ImageUrl = produit.ImageUrl,
                    Quantite = quantite,
                    StockDisponible = produit.Stock
                });
            }

            HttpContext.Session.SetObjectAsJson(PanierSessionKey, panierItems);

            TempData["SuccessMessage"] = $"{produit.Titre} ajouté au panier";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Panier/Modifier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Guid produitId, int quantite)
        {
            if (quantite <= 0)
            {
                return await Supprimer(produitId);
            }

            var produit = await _context.Produits.FindAsync(produitId);

            if (produit == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (quantite > produit.Stock)
            {
                TempData["ErrorMessage"] = "Stock insuffisant";
                return RedirectToAction(nameof(Index));
            }

            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();

            var item = panierItems.FirstOrDefault(i => i.ProduitId == produitId);

            if (item != null)
            {
                item.Quantite = quantite;
                HttpContext.Session.SetObjectAsJson(PanierSessionKey, panierItems);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Panier/Supprimer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Supprimer(Guid produitId)
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();

            var item = panierItems.FirstOrDefault(i => i.ProduitId == produitId);

            if (item != null)
            {
                panierItems.Remove(item);
                HttpContext.Session.SetObjectAsJson(PanierSessionKey, panierItems);
                TempData["SuccessMessage"] = "Article supprimé du panier";
            }

            return Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
        }

        // POST: /Panier/Vider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vider()
        {
            HttpContext.Session.Remove(PanierSessionKey);
            TempData["SuccessMessage"] = "Panier vidé";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Panier/Count (AJAX)
        public IActionResult Count()
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();
            
            return Json(new { count = panierItems.Sum(i => i.Quantite) });
        }

        // GET: /Panier/Mini (for navbar dropdown)
        public async Task<IActionResult> Mini()
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey) 
                              ?? new List<PanierItemViewModel>();

            // Refresh prices from DB
            var produitIds = panierItems.Select(i => i.ProduitId).ToList();
            var produits = await _context.Produits
                .Where(p => produitIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in panierItems)
            {
                if (produits.TryGetValue(item.ProduitId, out var produit))
                {
                    item.Prix = produit.Prix;
                    item.ImageUrl = produit.ImageUrl;
                }
            }

            return PartialView("_MiniPanier", new PanierViewModel { Items = panierItems });
        }
    }
}
