using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResortManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedVilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "ID", "CreatedDate", "Description", "ImageURL", "Name", "Occupancy", "Price", "Sqft", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, "The Royal Villa features a new kitchen, new bathrooms (5.5), flat screen TVs, interior and exterior paint, lanai furniture, and all linens. Relax like royalty in our sprawling villa that include FOUR KING MASTER bedrooms all with ENSUITE BATHROOMS and flat screen TVs.", "https://placehold.co/600x400", "Royal Villa", 4, 200.0, 550, null },
                    { 2, null, "Offering sea views, Poolvilla Gaon in Busan provides accommodations, a rooftop pool, an indoor pool, an open-air bath, and a terrace. Built in 2023, the property includes hot spring bath and hot tub. The villa also offers free Wifi, free private parking, and facilities for disabled guests.", "https://placehold.co/600x401", "Premium Pool Villa", 4, 300.0, 650, null },
                    { 3, null, "At the villa complex, each unit comes with air conditioning, a seating area, a flat-screen TV with streaming services, a kitchen, a dining area, and a private bathroom with slippers, a shower, and a hair dryer. A microwave, a fridge, and kitchenware are also featured, as well as a coffee machine and a kettle. At the villa complex, all units are fitted with bed linen and towels.", "https://placehold.co/600x402", "Luxury Pool Villa", 4, 400.0, 750, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "ID",
                keyValue: 3);
        }
    }
}
