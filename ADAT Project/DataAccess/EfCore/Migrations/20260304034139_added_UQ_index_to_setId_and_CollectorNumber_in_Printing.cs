using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAT_Project.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class added_UQ_index_to_setId_and_CollectorNumber_in_Printing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_card_printings_set_id",
                table: "card_printings");

            migrationBuilder.CreateIndex(
                name: "IX_card_printings_set_id_collector_number",
                table: "card_printings",
                columns: new[] { "set_id", "collector_number" },
                unique: true,
                filter: "[collector_number] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_card_printings_set_id_collector_number",
                table: "card_printings");

            migrationBuilder.CreateIndex(
                name: "IX_card_printings_set_id",
                table: "card_printings",
                column: "set_id");
        }
    }
}
