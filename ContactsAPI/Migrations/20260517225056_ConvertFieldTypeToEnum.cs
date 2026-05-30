using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactsAPI.Migrations
{
    public partial class ConvertFieldTypeToEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize FieldType values to PascalCase
            migrationBuilder.Sql(@"
                UPDATE ExtraFieldDefinitions
                SET FieldType = UPPER(LEFT(FieldType,1)) + LOWER(SUBSTRING(FieldType,2,LEN(FieldType)))
            ");

            // Update seeded admin password hash
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Rs9L2KSNdYQAnqywojQB4.F.zfcHU2hrA0oN4XMjma4kVBjxrbH2a");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert FieldType values back to lowercase
            migrationBuilder.Sql(@"
                UPDATE ExtraFieldDefinitions
                SET FieldType = LOWER(FieldType)
            ");

            // Revert seeded admin password hash
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$prL8DbPLX4xA2S6nqugKhOBckMGxN5.imWDm24.MZNH7ejj3cWL/q");
        }
    }
}
