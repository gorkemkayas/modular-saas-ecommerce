using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShipmentFulfillmentInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastFulfillmentRevision",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "LastFulfillmentShipmentId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastShipmentFulfillmentState",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Orders",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_LockedUntilUtc_ReceivedOnUtc",
                table: "InboxMessages",
                columns: new[] { "LockedUntilUtc", "ReceivedOnUtc" },
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_NextAttemptAtUtc_ReceivedOnUtc",
                table: "InboxMessages",
                columns: new[] { "NextAttemptAtUtc", "ReceivedOnUtc" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_ProcessedOnUtc",
                table: "InboxMessages",
                column: "ProcessedOnUtc",
                filter: "\"Status\" = 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "LastFulfillmentRevision",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LastFulfillmentShipmentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LastShipmentFulfillmentState",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Orders");
        }
    }
}
