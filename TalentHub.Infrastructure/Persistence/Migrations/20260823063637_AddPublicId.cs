using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumePublicId",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "CompanyImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePublicId",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPublicId",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconPublicId",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumePublicId",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "CompanyImages");

            migrationBuilder.DropColumn(
                name: "CoverImagePublicId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoPublicId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IconPublicId",
                table: "Categories");
        }
    }
}
