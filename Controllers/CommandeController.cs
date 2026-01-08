using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.Models.Domain;
using EcommerceDropshipping.Models.Domain.Enums;
using EcommerceDropshipping.ViewModels.Commande;
using EcommerceDropshipping.ViewModels.Panier;
using EcommerceDropshipping.ViewModels.Adresse;

namespace EcommerceDropshipping.Controllers
{
    [Authorize]
    public class CommandeController : Controller
    {
        private readonly AppDbContext _context;

        public CommandeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Commande/Checkout
        public async Task<IActionResult> Checkout()
        {
            var clientId = GetClientId();
            
            // Get cart from database
            var lignesPanier = await ObtenirLignesPanierAvecProduitsAsync(clientId);

            if (!lignesPanier.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            // Convert to PanierItemViewModel
            var panierItems = lignesPanier.Select(lp => new PanierItemViewModel
            {
                ProduitId = lp.ProduitId,
                Titre = lp.Produit.Titre,
                ImageUrl = lp.Produit.ImageUrl,
                Prix = lp.Produit.Prix,
                Quantite = lp.Quantite,
                StockDisponible = lp.Produit.Stock
            }).ToList();

            var adresses = await _context.Adresses
                .Where(a => a.ClientId == clientId)
                .Select(a => new AdresseListItemViewModel
                {
                    Id = a.Id,
                    Rue = a.Rue,
                    Ville = a.Ville,
                    CodePostal = a.CodePostal,
                    Pays = a.Pays,
                    EstPrincipale = a.EstPrincipale
                })
                .ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                Panier = new PanierViewModel 
                { 
                    Items = panierItems,
                    Total = await CalculerTotalAsync(clientId),
                    NombreArticles = await ObtenirNombreArticlesAsync(clientId)
                },
                AdressesDisponibles = adresses,
                AdresseLivraisonId = adresses.FirstOrDefault(a => a.EstPrincipale)?.Id ?? adresses.FirstOrDefault()?.Id
            };

            ViewBag.CartCount = viewModel.Panier.NombreArticles;
            return View(viewModel);
        }

        // POST: /Commande/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var clientId = GetClientId();
            
            // Get cart from database
            var lignesPanier = await ObtenirLignesPanierAvecProduitsAsync(clientId);

            if (!lignesPanier.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            // Handle new address creation
            if (model.CreerNouvelleAdresse && model.NouvelleAdresse != null)
            {
                if (string.IsNullOrWhiteSpace(model.NouvelleAdresse.Rue) ||
                    string.IsNullOrWhiteSpace(model.NouvelleAdresse.Ville) ||
                    string.IsNullOrWhiteSpace(model.NouvelleAdresse.CodePostal))
                {
                    ModelState.AddModelError("", "Veuillez remplir tous les champs de l'adresse");
                    return await ReloadCheckoutView(clientId);
                }

                var nouvelleAdresse = new Adresse
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    Rue = model.NouvelleAdresse.Rue,
                    Ville = model.NouvelleAdresse.Ville,
                    CodePostal = model.NouvelleAdresse.CodePostal,
                    Pays = model.NouvelleAdresse.Pays ?? "France",
                    EstPrincipale = false
                };

                await _context.Adresses.AddAsync(nouvelleAdresse);
                await _context.SaveChangesAsync();
                
                model.AdresseLivraisonId = nouvelleAdresse.Id;
            }

            if (!model.AdresseLivraisonId.HasValue)
            {
                ModelState.AddModelError("", "Veuillez sélectionner une adresse de livraison");
                return await ReloadCheckoutView(clientId);
            }

            // Verify address belongs to client
            var adresseLivraison = await _context.Adresses
                .FirstOrDefaultAsync(a => a.Id == model.AdresseLivraisonId && a.ClientId == clientId);

            if (adresseLivraison == null)
            {
                ModelState.AddModelError("", "Adresse invalide");
                return await ReloadCheckoutView(clientId);
            }

            // ========= ORDER CREATION FLOW =========
            var lignesCommande = new List<LigneCommande>();
            decimal montantTotal = 0;

            foreach (var lignePanier in lignesPanier)
            {
                var produit = lignePanier.Produit;

                if (produit == null || !produit.EstActif)
                {
                    TempData["ErrorMessage"] = $"Le produit '{lignePanier.Produit?.Titre}' n'est plus disponible";
                    return RedirectToAction("Index", "Panier");
                }

                if (produit.Stock < lignePanier.Quantite)
                {
                    TempData["ErrorMessage"] = $"Stock insuffisant pour '{produit.Titre}'. Disponible: {produit.Stock}";
                    return RedirectToAction("Index", "Panier");
                }

                var ligneCommande = new LigneCommande
                {
                    Id = Guid.NewGuid(),
                    ProduitId = produit.Id,
                    Quantite = lignePanier.Quantite,
                    PrixUnitaire = produit.Prix
                };

                lignesCommande.Add(ligneCommande);

                // Update stock
                produit.Stock -= lignePanier.Quantite;
            }

            // Calculate total
            montantTotal = lignesCommande.Sum(l => l.Quantite * l.PrixUnitaire);

            // Add shipping if under 50€
            if (montantTotal < 50)
            {
                montantTotal += 4.99m;
            }

            // Create order
            var commande = new Commande
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Date = DateTime.Now,
                Statut = StatutCommande.EnAttente,
                MontantTotal = montantTotal,
                AdresseLivraisonId = model.AdresseLivraisonId
            };

            // Link order lines to order
            foreach (var ligne in lignesCommande)
            {
                ligne.CommandeId = commande.Id;
            }

            commande.LignesCommande = lignesCommande;

            // Save to database
            await _context.Commandes.AddAsync(commande);
            await _context.SaveChangesAsync();

            // Clear cart
            await ViderPanierAsync(clientId);

            TempData["SuccessMessage"] = "Commande passée avec succès !";
            TempData["CommandeId"] = commande.Id.ToString();

            return RedirectToAction(nameof(Confirmation), new { id = commande.Id });
        }

        // GET: /Commande/Confirmation/{id}
        public async Task<IActionResult> Confirmation(Guid id)
        {
            var clientId = GetClientId();
            var commande = await _context.Commandes
                .Include(c => c.AdresseLivraison)
                .Include(c => c.LignesCommande)
                .FirstOrDefaultAsync(c => c.Id == id && c.ClientId == clientId);

            if (commande == null)
            {
                return NotFound();
            }

            var viewModel = new ConfirmationViewModel
            {
                CommandeId = commande.Id,
                DateCommande = commande.Date,
                MontantTotal = commande.MontantTotal,
                NombreArticles = commande.LignesCommande.Sum(l => l.Quantite),
                AdresseLivraison = commande.AdresseLivraison != null
                    ? $"{commande.AdresseLivraison.Rue}, {commande.AdresseLivraison.CodePostal} {commande.AdresseLivraison.Ville}"
                    : "Non spécifiée"
            };

            ViewBag.CartCount = 0;
            return View(viewModel);
        }

        // GET: /Commande/MesCommandes
        public async Task<IActionResult> MesCommandes()
        {
            var clientId = GetClientId();
            var commandes = await _context.Commandes
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.Date)
                .Include(c => c.LignesCommande)
                .Select(c => new CommandeListItemViewModel
                {
                    Id = c.Id,
                    Date = c.Date,
                    Statut = c.Statut,
                    MontantTotal = c.MontantTotal,
                    NombreArticles = c.LignesCommande.Sum(l => l.Quantite)
                })
                .ToListAsync();

            ViewBag.CartCount = await ObtenirNombreArticlesAsync(clientId);
            return View(commandes);
        }

        // GET: /Commande/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var clientId = GetClientId();
            var commande = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.AdresseLivraison)
                .Include(c => c.LignesCommande)
                    .ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync(c => c.Id == id && c.ClientId == clientId);

            if (commande == null)
            {
                return NotFound();
            }

            var viewModel = new CommandeDetailsViewModel
            {
                Id = commande.Id,
                Date = commande.Date,
                Statut = commande.Statut,
                MontantTotal = commande.MontantTotal,
                ClientNom = commande.Client.Prenom + " " + commande.Client.Nom,
                ClientEmail = commande.Client.Email,
                AdresseLivraison = commande.AdresseLivraison != null
                    ? $"{commande.AdresseLivraison.Rue}, {commande.AdresseLivraison.CodePostal} {commande.AdresseLivraison.Ville}, {commande.AdresseLivraison.Pays}"
                    : "Non spécifiée",
                Lignes = commande.LignesCommande.Select(l => new LigneCommandeViewModel
                {
                    ProduitId = l.ProduitId,
                    ProduitTitre = l.Produit.Titre,
                    ProduitImage = l.Produit.ImageUrl,
                    Quantite = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire
                }).ToList()
            };

            ViewBag.CartCount = await ObtenirNombreArticlesAsync(clientId);
            return View(viewModel);
        }

        private async Task<IActionResult> ReloadCheckoutView(Guid clientId)
        {
            var lignesPanier = await ObtenirLignesPanierAvecProduitsAsync(clientId);
            var panierItems = lignesPanier.Select(lp => new PanierItemViewModel
            {
                ProduitId = lp.ProduitId,
                Titre = lp.Produit.Titre,
                ImageUrl = lp.Produit.ImageUrl,
                Prix = lp.Produit.Prix,
                Quantite = lp.Quantite,
                StockDisponible = lp.Produit.Stock
            }).ToList();

            var adresses = await _context.Adresses
                .Where(a => a.ClientId == clientId)
                .Select(a => new AdresseListItemViewModel
                {
                    Id = a.Id,
                    Rue = a.Rue,
                    Ville = a.Ville,
                    CodePostal = a.CodePostal,
                    Pays = a.Pays,
                    EstPrincipale = a.EstPrincipale
                })
                .ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                Panier = new PanierViewModel 
                { 
                    Items = panierItems,
                    Total = await CalculerTotalAsync(clientId),
                    NombreArticles = await ObtenirNombreArticlesAsync(clientId)
                },
                AdressesDisponibles = adresses
            };

            ViewBag.CartCount = viewModel.Panier.NombreArticles;
            return View(viewModel);
        }

        #region Private Helper Methods (previously in PanierService)

        private async Task<List<LignePanier>> ObtenirLignesPanierAvecProduitsAsync(Guid clientId)
        {
            var panier = await _context.Paniers
                .Include(p => p.LignesPanier)
                .ThenInclude(lp => lp.Produit)
                .ThenInclude(pr => pr.Fournisseur)
                .FirstOrDefaultAsync(p => p.ClientId == clientId);

            return panier?.LignesPanier.ToList() ?? new List<LignePanier>();
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

        #endregion

        private Guid GetClientId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }
    }
}
