using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System;

namespace Backend.Data
{
    public class ProjectTrackingContext : DbContext
    {
        public ProjectTrackingContext(DbContextOptions<ProjectTrackingContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Ship> Ships => Set<Ship>();
        public DbSet<ProgramName> ProgramNames => Set<ProgramName>();
        public DbSet<ProjectType> ProjectTypes => Set<ProjectType>();
        public DbSet<ParticipantType> ParticipantTypes => Set<ParticipantType>();
        public DbSet<OutputType> OutputTypes => Set<OutputType>();
        public DbSet<SpecificOutputType> SpecificOutputTypes => Set<SpecificOutputType>();
        public DbSet<FinanceCode> FinanceCodes => Set<FinanceCode>();
        public DbSet<Institution> Institutions => Set<Institution>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<ProjectParticipantOutput> ProjectParticipantOutputs => Set<ProjectParticipantOutput>();
        public DbSet<PatientOutput> PatientOutputs => Set<PatientOutput>();
        public DbSet<DmsInfrastructureOutput> DmsInfrastructureOutputs => Set<DmsInfrastructureOutput>();
        public DbSet<PlannedOutput> PlannedOutputs => Set<PlannedOutput>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique indexes
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.ProjectID)
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.ParticipantID)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.CountryName)
                .IsUnique();

            modelBuilder.Entity<Ship>()
                .HasIndex(s => s.ShipName)
                .IsUnique();

            modelBuilder.Entity<ProgramName>()
                .HasIndex(pn => pn.Name)
                .IsUnique();

            // Set up Restrict delete behavior to prevent SQL Server multiple cascade path errors
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ==========================================
            // SEED DATA FOR LOOKUP TABLES
            // ==========================================

            // Seed Countries
            modelBuilder.Entity<Country>().HasData(
                new Country { CountryID = 1, CountryName = "Senegal", CountryCode = "SN" },
                new Country { CountryID = 2, CountryName = "Sierra Leone", CountryCode = "SL" },
                new Country { CountryID = 3, CountryName = "Liberia", CountryCode = "LR" },
                new Country { CountryID = 4, CountryName = "Guinea", CountryCode = "GN" },
                new Country { CountryID = 5, CountryName = "Madagascar", CountryCode = "MG" }
            );

            // Seed Cities/Regions
            modelBuilder.Entity<City>().HasData(
                new City { CityID = 1, CityName = "Dakar", CountryID = 1, RegionName = "Dakar Region" },
                new City { CityID = 2, CityName = "Thiès", CountryID = 1, RegionName = "Thiès Region" },
                new City { CityID = 3, CityName = "Saint-Louis", CountryID = 1, RegionName = "Saint-Louis" },
                new City { CityID = 4, CityName = "Freetown", CountryID = 2, RegionName = "Western Area" },
                new City { CityID = 5, CityName = "Bo", CountryID = 2, RegionName = "Southern Province" },
                new City { CityID = 6, CityName = "Monrovia", CountryID = 3, RegionName = "Montserrado" }
            );

            // Seed Ships
            modelBuilder.Entity<Ship>().HasData(
                new Ship { ShipID = 1, ShipName = "Global Mercy", IsActive = true },
                new Ship { ShipID = 2, ShipName = "Africa Mercy", IsActive = true },
                new Ship { ShipID = 3, ShipName = "None / Land-based", IsActive = true }
            );

            // Seed Program Names
            modelBuilder.Entity<ProgramName>().HasData(
                new ProgramName { ProgramID = 1, Name = "Ophthalmic (Eye) Surgery", Description = "Cataract and other ophthalmic surgeries" },
                new ProgramName { ProgramID = 2, Name = "Maxillofacial Surgery", Description = "Reconstructive surgeries for facial deformities" },
                new ProgramName { ProgramID = 3, Name = "General & Pediatric Surgery", Description = "Hernias, goiters, and pediatric specialized surgeries" },
                new ProgramName { ProgramID = 4, Name = "Orthopedic (Clubfoot) Program", Description = "Correction of clubfoot and pediatric orthopedic deformities" },
                new ProgramName { ProgramID = 5, Name = "Women's Health / Fistula", Description = "Vesicovaginal fistula repairs and maternal training" },
                new ProgramName { ProgramID = 6, Name = "Dental & Oral Health", Description = "Free dental care and training of local students" },
                new ProgramName { ProgramID = 7, Name = "Medical Capacity Building (MCB)", Description = "Training local surgeons, nurses, and technicians" }
            );

            // Seed Project Types
            modelBuilder.Entity<ProjectType>().HasData(
                new ProjectType { ProjectTypeID = 1, TypeName = "Surgical Outreach" },
                new ProjectType { ProjectTypeID = 2, TypeName = "Clinical Training Course" },
                new ProjectType { ProjectTypeID = 3, TypeName = "Mentorship & Shadowing" },
                new ProjectType { ProjectTypeID = 4, TypeName = "Hospital Infrastructure Setup" }
            );

            // Seed Participant Types
            modelBuilder.Entity<ParticipantType>().HasData(
                new ParticipantType { ParticipantTypeID = 1, TypeName = "Surgeon / Doctor" },
                new ParticipantType { ParticipantTypeID = 2, TypeName = "Anesthetic Provider" },
                new ParticipantType { ParticipantTypeID = 3, TypeName = "Registered Nurse" },
                new ParticipantType { ParticipantTypeID = 4, TypeName = "Biomedical Technician" },
                new ParticipantType { ParticipantTypeID = 5, TypeName = "Sterile Processing Staff" },
                new ParticipantType { ParticipantTypeID = 6, TypeName = "Community Health Facilitator" }
            );

            // Seed Output Types
            modelBuilder.Entity<OutputType>().HasData(
                new OutputType { OutputTypeID = 1, TypeName = "Participants Trained", Category = "Participant" },
                new OutputType { OutputTypeID = 2, TypeName = "Instruction Days Delivered", Category = "Participant" },
                new OutputType { OutputTypeID = 3, TypeName = "Mentorship Hours Completed", Category = "Participant" },
                new OutputType { OutputTypeID = 4, TypeName = "Surgeries Performed", Category = "Patient" },
                new OutputType { OutputTypeID = 5, TypeName = "Patients Screened", Category = "Patient" },
                new OutputType { OutputTypeID = 6, TypeName = "Dental Extractions", Category = "Patient" },
                new OutputType { OutputTypeID = 7, TypeName = "Anesthesia Cases Managed", Category = "Patient" },
                new OutputType { OutputTypeID = 8, TypeName = "Infrastructure Projects Completed", Category = "DMS" },
                new OutputType { OutputTypeID = 9, TypeName = "Equipment Donations Distributed", Category = "DMS" },
                new OutputType { OutputTypeID = 10, TypeName = "Advocacy Meetings Held", Category = "DMS" }
            );

            // Seed Specific Output Types
            modelBuilder.Entity<SpecificOutputType>().HasData(
                new SpecificOutputType { SpecificOutputTypeID = 1, OutputTypeID = 1, SpecificTypeName = "Primary Surgical Training" },
                new SpecificOutputType { SpecificOutputTypeID = 2, OutputTypeID = 1, SpecificTypeName = "Safe Surgery checklist Training" },
                new SpecificOutputType { SpecificOutputTypeID = 3, OutputTypeID = 4, SpecificTypeName = "Cataract Surgery" },
                new SpecificOutputType { SpecificOutputTypeID = 4, OutputTypeID = 4, SpecificTypeName = "Cleft Lip Repair" },
                new SpecificOutputType { SpecificOutputTypeID = 5, OutputTypeID = 4, SpecificTypeName = "Fistula Repair" },
                new SpecificOutputType { SpecificOutputTypeID = 6, OutputTypeID = 9, SpecificTypeName = "Anesthesia Machine Donation" },
                new SpecificOutputType { SpecificOutputTypeID = 7, OutputTypeID = 9, SpecificTypeName = "Surgical Pack Donation" }
            );

            // Seed Finance Codes
            modelBuilder.Entity<FinanceCode>().HasData(
                new FinanceCode { FinanceCodeID = 1, Code = "LOC-SEN-DKR", Description = "Dakar, Senegal location", CodeType = "Location" },
                new FinanceCode { FinanceCodeID = 2, Code = "LOC-SLE-FNA", Description = "Freetown, Sierra Leone location", CodeType = "Location" },
                new FinanceCode { FinanceCodeID = 3, Code = "LOC-LBR-MNR", Description = "Monrovia, Liberia location", CodeType = "Location" },
                new FinanceCode { FinanceCodeID = 4, Code = "PGM-EYE-CARE", Description = "Ophthalmic program line", CodeType = "Program" },
                new FinanceCode { FinanceCodeID = 5, Code = "PGM-MCB-TRN", Description = "Medical capacity building line", CodeType = "Program" },
                new FinanceCode { FinanceCodeID = 6, Code = "PGM-GEN-SURG", Description = "General surgery program line", CodeType = "Program" },
                new FinanceCode { FinanceCodeID = 7, Code = "PUR-TRN-NURS", Description = "Nurse clinical mentoring purpose", CodeType = "Purpose" },
                new FinanceCode { FinanceCodeID = 8, Code = "PUR-OPS-SURG", Description = "Direct surgical operations purpose", CodeType = "Purpose" }
            );

            // Seed Institutions
            modelBuilder.Entity<Institution>().HasData(
                new Institution { InstitutionID = 1, InstitutionName = "Hôpital Principal de Dakar", CountryID = 1 },
                new Institution { InstitutionID = 2, InstitutionName = "Thiès Regional Hospital", CountryID = 1 },
                new Institution { InstitutionID = 3, InstitutionName = "Connaught Hospital Freetown", CountryID = 2 },
                new Institution { InstitutionID = 4, InstitutionName = "JFK Medical Center Monrovia", CountryID = 3 }
            );
        }
    }
}
