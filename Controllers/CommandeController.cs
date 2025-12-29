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
using EcommerceDropshipping.Helpers;

namespace EcommerceDropshipping.Controllers
{
    [Authorize]
    public class CommandeController : Controller
    {
        private readonly AppDbContext _context;
        private const string PanierSessionKey = "Panier";

        public CommandeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Commande/Checkout
        public async Task<IActionResult> Checkout()
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey)
                              ?? new List<PanierItemViewModel>();

            if (!panierItems.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            // Refresh cart with current prices
            var produitIds = panierItems.Select(i => i.ProduitId).ToList();
            var produits = await _context.Produits
                .Where(p => produitIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in panierItems)
            {
                if (produits.TryGetValue(item.ProduitId, out var produit))
                {
                    item.Prix = produit.Prix;
                    item.Titre = produit.Titre;
                    item.ImageUrl = produit.ImageUrl;
                    item.StockDisponible = produit.Stock;
                }
            }

            HttpContext.Session.SetObjectAsJson(PanierSessionKey, panierItems);

            var clientId = GetClientId();
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
                Panier = new PanierViewModel { Items = panierItems },
                AdressesDisponibles = adresses,
                AdresseLivraisonId = adresses.FirstOrDefault(a => a.EstPrincipale)?.Id ?? adresses.FirstOrDefault()?.Id
            };

            ViewBag.CartCount = panierItems.Sum(i => i.Quantite);
            return View(viewModel);
        }

        // POST: /Commande/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var panierItems = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey)
                              ?? new List<PanierItemViewModel>();

            if (!panierItems.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            var clientId = GetClientId();

            // Handle new address creation
            if (model.CreerNouvelleAdresse && model.NouvelleAdresse != null)
            {
                if (string.IsNullOrWhiteSpace(model.NouvelleAdresse.Rue) ||
                    string.IsNullOrWhiteSpace(model.NouvelleAdresse.Ville) ||
                    string.IsNullOrWhiteSpace(model.NouvelleAdresse.CodePostal))
                {
                    ModelState.AddModelError("", "Veuillez remplir tous les champs de l'adresse");
                    return await ReloadCheckoutView(panierItems, clientId);
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
                return await ReloadCheckoutView(panierItems, clientId);
            }

            // Verify address belongs to client
            var adresseLivraison = await _context.Adresses
                .FirstOrDefaultAsync(a => a.Id == model.AdresseLivraisonId && a.ClientId == clientId);

            if (adresseLivraison == null)
            {
                ModelState.AddModelError("", "Adresse invalide");
                return await ReloadCheckoutView(panierItems, clientId);
            }

            // ========= ORDER CREATION FLOW (as per sequence diagram) =========

            var lignesCommande = new List<LigneCommande>();
            decimal montantTotal = 0;

            // For each item in cart: verify price and stock
            foreach (var item in panierItems)
            {
                // Step 3: VerifierPrixActuel(idProduit)
                var produit = await _context.Produits.FindAsync(item.ProduitId);

                if (produit == null || !produit.EstActif)
                {
                    TempData["ErrorMessage"] = $"Le produit '{item.Titre}' n'est plus disponible";
                    return RedirectToAction("Index", "Panier");
                }

                if (produit.Stock < item.Quantite)
                {
                    TempData["ErrorMessage"] = $"Stock insuffisant pour '{produit.Titre}'. Disponible: {produit.Stock}";
                    return RedirectToAction("Index", "Panier");
                }

                // Step 4: Retourner Prix - Use CURRENT price from DB
                var ligneCommande = new LigneCommande
                {
                    Id = Guid.NewGuid(),
                    ProduitId = produit.Id,
                    Quantite = item.Quantite,
                    PrixUnitaire = produit.Prix // Price at moment of order
                };

                lignesCommande.Add(ligneCommande);

                // Update stock
                produit.Stock -= item.Quantite;
            }

            // Step 5: CalculerTotal()
            montantTotal = lignesCommande.Sum(l => l.Quantite * l.PrixUnitaire);

            // Add shipping if under 50€
            if (montantTotal < 50)
            {
                montantTotal += 4.99m;
            }

            // Step 6: CreerNouvelleCommande(client, total, articles)
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

            // Step 7: SauvegarderEnBase()
            await _context.Commandes.AddAsync(commande);
            await _context.SaveChangesAsync();

            // Step 8: Confirmation (IdCommande)
            // Clear cart
            HttpContext.Session.Remove(PanierSessionKey);

            TempData["SuccessMessage"] = "Commande passée avec succès !";
            TempData["CommandeId"] = commande.Id.ToString();

            // Step 9 & 10: AfficherConfirmationSucces & Voir Page de Confirmation
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

            ViewBag.CartCount = GetCartItemCount();
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

            ViewBag.CartCount = GetCartItemCount();
            return View(viewModel);
        }

        private async Task<IActionResult> ReloadCheckoutView(List<PanierItemViewModel> panierItems, Guid clientId)
        {
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
                Panier = new PanierViewModel { Items = panierItems },
                AdressesDisponibles = adresses
            };

            ViewBag.CartCount = panierItems.Sum(i => i.Quantite);
            return View(viewModel);
        }

        private Guid GetClientId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }

        private int GetCartItemCount()
        {
            var panier = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>(PanierSessionKey);
            return panier?.Sum(i => i.Quantite) ?? 0;
        }
    }
}
