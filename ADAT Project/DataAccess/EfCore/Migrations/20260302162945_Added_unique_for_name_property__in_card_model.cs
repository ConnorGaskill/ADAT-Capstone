using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAT_Project.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_unique_for_name_property__in_card_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cards_name",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "IX_cards_name",
                table: "cards",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cards_name",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "IX_cards_name",
                table: "cards",
                column: "name");
        }
    }
}
