using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentProviderAttemptFence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasProviderAttempt",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            // Historical Pending rows may represent an interrupted provider call. Only newly
            // created payments are provably untouched; legacy uncertainty needs reconciliation.
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"HasProviderAttempt\" = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasProviderAttempt",
                table: "Payments");
        }
    }
}
