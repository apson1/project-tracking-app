using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
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
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryID);
                });

            migrationBuilder.CreateTable(
                name: "FinanceCodes",
                columns: table => new
                {
                    FinanceCodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCodes", x => x.FinanceCodeID);
                });

            migrationBuilder.CreateTable(
                name: "OutputTypes",
                columns: table => new
                {
                    OutputTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputTypes", x => x.OutputTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantTypes",
                columns: table => new
                {
                    ParticipantTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantTypes", x => x.ParticipantTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ProgramNames",
                columns: table => new
                {
                    ProgramID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramNames", x => x.ProgramID);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypes",
                columns: table => new
                {
                    ProjectTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypes", x => x.ProjectTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    ShipID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.ShipID);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityID);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    InstitutionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstitutionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.InstitutionID);
                    table.ForeignKey(
                        name: "FK_Institutions_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpecificOutputTypes",
                columns: table => new
                {
                    SpecificOutputTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutputTypeID = table.Column<int>(type: "int", nullable: false),
                    SpecificTypeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificOutputTypes", x => x.SpecificOutputTypeID);
                    table.ForeignKey(
                        name: "FK_SpecificOutputTypes_OutputTypes_OutputTypeID",
                        column: x => x.OutputTypeID,
                        principalTable: "OutputTypes",
                        principalColumn: "OutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectIDNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProjectTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FrenchProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProgramID = table.Column<int>(type: "int", nullable: true),
                    MasterStatsCategoryGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MasterStatsCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectTypeID = table.Column<int>(type: "int", nullable: true),
                    ParticipantTypeID = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstructionDays = table.Column<int>(type: "int", nullable: false),
                    ShipID = table.Column<int>(type: "int", nullable: true),
                    PreShipPost = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    CityID = table.Column<int>(type: "int", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinanceLocationID = table.Column<int>(type: "int", nullable: true),
                    FinanceProgramID = table.Column<int>(type: "int", nullable: true),
                    FinancePurposeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectIDNumber);
                    table.ForeignKey(
                        name: "FK_Projects_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_FinanceCodes_FinanceLocationID",
                        column: x => x.FinanceLocationID,
                        principalTable: "FinanceCodes",
                        principalColumn: "FinanceCodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_FinanceCodes_FinanceProgramID",
                        column: x => x.FinanceProgramID,
                        principalTable: "FinanceCodes",
                        principalColumn: "FinanceCodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_FinanceCodes_FinancePurposeID",
                        column: x => x.FinancePurposeID,
                        principalTable: "FinanceCodes",
                        principalColumn: "FinanceCodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_ParticipantTypes_ParticipantTypeID",
                        column: x => x.ParticipantTypeID,
                        principalTable: "ParticipantTypes",
                        principalColumn: "ParticipantTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_ProgramNames_ProgramID",
                        column: x => x.ProgramID,
                        principalTable: "ProgramNames",
                        principalColumn: "ProgramID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_ProjectTypes_ProjectTypeID",
                        column: x => x.ProjectTypeID,
                        principalTable: "ProjectTypes",
                        principalColumn: "ProjectTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Ships_ShipID",
                        column: x => x.ShipID,
                        principalTable: "Ships",
                        principalColumn: "ShipID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantIDNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InstitutionID = table.Column<int>(type: "int", nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfessionTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityID = table.Column<int>(type: "int", nullable: true),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantIDNumber);
                    table.ForeignKey(
                        name: "FK_Participants_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participants_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participants_Institutions_InstitutionID",
                        column: x => x.InstitutionID,
                        principalTable: "Institutions",
                        principalColumn: "InstitutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DmsInfrastructureOutputs",
                columns: table => new
                {
                    DmsInfrastructureOutputID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectIDNumber = table.Column<int>(type: "int", nullable: false),
                    ReportingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportingPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutputAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutputTypeID = table.Column<int>(type: "int", nullable: false),
                    SpecificOutputTypeID = table.Column<int>(type: "int", nullable: true),
                    AdultChild = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProgramTypeOfOutput = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmsInfrastructureOutputs", x => x.DmsInfrastructureOutputID);
                    table.ForeignKey(
                        name: "FK_DmsInfrastructureOutputs_OutputTypes_OutputTypeID",
                        column: x => x.OutputTypeID,
                        principalTable: "OutputTypes",
                        principalColumn: "OutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DmsInfrastructureOutputs_Projects_ProjectIDNumber",
                        column: x => x.ProjectIDNumber,
                        principalTable: "Projects",
                        principalColumn: "ProjectIDNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DmsInfrastructureOutputs_SpecificOutputTypes_SpecificOutputTypeID",
                        column: x => x.SpecificOutputTypeID,
                        principalTable: "SpecificOutputTypes",
                        principalColumn: "SpecificOutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientOutputs",
                columns: table => new
                {
                    PatientOutputID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectIDNumber = table.Column<int>(type: "int", nullable: false),
                    PatientID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AgeGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CityID = table.Column<int>(type: "int", nullable: true),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    ReportingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutputAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutputTypeID = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientOutputs", x => x.PatientOutputID);
                    table.ForeignKey(
                        name: "FK_PatientOutputs_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientOutputs_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientOutputs_OutputTypes_OutputTypeID",
                        column: x => x.OutputTypeID,
                        principalTable: "OutputTypes",
                        principalColumn: "OutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientOutputs_Projects_ProjectIDNumber",
                        column: x => x.ProjectIDNumber,
                        principalTable: "Projects",
                        principalColumn: "ProjectIDNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlannedOutputs",
                columns: table => new
                {
                    PlannedOutputID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectIDNumber = table.Column<int>(type: "int", nullable: false),
                    PlannedOutputTypeID = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReportingPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlannedDateYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedOutputs", x => x.PlannedOutputID);
                    table.ForeignKey(
                        name: "FK_PlannedOutputs_OutputTypes_PlannedOutputTypeID",
                        column: x => x.PlannedOutputTypeID,
                        principalTable: "OutputTypes",
                        principalColumn: "OutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlannedOutputs_Projects_ProjectIDNumber",
                        column: x => x.ProjectIDNumber,
                        principalTable: "Projects",
                        principalColumn: "ProjectIDNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectParticipantOutputs",
                columns: table => new
                {
                    ProjectParticipantOutputID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectIDNumber = table.Column<int>(type: "int", nullable: false),
                    ParticipantIDNumber = table.Column<int>(type: "int", nullable: false),
                    ReportingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportingPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutputAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutputTypeID = table.Column<int>(type: "int", nullable: false),
                    SpecificOutputTypeID = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectParticipantOutputs", x => x.ProjectParticipantOutputID);
                    table.ForeignKey(
                        name: "FK_ProjectParticipantOutputs_OutputTypes_OutputTypeID",
                        column: x => x.OutputTypeID,
                        principalTable: "OutputTypes",
                        principalColumn: "OutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectParticipantOutputs_Participants_ParticipantIDNumber",
                        column: x => x.ParticipantIDNumber,
                        principalTable: "Participants",
                        principalColumn: "ParticipantIDNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectParticipantOutputs_Projects_ProjectIDNumber",
                        column: x => x.ProjectIDNumber,
                        principalTable: "Projects",
                        principalColumn: "ProjectIDNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectParticipantOutputs_SpecificOutputTypes_SpecificOutputTypeID",
                        column: x => x.SpecificOutputTypeID,
                        principalTable: "SpecificOutputTypes",
                        principalColumn: "SpecificOutputTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryID", "CountryCode", "CountryName" },
                values: new object[,]
                {
                    { 1, "SN", "Senegal" },
                    { 2, "SL", "Sierra Leone" },
                    { 3, "LR", "Liberia" },
                    { 4, "GN", "Guinea" },
                    { 5, "MG", "Madagascar" }
                });

            migrationBuilder.InsertData(
                table: "FinanceCodes",
                columns: new[] { "FinanceCodeID", "Code", "CodeType", "Description" },
                values: new object[,]
                {
                    { 1, "LOC-SEN-DKR", "Location", "Dakar, Senegal location" },
                    { 2, "LOC-SLE-FNA", "Location", "Freetown, Sierra Leone location" },
                    { 3, "LOC-LBR-MNR", "Location", "Monrovia, Liberia location" },
                    { 4, "PGM-EYE-CARE", "Program", "Ophthalmic program line" },
                    { 5, "PGM-MCB-TRN", "Program", "Medical capacity building line" },
                    { 6, "PGM-GEN-SURG", "Program", "General surgery program line" },
                    { 7, "PUR-TRN-NURS", "Purpose", "Nurse clinical mentoring purpose" },
                    { 8, "PUR-OPS-SURG", "Purpose", "Direct surgical operations purpose" }
                });

            migrationBuilder.InsertData(
                table: "OutputTypes",
                columns: new[] { "OutputTypeID", "Category", "TypeName" },
                values: new object[,]
                {
                    { 1, "Participant", "Participants Trained" },
                    { 2, "Participant", "Instruction Days Delivered" },
                    { 3, "Participant", "Mentorship Hours Completed" },
                    { 4, "Patient", "Surgeries Performed" },
                    { 5, "Patient", "Patients Screened" },
                    { 6, "Patient", "Dental Extractions" },
                    { 7, "Patient", "Anesthesia Cases Managed" },
                    { 8, "DMS", "Infrastructure Projects Completed" },
                    { 9, "DMS", "Equipment Donations Distributed" },
                    { 10, "DMS", "Advocacy Meetings Held" }
                });

            migrationBuilder.InsertData(
                table: "ParticipantTypes",
                columns: new[] { "ParticipantTypeID", "TypeName" },
                values: new object[,]
                {
                    { 1, "Surgeon / Doctor" },
                    { 2, "Anesthetic Provider" },
                    { 3, "Registered Nurse" },
                    { 4, "Biomedical Technician" },
                    { 5, "Sterile Processing Staff" },
                    { 6, "Community Health Facilitator" }
                });

            migrationBuilder.InsertData(
                table: "ProgramNames",
                columns: new[] { "ProgramID", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Cataract and other ophthalmic surgeries", "Ophthalmic (Eye) Surgery" },
                    { 2, "Reconstructive surgeries for facial deformities", "Maxillofacial Surgery" },
                    { 3, "Hernias, goiters, and pediatric specialized surgeries", "General & Pediatric Surgery" },
                    { 4, "Correction of clubfoot and pediatric orthopedic deformities", "Orthopedic (Clubfoot) Program" },
                    { 5, "Vesicovaginal fistula repairs and maternal training", "Women's Health / Fistula" },
                    { 6, "Free dental care and training of local students", "Dental & Oral Health" },
                    { 7, "Training local surgeons, nurses, and technicians", "Medical Capacity Building (MCB)" }
                });

            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "ProjectTypeID", "TypeName" },
                values: new object[,]
                {
                    { 1, "Surgical Outreach" },
                    { 2, "Clinical Training Course" },
                    { 3, "Mentorship & Shadowing" },
                    { 4, "Hospital Infrastructure Setup" }
                });

            migrationBuilder.InsertData(
                table: "Ships",
                columns: new[] { "ShipID", "IsActive", "ShipName" },
                values: new object[,]
                {
                    { 1, true, "Global Mercy" },
                    { 2, true, "Africa Mercy" },
                    { 3, true, "None / Land-based" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "CityID", "CityName", "CountryID", "RegionName" },
                values: new object[,]
                {
                    { 1, "Dakar", 1, "Dakar Region" },
                    { 2, "Thiès", 1, "Thiès Region" },
                    { 3, "Saint-Louis", 1, "Saint-Louis" },
                    { 4, "Freetown", 2, "Western Area" },
                    { 5, "Bo", 2, "Southern Province" },
                    { 6, "Monrovia", 3, "Montserrado" }
                });

            migrationBuilder.InsertData(
                table: "Institutions",
                columns: new[] { "InstitutionID", "CountryID", "InstitutionName" },
                values: new object[,]
                {
                    { 1, 1, "Hôpital Principal de Dakar" },
                    { 2, 1, "Thiès Regional Hospital" },
                    { 3, 2, "Connaught Hospital Freetown" },
                    { 4, 3, "JFK Medical Center Monrovia" }
                });

            migrationBuilder.InsertData(
                table: "SpecificOutputTypes",
                columns: new[] { "SpecificOutputTypeID", "OutputTypeID", "SpecificTypeName" },
                values: new object[,]
                {
                    { 1, 1, "Primary Surgical Training" },
                    { 2, 1, "Safe Surgery checklist Training" },
                    { 3, 4, "Cataract Surgery" },
                    { 4, 4, "Cleft Lip Repair" },
                    { 5, 4, "Fistula Repair" },
                    { 6, 9, "Anesthesia Machine Donation" },
                    { 7, 9, "Surgical Pack Donation" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryID",
                table: "Cities",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryName",
                table: "Countries",
                column: "CountryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DmsInfrastructureOutputs_OutputTypeID",
                table: "DmsInfrastructureOutputs",
                column: "OutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DmsInfrastructureOutputs_ProjectIDNumber",
                table: "DmsInfrastructureOutputs",
                column: "ProjectIDNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DmsInfrastructureOutputs_SpecificOutputTypeID",
                table: "DmsInfrastructureOutputs",
                column: "SpecificOutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_CountryID",
                table: "Institutions",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_CityID",
                table: "Participants",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_CountryID",
                table: "Participants",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_InstitutionID",
                table: "Participants",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ParticipantID",
                table: "Participants",
                column: "ParticipantID",
                unique: true,
                filter: "[ParticipantID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientOutputs_CityID",
                table: "PatientOutputs",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientOutputs_CountryID",
                table: "PatientOutputs",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientOutputs_OutputTypeID",
                table: "PatientOutputs",
                column: "OutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientOutputs_ProjectIDNumber",
                table: "PatientOutputs",
                column: "ProjectIDNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedOutputs_PlannedOutputTypeID",
                table: "PlannedOutputs",
                column: "PlannedOutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedOutputs_ProjectIDNumber",
                table: "PlannedOutputs",
                column: "ProjectIDNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramNames_Name",
                table: "ProgramNames",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantOutputs_OutputTypeID",
                table: "ProjectParticipantOutputs",
                column: "OutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantOutputs_ParticipantIDNumber",
                table: "ProjectParticipantOutputs",
                column: "ParticipantIDNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantOutputs_ProjectIDNumber",
                table: "ProjectParticipantOutputs",
                column: "ProjectIDNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantOutputs_SpecificOutputTypeID",
                table: "ProjectParticipantOutputs",
                column: "SpecificOutputTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CityID",
                table: "Projects",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CountryID",
                table: "Projects",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FinanceLocationID",
                table: "Projects",
                column: "FinanceLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FinanceProgramID",
                table: "Projects",
                column: "FinanceProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FinancePurposeID",
                table: "Projects",
                column: "FinancePurposeID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ParticipantTypeID",
                table: "Projects",
                column: "ParticipantTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProgramID",
                table: "Projects",
                column: "ProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectID",
                table: "Projects",
                column: "ProjectID",
                unique: true,
                filter: "[ProjectID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectTypeID",
                table: "Projects",
                column: "ProjectTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ShipID",
                table: "Projects",
                column: "ShipID");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_ShipName",
                table: "Ships",
                column: "ShipName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecificOutputTypes_OutputTypeID",
                table: "SpecificOutputTypes",
                column: "OutputTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DmsInfrastructureOutputs");

            migrationBuilder.DropTable(
                name: "PatientOutputs");

            migrationBuilder.DropTable(
                name: "PlannedOutputs");

            migrationBuilder.DropTable(
                name: "ProjectParticipantOutputs");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "SpecificOutputTypes");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "FinanceCodes");

            migrationBuilder.DropTable(
                name: "ParticipantTypes");

            migrationBuilder.DropTable(
                name: "ProgramNames");

            migrationBuilder.DropTable(
                name: "ProjectTypes");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "OutputTypes");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
