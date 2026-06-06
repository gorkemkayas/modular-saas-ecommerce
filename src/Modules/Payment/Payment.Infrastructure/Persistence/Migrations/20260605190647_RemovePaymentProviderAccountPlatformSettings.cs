using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentProviderAccountPlatformSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "PaymentProviderAccounts");

            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "PaymentProviderAccounts");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "PaymentProviderAccounts");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "PaymentProviderAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "PaymentProviderAccounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "PaymentProviderAccounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Environment",
                table: "PaymentProviderAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "PaymentProviderAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
