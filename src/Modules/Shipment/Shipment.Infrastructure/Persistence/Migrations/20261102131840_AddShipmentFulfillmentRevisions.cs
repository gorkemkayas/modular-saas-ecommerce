using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shipment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentFulfillmentRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_StoreId_OrderId",
                table: "Shipments");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Shipments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "ShipmentFulfillmentSequences",
                columns: table => new
                {
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastRevision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentFulfillmentSequences", x => new { x.StoreId, x.OrderId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_StoreId_OrderId",
                table: "Shipments",
                columns: new[] { "StoreId", "OrderId" },
                unique: true,
                filter: "\"Status\" <> 5");

            // Establish one baseline snapshot per order without resending notification events.
            // Run with old Shipment writers stopped: subsequent revisions continue from this counter.
            migrationBuilder.Sql("""
                WITH latest AS (
                    SELECT DISTINCT ON ("StoreId", "OrderId") *
                    FROM "Shipments"
                    ORDER BY "StoreId", "OrderId", ("Status" <> 5) DESC,
                             "UpdatedAtUtc" DESC, "CreatedAtUtc" DESC, "Id"
                )
                INSERT INTO "ShipmentFulfillmentSequences" ("StoreId", "OrderId", "LastRevision")
                SELECT "StoreId", "OrderId", 1 FROM latest;

                WITH latest AS (
                    SELECT DISTINCT ON ("StoreId", "OrderId") *
                    FROM "Shipments"
                    ORDER BY "StoreId", "OrderId", ("Status" <> 5) DESC,
                             "UpdatedAtUtc" DESC, "CreatedAtUtc" DESC, "Id"
                ), snapshots AS (
                    SELECT *, gen_random_uuid() AS "EventId" FROM latest
                )
                INSERT INTO "OutboxMessages"
                    ("Id", "StoreId", "EventType", "RoutingKey", "Payload", "OccurredOnUtc",
                     "CreatedAtUtc", "Status", "AttemptCount", "NextAttemptAtUtc")
                SELECT "EventId", "StoreId", 'shipment.fulfillment-changed.v1',
                    'shipment.fulfillment-changed.v1',
                    jsonb_build_object('id', "EventId", 'occurredOnUtc', "UpdatedAtUtc",
                        'storeId', "StoreId", 'orderId', "OrderId", 'shipmentId', "Id",
                        'shipmentNumber', "ShipmentNumber", 'status', "Status", 'fulfillmentRevision', 1),
                    "UpdatedAtUtc", NOW(), 0, 0, NOW()
                FROM snapshots;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentFulfillmentSequences");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_StoreId_OrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_StoreId_OrderId",
                table: "Shipments",
                columns: new[] { "StoreId", "OrderId" });
        }
    }
}
