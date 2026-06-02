using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // ==========================================
    // REFERENCE LOOKUP TABLES
    // ==========================================

    public class Country
    {
        [Key]
        public int CountryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string CountryCode { get; set; } = string.Empty; // e.g. ISO-2/3 Code
    }

    public class City
    {
        [Key]
        public int CityID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CityName { get; set; } = string.Empty;

        [Required]
        public int CountryID { get; set; }

        [ForeignKey("CountryID")]
        public Country? Country { get; set; }

        [MaxLength(100)]
        public string RegionName { get; set; } = string.Empty;
    }

    public class Ship
    {
        [Key]
        public int ShipID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ShipName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class ProgramName
    {
        [Key]
        public int ProgramID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class ProjectType
    {
        [Key]
        public int ProjectTypeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;
    }

    public class ParticipantType
    {
        [Key]
        public int ParticipantTypeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;
    }

    public class OutputType
    {
        [Key]
        public int OutputTypeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // e.g. "Participant", "Patient", "DMS"
    }

    public class SpecificOutputType
    {
        [Key]
        public int SpecificOutputTypeID { get; set; }

        [Required]
        public int OutputTypeID { get; set; }

        [ForeignKey("OutputTypeID")]
        public OutputType? OutputType { get; set; }

        [Required]
        [MaxLength(150)]
        public string SpecificTypeName { get; set; } = string.Empty;
    }

    public class FinanceCode
    {
        [Key]
        public int FinanceCodeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CodeType { get; set; } = string.Empty; // "Location", "Program", "Purpose"
    }

    public class Institution
    {
        [Key]
        public int InstitutionID { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        public int? CountryID { get; set; }

        [ForeignKey("CountryID")]
        public Country? Country { get; set; }
    }

    // ==========================================
    // CORE ENTITIES
    // ==========================================

    public class Project
    {
        [Key]
        public int ProjectIDNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProjectID { get; set; } = string.Empty; // Unique Code (e.g. PRJ-2026-001)

        [Required]
        [MaxLength(200)]
        public string ProjectTitle { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FrenchProjectName { get; set; } = string.Empty;

        public int? ProgramID { get; set; }
        [ForeignKey("ProgramID")]
        public ProgramName? Program { get; set; }

        [MaxLength(100)]
        public string MasterStatsCategoryGroup { get; set; } = string.Empty;

        [MaxLength(100)]
        public string MasterStatsCategory { get; set; } = string.Empty;

        public int? ProjectTypeID { get; set; }
        [ForeignKey("ProjectTypeID")]
        public ProjectType? ProjectType { get; set; }

        public int? ParticipantTypeID { get; set; }
        [ForeignKey("ParticipantTypeID")]
        public ParticipantType? ParticipantType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int InstructionDays { get; set; }

        public int? ShipID { get; set; }
        [ForeignKey("ShipID")]
        public Ship? Ship { get; set; }

        [MaxLength(50)]
        public string PreShipPost { get; set; } = string.Empty; // "Pre", "Ship", "Post"

        public int? CountryID { get; set; }
        [ForeignKey("CountryID")]
        public Country? Country { get; set; }

        public int? CityID { get; set; }
        [ForeignKey("CityID")]
        public City? City { get; set; }

        [MaxLength(200)]
        public string Venue { get; set; } = string.Empty;

        public string ProjectComments { get; set; } = string.Empty;

        public int? FinanceLocationID { get; set; }
        [ForeignKey("FinanceLocationID")]
        public FinanceCode? FinanceLocation { get; set; }

        public int? FinanceProgramID { get; set; }
        [ForeignKey("FinanceProgramID")]
        public FinanceCode? FinanceProgram { get; set; }

        public int? FinancePurposeID { get; set; }
        [ForeignKey("FinancePurposeID")]
        public FinanceCode? FinancePurpose { get; set; }

        // Navigation properties
        public ICollection<ProjectParticipantOutput> ParticipantOutputs { get; set; } = new List<ProjectParticipantOutput>();
        public ICollection<PatientOutput> PatientOutputs { get; set; } = new List<PatientOutput>();
        public ICollection<DmsInfrastructureOutput> DmsInfrastructureOutputs { get; set; } = new List<DmsInfrastructureOutput>();
        public ICollection<PlannedOutput> PlannedOutputs { get; set; } = new List<PlannedOutput>();
    }

    public class Participant
    {
        [Key]
        public int ParticipantIDNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string ParticipantID { get; set; } = string.Empty; // Unique Code (e.g. PAR-00001)

        [MaxLength(20)]
        public string Title { get; set; } = string.Empty; // Dr, Mr, Ms

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        public int? InstitutionID { get; set; }
        [ForeignKey("InstitutionID")]
        public Institution? Institution { get; set; }

        [MaxLength(50)]
        public string MobilePhone { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ProfessionTitle { get; set; } = string.Empty;

        public int? CityID { get; set; } // Region
        [ForeignKey("CityID")]
        public City? City { get; set; }

        public int? CountryID { get; set; }
        [ForeignKey("CountryID")]
        public Country? Country { get; set; }

        public string Comments { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<ProjectParticipantOutput> AttendedProjects { get; set; } = new List<ProjectParticipantOutput>();
    }

    // ==========================================
    // BRIDGE / OUTPUT TABLES
    // ==========================================

    public class ProjectParticipantOutput
    {
        [Key]
        public int ProjectParticipantOutputID { get; set; }

        [Required]
        public int ProjectIDNumber { get; set; }
        [ForeignKey("ProjectIDNumber")]
        public Project? Project { get; set; }

        [Required]
        public int ParticipantIDNumber { get; set; }
        [ForeignKey("ParticipantIDNumber")]
        public Participant? Participant { get; set; }

        [Required]
        public DateTime ReportingDate { get; set; }

        [MaxLength(50)]
        public string ReportingPeriod { get; set; } = string.Empty; // e.g. "Monthly", "Q1 2026"

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutputAmount { get; set; } // e.g. Hours, sessions

        [Required]
        public int OutputTypeID { get; set; }
        [ForeignKey("OutputTypeID")]
        public OutputType? OutputType { get; set; }

        public int? SpecificOutputTypeID { get; set; }
        [ForeignKey("SpecificOutputTypeID")]
        public SpecificOutputType? SpecificOutputType { get; set; }

        public string Comments { get; set; } = string.Empty;
    }

    public class PatientOutput
    {
        [Key]
        public int PatientOutputID { get; set; }

        [Required]
        public int ProjectIDNumber { get; set; }
        [ForeignKey("ProjectIDNumber")]
        public Project? Project { get; set; }

        [Required]
        [MaxLength(50)]
        public string PatientID { get; set; } = string.Empty; // Anonymized patient code

        [MaxLength(20)]
        public string Sex { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AgeGroup { get; set; } = string.Empty; // e.g., "0-5", "6-12", "18+"

        public int? CityID { get; set; } // Region of residence
        [ForeignKey("CityID")]
        public City? City { get; set; }

        public int? CountryID { get; set; } // Country of residence
        [ForeignKey("CountryID")]
        public Country? Country { get; set; }

        [Required]
        public DateTime ReportingDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutputAmount { get; set; }

        [Required]
        public int OutputTypeID { get; set; }
        [ForeignKey("OutputTypeID")]
        public OutputType? OutputType { get; set; }

        public string Comments { get; set; } = string.Empty;
    }

    public class DmsInfrastructureOutput
    {
        [Key]
        public int DmsInfrastructureOutputID { get; set; }

        [Required]
        public int ProjectIDNumber { get; set; }
        [ForeignKey("ProjectIDNumber")]
        public Project? Project { get; set; }

        [Required]
        public DateTime ReportingDate { get; set; }

        [MaxLength(50)]
        public string ReportingPeriod { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutputAmount { get; set; }

        [Required]
        public int OutputTypeID { get; set; }
        [ForeignKey("OutputTypeID")]
        public OutputType? OutputType { get; set; }

        public int? SpecificOutputTypeID { get; set; }
        [ForeignKey("SpecificOutputTypeID")]
        public SpecificOutputType? SpecificOutputType { get; set; }

        [MaxLength(20)]
        public string AdultChild { get; set; } = string.Empty; // "Adult", "Child"

        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ProgramTypeOfOutput { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;
    }

    public class PlannedOutput
    {
        [Key]
        public int PlannedOutputID { get; set; }

        [Required]
        public int ProjectIDNumber { get; set; }
        [ForeignKey("ProjectIDNumber")]
        public Project? Project { get; set; }

        [Required]
        public int PlannedOutputTypeID { get; set; }
        [ForeignKey("PlannedOutputTypeID")]
        public OutputType? PlannedOutputType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlannedAmount { get; set; }

        [MaxLength(50)]
        public string ReportingPeriod { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PlannedDateYear { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;
    }
}
