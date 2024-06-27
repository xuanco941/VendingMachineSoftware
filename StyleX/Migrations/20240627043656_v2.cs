using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleX.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosterDesignUrl1",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "PosterDesignUrl2",
                table: "Product");

            migrationBuilder.AddColumn<int>(
                name: "NumberAvailable",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberAvailable",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "PosterDesignUrl1",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PosterDesignUrl2",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
