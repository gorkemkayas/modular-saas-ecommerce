using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(OrderDbContext))]
    [Migration("20260523110500_AddOrderShippingCarrierSnapshot")]
    public partial class AddOrderShippingCarrierSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShippingCarrierId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierCode",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierName",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierServiceCode",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierServiceName",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierTrackingUrl",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCarrierId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrierCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrierName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrierServiceCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrierServiceName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrierTrackingUrl",
                table: "Orders");
        }
    }
}
