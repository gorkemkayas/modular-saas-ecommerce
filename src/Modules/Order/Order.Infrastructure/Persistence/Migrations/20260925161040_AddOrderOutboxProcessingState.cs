using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderOutboxProcessingState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_CreatedAtUtc",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "OutboxMessages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OutboxMessages",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "OutboxMessages"
                SET
                    "AttemptCount" = 0,
                    "NextAttemptAtUtc" = "CreatedAtUtc",
                    "Status" = CASE
                        WHEN "PublishedAtUtc" IS NULL THEN 0
                        ELSE 2
                    END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "AttemptCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_LockedUntilUtc_CreatedAtUtc",
                table: "OutboxMessages",
                columns: new[] { "LockedUntilUtc", "CreatedAtUtc" },
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_NextAttemptAtUtc_CreatedAtUtc",
                table: "OutboxMessages",
                columns: new[] { "NextAttemptAtUtc", "CreatedAtUtc" },
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_LockedUntilUtc_CreatedAtUtc",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_NextAttemptAtUtc_CreatedAtUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_CreatedAtUtc",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "CreatedAtUtc" });
        }
    }
}
