using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HevySync.Migrations
{
    /// <inheritdoc />
    public partial class ExerciseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Exercise");
        }
    }
}
