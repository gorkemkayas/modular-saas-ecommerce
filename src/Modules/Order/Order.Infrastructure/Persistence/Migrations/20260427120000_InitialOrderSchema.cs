using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Persistence.Migrations
{
    public partial class InitialOrderSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PlacedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReservationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShipmentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SnapshotCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CustomerFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BillingTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BillingContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BillingPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BillingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BillingDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BillingLine1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BillingLine2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ShippingTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShippingContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShippingPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingLine1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ShippingLine2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LineSubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UnitPriceCompareAtAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UnitPricePriceListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPricePriceEntryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId_CustomerId_CreatedAtUtc",
                table: "Orders",
                columns: new[] { "StoreId", "CustomerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId_Status_CreatedAtUtc",
                table: "Orders",
                columns: new[] { "StoreId", "Status", "CreatedAtUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
