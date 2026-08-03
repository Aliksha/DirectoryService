using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "positions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "positions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_positions_SoftDeleted",
                table: "positions",
                column: "SoftDeleted",
                filter: "\"SoftDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_locations_SoftDeleted",
                table: "locations",
                column: "SoftDeleted",
                filter: "\"SoftDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_departments_SoftDeleted",
                table: "departments",
                column: "SoftDeleted",
                filter: "\"SoftDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_positions_SoftDeleted",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_locations_SoftDeleted",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_departments_SoftDeleted",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "departments");
        }
    }
}
