using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HevySync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleGroupsToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryMuscleGroup",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int[]>(
                name: "SecondaryMuscleGroups",
                table: "Exercises",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryMuscleGroup",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMuscleGroups",
                table: "Exercises");
        }
    }
}
