using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HevySync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyExercisePlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyExercisePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlannedSets = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyExercisePlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyExercisePlans_ExerciseId_Week",
                table: "WeeklyExercisePlans",
                columns: new[] { "ExerciseId", "Week" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyExercisePlans_WorkoutId_Week",
                table: "WeeklyExercisePlans",
                columns: new[] { "WorkoutId", "Week" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyExercisePlans");
        }
    }
}
