using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(NotificationDbContext))]
    [Migration("20260609230000_AddContactFeedbacks")]
    public partial class AddContactFeedbacks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactFeedbacks_CreatedAtUtc",
                table: "ContactFeedbacks",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContactFeedbacks_Email_CreatedAtUtc",
                table: "ContactFeedbacks",
                columns: new[] { "Email", "CreatedAtUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ContactFeedbacks");
        }
    }
}
