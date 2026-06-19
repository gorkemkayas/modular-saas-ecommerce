using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReferenceCodeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalSubscriptionReferenceCode",
                table: "TenantSubscriptions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPricingPlanReferenceCode",
                table: "Plans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalSubscriptionReferenceCode",
                table: "TenantSubscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalPricingPlanReferenceCode",
                table: "Plans");
        }
    }
}
