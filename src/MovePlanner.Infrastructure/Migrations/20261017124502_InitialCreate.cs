using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The initial migration only creates the migrations history table.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // There is no schema to roll back yet.
        }
    }
}
