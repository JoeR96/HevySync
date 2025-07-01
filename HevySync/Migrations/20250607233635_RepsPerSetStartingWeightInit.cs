using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HevySync.Migrations
{
    /// <inheritdoc />
    public partial class RepsPerSetStartingWeightInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StartingWeight",
                table: "ExerciseDetail",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingWeight",
                table: "ExerciseDetail");
        }
    }
}
