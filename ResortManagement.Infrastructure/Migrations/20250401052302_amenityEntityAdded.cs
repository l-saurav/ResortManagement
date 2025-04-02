using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResortManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class amenityEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    villaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Amenities_Villas_villaID",
                        column: x => x.villaID,
                        principalTable: "Villas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "ID", "Description", "Name", "villaID" },
                values: new object[,]
                {
                    { 1, "Large outdoor pool with temperature control.", "Swimming Pool", 1 },
                    { 2, "Fully equipped fitness center.", "Gym", 1 },
                    { 3, "Relaxing spa with professional masseurs.", "Spa", 1 },
                    { 4, "High-speed internet access.", "WiFi", 2 },
                    { 5, "Spacious balcony with a scenic view.", "Private Balcony", 2 },
                    { 6, "Stocked mini-bar with refreshments.", "Mini Bar", 2 },
                    { 7, "Luxury hot tub for relaxation.", "Jacuzzi", 3 },
                    { 8, "Latest 55-inch Smart TV with streaming services.", "Smart TV", 3 },
                    { 9, "4/7 room service for your convenience.", "Room Service", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_villaID",
                table: "Amenities",
                column: "villaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amenities");
        }
    }
}
