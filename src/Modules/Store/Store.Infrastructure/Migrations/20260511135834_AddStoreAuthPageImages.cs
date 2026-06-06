using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreAuthPageImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginPageImageUrl",
                table: "Stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterPageImageUrl",
                table: "Stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginPageImageUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "RegisterPageImageUrl",
                table: "Stores");
        }
    }
}
