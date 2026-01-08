using EcommerceDropshipping.Models.Domain;

namespace EcommerceDropshipping.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if already seeded
            if (context.Clients.Any())
            {
                return;
            }

            // Create Admin user
            var admin = new Client
            {
                Id = Guid.NewGuid(),
                Nom = "Admin",
                Prenom = "Super",
                Email = "admin@ecommerce.com",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                DateInscription = DateTime.Now
            };
            context.Clients.Add(admin);

            // Create test client
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "jean.dupont@email.com",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Client123!"),
                Role = "Client",
                DateInscription = DateTime.Now
            };
            context.Clients.Add(client);

            // Create test address for client
            var adresse = new Adresse
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                Rue = "123 Rue de Paris",
                Ville = "Paris",
                CodePostal = "75001",
                Pays = "France",
                EstPrincipale = true
            };
            context.Adresses.Add(adresse);

            // Create suppliers
            var fournisseur1 = new Fournisseur
            {
                Id = Guid.NewGuid(),
                Nom = "TechSupply Co.",
                Email = "contact@techsupply.com",
                Telephone = "0612345678"
            };

            var fournisseur2 = new Fournisseur
            {
                Id = Guid.NewGuid(),
                Nom = "Fashion World",
                Email = "info@fashionworld.com",
                Telephone = "0623456789"
            };

            var fournisseur3 = new Fournisseur
            {
                Id = Guid.NewGuid(),
                Nom = "Home & Living",
                Email = "contact@homeliving.com",
                Telephone = "0634567890"
            };

            context.Fournisseurs.AddRange(fournisseur1, fournisseur2, fournisseur3);

            // Create products
            var produits = new List<Produit>
            {
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Smartphone Pro Max",
                    Description = "Le dernier smartphone avec écran OLED 6.7 pouces, caméra 108MP et batterie longue durée. Performance exceptionnelle pour les utilisateurs exigeants.",
                    Prix = 899.99m,
                    Stock = 45,
                    ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400",
                    DateAjout = DateTime.Now.AddDays(-5),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Laptop Ultra Slim",
                    Description = "Ordinateur portable léger et puissant avec processeur dernière génération, 16GB RAM et SSD 512GB. Parfait pour le travail et les loisirs.",
                    Prix = 1299.99m,
                    Stock = 30,
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400",
                    DateAjout = DateTime.Now.AddDays(-10),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Écouteurs Bluetooth Premium",
                    Description = "Écouteurs sans fil avec réduction de bruit active, autonomie 30h et son haute résolution. Confort optimal pour une utilisation prolongée.",
                    Prix = 249.99m,
                    Stock = 100,
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                    DateAjout = DateTime.Now.AddDays(-3),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Montre Connectée Sport",
                    Description = "Montre intelligente avec GPS intégré, suivi cardiaque et plus de 100 modes sportifs. Résistante à l'eau jusqu'à 50 mètres.",
                    Prix = 349.99m,
                    Stock = 60,
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400",
                    DateAjout = DateTime.Now.AddDays(-7),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur2.Id,
                    Titre = "Veste en Cuir Classique",
                    Description = "Veste en cuir véritable de haute qualité, doublure confortable et coupe ajustée. Un intemporel de la garde-robe.",
                    Prix = 299.99m,
                    Stock = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400",
                    DateAjout = DateTime.Now.AddDays(-15),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur2.Id,
                    Titre = "Sneakers Urban Style",
                    Description = "Baskets tendance avec semelle confortable et design moderne. Parfaites pour un style décontracté au quotidien.",
                    Prix = 129.99m,
                    Stock = 80,
                    ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400",
                    DateAjout = DateTime.Now.AddDays(-2),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur2.Id,
                    Titre = "Sac à Dos Designer",
                    Description = "Sac à dos élégant avec compartiment laptop, multiples poches et matériaux premium. Idéal pour le travail et les voyages.",
                    Prix = 189.99m,
                    Stock = 40,
                    ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400",
                    DateAjout = DateTime.Now.AddDays(-8),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur2.Id,
                    Titre = "Lunettes de Soleil Aviator",
                    Description = "Lunettes de soleil style aviateur avec verres polarisés et monture métallique. Protection UV 400.",
                    Prix = 159.99m,
                    Stock = 55,
                    ImageUrl = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=400",
                    DateAjout = DateTime.Now.AddDays(-12),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur3.Id,
                    Titre = "Lampe LED Design",
                    Description = "Lampe de bureau LED avec intensité réglable et température de couleur ajustable. Design minimaliste et élégant.",
                    Prix = 79.99m,
                    Stock = 70,
                    ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400",
                    DateAjout = DateTime.Now.AddDays(-6),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur3.Id,
                    Titre = "Coussin Décoratif Premium",
                    Description = "Coussin en velours doux avec rembourrage de qualité. Touche d'élégance pour votre intérieur.",
                    Prix = 49.99m,
                    Stock = 120,
                    ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400",
                    DateAjout = DateTime.Now.AddDays(-4),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur3.Id,
                    Titre = "Diffuseur d'Arômes",
                    Description = "Diffuseur ultrasonique avec LED d'ambiance et arrêt automatique. Capacité 300ml pour une utilisation prolongée.",
                    Prix = 59.99m,
                    Stock = 85,
                    ImageUrl = "https://images.unsplash.com/photo-1608571423902-eed4a5ad8108?w=400",
                    DateAjout = DateTime.Now.AddDays(-1),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Tablette 10 Pouces",
                    Description = "Tablette performante avec écran Full HD, 64GB de stockage et autonomie 12h. Parfaite pour le divertissement et la productivité.",
                    Prix = 449.99m,
                    Stock = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1561154464-82e9adf32764?w=400",
                    DateAjout = DateTime.Now.AddDays(-20),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur3.Id,
                    Titre = "Tapis de Yoga Premium",
                    Description = "Tapis antidérapant en matière écologique, épaisseur 6mm pour un confort optimal. Livré avec sangle de transport.",
                    Prix = 69.99m,
                    Stock = 50,
                    ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400",
                    DateAjout = DateTime.Now.AddDays(-9),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur2.Id,
                    Titre = "Montre Classique Homme",
                    Description = "Montre élégante avec bracelet en cuir et cadran minimaliste. Mouvement quartz précis et verre mineral.",
                    Prix = 199.99m,
                    Stock = 35,
                    ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=400",
                    DateAjout = DateTime.Now.AddDays(-14),
                    EstActif = true
                },
                new Produit
                {
                    Id = Guid.NewGuid(),
                    FournisseurId = fournisseur1.Id,
                    Titre = "Enceinte Bluetooth Portable",
                    Description = "Enceinte waterproof avec son 360°, autonomie 24h et appairage multiple. Idéale pour vos aventures.",
                    Prix = 129.99m,
                    Stock = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400",
                    DateAjout = DateTime.Now.AddDays(-11),
                    EstActif = true
                }
            };

            context.Produits.AddRange(produits);
            context.SaveChanges();
        }
    }
}
