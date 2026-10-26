using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDispatchDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationDispatchDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PreparedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDispatchDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDispatchDeliveries_NotificationDispatches_Id",
                        column: x => x.Id,
                        principalTable: "NotificationDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchDeliveries_Status_LockedUntilUtc",
                table: "NotificationDispatchDeliveries",
                columns: new[] { "Status", "LockedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchDeliveries_Status_NextAttemptAtUtc_Crea~",
                table: "NotificationDispatchDeliveries",
                columns: new[] { "Status", "NextAttemptAtUtc", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationDispatchDeliveries");
        }
    }
}
