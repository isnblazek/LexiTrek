using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiTrek.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename Word columns: Czech → Term, English → Definition
            migrationBuilder.RenameColumn(
                name: "Czech",
                table: "Words",
                newName: "Term");

            migrationBuilder.RenameColumn(
                name: "English",
                table: "Words",
                newName: "Definition");

            // 2. Create Dictionaries table
            migrationBuilder.CreateTable(
                name: "Dictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLanguage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetLanguage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dictionaries_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dictionaries_OwnerId",
                table: "Dictionaries",
                column: "OwnerId");

            // 3. Seed default dictionary
            migrationBuilder.Sql(@"
                INSERT INTO ""Dictionaries"" (""Id"", ""SourceLanguage"", ""TargetLanguage"", ""OwnerId"", ""Visibility"", ""CreatedAt"")
                VALUES ('00000000-0000-0000-0000-000000000001', 'Čeština', 'Angličtina', NULL, 1, NOW())
                ON CONFLICT DO NOTHING;
            ");

            // 4. Add DictionaryId as nullable first
            migrationBuilder.AddColumn<Guid>(
                name: "DictionaryId",
                table: "WordGroups",
                type: "uuid",
                nullable: true);

            // 5. Update all existing groups to use the default dictionary
            migrationBuilder.Sql(@"
                UPDATE ""WordGroups"" SET ""DictionaryId"" = '00000000-0000-0000-0000-000000000001';
            ");

            // 6. Make DictionaryId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "DictionaryId",
                table: "WordGroups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // 7. Add index and FK constraint
            migrationBuilder.CreateIndex(
                name: "IX_WordGroups_DictionaryId",
                table: "WordGroups",
                column: "DictionaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_WordGroups_Dictionaries_DictionaryId",
                table: "WordGroups",
                column: "DictionaryId",
                principalTable: "Dictionaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordGroups_Dictionaries_DictionaryId",
                table: "WordGroups");

            migrationBuilder.DropIndex(
                name: "IX_WordGroups_DictionaryId",
                table: "WordGroups");

            migrationBuilder.DropColumn(
                name: "DictionaryId",
                table: "WordGroups");

            migrationBuilder.DropTable(
                name: "Dictionaries");

            migrationBuilder.RenameColumn(
                name: "Term",
                table: "Words",
                newName: "Czech");

            migrationBuilder.RenameColumn(
                name: "Definition",
                table: "Words",
                newName: "English");
        }
    }
}
