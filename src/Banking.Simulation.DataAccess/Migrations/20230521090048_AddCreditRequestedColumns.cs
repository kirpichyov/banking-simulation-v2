using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banking.Simulation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditRequestedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "RequestedCreditPercent",
                table: "Payments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedCreditPricePerMonth",
                table: "Payments",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedCreditPercent",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RequestedCreditPricePerMonth",
                table: "Payments");
        }
    }
}
