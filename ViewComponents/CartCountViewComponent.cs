using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Data;

namespace EcommerceDropshipping.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CartCountViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                var clientIdClaim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(clientIdClaim, out Guid clientId))
                {
                    var panier = await _context.Paniers
                        .Include(p => p.LignesPanier)
                        .FirstOrDefaultAsync(p => p.ClientId == clientId);

                    if (panier != null)
                    {
                        count = panier.LignesPanier.Sum(lp => lp.Quantite);
                    }
                }
            }

            return View(count);
        }
    }
}
