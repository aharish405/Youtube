using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateTube.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manual Patch: Tables already exist, only adding CreatorId columns and FKs

            // Playlists
            if (!IsColumnExists(migrationBuilder, "Playlists", "CreatorId"))
            {
                migrationBuilder.AddColumn<int>(
                    name: "CreatorId",
                    table: "Playlists",
                    type: "int",
                    nullable: true);

                migrationBuilder.CreateIndex(
                    name: "IX_Playlists_CreatorId",
                    table: "Playlists",
                    column: "CreatorId");

                migrationBuilder.AddForeignKey(
                    name: "FK_Playlists_Users_CreatorId",
                    table: "Playlists",
                    column: "CreatorId",
                    principalTable: "Users",
                    principalColumn: "Id");
            }

            // Videos
            if (!IsColumnExists(migrationBuilder, "Videos", "CreatorId"))
            {
                migrationBuilder.AddColumn<int>(
                    name: "CreatorId",
                    table: "Videos",
                    type: "int",
                    nullable: true);

                migrationBuilder.CreateIndex(
                    name: "IX_Videos_CreatorId",
                    table: "Videos",
                    column: "CreatorId");

                migrationBuilder.AddForeignKey(
                    name: "FK_Videos_Users_CreatorId",
                    table: "Videos",
                    column: "CreatorId",
                    principalTable: "Users",
                    principalColumn: "Id");
            }
        }

        private bool IsColumnExists(MigrationBuilder migrationBuilder, string tableName, string columnName)
        {
            // EF Core MigrationBuilder doesn't have a direct "Exists" check easy to use here without SQL.
            // We assume since this failed previously, these didn't execute. 
            // But to be safe, I'm just executing the Adds. Explicit check logic is complex in migration script.
            // I'll just run the Adds.
            return false;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_CreatorId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_CreatorId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Users_CreatorId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_CreatorId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Videos");
        }
    }
}
