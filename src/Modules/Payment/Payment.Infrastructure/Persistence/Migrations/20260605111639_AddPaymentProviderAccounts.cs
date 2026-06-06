using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentProviderAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProviderAccountId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentProviderAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKeyCipherText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SecretKeyCipherText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ApiKeyLastFour = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CallbackUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DefaultBuyerIdentityNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviderAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderAccountId",
                table: "Payments",
                column: "ProviderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviderAccounts_StoreId_Provider",
                table: "PaymentProviderAccounts",
                columns: new[] { "StoreId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviderAccounts_StoreId_Status",
                table: "PaymentProviderAccounts",
                columns: new[] { "StoreId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentProviderAccounts_ProviderAccountId",
                table: "Payments",
                column: "ProviderAccountId",
                principalTable: "PaymentProviderAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentProviderAccounts_ProviderAccountId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentProviderAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ProviderAccountId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderAccountId",
                table: "Payments");
        }
    }
}
