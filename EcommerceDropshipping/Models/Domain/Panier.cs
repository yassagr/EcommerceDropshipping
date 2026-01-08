using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceDropshipping.Models.Domain
{
    public class Panier
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ClientId { get; set; }
        
        public DateTime DateCreation { get; set; }
        
        public DateTime DateModification { get; set; }
        
        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual ICollection<LignePanier> LignesPanier { get; set; } = new List<LignePanier>();
    }
}
