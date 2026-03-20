using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiTrek.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLanguageToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map existing string values to enum integers before type conversion
            migrationBuilder.Sql("""
                UPDATE "Dictionaries" SET "SourceLanguage" = CASE "SourceLanguage"
                    WHEN 'Čeština' THEN '0'
                    WHEN 'Angličtina' THEN '1'
                    WHEN 'Němčina' THEN '2'
                    WHEN 'Francouzština' THEN '3'
                    WHEN 'Španělština' THEN '4'
                    WHEN 'Italština' THEN '5'
                    WHEN 'Polština' THEN '6'
                    WHEN 'Slovenština' THEN '7'
                    WHEN 'Ruština' THEN '8'
                    WHEN 'Portugalština' THEN '9'
                    WHEN 'Holandština' THEN '10'
                    WHEN 'Ukrajinština' THEN '11'
                    ELSE '0'
                END;

                UPDATE "Dictionaries" SET "TargetLanguage" = CASE "TargetLanguage"
                    WHEN 'Čeština' THEN '0'
                    WHEN 'Angličtina' THEN '1'
                    WHEN 'Němčina' THEN '2'
                    WHEN 'Francouzština' THEN '3'
                    WHEN 'Španělština' THEN '4'
                    WHEN 'Italština' THEN '5'
                    WHEN 'Polština' THEN '6'
                    WHEN 'Slovenština' THEN '7'
                    WHEN 'Ruština' THEN '8'
                    WHEN 'Portugalština' THEN '9'
                    WHEN 'Holandština' THEN '10'
                    WHEN 'Ukrajinština' THEN '11'
                    ELSE '0'
                END;
                """);

            // Convert column type from varchar to integer
            migrationBuilder.Sql("""
                ALTER TABLE "Dictionaries" ALTER COLUMN "SourceLanguage" TYPE integer USING "SourceLanguage"::integer;
                ALTER TABLE "Dictionaries" ALTER COLUMN "TargetLanguage" TYPE integer USING "TargetLanguage"::integer;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back to varchar
            migrationBuilder.Sql("""
                ALTER TABLE "Dictionaries" ALTER COLUMN "SourceLanguage" TYPE character varying(100) USING "SourceLanguage"::text;
                ALTER TABLE "Dictionaries" ALTER COLUMN "TargetLanguage" TYPE character varying(100) USING "TargetLanguage"::text;
                """);

            // Map enum integers back to string names
            migrationBuilder.Sql("""
                UPDATE "Dictionaries" SET "SourceLanguage" = CASE "SourceLanguage"
                    WHEN '0' THEN 'Čeština'
                    WHEN '1' THEN 'Angličtina'
                    WHEN '2' THEN 'Němčina'
                    WHEN '3' THEN 'Francouzština'
                    WHEN '4' THEN 'Španělština'
                    WHEN '5' THEN 'Italština'
                    WHEN '6' THEN 'Polština'
                    WHEN '7' THEN 'Slovenština'
                    WHEN '8' THEN 'Ruština'
                    WHEN '9' THEN 'Portugalština'
                    WHEN '10' THEN 'Holandština'
                    WHEN '11' THEN 'Ukrajinština'
                    ELSE 'Čeština'
                END;

                UPDATE "Dictionaries" SET "TargetLanguage" = CASE "TargetLanguage"
                    WHEN '0' THEN 'Čeština'
                    WHEN '1' THEN 'Angličtina'
                    WHEN '2' THEN 'Němčina'
                    WHEN '3' THEN 'Francouzština'
                    WHEN '4' THEN 'Španělština'
                    WHEN '5' THEN 'Italština'
                    WHEN '6' THEN 'Polština'
                    WHEN '7' THEN 'Slovenština'
                    WHEN '8' THEN 'Ruština'
                    WHEN '9' THEN 'Portugalština'
                    WHEN '10' THEN 'Holandština'
                    WHEN '11' THEN 'Ukrajinština'
                    ELSE 'Angličtina'
                END;
                """);
        }
    }
}
