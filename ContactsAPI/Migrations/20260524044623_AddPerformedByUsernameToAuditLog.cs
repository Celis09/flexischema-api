using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformedByUsernameToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerformedByUsername",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Anonymous");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$K0PFLg0DcErkpkJS6QGObuhOTvpjGDZl5TDlMQOqmIREMTnMvkBte");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformedByUsername",
                table: "AuditLogs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$axTR4GIap2fi3UM/9yzste0qmRzU3/9DUfMEfP5bJnHfkr7nm9.Ru");
        }
    }
}
