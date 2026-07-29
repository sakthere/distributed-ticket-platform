using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketImpactAndUrgency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Impact",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Urgency",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Impact",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "Tickets");
        }
    }
}
