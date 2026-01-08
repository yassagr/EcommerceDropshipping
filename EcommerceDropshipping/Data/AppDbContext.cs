using Microsoft.EntityFrameworkCore;
using EcommerceDropshipping.Models.Domain;
using EcommerceDropshipping.Models.Domain.Enums;

namespace EcommerceDropshipping.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<Fournisseur> Fournisseurs { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<LigneCommande> LignesCommande { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<LignePanier> LignesPanier { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Prenom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MotDePasse).IsRequired();
                entity.Property(e => e.Role).HasDefaultValue("Client");
                entity.Property(e => e.DateInscription).HasDefaultValueSql("GETDATE()");
            });

            // Adresse configuration
            modelBuilder.Entity<Adresse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rue).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Ville).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CodePostal).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Pays).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Adresses)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Fournisseur configuration
            modelBuilder.Entity<Fournisseur>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telephone).HasMaxLength(20);
            });

            // Produit configuration
            modelBuilder.Entity<Produit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Prix).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.EstActif).HasDefaultValue(true);
                entity.Property(e => e.DateAjout).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Fournisseur)
                      .WithMany(f => f.Produits)
                      .HasForeignKey(e => e.FournisseurId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Commande configuration
            modelBuilder.Entity<Commande>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontantTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Statut).HasConversion<string>();
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Commandes)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.AdresseLivraison)
                      .WithMany()
                      .HasForeignKey(e => e.AdresseLivraisonId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // LigneCommande configuration
            modelBuilder.Entity<LigneCommande>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrixUnitaire).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Commande)
                      .WithMany(c => c.LignesCommande)
                      .HasForeignKey(e => e.CommandeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Produit)
                      .WithMany(p => p.LignesCommande)
                      .HasForeignKey(e => e.ProduitId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Panier configuration
            modelBuilder.Entity<Panier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.DateModification).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ClientId).IsUnique();
            });

            // LignePanier configuration
            modelBuilder.Entity<LignePanier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DateAjout).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Panier)
                      .WithMany(p => p.LignesPanier)
                      .HasForeignKey(e => e.PanierId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Produit)
                      .WithMany()
                      .HasForeignKey(e => e.ProduitId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.PanierId, e.ProduitId }).IsUnique();
            });
        }
    }
}
