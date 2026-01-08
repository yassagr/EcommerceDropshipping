using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcommerceDropshipping.Data;
using EcommerceDropshipping.Models.Domain;
using EcommerceDropshipping.ViewModels.Client;
using EcommerceDropshipping.ViewModels.Adresse;
using EcommerceDropshipping.Helpers;
using EcommerceDropshipping.ViewModels.Panier;

namespace EcommerceDropshipping.Controllers
{
    public class ClientController : Controller
    {
        private readonly AppDbContext _context;

        public ClientController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Client/Register
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CartCount = GetCartItemCount();
            return View(new RegisterViewModel());
        }

        // POST: /Client/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            // Check if email already exists
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (existingClient != null)
            {
                ModelState.AddModelError("Email", "Un compte avec cet email existe déjà");
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Nom = model.Nom,
                Prenom = model.Prenom,
                Email = model.Email,
                MotDePasse = BCrypt.Net.BCrypt.HashPassword(model.MotDePasse),
                DateInscription = DateTime.Now,
                Role = "Client"
            };

            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Compte créé avec succès ! Vous pouvez maintenant vous connecter.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Client/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CartCount = GetCartItemCount();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Client/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == model.Email);

            if (client == null || !BCrypt.Net.BCrypt.Verify(model.MotDePasse, client.MotDePasse))
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect");
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Name, $"{client.Prenom} {client.Nom}"),
                new Claim(ClaimTypes.Role, client.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            TempData["SuccessMessage"] = $"Bienvenue, {client.Prenom} !";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Client/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Vous avez été déconnecté";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Client/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var clientId = GetClientId();
            var client = await _context.Clients
                .Include(c => c.Adresses)
                .Include(c => c.Commandes)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                Id = client.Id,
                Nom = client.Nom,
                Prenom = client.Prenom,
                Email = client.Email,
                DateInscription = client.DateInscription,
                NombreCommandes = client.Commandes.Count,
                TotalAchats = client.Commandes.Where(c => c.Statut != Models.Domain.Enums.StatutCommande.Annulee).Sum(c => c.MontantTotal),
                Adresses = client.Adresses.Select(a => new AdresseListItemViewModel
                {
                    Id = a.Id,
                    Rue = a.Rue,
                    Ville = a.Ville,
                    CodePostal = a.CodePostal,
                    Pays = a.Pays,
                    EstPrincipale = a.EstPrincipale
                }).ToList()
            };

            ViewBag.CartCount = GetCartItemCount();
            return View(viewModel);
        }

        // POST: /Client/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var clientId = GetClientId();
            var client = await _context.Clients.FindAsync(clientId);

            if (client == null)
            {
                return NotFound();
            }

            client.Nom = model.Nom;
            client.Prenom = model.Prenom;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profil mis à jour avec succès";
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Client/Adresses
        [Authorize]
        public async Task<IActionResult> Adresses()
        {
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

            ViewBag.CartCount = GetCartItemCount();
            return View(adresses);
        }

        // GET: /Client/AddAdresse
        [Authorize]
        public IActionResult AddAdresse()
        {
            ViewBag.CartCount = GetCartItemCount();
            return View(new AddAdresseViewModel());
        }

        // POST: /Client/AddAdresse
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddAdresse(AddAdresseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            var clientId = GetClientId();

            // If this is set as primary, unset other primary addresses
            if (model.EstPrincipale)
            {
                var existingPrimary = await _context.Adresses
                    .Where(a => a.ClientId == clientId && a.EstPrincipale)
                    .ToListAsync();
                
                foreach (var addr in existingPrimary)
                {
                    addr.EstPrincipale = false;
                }
            }

            var adresse = new Adresse
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Rue = model.Rue,
                Ville = model.Ville,
                CodePostal = model.CodePostal,
                Pays = model.Pays,
                EstPrincipale = model.EstPrincipale
            };

            await _context.Adresses.AddAsync(adresse);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Adresse ajoutée avec succès";
            return RedirectToAction(nameof(Adresses));
        }

        // GET: /Client/EditAdresse/{id}
        [Authorize]
        public async Task<IActionResult> EditAdresse(Guid id)
        {
            var clientId = GetClientId();
            var adresse = await _context.Adresses
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == clientId);

            if (adresse == null)
            {
                return NotFound();
            }

            var viewModel = new EditAdresseViewModel
            {
                Id = adresse.Id,
                Rue = adresse.Rue,
                Ville = adresse.Ville,
                CodePostal = adresse.CodePostal,
                Pays = adresse.Pays,
                EstPrincipale = adresse.EstPrincipale
            };

            ViewBag.CartCount = GetCartItemCount();
            return View(viewModel);
        }

        // POST: /Client/EditAdresse
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditAdresse(EditAdresseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CartCount = GetCartItemCount();
                return View(model);
            }

            var clientId = GetClientId();
            var adresse = await _context.Adresses
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.ClientId == clientId);

            if (adresse == null)
            {
                return NotFound();
            }

            // If setting as primary, unset others
            if (model.EstPrincipale && !adresse.EstPrincipale)
            {
                var existingPrimary = await _context.Adresses
                    .Where(a => a.ClientId == clientId && a.EstPrincipale && a.Id != model.Id)
                    .ToListAsync();
                
                foreach (var addr in existingPrimary)
                {
                    addr.EstPrincipale = false;
                }
            }

            adresse.Rue = model.Rue;
            adresse.Ville = model.Ville;
            adresse.CodePostal = model.CodePostal;
            adresse.Pays = model.Pays;
            adresse.EstPrincipale = model.EstPrincipale;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Adresse modifiée avec succès";
            return RedirectToAction(nameof(Adresses));
        }

        // POST: /Client/DeleteAdresse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteAdresse(Guid id)
        {
            var clientId = GetClientId();
            var adresse = await _context.Adresses
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == clientId);

            if (adresse == null)
            {
                return NotFound();
            }

            _context.Adresses.Remove(adresse);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Adresse supprimée avec succès";
            return RedirectToAction(nameof(Adresses));
        }

        // GET: /Client/AccessDenied
        public IActionResult AccessDenied()
        {
            ViewBag.CartCount = GetCartItemCount();
            return View();
        }

        private Guid GetClientId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }

        private int GetCartItemCount()
        {
            var panier = HttpContext.Session.GetObjectFromJson<List<PanierItemViewModel>>("Panier");
            return panier?.Sum(i => i.Quantite) ?? 0;
        }
    }
}
