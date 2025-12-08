using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowApplicationApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeOfConductFieldsToFlowMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeOfConductPdfPath",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeOfConductUploadedAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasUploadedCodeOfConduct",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeOfConductPdfPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CodeOfConductUploadedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HasUploadedCodeOfConduct",
                table: "AspNetUsers");
        }
    }
}
