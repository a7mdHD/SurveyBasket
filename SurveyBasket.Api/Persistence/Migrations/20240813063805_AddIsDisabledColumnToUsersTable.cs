using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisabledColumnToUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "88c12d62-e773-4696-86e4-dfab144701fc",
                columns: new[] { "ConcurrencyStamp", "IsDisabled", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d612308e-3758-4ec2-b406-5436741bd1d3", false, "AQAAAAIAAYagAAAAEE8u2VNAAvv1uVMPtGDffz+cvvZlHyNhXXiLRiGgs7fdODVJIE9VJjNtYxyPRShvaA==", "897fb3db-822f-4ede-99d6-111ad99672a5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "88c12d62-e773-4696-86e4-dfab144701fc",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6e91fa85-6e3c-42c1-97bd-edc5d37c4670", "AQAAAAIAAYagAAAAEOcmr/Zyoj6Ei/CVDfYqaOQXdL6PDW2fbtVlv2/qO2/l0FpMtVYbQp9vIxx/MEft4A==", "ce4aec4e-39bb-4cd2-8ed0-47a8bcefdc66" });
        }
    }
}
