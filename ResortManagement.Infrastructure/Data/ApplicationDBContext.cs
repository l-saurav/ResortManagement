using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Infrastructure.Data
{
    public class ApplicationDBContext: IdentityDbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        { 
        }
        public DbSet<Villa> Villas { get; set; }
        public DbSet<VillaNumber> VillaNumbers { get; set; }
        public DbSet<Amenity> Amenities { get; set; }

        //Seed Villas table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Villa>().HasData(
                new Villa
                {
                    ID = 1,
                    Name = "Royal Villa",
                    Description = "The Royal Villa features a new kitchen, new bathrooms (5.5), flat screen TVs, interior and exterior paint, lanai furniture, and all linens. Relax like royalty in our sprawling villa that include FOUR KING MASTER bedrooms all with ENSUITE BATHROOMS and flat screen TVs.",
                    ImageURL = "https://placehold.co/600x400",
                    Occupancy = 4,
                    Price = 200,
                    Sqft = 550
                },
                new Villa
                {
                    ID = 2,
                    Name = "Premium Pool Villa",
                    Description = "Offering sea views, Poolvilla Gaon in Busan provides accommodations, a rooftop pool, an indoor pool, an open-air bath, and a terrace. Built in 2023, the property includes hot spring bath and hot tub. The villa also offers free Wifi, free private parking, and facilities for disabled guests.",
                    ImageURL = "https://placehold.co/600x401",
                    Occupancy = 4,
                    Price = 300,
                    Sqft = 650
                },
                new Villa
                {
                    ID = 3,
                    Name = "Luxury Pool Villa",
                    Description = "At the villa complex, each unit comes with air conditioning, a seating area, a flat-screen TV with streaming services, a kitchen, a dining area, and a private bathroom with slippers, a shower, and a hair dryer. A microwave, a fridge, and kitchenware are also featured, as well as a coffee machine and a kettle. At the villa complex, all units are fitted with bed linen and towels.",
                    ImageURL = "https://placehold.co/600x402",
                    Occupancy = 4,
                    Price = 400,
                    Sqft = 750
                }
            );

            modelBuilder.Entity<VillaNumber>().HasData(
                new VillaNumber
                {
                    Villa_Number = 101,
                    VillaID = 1
                },
                new VillaNumber
                {
                    Villa_Number = 102,
                    VillaID = 1
                },
                new VillaNumber
                {
                    Villa_Number = 103,
                    VillaID = 1
                },
                new VillaNumber
                {
                    Villa_Number = 201,
                    VillaID = 2
                },
                new VillaNumber
                {
                    Villa_Number = 202,
                    VillaID = 2
                },
                new VillaNumber
                {
                    Villa_Number = 203,
                    VillaID = 2
                },
                new VillaNumber
                {
                    Villa_Number = 301,
                    VillaID = 3
                },
                new VillaNumber
                {
                    Villa_Number = 302,
                    VillaID = 3
                },
                new VillaNumber
                {
                    Villa_Number = 303,
                    VillaID = 3
                }
                );
            modelBuilder.Entity<Amenity>().HasData(
                new Amenity
                {
                    ID = 1,
                    Name = "Swimming Pool",
                    Description = "Large outdoor pool with temperature control.",
                    villaID = 1
                },
                new Amenity
                {
                    ID = 2,
                    Name = "Gym",
                    Description = "Fully equipped fitness center.",
                    villaID = 1
                },
                new Amenity
                {
                    ID = 3,
                    Name = "Spa",
                    Description = "Relaxing spa with professional masseurs.",
                    villaID = 1
                },
                new Amenity
                {
                    ID = 4,
                    Name = "WiFi",
                    Description = "High-speed internet access.",
                    villaID = 2
                },
                new Amenity
                {
                    ID = 5,
                    Name = "Private Balcony",
                    Description = "Spacious balcony with a scenic view.",
                    villaID = 2
                },
                new Amenity
                {
                    ID = 6,
                    Name = "Mini Bar",
                    Description = "Stocked mini-bar with refreshments.",
                    villaID = 2
                },
                new Amenity
                {
                    ID = 7,
                    Name = "Jacuzzi",
                    Description = "Luxury hot tub for relaxation.",
                    villaID = 3
                },
                new Amenity
                {
                    ID = 8,
                    Name = "Smart TV",
                    Description = "Latest 55-inch Smart TV with streaming services.",
                    villaID = 3
                },
                new Amenity
                {
                    ID = 9,
                    Name = "Room Service",
                    Description = "4/7 room service for your convenience.",
                    villaID = 3
                }
                );
        }
    }
}
