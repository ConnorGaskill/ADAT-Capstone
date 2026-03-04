using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAT_Project.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_cardtypes_cards_card_id",
                table: "card_cardtypes");

            migrationBuilder.DropForeignKey(
                name: "FK_card_cardtypes_cardtypes_cardtype_id",
                table: "card_cardtypes");

            migrationBuilder.DropForeignKey(
                name: "FK_card_colors_cards_card_id",
                table: "card_colors");

            migrationBuilder.DropForeignKey(
                name: "FK_card_colors_colors_color_id",
                table: "card_colors");

            migrationBuilder.DropForeignKey(
                name: "FK_card_printings_cards_card_id",
                table: "card_printings");

            migrationBuilder.DropForeignKey(
                name: "FK_card_printings_sets_set_id",
                table: "card_printings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sets",
                table: "sets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_colors",
                table: "colors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cardtypes",
                table: "cardtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cards",
                table: "cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_card_printings",
                table: "card_printings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_card_colors",
                table: "card_colors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_card_cardtypes",
                table: "card_cardtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "audit_log",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "sysutcdatetime()");

            migrationBuilder.AddPrimaryKey(
                name: "PK__sets__14B092A38E8A0E17",
                table: "sets",
                column: "set_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__colors__1143CECB01BE8A19",
                table: "colors",
                column: "color_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__types__2C0005983128B7D4",
                table: "cardtypes",
                column: "cardtype_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__cards__BDF201DD93F2652B",
                table: "cards",
                column: "card_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__card_pri__15FFEFA27DBF2B4F",
                table: "card_printings",
                column: "printing_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__card_col__1CE63D317B235600",
                table: "card_colors",
                columns: new[] { "card_id", "color_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK__card_typ__2F320184F90159ED",
                table: "card_cardtypes",
                columns: new[] { "card_id", "cardtype_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK__audit_lo__5AF33E33520C6B02",
                table: "audit_log",
                column: "audit_id");

            migrationBuilder.AddForeignKey(
                name: "FK__card_type__card___59FA5E80",
                table: "card_cardtypes",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__card_type__type___5AEE82B9",
                table: "card_cardtypes",
                column: "cardtype_id",
                principalTable: "cardtypes",
                principalColumn: "cardtype_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__card_colo__card___5629CD9C",
                table: "card_colors",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__card_colo__color__571DF1D5",
                table: "card_colors",
                column: "color_id",
                principalTable: "colors",
                principalColumn: "color_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__card_prin__card___5DCAEF64",
                table: "card_printings",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__card_prin__set_i__5EBF139D",
                table: "card_printings",
                column: "set_id",
                principalTable: "sets",
                principalColumn: "set_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__card_type__card___59FA5E80",
                table: "card_cardtypes");

            migrationBuilder.DropForeignKey(
                name: "FK__card_type__type___5AEE82B9",
                table: "card_cardtypes");

            migrationBuilder.DropForeignKey(
                name: "FK__card_colo__card___5629CD9C",
                table: "card_colors");

            migrationBuilder.DropForeignKey(
                name: "FK__card_colo__color__571DF1D5",
                table: "card_colors");

            migrationBuilder.DropForeignKey(
                name: "FK__card_prin__card___5DCAEF64",
                table: "card_printings");

            migrationBuilder.DropForeignKey(
                name: "FK__card_prin__set_i__5EBF139D",
                table: "card_printings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__sets__14B092A38E8A0E17",
                table: "sets");

            migrationBuilder.DropPrimaryKey(
                name: "PK__colors__1143CECB01BE8A19",
                table: "colors");

            migrationBuilder.DropPrimaryKey(
                name: "PK__types__2C0005983128B7D4",
                table: "cardtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK__cards__BDF201DD93F2652B",
                table: "cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK__card_pri__15FFEFA27DBF2B4F",
                table: "card_printings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__card_col__1CE63D317B235600",
                table: "card_colors");

            migrationBuilder.DropPrimaryKey(
                name: "PK__card_typ__2F320184F90159ED",
                table: "card_cardtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK__audit_lo__5AF33E33520C6B02",
                table: "audit_log");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "audit_log",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "sysutcdatetime()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sets",
                table: "sets",
                column: "set_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_colors",
                table: "colors",
                column: "color_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cardtypes",
                table: "cardtypes",
                column: "cardtype_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cards",
                table: "cards",
                column: "card_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_card_printings",
                table: "card_printings",
                column: "printing_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_card_colors",
                table: "card_colors",
                columns: new[] { "card_id", "color_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_card_cardtypes",
                table: "card_cardtypes",
                columns: new[] { "card_id", "cardtype_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log",
                column: "audit_id");

            migrationBuilder.CreateIndex(
                name: "UQ__sets__357D4CF97C5356A3",
                table: "sets",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_sets_code",
                table: "sets",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_colors_name",
                table: "colors",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_types_name",
                table: "cardtypes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cards_name",
                table: "cards",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_card_cardtypes_cards_card_id",
                table: "card_cardtypes",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_cardtypes_cardtypes_cardtype_id",
                table: "card_cardtypes",
                column: "cardtype_id",
                principalTable: "cardtypes",
                principalColumn: "cardtype_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_colors_cards_card_id",
                table: "card_colors",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_colors_colors_color_id",
                table: "card_colors",
                column: "color_id",
                principalTable: "colors",
                principalColumn: "color_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_printings_cards_card_id",
                table: "card_printings",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_printings_sets_set_id",
                table: "card_printings",
                column: "set_id",
                principalTable: "sets",
                principalColumn: "set_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
