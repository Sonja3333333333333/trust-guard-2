using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrustGuard.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyTriggersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "NewsChecks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KeyTriggers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Word = table.Column<string>(type: "text", nullable: false),
                    NewsCheckId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyTriggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyTriggers_NewsChecks_NewsCheckId",
                        column: x => x.NewsCheckId,
                        principalTable: "NewsChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyTriggers_NewsCheckId",
                table: "KeyTriggers",
                column: "NewsCheckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyTriggers");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "NewsChecks");
        }
    }
}
