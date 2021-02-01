using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UserCollections.Migrations
{
    public partial class Migration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Items_ItemId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ItemId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WordFields");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TextFields");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DigitFields");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DateFields");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CheckboxFields");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Comments",
                newName: "User");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Collections",
                newName: "CollectionName");

            migrationBuilder.AddColumn<List<string>>(
                name: "Likes",
                table: "Items",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "User",
                table: "Comments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CollectionName",
                table: "Collections",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WordFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TextFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DigitFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DateFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CheckboxFields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ItemId",
                table: "AspNetUsers",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Items_ItemId",
                table: "AspNetUsers",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
