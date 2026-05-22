using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "ServiceTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ServiceTypes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "ServiceRequests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ServiceRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ServiceRequests");
        }
    }
}
