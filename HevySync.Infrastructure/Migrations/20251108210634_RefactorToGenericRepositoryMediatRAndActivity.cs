using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HevySync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToGenericRepositoryMediatRAndActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyCategory",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "Exercises");

            migrationBuilder.AddColumn<decimal>(
                name: "RepsPerSetWeightProgression",
                table: "ExerciseProgressions",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_Status",
                table: "Activities",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropColumn(
                name: "RepsPerSetWeightProgression",
                table: "ExerciseProgressions");

            migrationBuilder.AddColumn<string>(
                name: "BodyCategory",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentType",
                table: "Exercises",
                type: "text",
                nullable: true);
        }
    }
}
