using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.ViewModels.Produit;
using EcommerceDropshipping.Helpers;
using EcommerceDropshipping.ViewModels.Panier;

namespace EcommerceDropshipping.Controllers
{
    public class ProduitController : Controller
    {
        private readonly AppDbContext _context;

        public ProduitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Produit (Catalogue)
        public async Task<IActionResult> Index(
            string? search,
            decimal? prixMin,
            decimal? prixMax,
            Guid? fournisseurId,
            string? tri,
            int page = 1)
        {
            var pageSize = 12;

            var query = _context.Produits
                .Where(p => p.EstActif)
                .Include(p => p.Fournisseur)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Titre.Contains(search) || 
                                         (p.Description != null && p.Description.Contains(search)));
            }

            // Price filter
            if (prixMin.HasValue)
            {
                query = query.Where(p => p.Prix >= prixMin.Value);
            }
            if (prixMax.HasValue)
            {
                query = query.Where(p => p.Prix <= prixMax.Value);
            }

            // Supplier filter
            if (fournisseurId.HasValue)
            {
                query = query.Where(p => p.FournisseurId == fournisseurId.Value);
            }

            // Sorting
            query = tri switch
            {
                "prix_asc" => query.OrderBy(p => p.Prix),
                "prix_desc" => query.OrderByDescending(p => p.Prix),
                "nom" => query.OrderBy(p => p.Titre),
                "nouveau" => query.OrderByDescending(p => p.DateAjout),
                _ => query.OrderByDescending(p => p.DateAjout)
            };

            var totalItems = await query.CountAsync();

            var produits = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            // Get suppliers for filter
            var fournisseurs = await _context.Fournisseurs
                .Select(f => new FournisseurFilterViewModel
                {
                    Id = f.Id,
                    Nom = f.Nom,
                    NombreProduits = f.Produits.Count(p => p.EstActif)
                })
                .Where(f => f.NombreProduits > 0)
                .ToListAsync();

            var viewModel = new CatalogueViewModel
            {
                Produits = produits,
                SearchQuery = search,
                PrixMin = prixMin,
                PrixMax = prixMax,
                FournisseurId = fournisseurId,
                TriPar = tri,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Fournisseurs = fournisseurs
            };

            ViewBag.CartCount = GetCartItemCount();
            return View(viewModel);
        }

        // GET: /Produit/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var produit = await _context.Produits
                .Include(p => p.Fournisseur)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null || !produit.EstActif)
            {
                return NotFound();
            }

            var viewModel = new ProduitDetailsViewModel
            {
                Id = produit.Id,
                Titre = produit.Titre,
                Description = produit.Description,
                Prix = produit.Prix,
                Stock = produit.Stock,
                ImageUrl = produit.ImageUrl,
                DateAjout = produit.DateAjout,
                EstActif = produit.EstActif,
                FournisseurNom = produit.Fournisseur.Nom
            };

            // Get related products
            var relatedProducts = await _context.Produits
                .Where(p => p.FournisseurId == produit.FournisseurId && p.Id != produit.Id && p.EstActif)
                .Take(4)
                .Select(p => new ProduitListItemViewModel
                {
                    Id = p.Id,
                    Titre = p.Titre,
                    Prix = p.Prix,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    DateAjout = p.DateAjout
                })
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.CartCount = GetCartItemCount();
            return View(viewModel);
        }

        // GET: /Produit/Search (AJAX)
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var results = await _context.Produits
                .Where(p => p.EstActif && (p.Titre.Contains(term) || (p.Description != null && p.Description.Contains(term))))
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    titre = p.Titre,
                    prix = p.Prix,
                    image = p.ImageUrl
                })
                .ToListAsync();

            return Json(results);
        }

        private int GetCartItemCount()
        {
            var panier = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>("Panier");
            return panier?.Sum(i => i.Quantite) ?? 0;
        }
    }
}
