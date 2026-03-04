using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAT_Project.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_UQ_for_properties_in_cards_cardtypes_colors_sets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_sets_code",
                table: "sets",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sets_name",
                table: "sets",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colors_name",
                table: "colors",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cardtypes_name",
                table: "cardtypes",
                column: "name",
                unique: true);

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
                name: "IX_sets_code",
                table: "sets");

            migrationBuilder.DropIndex(
                name: "IX_sets_name",
                table: "sets");

            migrationBuilder.DropIndex(
                name: "IX_colors_name",
                table: "colors");

            migrationBuilder.DropIndex(
                name: "IX_cardtypes_name",
                table: "cardtypes");

            migrationBuilder.DropIndex(
                name: "IX_cards_name",
                table: "cards");
        }
    }
}
