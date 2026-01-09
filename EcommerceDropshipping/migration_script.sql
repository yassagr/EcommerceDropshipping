IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [Id] uniqueidentifier NOT NULL,
        [Nom] nvarchar(100) NOT NULL,
        [Prenom] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [MotDePasse] nvarchar(max) NOT NULL,
        [DateInscription] datetime2 NOT NULL DEFAULT (GETDATE()),
        [Role] nvarchar(50) NOT NULL DEFAULT N'Client',
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [Fournisseurs] (
        [Id] uniqueidentifier NOT NULL,
        [Nom] nvarchar(200) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Telephone] nvarchar(20) NULL,
        CONSTRAINT [PK_Fournisseurs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [Adresses] (
        [Id] uniqueidentifier NOT NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [Rue] nvarchar(200) NOT NULL,
        [Ville] nvarchar(100) NOT NULL,
        [CodePostal] nvarchar(20) NOT NULL,
        [Pays] nvarchar(100) NOT NULL,
        [EstPrincipale] bit NOT NULL,
        CONSTRAINT [PK_Adresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Adresses_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [Produits] (
        [Id] uniqueidentifier NOT NULL,
        [FournisseurId] uniqueidentifier NOT NULL,
        [Titre] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Prix] decimal(18,2) NOT NULL,
        [Stock] int NOT NULL,
        [ImageUrl] nvarchar(500) NULL,
        [DateAjout] datetime2 NOT NULL DEFAULT (GETDATE()),
        [EstActif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Produits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Produits_Fournisseurs_FournisseurId] FOREIGN KEY ([FournisseurId]) REFERENCES [Fournisseurs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [Commandes] (
        [Id] uniqueidentifier NOT NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [Date] datetime2 NOT NULL DEFAULT (GETDATE()),
        [Statut] nvarchar(max) NOT NULL,
        [MontantTotal] decimal(18,2) NOT NULL,
        [AdresseLivraisonId] uniqueidentifier NULL,
        CONSTRAINT [PK_Commandes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Commandes_Adresses_AdresseLivraisonId] FOREIGN KEY ([AdresseLivraisonId]) REFERENCES [Adresses] ([Id]),
        CONSTRAINT [FK_Commandes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE TABLE [LignesCommande] (
        [Id] uniqueidentifier NOT NULL,
        [CommandeId] uniqueidentifier NOT NULL,
        [ProduitId] uniqueidentifier NOT NULL,
        [Quantite] int NOT NULL,
        [PrixUnitaire] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_LignesCommande] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LignesCommande_Commandes_CommandeId] FOREIGN KEY ([CommandeId]) REFERENCES [Commandes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LignesCommande_Produits_ProduitId] FOREIGN KEY ([ProduitId]) REFERENCES [Produits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Adresses_ClientId] ON [Adresses] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_Email] ON [Clients] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Commandes_AdresseLivraisonId] ON [Commandes] ([AdresseLivraisonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Commandes_ClientId] ON [Commandes] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LignesCommande_CommandeId] ON [LignesCommande] ([CommandeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LignesCommande_ProduitId] ON [LignesCommande] ([ProduitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Produits_FournisseurId] ON [Produits] ([FournisseurId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228185738_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228185738_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229084423_Migration Initiale'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229084423_Migration Initiale', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    CREATE TABLE [Paniers] (
        [Id] uniqueidentifier NOT NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (GETDATE()),
        [DateModification] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Paniers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Paniers_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    CREATE TABLE [LignesPanier] (
        [Id] uniqueidentifier NOT NULL,
        [PanierId] uniqueidentifier NOT NULL,
        [ProduitId] uniqueidentifier NOT NULL,
        [Quantite] int NOT NULL,
        [DateAjout] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_LignesPanier] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LignesPanier_Paniers_PanierId] FOREIGN KEY ([PanierId]) REFERENCES [Paniers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LignesPanier_Produits_ProduitId] FOREIGN KEY ([ProduitId]) REFERENCES [Produits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LignesPanier_PanierId_ProduitId] ON [LignesPanier] ([PanierId], [ProduitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    CREATE INDEX [IX_LignesPanier_ProduitId] ON [LignesPanier] ([ProduitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Paniers_ClientId] ON [Paniers] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229221343_AjoutTablePanier'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229221343_AjoutTablePanier', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108235951_AjoutProduitSnapshotCommande'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LignesCommande]') AND [c].[name] = N'ProduitId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [LignesCommande] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [LignesCommande] ALTER COLUMN [ProduitId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108235951_AjoutProduitSnapshotCommande'
)
BEGIN
    ALTER TABLE [LignesCommande] ADD [ProduitImage] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108235951_AjoutProduitSnapshotCommande'
)
BEGIN
    ALTER TABLE [LignesCommande] ADD [ProduitTitre] nvarchar(200) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108235951_AjoutProduitSnapshotCommande'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260108235951_AjoutProduitSnapshotCommande', N'8.0.0');
END;
GO

COMMIT;
GO

