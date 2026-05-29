using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wpf_projekt.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransferGroupId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SharedAccounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PersonalAccounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferGroupId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SharedAccounts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PersonalAccounts");
        }
    }
}
