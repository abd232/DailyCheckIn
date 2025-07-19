using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyCheckIn.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CheckIn",
                table: "Attendances",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CheckIn",
                table: "Attendances",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");
        }
    }
}
