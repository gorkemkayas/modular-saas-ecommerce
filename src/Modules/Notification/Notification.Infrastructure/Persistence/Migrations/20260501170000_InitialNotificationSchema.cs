using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(NotificationDbContext))]
    [Migration("20260501170000_InitialNotificationSchema")]
    public partial class InitialNotificationSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RecipientAddress = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    BusinessEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BusinessEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SuppressionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastProviderEventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastProviderEventAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClickedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BouncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComplainedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationDispatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderRequestReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationAttempts_NotificationDispatches_NotificationDispatchId",
                        column: x => x.NotificationDispatchId,
                        principalTable: "NotificationDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttempts_NotificationDispatchId_AttemptNumber",
                table: "NotificationAttempts",
                columns: new[] { "NotificationDispatchId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttempts_NotificationDispatchId_AttemptedAtUtc",
                table: "NotificationAttempts",
                columns: new[] { "NotificationDispatchId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatches_ProviderName_ProviderMessageId",
                table: "NotificationDispatches",
                columns: new[] { "ProviderName", "ProviderMessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatches_StoreId_Status_CreatedAtUtc",
                table: "NotificationDispatches",
                columns: new[] { "StoreId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatches_StoreId_Trigger_Channel_BusinessEntityType_BusinessEntityId",
                table: "NotificationDispatches",
                columns: new[] { "StoreId", "Trigger", "Channel", "BusinessEntityType", "BusinessEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_StoreId_IsActive_UpdatedAtUtc",
                table: "NotificationTemplates",
                columns: new[] { "StoreId", "IsActive", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_StoreId_Trigger_Channel_Locale",
                table: "NotificationTemplates",
                columns: new[] { "StoreId", "Trigger", "Channel", "Locale" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "NotificationAttempts");
            migrationBuilder.DropTable(name: "NotificationTemplates");
            migrationBuilder.DropTable(name: "NotificationDispatches");
        }
    }
}
