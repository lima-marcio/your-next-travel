using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourNextTravel.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsoCode2 = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BaseCurrencyCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    QuoteCurrencyCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    GoogleSubjectId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CountryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CountryCostIndexes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CountryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Multiplier = table.Column<decimal>(type: "TEXT", precision: 6, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryCostIndexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryCostIndexes_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalHealthRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CountryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VisaRequirementText = table.Column<string>(type: "TEXT", nullable: false),
                    VaccinationRequirementText = table.Column<string>(type: "TEXT", nullable: false),
                    OtherHealthNotes = table.Column<string>(type: "TEXT", nullable: true),
                    SourceNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalHealthRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalHealthRequirements_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Detail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DestinationSearches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TravelerProfileTypeUsed = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationSearches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationSearches_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DestinationSearches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VenueName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ExternalUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventListings_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LodgingPriceEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SampleWindowStart = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SampleWindowEnd = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    AvgNightlyAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    MinNightlyAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    MaxNightlyAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    SampleSize = table.Column<int>(type: "INTEGER", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LodgingPriceEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LodgingPriceEstimates_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Granularity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: true),
                    ForDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AvgTempC = table.Column<double>(type: "REAL", nullable: false),
                    MinTempC = table.Column<double>(type: "REAL", nullable: false),
                    MaxTempC = table.Column<double>(type: "REAL", nullable: false),
                    PrecipitationMm = table.Column<double>(type: "REAL", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherSnapshots_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DestinationSearchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LodgingComponentAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    MiscDailyComponentAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    AssumptionsNote = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetEstimates_DestinationSearches_DestinationSearchId",
                        column: x => x.DestinationSearchId,
                        principalTable: "DestinationSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEstimates_DestinationSearchId",
                table: "BudgetEstimates",
                column: "DestinationSearchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId_Name",
                table: "Cities",
                columns: new[] { "CountryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsoCode2",
                table: "Countries",
                column: "IsoCode2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryCostIndexes_CountryId",
                table: "CountryCostIndexes",
                column: "CountryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_BaseCurrencyCode_QuoteCurrencyCode_AsOfDate",
                table: "CurrencyRates",
                columns: new[] { "BaseCurrencyCode", "QuoteCurrencyCode", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DestinationSearches_CityId",
                table: "DestinationSearches",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationSearches_UserId_CreatedAtUtc",
                table: "DestinationSearches",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EventListings_Category_StartUtc",
                table: "EventListings",
                columns: new[] { "Category", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EventListings_CityId",
                table: "EventListings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_EventListings_ProviderName_ExternalId",
                table: "EventListings",
                columns: new[] { "ProviderName", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interests_UserId_Category_Detail",
                table: "Interests",
                columns: new[] { "UserId", "Category", "Detail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalHealthRequirements_CountryId",
                table: "LegalHealthRequirements",
                column: "CountryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LodgingPriceEstimates_CityId_FetchedAtUtc",
                table: "LodgingPriceEstimates",
                columns: new[] { "CityId", "FetchedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TravelerProfiles_UserId",
                table: "TravelerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleSubjectId",
                table: "Users",
                column: "GoogleSubjectId",
                unique: true,
                filter: "[GoogleSubjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSnapshots_CityId_Granularity_ForDate",
                table: "WeatherSnapshots",
                columns: new[] { "CityId", "Granularity", "ForDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSnapshots_CityId_Granularity_Month",
                table: "WeatherSnapshots",
                columns: new[] { "CityId", "Granularity", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetEstimates");

            migrationBuilder.DropTable(
                name: "CountryCostIndexes");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "EventListings");

            migrationBuilder.DropTable(
                name: "Interests");

            migrationBuilder.DropTable(
                name: "LegalHealthRequirements");

            migrationBuilder.DropTable(
                name: "LodgingPriceEstimates");

            migrationBuilder.DropTable(
                name: "TravelerProfiles");

            migrationBuilder.DropTable(
                name: "WeatherSnapshots");

            migrationBuilder.DropTable(
                name: "DestinationSearches");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
