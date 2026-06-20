using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceCity.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingActionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId1",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedArrivalTime",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BookingId1",
                table: "Notifications",
                column: "BookingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Bookings_BookingId1",
                table: "Notifications",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Bookings_BookingId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_BookingId1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BookingId1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EstimatedArrivalTime",
                table: "Bookings");
        }
    }
}
