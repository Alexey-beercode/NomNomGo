using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NomNomGo.IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDat2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("87b47ce2-203c-41ba-9509-bc9152ddb45b"), "ManageOrders" },
                    { new Guid("9fca3c46-dd32-471c-9a89-a4e7a7bd4fe5"), "ViewReports" },
                    { new Guid("b6721810-4e43-4845-82db-87bda55b88db"), "ManageRestaurants" },
                    { new Guid("e5cac956-e00c-4a35-b5ed-35598ae4143b"), "ManageUsers" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("d9d73d27-2c3b-4543-b201-5c2be0673b1d"), "User" },
                    { new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BlockedUntil", "CreatedAt", "Email", "PasswordHash", "PhoneNumber", "UpdatedAt", "Username" },
                values: new object[] { new Guid("2dce8418-d65d-43e0-84cc-84016bf9a6d0"), null, new DateTime(2025, 6, 1, 11, 53, 48, 722, DateTimeKind.Utc).AddTicks(5751), "admin@nomnom.local", "$2b$10$sSK80WLv.MyWGwaZl8PRR.LwJAg4dbjM0sZ5DKEd0spkX340fSuWm", "+375447777777", new DateTime(2025, 6, 1, 11, 53, 48, 722, DateTimeKind.Utc).AddTicks(5766), "admin" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("87b47ce2-203c-41ba-9509-bc9152ddb45b"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") },
                    { new Guid("9fca3c46-dd32-471c-9a89-a4e7a7bd4fe5"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") },
                    { new Guid("b6721810-4e43-4845-82db-87bda55b88db"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") },
                    { new Guid("e5cac956-e00c-4a35-b5ed-35598ae4143b"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "Id" },
                values: new object[] { new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041"), new Guid("2dce8418-d65d-43e0-84cc-84016bf9a6d0"), new Guid("fa63045d-a390-4e17-8253-20502eb8657c") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("87b47ce2-203c-41ba-9509-bc9152ddb45b"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9fca3c46-dd32-471c-9a89-a4e7a7bd4fe5"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b6721810-4e43-4845-82db-87bda55b88db"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e5cac956-e00c-4a35-b5ed-35598ae4143b"), new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d9d73d27-2c3b-4543-b201-5c2be0673b1d"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041"), new Guid("2dce8418-d65d-43e0-84cc-84016bf9a6d0") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("87b47ce2-203c-41ba-9509-bc9152ddb45b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9fca3c46-dd32-471c-9a89-a4e7a7bd4fe5"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6721810-4e43-4845-82db-87bda55b88db"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5cac956-e00c-4a35-b5ed-35598ae4143b"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("da96c1cc-4ffa-4434-a85c-cb822bc8a041"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2dce8418-d65d-43e0-84cc-84016bf9a6d0"));
        }
    }
}
