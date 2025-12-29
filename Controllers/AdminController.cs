using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.Models.Domain;
using EcommerceDropshipping.Models.Domain.Enums;
using EcommerceDropshipping.ViewModels.Admin;
using EcommerceDropshipping.ViewModels.Produit;
using EcommerceDropshipping.ViewModels.Fournisseur;
using EcommerceDropshipping.ViewModels.Commande;

namespace EcommerceDropshipping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel
            {
                TotalCommandes = await _context.Commandes.CountAsync(),
                CommandesEnAttente = await _context.Commandes.CountAsync(c => c.Statut == StatutCommande.EnAttente),
                RevenusTotal = await _context.Commandes.Where(c => c.Statut != StatutCommande.Annulee).SumAsync(c => c.MontantTotal),
                RevenusMois = await _context.Commandes
                    .Where(c => c.Date.Month == DateTime.Now.Month && c.Date.Year == DateTime.Now.Year && c.Statut != StatutCommande.Annulee)
                    .SumAsync(c => c.MontantTotal),
                TotalProduits = await _context.Produits.CountAsync(),
                ProduitsActifs = await _context.Produits.CountAsync(p => p.EstActif),
                TotalClients = await _context.Clients.CountAsync(c => c.Role == "Client"),
                TotalFournisseurs = await _context.Fournisseurs.CountAsync(),
                ProduitsStockFaible = await _context.Produits.CountAsync(p => p.Stock < 10 && p.EstActif)
            };

            // Recent orders
            viewModel.CommandesRecentes = await _context.Commandes
                .Include(c => c.Client)
                .OrderByDescending(c => c.Date)
                .Take(10)
                .Select(c => new CommandeRecenteViewModel
                {
                    Id = c.Id,
                    ClientNom = c.Client.Prenom + " " + c.Client.Nom,
                    Date = c.Date,
                    Montant = c.MontantTotal,
                    Statut = c.Statut.ToString(),
                    StatutClass = c.Statut == StatutCommande.EnAttente ? "warning" :
                                  c.Statut == StatutCommande.EnCours ? "info" :
                                  c.Statut == StatutCommande.Expediee ? "primary" :
                                  c.Statut == StatutCommande.Livree ? "success" : "danger"
                })
                .ToListAsync();

            // Top selling products
            viewModel.ProduitsPopulaires = await _context.LignesCommande
                .Include(l => l.Produit)
                .GroupBy(l => new { l.ProduitId, l.Produit.Titre, l.Produit.ImageUrl })
                .Select(g => new ProduitPopulaireViewModel
                {
                    Id = g.Key.ProduitId,
                    Titre = g.Key.Titre,
                    ImageUrl = g.Key.ImageUrl,
                    VentesTotal = g.Sum(l => l.Quantite),
                    RevenusTotal = g.Sum(l => l.Quantite * l.PrixUnitaire)
                })
                .OrderByDescending(p => p.VentesTotal)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }

        #region Produits CRUD

        // GET: /Admin/Produits
        public async Task<IActionResult> Produits()
        {
            var produits = await _context.Produits
                .Include(p => p.Fournisseur)
                .OrderByDescending(p => p.DateAjout)
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

            return View(produits);
        }

        // GET: /Admin/AddProduit
        public async Task<IActionResult> AddProduit()
        {
            await LoadFournisseursSelectList();
            return View(new AddProduitViewModel { EstActif = true });
        }

        // POST: /Admin/AddProduit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduit(AddProduitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFournisseursSelectList();
                return View(model);
            }

            var produit = new Produit
            {
                Id = Guid.NewGuid(),
                FournisseurId = model.FournisseurId,
                Titre = model.Titre,
                Description = model.Description,
                Prix = model.Prix,
                Stock = model.Stock,
                ImageUrl = model.ImageUrl,
                DateAjout = DateTime.Now,
                EstActif = model.EstActif
            };

            await _context.Produits.AddAsync(produit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produit ajouté avec succès";
            return RedirectToAction(nameof(Produits));
        }

        // GET: /Admin/EditProduit/{id}
        public async Task<IActionResult> EditProduit(Guid id)
        {
            var produit = await _context.Produits.FindAsync(id);

            if (produit == null)
            {
                return NotFound();
            }

            var viewModel = new EditProduitViewModel
            {
                Id = produit.Id,
                Titre = produit.Titre,
                Description = produit.Description,
                Prix = produit.Prix,
                Stock = produit.Stock,
                ImageUrl = produit.ImageUrl,
                FournisseurId = produit.FournisseurId,
                EstActif = produit.EstActif,
                DateAjout = produit.DateAjout
            };

            await LoadFournisseursSelectList();
            return View(viewModel);
        }

        // POST: /Admin/EditProduit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduit(EditProduitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFournisseursSelectList();
                return View(model);
            }

            var produit = await _context.Produits.FindAsync(model.Id);

            if (produit == null)
            {
                return NotFound();
            }

            produit.Titre = model.Titre;
            produit.Description = model.Description;
            produit.Prix = model.Prix;
            produit.Stock = model.Stock;
            produit.ImageUrl = model.ImageUrl;
            produit.FournisseurId = model.FournisseurId;
            produit.EstActif = model.EstActif;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produit modifié avec succès";
            return RedirectToAction(nameof(Produits));
        }

        // POST: /Admin/DeleteProduit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduit(Guid id)
        {
            var produit = await _context.Produits.FindAsync(id);

            if (produit == null)
            {
                return NotFound();
            }

            // Check if product has orders
            var hasOrders = await _context.LignesCommande.AnyAsync(l => l.ProduitId == id);
            if (hasOrders)
            {
                // Soft delete - just deactivate
                produit.EstActif = false;
                TempData["WarningMessage"] = "Le produit a été désactivé car il a des commandes associées";
            }
            else
            {
                _context.Produits.Remove(produit);
                TempData["SuccessMessage"] = "Produit supprimé avec succès";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Produits));
        }

        #endregion

        #region Fournisseurs CRUD

        // GET: /Admin/Fournisseurs
        public async Task<IActionResult> Fournisseurs()
        {
            var fournisseurs = await _context.Fournisseurs
                .Include(f => f.Produits)
                .Select(f => new FournisseurListItemViewModel
                {
                    Id = f.Id,
                    Nom = f.Nom,
                    Email = f.Email,
                    Telephone = f.Telephone,
                    NombreProduits = f.Produits.Count
                })
                .ToListAsync();

            return View(fournisseurs);
        }

        // GET: /Admin/AddFournisseur
        public IActionResult AddFournisseur()
        {
            return View(new AddFournisseurViewModel());
        }

        // POST: /Admin/AddFournisseur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFournisseur(AddFournisseurViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fournisseur = new Fournisseur
            {
                Id = Guid.NewGuid(),
                Nom = model.Nom,
                Email = model.Email,
                Telephone = model.Telephone
            };

            await _context.Fournisseurs.AddAsync(fournisseur);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Fournisseur ajouté avec succès";
            return RedirectToAction(nameof(Fournisseurs));
        }

        // GET: /Admin/EditFournisseur/{id}
        public async Task<IActionResult> EditFournisseur(Guid id)
        {
            var fournisseur = await _context.Fournisseurs
                .Include(f => f.Produits)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fournisseur == null)
            {
                return NotFound();
            }

            var viewModel = new EditFournisseurViewModel
            {
                Id = fournisseur.Id,
                Nom = fournisseur.Nom,
                Email = fournisseur.Email,
                Telephone = fournisseur.Telephone,
                NombreProduits = fournisseur.Produits.Count
            };

            return View(viewModel);
        }

        // POST: /Admin/EditFournisseur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFournisseur(EditFournisseurViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fournisseur = await _context.Fournisseurs.FindAsync(model.Id);

            if (fournisseur == null)
            {
                return NotFound();
            }

            fournisseur.Nom = model.Nom;
            fournisseur.Email = model.Email;
            fournisseur.Telephone = model.Telephone;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Fournisseur modifié avec succès";
            return RedirectToAction(nameof(Fournisseurs));
        }

        // POST: /Admin/DeleteFournisseur/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFournisseur(Guid id)
        {
            var fournisseur = await _context.Fournisseurs
                .Include(f => f.Produits)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fournisseur == null)
            {
                return NotFound();
            }

            if (fournisseur.Produits.Any())
            {
                TempData["ErrorMessage"] = "Impossible de supprimer ce fournisseur car il a des produits associés";
                return RedirectToAction(nameof(Fournisseurs));
            }

            _context.Fournisseurs.Remove(fournisseur);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Fournisseur supprimé avec succès";
            return RedirectToAction(nameof(Fournisseurs));
        }

        #endregion

        #region Commandes Management

        // GET: /Admin/Commandes
        public async Task<IActionResult> Commandes(StatutCommande? statut = null)
        {
            var query = _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.LignesCommande)
                .AsQueryable();

            if (statut.HasValue)
            {
                query = query.Where(c => c.Statut == statut.Value);
            }

            var commandes = await query
                .OrderByDescending(c => c.Date)
                .Select(c => new CommandeListItemViewModel
                {
                    Id = c.Id,
                    Date = c.Date,
                    Statut = c.Statut,
                    MontantTotal = c.MontantTotal,
                    NombreArticles = c.LignesCommande.Sum(l => l.Quantite),
                    ClientNom = c.Client.Prenom + " " + c.Client.Nom,
                    ClientEmail = c.Client.Email
                })
                .ToListAsync();

            ViewBag.StatutFilter = statut;
            return View(commandes);
        }

        // GET: /Admin/CommandeDetails/{id}
        public async Task<IActionResult> CommandeDetails(Guid id)
        {
            var commande = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.AdresseLivraison)
                .Include(c => c.LignesCommande)
                    .ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync(c => c.Id == id);

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

            return View(viewModel);
        }

        // POST: /Admin/UpdateStatutCommande
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatutCommande(Guid id, StatutCommande statut)
        {
            var commande = await _context.Commandes.FindAsync(id);

            if (commande == null)
            {
                return NotFound();
            }

            commande.Statut = statut;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Statut de la commande mis à jour : {statut}";
            return RedirectToAction(nameof(CommandeDetails), new { id });
        }

        #endregion

        private async Task LoadFournisseursSelectList()
        {
            var fournisseurs = await _context.Fournisseurs
                .OrderBy(f => f.Nom)
                .Select(f => new { f.Id, f.Nom })
                .ToListAsync();

            ViewBag.Fournisseurs = new SelectList(fournisseurs, "Id", "Nom");
        }
    }
}
