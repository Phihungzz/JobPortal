using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.Data.Migrations
{
    public partial class AddCVFilePathToCV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVFilePath",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("376c1d1e-0b04-47da-9657-a2a87faf0a59"),
                column: "ConcurrencyStamp",
                value: "0e647a95-4b05-4020-982f-3c39480c0801");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("4e233be7-c199-4567-9c07-9271a9de4c64"),
                column: "ConcurrencyStamp",
                value: "bb23451c-ab4b-48da-b0bc-2a7345da0fed");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("9f685d0f-bd6f-44dd-ab60-c606952eb2a8"),
                column: "ConcurrencyStamp",
                value: "7ebb1089-e6a7-495c-8fb8-bb1f78675ecf");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("769f41bd-ccd4-45ba-abbd-550ccd0b62e3"),
                columns: new[] { "ConcurrencyStamp", "CreateDate", "PasswordHash" },
                values: new object[] { "6062f17a-4e1a-4c11-93ad-66bd903498bf", new DateTime(2025, 4, 29, 23, 3, 21, 644, DateTimeKind.Local).AddTicks(779), "AQAAAAEAACcQAAAAEP+RwLirfiEK4r5s3bxm3Inwo+uYiFMxzRiAXn3Dhx6hpH/cW6iYO1n3taZlcmNmDQ==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVFilePath",
                table: "CVs");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("376c1d1e-0b04-47da-9657-a2a87faf0a59"),
                column: "ConcurrencyStamp",
                value: "27272abf-5e73-4950-b2bb-c894a526db80");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("4e233be7-c199-4567-9c07-9271a9de4c64"),
                column: "ConcurrencyStamp",
                value: "d6a109b0-da6b-4858-8322-384cff41c0c2");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("9f685d0f-bd6f-44dd-ab60-c606952eb2a8"),
                column: "ConcurrencyStamp",
                value: "6a52897b-bb6b-4202-8942-12874d68130e");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("769f41bd-ccd4-45ba-abbd-550ccd0b62e3"),
                columns: new[] { "ConcurrencyStamp", "CreateDate", "PasswordHash" },
                values: new object[] { "2d855d61-f1da-495b-beb2-5e2d9f3ccecb", new DateTime(2025, 4, 29, 15, 47, 16, 168, DateTimeKind.Local).AddTicks(6144), "AQAAAAEAACcQAAAAEBKYNcrgQLvf/YdGw5pqVgZVBRwhxLJ0SZZ8jfNEG3bC/bHQ14TyU7QoXnivv62hdg==" });
        }
    }
}
