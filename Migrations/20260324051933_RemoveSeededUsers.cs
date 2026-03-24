using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeededUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Extraits"
                SET "UserId" = NULL
                WHERE "UserId" IN (
                    '00000000-0000-0000-0000-000000000001',
                    '00000000-0000-0000-0000-000000000002'
                );
            """);

            migrationBuilder.Sql("""
                UPDATE "Comments"
                SET "UserId" = NULL
                WHERE "UserId" IN (
                    '00000000-0000-0000-0000-000000000001',
                    '00000000-0000-0000-0000-000000000002'
                );
            """);

            // Option robuste: supprime tous les liens de rôles de ces users
            migrationBuilder.Sql("""
                DELETE FROM "AspNetUserRoles"
                WHERE "UserId" IN (
                    '00000000-0000-0000-0000-000000000001',
                    '00000000-0000-0000-0000-000000000002'
                );
            """);

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "00000000-0000-0000-0000-000000000001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "00000000-0000-0000-0000-000000000002" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000002");

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
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmationToken", "EmailConfirmationTokenExpiry", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "00000000-0000-0000-0000-000000000001", 0, "906e176a-40c2-46bc-9f1b-a3a9692e058f", "seed@example.invalid", null, null, true, true, null, "PABLO@ADMIN.COM", "PABLO", "REMOVED_PASSWORD_HASH", null, false, "83aea259-b2a7-4103-a4db-fdd6858a0d52", false, "pablo" },
                    { "00000000-0000-0000-0000-000000000002", 0, "c2d88127-a490-47ea-afa3-8ae5b14e19dd", "seed@example.invalid", null, null, true, true, null, "SAM@ADMIN.COM", "SAM", "REMOVED_PASSWORD_HASH", null, false, "6094047e-a62b-426c-8fb9-621dbd53a409", false, "sam" }
                });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "CommentsId",
                value: new List<int> { 1 });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "1", "00000000-0000-0000-0000-000000000001" },
                    { "1", "00000000-0000-0000-0000-000000000002" }
                });
        }
    }
}
