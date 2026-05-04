using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrustGuard.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainTrustRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainTrustRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TrustScore = table.Column<int>(type: "integer", nullable: false),
                    FactorsJson = table.Column<string>(type: "text", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainTrustRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainTrustRecords_DomainName",
                table: "DomainTrustRecords",
                column: "DomainName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainTrustRecords");
        }
    }
}
