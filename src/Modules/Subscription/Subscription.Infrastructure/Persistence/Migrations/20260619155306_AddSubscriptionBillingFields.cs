using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentPeriodEndUtc",
                table: "TenantSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentPeriodStartUtc",
                table: "TenantSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPaymentToken",
                table: "TenantSubscriptions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Plans",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPriceAmount",
                table: "Plans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPeriodEndUtc",
                table: "TenantSubscriptions");

            migrationBuilder.DropColumn(
                name: "CurrentPeriodStartUtc",
                table: "TenantSubscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalPaymentToken",
                table: "TenantSubscriptions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "MonthlyPriceAmount",
                table: "Plans");
        }
    }
}
