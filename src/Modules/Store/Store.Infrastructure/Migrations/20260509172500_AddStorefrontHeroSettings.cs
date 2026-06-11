using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Store.Infrastructure.Persistance;

#nullable disable

namespace Store.Infrastructure.Migrations
{
    [DbContext(typeof(StoreDbContext))]
    [Migration("20260509172500_AddStorefrontHeroSettings")]
    public partial class AddStorefrontHeroSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroAccentTitle",
                table: "Stores",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroDescription",
                table: "Stores",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroEyebrowText",
                table: "Stores",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "Stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroPrimaryButtonText",
                table: "Stores",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "Stores",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroAccentTitle",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HeroDescription",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HeroEyebrowText",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HeroPrimaryButtonText",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "Stores");
        }
    }
}
