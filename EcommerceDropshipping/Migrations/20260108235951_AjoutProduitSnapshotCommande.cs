using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDropshipping.Migrations
{
    /// <inheritdoc />
    public partial class AjoutProduitSnapshotCommande : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProduitId",
                table: "LignesCommande",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ProduitImage",
                table: "LignesCommande",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProduitTitre",
                table: "LignesCommande",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProduitImage",
                table: "LignesCommande");

            migrationBuilder.DropColumn(
                name: "ProduitTitre",
                table: "LignesCommande");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProduitId",
                table: "LignesCommande",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
