using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class email : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpiry",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationToken", "EmailConfirmationTokenExpiry", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9f4145b1-fad1-4d42-bcae-831c87dcdb2d", null, null, "REMOVED_PASSWORD_HASH", "2febe562-450a-4530-89fa-d8f93a40b8e5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000002",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationToken", "EmailConfirmationTokenExpiry", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2bf4c651-1bdd-4bbe-ae5e-e59c7065a432", null, null, "REMOVED_PASSWORD_HASH", "6b6b8161-4d18-4a81-ab07-c08b43fc986c" });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "CommentsId",
                value: new List<int> { 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpiry",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "178f74e9-f341-4f36-9fa0-10d6c926eaca", "REMOVED_PASSWORD_HASH", "d832b542-274a-4ec0-9ad8-0f35f1dd5a46" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0f94d27d-d0ff-4a00-9775-44bc877eb49d", "REMOVED_PASSWORD_HASH", "96da619f-7995-4492-a629-969658a0c686" });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "CommentsId",
                value: new List<int> { 1 });
        }
    }
}
