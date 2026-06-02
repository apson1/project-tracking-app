using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public ImportController(ProjectTrackingContext context)
        {
            _context = context;
        }

        public class ImportRequest
        {
            public string CsvData { get; set; } = string.Empty;
        }

        public class ProjectImportRow
        {
            public int Index { get; set; }
            public string ProjectID { get; set; } = string.Empty;
            public string ProjectTitle { get; set; } = string.Empty;
            public string FrenchProjectName { get; set; } = string.Empty;
            public string ProgramName { get; set; } = string.Empty;
            public string ProjectType { get; set; } = string.Empty;
            public string ParticipantType { get; set; } = string.Empty;
            public string StartDateStr { get; set; } = string.Empty;
            public string EndDateStr { get; set; } = string.Empty;
            public string InstructionDaysStr { get; set; } = string.Empty;
            public string Ship { get; set; } = string.Empty;
            public string PreShipPost { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Venue { get; set; } = string.Empty;
            public string ProjectComments { get; set; } = string.Empty;
            public string FinanceLocation { get; set; } = string.Empty;
            public string FinanceProgram { get; set; } = string.Empty;
            public string FinancePurpose { get; set; } = string.Empty;

            // Mapping state
            public bool IsValid { get; set; } = true;
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public bool IsDuplicate { get; set; }
            
            // Resolved IDs
            public int? ProgramID { get; set; }
            public int? ProjectTypeID { get; set; }
            public int? ParticipantTypeID { get; set; }
            public int? ShipID { get; set; }
            public int? CountryID { get; set; }
            public int? CityID { get; set; }
            public int? FinanceLocationID { get; set; }
            public int? FinanceProgramID { get; set; }
            public int? FinancePurposeID { get; set; }
        }

        public class ParticipantImportRow
        {
            public int Index { get; set; }
            public string ParticipantID { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public string Institution { get; set; } = string.Empty;
            public string MobilePhone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string ProfessionTitle { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public string Comments { get; set; } = string.Empty;

            // Mapping state
            public bool IsValid { get; set; } = true;
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public bool IsDuplicate { get; set; }

            // Resolved IDs
            public int? CountryID { get; set; }
            public int? CityID { get; set; }
            public int? InstitutionID { get; set; }
        }

        // POST: api/import/projects/validate
        [HttpPost("projects/validate")]
        public async Task<IActionResult> ValidateProjects(ImportRequest request)
        {
            var csv = request.CsvData ?? string.Empty;
            if (csv.StartsWith("\uFEFF"))
            {
                csv = csv.Substring(1);
            }

            var rows = ParseCsv(csv);
            if (rows.Count == 0) return BadRequest("Empty CSV or invalid formatting.");

            var headers = rows[0].Select(h => h.Trim().ToLower()).ToList();

            // Validate required headers
            var missingHeaders = new List<string>();
            int idxProjectId = headers.IndexOf("project id");
            int idxTitle = headers.IndexOf("project title");
            int idxStart = headers.IndexOf("start date");
            int idxEnd = headers.IndexOf("end date");
            int idxCountry = headers.IndexOf("country");

            if (idxProjectId < 0) missingHeaders.Add("Project ID");
            if (idxTitle < 0) missingHeaders.Add("Project Title");
            if (idxStart < 0) missingHeaders.Add("Start Date");
            if (idxEnd < 0) missingHeaders.Add("End Date");
            if (idxCountry < 0) missingHeaders.Add("Country");

            if (missingHeaders.Any())
            {
                return BadRequest($"Invalid CSV structure. Missing required column header(s): {string.Join(", ", missingHeaders)}. Verify spelling matches exactly.");
            }
            
            // Map headers to column indices
            int idxFrenchName = headers.IndexOf("french project name");
            int idxProgram = headers.IndexOf("program name");
            int idxType = headers.IndexOf("project type");
            int idxPartType = headers.IndexOf("participant type");
            int idxDays = headers.IndexOf("instruction days");
            int idxShip = headers.IndexOf("ship");
            int idxPreShipPost = headers.IndexOf("pre/ship/post");
            int idxCity = headers.IndexOf("city");
            int idxVenue = headers.IndexOf("venue");
            int idxComments = headers.IndexOf("project comments");
            int idxFinLoc = headers.IndexOf("finance location code");
            int idxFinProg = headers.IndexOf("finance program code");
            int idxFinPurp = headers.IndexOf("finance purpose code");

            // Lookups for mapping
            var countries = await _context.Countries.ToListAsync();
            var cities = await _context.Cities.ToListAsync();
            var ships = await _context.Ships.ToListAsync();
            var programs = await _context.ProgramNames.ToListAsync();
            var projectTypes = await _context.ProjectTypes.ToListAsync();
            var participantTypes = await _context.ParticipantTypes.ToListAsync();
            var financeCodes = await _context.FinanceCodes.ToListAsync();
            var existingProjectIds = await _context.Projects.Select(p => p.ProjectID.ToLower()).ToListAsync();

            var list = new List<ProjectImportRow>();

            for (int i = 1; i < rows.Count; i++)
            {
                var rowData = rows[i];
                if (rowData.Count == 0 || (rowData.Count == 1 && string.IsNullOrWhiteSpace(rowData[0]))) continue;

                var row = new ProjectImportRow
                {
                    Index = i,
                    ProjectID = GetVal(rowData, idxProjectId),
                    ProjectTitle = GetVal(rowData, idxTitle),
                    FrenchProjectName = GetVal(rowData, idxFrenchName),
                    ProgramName = GetVal(rowData, idxProgram),
                    ProjectType = GetVal(rowData, idxType),
                    ParticipantType = GetVal(rowData, idxPartType),
                    StartDateStr = GetVal(rowData, idxStart),
                    EndDateStr = GetVal(rowData, idxEnd),
                    InstructionDaysStr = GetVal(rowData, idxDays),
                    Ship = GetVal(rowData, idxShip),
                    PreShipPost = GetVal(rowData, idxPreShipPost),
                    Country = GetVal(rowData, idxCountry),
                    City = GetVal(rowData, idxCity),
                    Venue = GetVal(rowData, idxVenue),
                    ProjectComments = GetVal(rowData, idxComments),
                    FinanceLocation = GetVal(rowData, idxFinLoc),
                    FinanceProgram = GetVal(rowData, idxFinProg),
                    FinancePurpose = GetVal(rowData, idxFinPurp)
                };

                // Validate Project ID
                if (string.IsNullOrWhiteSpace(row.ProjectID))
                {
                    row.Errors.Add("Project ID is required.");
                }
                else if (existingProjectIds.Contains(row.ProjectID.ToLower()))
                {
                    row.IsDuplicate = true;
                    row.Warnings.Add($"Project ID '{row.ProjectID}' already exists in database. Importing will update it.");
                }

                // Validate Title
                if (string.IsNullOrWhiteSpace(row.ProjectTitle))
                {
                    row.Errors.Add("Project Title is required.");
                }

                // Resolve Program Name
                if (!string.IsNullOrWhiteSpace(row.ProgramName))
                {
                    var prog = programs.FirstOrDefault(p => p.Name.Equals(row.ProgramName, StringComparison.OrdinalIgnoreCase));
                    if (prog != null) row.ProgramID = prog.ProgramID;
                    else row.Errors.Add($"Program '{row.ProgramName}' not found in lookup tables. Resolve spelling or add first.");
                }

                // Resolve Project Type
                if (!string.IsNullOrWhiteSpace(row.ProjectType))
                {
                    var pt = projectTypes.FirstOrDefault(t => t.TypeName.Equals(row.ProjectType, StringComparison.OrdinalIgnoreCase));
                    if (pt != null) row.ProjectTypeID = pt.ProjectTypeID;
                    else row.Errors.Add($"Project Type '{row.ProjectType}' not found in lookups.");
                }

                // Resolve Participant Type
                if (!string.IsNullOrWhiteSpace(row.ParticipantType))
                {
                    var pt = participantTypes.FirstOrDefault(t => t.TypeName.Equals(row.ParticipantType, StringComparison.OrdinalIgnoreCase));
                    if (pt != null) row.ParticipantTypeID = pt.ParticipantTypeID;
                    else row.Errors.Add($"Participant Type '{row.ParticipantType}' not found in lookups.");
                }

                // Resolve Country
                if (!string.IsNullOrWhiteSpace(row.Country))
                {
                    var c = countries.FirstOrDefault(x => x.CountryName.Equals(row.Country, StringComparison.OrdinalIgnoreCase));
                    if (c != null)
                    {
                        row.CountryID = c.CountryID;
                        // Resolve City under Country
                        if (!string.IsNullOrWhiteSpace(row.City))
                        {
                            var ct = cities.FirstOrDefault(x => x.CountryID == c.CountryID && x.CityName.Equals(row.City, StringComparison.OrdinalIgnoreCase));
                            if (ct != null) row.CityID = ct.CityID;
                            else row.Warnings.Add($"City '{row.City}' not matched under country {row.Country}. Double-check spelling (e.g. Thies vs Thiès).");
                        }
                    }
                    else
                    {
                        row.Errors.Add($"Country '{row.Country}' not found in database.");
                    }
                }

                // Resolve Ship
                if (!string.IsNullOrWhiteSpace(row.Ship))
                {
                    var s = ships.FirstOrDefault(x => x.ShipName.Equals(row.Ship, StringComparison.OrdinalIgnoreCase));
                    if (s != null) row.ShipID = s.ShipID;
                    else row.Errors.Add($"Ship '{row.Ship}' not found in database.");
                }

                // Resolve Finance Codes
                if (!string.IsNullOrWhiteSpace(row.FinanceLocation))
                {
                    var fc = financeCodes.FirstOrDefault(f => f.CodeType == "Location" && f.Code.Equals(row.FinanceLocation, StringComparison.OrdinalIgnoreCase));
                    if (fc != null) row.FinanceLocationID = fc.FinanceCodeID;
                    else row.Warnings.Add($"Finance Location Code '{row.FinanceLocation}' not matched.");
                }
                if (!string.IsNullOrWhiteSpace(row.FinanceProgram))
                {
                    var fc = financeCodes.FirstOrDefault(f => f.CodeType == "Program" && f.Code.Equals(row.FinanceProgram, StringComparison.OrdinalIgnoreCase));
                    if (fc != null) row.FinanceProgramID = fc.FinanceCodeID;
                    else row.Warnings.Add($"Finance Program Code '{row.FinanceProgram}' not matched.");
                }
                if (!string.IsNullOrWhiteSpace(row.FinancePurpose))
                {
                    var fc = financeCodes.FirstOrDefault(f => f.CodeType == "Purpose" && f.Code.Equals(row.FinancePurpose, StringComparison.OrdinalIgnoreCase));
                    if (fc != null) row.FinancePurposeID = fc.FinanceCodeID;
                    else row.Warnings.Add($"Finance Purpose Code '{row.FinancePurpose}' not matched.");
                }

                // Parse Dates
                if (DateTime.TryParse(row.StartDateStr, out var sd))
                {
                    // OK
                }
                else
                {
                    row.Errors.Add($"Invalid Start Date: '{row.StartDateStr}'.");
                }

                if (DateTime.TryParse(row.EndDateStr, out var ed))
                {
                    // OK
                }
                else
                {
                    row.Errors.Add($"Invalid End Date: '{row.EndDateStr}'.");
                }

                if (row.Errors.Count > 0) row.IsValid = false;
                list.Add(row);
            }

            return Ok(list);
        }

        // POST: api/import/projects/commit
        [HttpPost("projects/commit")]
        public async Task<IActionResult> CommitProjects(List<ProjectImportRow> rows)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int created = 0;
                int updated = 0;

                foreach (var row in rows)
                {
                    if (row.Errors.Count > 0) continue; // Skip invalid records

                    var sd = DateTime.Parse(row.StartDateStr);
                    var ed = DateTime.Parse(row.EndDateStr);
                    int.TryParse(row.InstructionDaysStr, out int days);

                    var existing = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID.ToLower() == row.ProjectID.ToLower());
                    if (existing != null)
                    {
                        // Update
                        existing.ProjectTitle = row.ProjectTitle;
                        existing.FrenchProjectName = row.FrenchProjectName;
                        existing.ProgramID = row.ProgramID;
                        existing.ProjectTypeID = row.ProjectTypeID;
                        existing.ParticipantTypeID = row.ParticipantTypeID;
                        existing.StartDate = sd;
                        existing.EndDate = ed;
                        existing.InstructionDays = days;
                        existing.ShipID = row.ShipID;
                        existing.PreShipPost = row.PreShipPost;
                        existing.CountryID = row.CountryID;
                        existing.CityID = row.CityID;
                        existing.Venue = row.Venue;
                        existing.ProjectComments = row.ProjectComments;
                        existing.FinanceLocationID = row.FinanceLocationID;
                        existing.FinanceProgramID = row.FinanceProgramID;
                        existing.FinancePurposeID = row.FinancePurposeID;
                        _context.Entry(existing).State = EntityState.Modified;
                        updated++;
                    }
                    else
                    {
                        // Create new
                        var proj = new Project
                        {
                            ProjectID = row.ProjectID,
                            ProjectTitle = row.ProjectTitle,
                            FrenchProjectName = row.FrenchProjectName,
                            ProgramID = row.ProgramID,
                            ProjectTypeID = row.ProjectTypeID,
                            ParticipantTypeID = row.ParticipantTypeID,
                            StartDate = sd,
                            EndDate = ed,
                            InstructionDays = days,
                            ShipID = row.ShipID,
                            PreShipPost = row.PreShipPost,
                            CountryID = row.CountryID,
                            CityID = row.CityID,
                            Venue = row.Venue,
                            ProjectComments = row.ProjectComments,
                            FinanceLocationID = row.FinanceLocationID,
                            FinanceProgramID = row.FinanceProgramID,
                            FinancePurposeID = row.FinancePurposeID
                        };
                        _context.Projects.Add(proj);
                        created++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Created = created, Updated = updated });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Transaction rolled back: {ex.Message}");
            }
        }

        // POST: api/import/participants/validate
        [HttpPost("participants/validate")]
        public async Task<IActionResult> ValidateParticipants(ImportRequest request)
        {
            var csv = request.CsvData ?? string.Empty;
            if (csv.StartsWith("\uFEFF"))
            {
                csv = csv.Substring(1);
            }

            var rows = ParseCsv(csv);
            if (rows.Count == 0) return BadRequest("Empty CSV or invalid formatting.");

            var headers = rows[0].Select(h => h.Trim().ToLower()).ToList();

            // Validate required headers
            var missingHeaders = new List<string>();
            int idxFirst = headers.IndexOf("first name");
            int idxLast = headers.IndexOf("last name");

            if (idxFirst < 0) missingHeaders.Add("First Name");
            if (idxLast < 0) missingHeaders.Add("Last Name");

            if (missingHeaders.Any())
            {
                return BadRequest($"Invalid CSV structure. Missing required column header(s): {string.Join(", ", missingHeaders)}. Verify spelling matches exactly.");
            }

            int idxPartId = headers.IndexOf("participant id");
            int idxTitle = headers.IndexOf("title");
            int idxGender = headers.IndexOf("gender");
            int idxInst = headers.IndexOf("institution / facility");
            int idxPhone = headers.IndexOf("mobile phone");
            int idxEmail = headers.IndexOf("email");
            int idxProfession = headers.IndexOf("profession title");
            int idxRegion = headers.IndexOf("region");
            int idxCountry = headers.IndexOf("country");
            int idxComments = headers.IndexOf("comments");

            var countries = await _context.Countries.ToListAsync();
            var cities = await _context.Cities.ToListAsync();
            var institutions = await _context.Institutions.ToListAsync();
            var existingPartIds = await _context.Participants.Select(p => p.ParticipantID.ToLower()).ToListAsync();

            var list = new List<ParticipantImportRow>();

            for (int i = 1; i < rows.Count; i++)
            {
                var rowData = rows[i];
                if (rowData.Count == 0 || (rowData.Count == 1 && string.IsNullOrWhiteSpace(rowData[0]))) continue;

                var row = new ParticipantImportRow
                {
                    Index = i,
                    ParticipantID = GetVal(rowData, idxPartId),
                    Title = GetVal(rowData, idxTitle),
                    FirstName = GetVal(rowData, idxFirst),
                    LastName = GetVal(rowData, idxLast),
                    Gender = GetVal(rowData, idxGender),
                    Institution = GetVal(rowData, idxInst),
                    MobilePhone = GetVal(rowData, idxPhone),
                    Email = GetVal(rowData, idxEmail),
                    ProfessionTitle = GetVal(rowData, idxProfession),
                    Region = GetVal(rowData, idxRegion),
                    Country = GetVal(rowData, idxCountry),
                    Comments = GetVal(rowData, idxComments)
                };

                // Validate Name
                if (string.IsNullOrWhiteSpace(row.FirstName) || string.IsNullOrWhiteSpace(row.LastName))
                {
                    row.Errors.Add("First Name and Last Name are required.");
                }

                // Check ParticipantID
                if (string.IsNullOrWhiteSpace(row.ParticipantID))
                {
                    // Will auto-generate on commit
                }
                else if (existingPartIds.Contains(row.ParticipantID.ToLower()))
                {
                    row.IsDuplicate = true;
                    row.Warnings.Add($"Participant ID '{row.ParticipantID}' already exists. Importing will update their details.");
                }

                // Resolve Country & Region
                if (!string.IsNullOrWhiteSpace(row.Country))
                {
                    var c = countries.FirstOrDefault(x => x.CountryName.Equals(row.Country, StringComparison.OrdinalIgnoreCase));
                    if (c != null)
                    {
                        row.CountryID = c.CountryID;
                        if (!string.IsNullOrWhiteSpace(row.Region))
                        {
                            var ct = cities.FirstOrDefault(x => x.CountryID == c.CountryID && x.CityName.Equals(row.Region, StringComparison.OrdinalIgnoreCase));
                            if (ct != null) row.CityID = ct.CityID;
                            else row.Warnings.Add($"Region/City '{row.Region}' not matched in our databases for Country {row.Country}.");
                        }
                    }
                    else
                    {
                        row.Warnings.Add($"Country '{row.Country}' not found in lookups.");
                    }
                }

                // Resolve Institution
                if (!string.IsNullOrWhiteSpace(row.Institution))
                {
                    var inst = institutions.FirstOrDefault(x => x.InstitutionName.Equals(row.Institution, StringComparison.OrdinalIgnoreCase));
                    if (inst != null) row.InstitutionID = inst.InstitutionID;
                    else row.Warnings.Add($"Institution '{row.Institution}' not matched. A new one will be registered on import.");
                }

                if (row.Errors.Count > 0) row.IsValid = false;
                list.Add(row);
            }

            return Ok(list);
        }

        // POST: api/import/participants/commit
        [HttpPost("participants/commit")]
        public async Task<IActionResult> CommitParticipants(List<ParticipantImportRow> rows)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int created = 0;
                int updated = 0;

                foreach (var row in rows)
                {
                    if (row.Errors.Count > 0) continue;

                    // Dynamically register missing institutions
                    if (row.InstitutionID == null && !string.IsNullOrWhiteSpace(row.Institution))
                    {
                        var existInst = await _context.Institutions.FirstOrDefaultAsync(i => i.InstitutionName == row.Institution);
                        if (existInst != null)
                        {
                            row.InstitutionID = existInst.InstitutionID;
                        }
                        else
                        {
                            var newInst = new Institution
                            {
                                InstitutionName = row.Institution,
                                CountryID = row.CountryID
                            };
                            _context.Institutions.Add(newInst);
                            await _context.SaveChangesAsync();
                            row.InstitutionID = newInst.InstitutionID;
                        }
                    }

                    Participant? existing = null;
                    if (!string.IsNullOrWhiteSpace(row.ParticipantID))
                    {
                        existing = await _context.Participants.FirstOrDefaultAsync(p => p.ParticipantID.ToLower() == row.ParticipantID.ToLower());
                    }

                    if (existing != null)
                    {
                        existing.Title = row.Title;
                        existing.FirstName = row.FirstName;
                        existing.LastName = row.LastName;
                        existing.Gender = row.Gender;
                        existing.InstitutionID = row.InstitutionID;
                        existing.MobilePhone = row.MobilePhone;
                        existing.Email = row.Email;
                        existing.ProfessionTitle = row.ProfessionTitle;
                        existing.CityID = row.CityID;
                        existing.CountryID = row.CountryID;
                        existing.Comments = row.Comments;
                        _context.Entry(existing).State = EntityState.Modified;
                        updated++;
                    }
                    else
                    {
                        var part = new Participant
                        {
                            ParticipantID = string.IsNullOrWhiteSpace(row.ParticipantID) ? $"PAR-{new Random().Next(10000, 99999)}" : row.ParticipantID,
                            Title = row.Title,
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            Gender = row.Gender,
                            InstitutionID = row.InstitutionID,
                            MobilePhone = row.MobilePhone,
                            Email = row.Email,
                            ProfessionTitle = row.ProfessionTitle,
                            CityID = row.CityID,
                            CountryID = row.CountryID,
                            Comments = row.Comments
                        };
                        _context.Participants.Add(part);
                        created++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Created = created, Updated = updated });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Transaction rolled back: {ex.Message}");
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private string GetVal(List<string> row, int index)
        {
            if (index < 0 || index >= row.Count) return string.Empty;
            return row[index]?.Trim() ?? string.Empty;
        }

        private List<List<string>> ParseCsv(string csvText)
        {
            var result = new List<List<string>>();
            if (string.IsNullOrEmpty(csvText)) return result;

            // Detect delimiter dynamically
            char delimiter = ',';
            using (var reader = new StringReader(csvText))
            {
                string? firstLine = reader.ReadLine();
                if (firstLine != null)
                {
                    int commaCount = firstLine.Count(c => c == ',');
                    int semicolonCount = firstLine.Count(c => c == ';');
                    if (semicolonCount > commaCount)
                    {
                        delimiter = ';';
                    }
                }
            }

            using (var reader = new StringReader(csvText))
            {
                string? line;
                string escapedDelimiter = Regex.Escape(delimiter.ToString());
                string pattern = $"(?<=^|{escapedDelimiter})(\"(?:[^\"]|\"\")*\"|[^{escapedDelimiter}]*)";
                
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var row = new List<string>();
                    var matches = Regex.Matches(line, pattern);
                    foreach (Match m in matches)
                    {
                        var value = m.Value;
                        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
                        {
                            value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
                        }
                        row.Add(value);
                    }
                    result.Add(row);
                }
            }
            return result;
        }

    }
}
