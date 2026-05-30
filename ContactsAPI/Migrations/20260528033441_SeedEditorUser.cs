using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactsAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedEditorUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$6QR6oAAIBpD9Z.Z7xEXKk.pzK275JRyImXkvEA8.MaqjKKshKowyW");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedDate", "Email", "PasswordHash", "Role", "Status", "Username" },
                values: new object[] { 2, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), "editor@example.com", "$2a$11$6QR6oAAIBpD9Z.Z7xEXKk.pzK275JRyImXkvEA8.MaqjKKshKowyW", "Editor", "Active", "editor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$K0PFLg0DcErkpkJS6QGObuhOTvpjGDZl5TDlMQOqmIREMTnMvkBte");
        }
    }
}
