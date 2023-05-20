using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banking.Simulation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookConfigsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebhookConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookConfigs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookConfigs_OrganizationId",
                table: "WebhookConfigs",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookConfigs");
        }
    }
}
