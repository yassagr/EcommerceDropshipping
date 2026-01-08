using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.Models.Domain;
using EcommerceDropshipping.ViewModels.Panier;

namespace EcommerceDropshipping.Controllers
{
    [Authorize]
    public class PanierController : Controller
    {
        private readonly AppDbContext _context;

        public PanierController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Panier
        public async Task<IActionResult> Index()
        {
            var clientId = ObtenirClientId();
            var lignesPanier = await ObtenirLignesPanierAvecProduitsAsync(clientId);

            var viewModel = new PanierViewModel
            {
                Items = lignesPanier.Select(lp => new PanierItemViewModel
                {
                    ProduitId = lp.ProduitId,
                    Titre = lp.Produit.Titre,
                    ImageUrl = lp.Produit.ImageUrl,
                    Prix = lp.Produit.Prix,
                    Quantite = lp.Quantite,
                    StockDisponible = lp.Produit.Stock
                }).ToList(),
                Total = await CalculerTotalAsync(clientId),
                NombreArticles = await ObtenirNombreArticlesAsync(clientId)
            };

            return View(viewModel);
        }

        // POST: /Panier/Ajouter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajouter(Guid produitId, int quantite = 1)
        {
            var clientId = ObtenirClientId();
            var succes = await AjouterProduitAsync(clientId, produitId, quantite);

            if (succes)
            {
                TempData["SuccessMessage"] = "Produit ajouté au panier";
            }
            else
            {
                TempData["ErrorMessage"] = "Impossible d'ajouter ce produit (stock insuffisant ou produit indisponible)";
            }

            var returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Produit");
        }

        // POST: /Panier/Modifier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Guid produitId, int quantite)
        {
            var clientId = ObtenirClientId();
            var succes = await ModifierQuantiteAsync(clientId, produitId, quantite);

            if (succes)
            {
                TempData["SuccessMessage"] = "Quantité mise à jour";
            }
            else
            {
                TempData["ErrorMessage"] = "Impossible de modifier la quantité (stock insuffisant)";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Panier/Supprimer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Supprimer(Guid produitId)
        {
            var clientId = ObtenirClientId();
            var succes = await SupprimerProduitAsync(clientId, produitId);

            if (succes)
            {
                TempData["SuccessMessage"] = "Produit retiré du panier";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Panier/Vider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vider()
        {
            var clientId = ObtenirClientId();
            await ViderPanierAsync(clientId);

            TempData["SuccessMessage"] = "Panier vidé";
            return RedirectToAction(nameof(Index));
        }

        #region Private Helper Methods (previously in PanierService)

        private async Task<Panier> ObtenirOuCreerPanierAsync(Guid clientId)
        {
            var panier = await _context.Paniers
                .Include(p => p.LignesPanier)
                .ThenInclude(lp => lp.Produit)
                .FirstOrDefaultAsync(p => p.ClientId == clientId);

            if (panier == null)
            {
                panier = new Panier
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                };

                await _context.Paniers.AddAsync(panier);
                await _context.SaveChangesAsync();
            }

            return panier;
        }

        private async Task<bool> AjouterProduitAsync(Guid clientId, Guid produitId, int quantite = 1)
        {
            try
            {
                var produit = await _context.Produits.FindAsync(produitId);
                if (produit == null || !produit.EstActif)
                    return false;

                if (produit.Stock < quantite)
                    return false;

                var panier = await ObtenirOuCreerPanierAsync(clientId);

                var ligneExistante = await _context.LignesPanier
                    .FirstOrDefaultAsync(lp => lp.PanierId == panier.Id && lp.ProduitId == produitId);

                if (ligneExistante != null)
                {
                    int nouvelleQuantite = ligneExistante.Quantite + quantite;
                    
                    if (nouvelleQuantite > produit.Stock)
                        return false;

                    ligneExistante.Quantite = nouvelleQuantite;
                }
                else
                {
                    var nouvelleLigne = new LignePanier
                    {
                        Id = Guid.NewGuid(),
                        PanierId = panier.Id,
                        ProduitId = produitId,
                        Quantite = quantite,
                        DateAjout = DateTime.Now
                    };

                    await _context.LignesPanier.AddAsync(nouvelleLigne);
                }

                panier.DateModification = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ModifierQuantiteAsync(Guid clientId, Guid produitId, int nouvelleQuantite)
        {
            try
            {
                if (nouvelleQuantite < 1)
                    return false;

                var panier = await ObtenirOuCreerPanierAsync(clientId);

                var ligne = await _context.LignesPanier
                    .Include(lp => lp.Produit)
                    .FirstOrDefaultAsync(lp => lp.PanierId == panier.Id && lp.ProduitId == produitId);

                if (ligne == null)
                    return false;

                if (nouvelleQuantite > ligne.Produit.Stock)
                    return false;

                ligne.Quantite = nouvelleQuantite;
                panier.DateModification = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SupprimerProduitAsync(Guid clientId, Guid produitId)
        {
            try
            {
                var panier = await ObtenirOuCreerPanierAsync(clientId);

                var ligne = await _context.LignesPanier
                    .FirstOrDefaultAsync(lp => lp.PanierId == panier.Id && lp.ProduitId == produitId);

                if (ligne == null)
                    return false;

                _context.LignesPanier.Remove(ligne);
                panier.DateModification = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ViderPanierAsync(Guid clientId)
        {
            try
            {
                var panier = await _context.Paniers
                    .Include(p => p.LignesPanier)
                    .FirstOrDefaultAsync(p => p.ClientId == clientId);

                if (panier == null)
                    return true;

                _context.LignesPanier.RemoveRange(panier.LignesPanier);
                panier.DateModification = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<decimal> CalculerTotalAsync(Guid clientId)
        {
            var panier = await _context.Paniers
                .Include(p => p.LignesPanier)
                .ThenInclude(lp => lp.Produit)
                .FirstOrDefaultAsync(p => p.ClientId == clientId);

            if (panier == null)
                return 0;

            return panier.LignesPanier.Sum(lp => lp.Quantite * lp.Produit.Prix);
        }

        private async Task<int> ObtenirNombreArticlesAsync(Guid clientId)
        {
            var panier = await _context.Paniers
                .Include(p => p.LignesPanier)
                .FirstOrDefaultAsync(p => p.ClientId == clientId);

            if (panier == null)
                return 0;

            return panier.LignesPanier.Sum(lp => lp.Quantite);
        }

        private async Task<List<LignePanier>> ObtenirLignesPanierAvecProduitsAsync(Guid clientId)
        {
            var panier = await _context.Paniers
                .Include(p => p.LignesPanier)
                .ThenInclude(lp => lp.Produit)
                .ThenInclude(pr => pr.Fournisseur)
                .FirstOrDefaultAsync(p => p.ClientId == clientId);

            return panier?.LignesPanier.ToList() ?? new List<LignePanier>();
        }

        #endregion

        private Guid ObtenirClientId()
        {
            var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(clientIdClaim!);
        }
    }
}
