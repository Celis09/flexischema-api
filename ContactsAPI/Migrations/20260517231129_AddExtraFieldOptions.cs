using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraFieldOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtraFieldOptions",
                columns: table => new
                {
                    ExtraFieldOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtraFieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraFieldOptions", x => x.ExtraFieldOptionId);
                    table.ForeignKey(
                        name: "FK_ExtraFieldOptions_ExtraFieldDefinitions_ExtraFieldDefinitionId",
                        column: x => x.ExtraFieldDefinitionId,
                        principalTable: "ExtraFieldDefinitions",
                        principalColumn: "ExtraFieldDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$axTR4GIap2fi3UM/9yzste0qmRzU3/9DUfMEfP5bJnHfkr7nm9.Ru");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraFieldOptions_ExtraFieldDefinitionId",
                table: "ExtraFieldOptions",
                column: "ExtraFieldDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtraFieldOptions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Rs9L2KSNdYQAnqywojQB4.F.zfcHU2hrA0oN4XMjma4kVBjxrbH2a");
        }
    }
}
