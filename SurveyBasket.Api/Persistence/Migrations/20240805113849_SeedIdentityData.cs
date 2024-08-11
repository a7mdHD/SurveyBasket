using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveyBasket.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11ff7b3c-ec6c-4436-bd99-8c091651ceef", "a0c0e575-dca8-4a6e-891f-f514b67afeab", true, false, "Member", "MEMBER" },
                    { "2797470a-198c-460b-aa33-9a156c72ab66", "22e2c018-8cd0-4ce6-b061-fde62b6486eb", false, false, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "88c12d62-e773-4696-86e4-dfab144701fc", 0, "6e91fa85-6e3c-42c1-97bd-edc5d37c4670", "admin@basket-survay.com", true, "Basket", "Survay", false, null, "ADMIN@BASKET-SURVAY.COM", "ADMIN@BASKET-SURVAY.COM", "AQAAAAIAAYagAAAAEOcmr/Zyoj6Ei/CVDfYqaOQXdL6PDW2fbtVlv2/qO2/l0FpMtVYbQp9vIxx/MEft4A==", null, false, "ce4aec4e-39bb-4cd2-8ed0-47a8bcefdc66", false, "admin@basket-survay.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permissions", "polls:read", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 2, "permissions", "polls:add", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 3, "permissions", "polls:update", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 4, "permissions", "polls:delete", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 5, "permissions", "questions:read", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 6, "permissions", "questions:add", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 7, "permissions", "questions:update", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 8, "permissions", "users:read", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 9, "permissions", "users:add", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 10, "permissions", "users:update", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 11, "permissions", "roles:read", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 12, "permissions", "roles:add", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 13, "permissions", "roles:update", "2797470a-198c-460b-aa33-9a156c72ab66" },
                    { 14, "permissions", "results:read", "2797470a-198c-460b-aa33-9a156c72ab66" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "2797470a-198c-460b-aa33-9a156c72ab66", "88c12d62-e773-4696-86e4-dfab144701fc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11ff7b3c-ec6c-4436-bd99-8c091651ceef");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2797470a-198c-460b-aa33-9a156c72ab66", "88c12d62-e773-4696-86e4-dfab144701fc" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2797470a-198c-460b-aa33-9a156c72ab66");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "88c12d62-e773-4696-86e4-dfab144701fc");
        }
    }
}
