using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowApplicationApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeOfConductDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeOfConductDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    OriginalFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HtmlContent = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeOfConductDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeOfConductDocuments_AspNetUsers_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeOfConductDocuments_IsActive",
                table: "CodeOfConductDocuments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CodeOfConductDocuments_UploadedBy",
                table: "CodeOfConductDocuments",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CodeOfConductDocuments_Version",
                table: "CodeOfConductDocuments",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeOfConductDocuments");
        }
    }
}
