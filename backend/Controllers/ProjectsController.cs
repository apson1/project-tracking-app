using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public ProjectsController(ProjectTrackingContext context)
        {
            _context = context;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<IActionResult> GetProjects(
            [FromQuery] string? search,
            [FromQuery] int? countryId,
            [FromQuery] int? cityId,
            [FromQuery] int? programId,
            [FromQuery] int? projectTypeId,
            [FromQuery] int? shipId,
            [FromQuery] int? year,
            [FromQuery] string? status, // "Upcoming", "Active", "Completed"
            [FromQuery] bool? missingFinanceCodes,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Projects
                .Include(p => p.Country)
                .Include(p => p.City)
                .Include(p => p.Program)
                .Include(p => p.ProjectType)
                .Include(p => p.Ship)
                .AsQueryable();

            // Apply Search Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => 
                    p.ProjectTitle.ToLower().Contains(lowerSearch) ||
                    p.ProjectID.ToLower().Contains(lowerSearch) ||
                    p.FrenchProjectName.ToLower().Contains(lowerSearch) ||
                    (p.Venue != null && p.Venue.ToLower().Contains(lowerSearch))
                );
            }

            // Apply Select Filters
            if (countryId.HasValue) query = query.Where(p => p.CountryID == countryId.Value);
            if (cityId.HasValue) query = query.Where(p => p.CityID == cityId.Value);
            if (programId.HasValue) query = query.Where(p => p.ProgramID == programId.Value);
            if (projectTypeId.HasValue) query = query.Where(p => p.ProjectTypeID == projectTypeId.Value);
            if (shipId.HasValue) query = query.Where(p => p.ShipID == shipId.Value);
            
            if (missingFinanceCodes.HasValue && missingFinanceCodes.Value)
            {
                query = query.Where(p => p.FinanceLocationID == null || p.FinanceProgramID == null || p.FinancePurposeID == null);
            }

            if (year.HasValue)
            {
                query = query.Where(p => p.StartDate.Year == year.Value || p.EndDate.Year == year.Value);
            }

            // Filter by Calculated Status
            var now = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Upcoming", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.StartDate > now);
                }
                else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.StartDate <= now && p.EndDate >= now);
                }
                else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.EndDate < now);
                }
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map Status onto responses
            var result = projects.Select(p => new
            {
                p.ProjectIDNumber,
                p.ProjectID,
                p.ProjectTitle,
                p.FrenchProjectName,
                p.StartDate,
                p.EndDate,
                p.InstructionDays,
                p.Venue,
                p.PreShipPost,
                CountryName = p.Country?.CountryName,
                CityName = p.City?.CityName,
                ProgramName = p.Program?.Name,
                ProjectTypeName = p.ProjectType?.TypeName,
                ShipName = p.Ship?.ShipName,
                Status = p.StartDate > now ? "Upcoming" : (p.EndDate < now ? "Completed" : "Active")
            });

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = result
            });
        }

        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Country)
                .Include(p => p.City)
                .Include(p => p.Program)
                .Include(p => p.ProjectType)
                .Include(p => p.ParticipantType)
                .Include(p => p.Ship)
                .Include(p => p.FinanceLocation)
                .Include(p => p.FinanceProgram)
                .Include(p => p.FinancePurpose)
                // Tabs lists
                .Include(p => p.ParticipantOutputs).ThenInclude(po => po.Participant)
                .Include(p => p.ParticipantOutputs).ThenInclude(po => po.OutputType)
                .Include(p => p.ParticipantOutputs).ThenInclude(po => po.SpecificOutputType)
                .Include(p => p.PatientOutputs).ThenInclude(po => po.OutputType)
                .Include(p => p.PatientOutputs).ThenInclude(po => po.Country)
                .Include(p => p.PatientOutputs).ThenInclude(po => po.City)
                .Include(p => p.DmsInfrastructureOutputs).ThenInclude(io => io.OutputType)
                .Include(p => p.DmsInfrastructureOutputs).ThenInclude(io => io.SpecificOutputType)
                .Include(p => p.PlannedOutputs).ThenInclude(po => po.PlannedOutputType)
                .FirstOrDefaultAsync(p => p.ProjectIDNumber == id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            var now = DateTime.UtcNow;
            var status = project.StartDate > now ? "Upcoming" : (project.EndDate < now ? "Completed" : "Active");

            // Calculate planned vs actual totals
            var plannedActual = project.PlannedOutputs.Select(planned => {
                decimal actualAmount = 0;
                if (planned.PlannedOutputType?.Category == "Participant")
                {
                    actualAmount = project.ParticipantOutputs
                        .Where(o => o.OutputTypeID == planned.PlannedOutputTypeID)
                        .Sum(o => o.OutputAmount);
                }
                else if (planned.PlannedOutputType?.Category == "Patient")
                {
                    actualAmount = project.PatientOutputs
                        .Where(o => o.OutputTypeID == planned.PlannedOutputTypeID)
                        .Sum(o => o.OutputAmount);
                }
                else if (planned.PlannedOutputType?.Category == "DMS")
                {
                    actualAmount = project.DmsInfrastructureOutputs
                        .Where(o => o.OutputTypeID == planned.PlannedOutputTypeID)
                        .Sum(o => o.OutputAmount);
                }

                return new
                {
                    OutputTypeId = planned.PlannedOutputTypeID,
                    TypeName = planned.PlannedOutputType?.TypeName,
                    Category = planned.PlannedOutputType?.Category,
                    Planned = planned.PlannedAmount,
                    Actual = actualAmount,
                    Percentage = planned.PlannedAmount > 0 ? (actualAmount / planned.PlannedAmount) * 100 : 0
                };
            }).ToList();

            return Ok(new
            {
                project.ProjectIDNumber,
                project.ProjectID,
                project.ProjectTitle,
                project.FrenchProjectName,
                project.StartDate,
                project.EndDate,
                project.InstructionDays,
                project.PreShipPost,
                project.Venue,
                project.ProjectComments,
                project.MasterStatsCategoryGroup,
                project.MasterStatsCategory,
                
                // Dropdowns Details
                project.CountryID,
                CountryName = project.Country?.CountryName,
                project.CityID,
                CityName = project.City?.CityName,
                project.ProgramID,
                ProgramName = project.Program?.Name,
                project.ProjectTypeID,
                ProjectTypeName = project.ProjectType?.TypeName,
                project.ParticipantTypeID,
                ParticipantTypeName = project.ParticipantType?.TypeName,
                project.ShipID,
                ShipName = project.Ship?.ShipName,
                
                // Finance
                FinanceLocation = project.FinanceLocation != null ? new { project.FinanceLocation.FinanceCodeID, project.FinanceLocation.Code, project.FinanceLocation.Description } : null,
                FinanceProgram = project.FinanceProgram != null ? new { project.FinanceProgram.FinanceCodeID, project.FinanceProgram.Code, project.FinanceProgram.Description } : null,
                FinancePurpose = project.FinancePurpose != null ? new { project.FinancePurpose.FinanceCodeID, project.FinancePurpose.Code, project.FinancePurpose.Description } : null,
                
                Status = status,

                // Outputs summaries / tabs list
                Participants = project.ParticipantOutputs.Select(po => new
                {
                    po.ProjectParticipantOutputID,
                    po.ParticipantIDNumber,
                    ParticipantCode = po.Participant?.ParticipantID,
                    FullName = $"{po.Participant?.FirstName} {po.Participant?.LastName}",
                    po.ReportingDate,
                    po.ReportingPeriod,
                    po.OutputAmount,
                    OutputType = po.OutputType?.TypeName,
                    SpecificOutputType = po.SpecificOutputType?.SpecificTypeName,
                    po.Comments
                }).ToList(),

                Patients = project.PatientOutputs.Select(po => new
                {
                    po.PatientOutputID,
                    po.PatientID,
                    po.Sex,
                    po.AgeGroup,
                    ResidenceCountry = po.Country?.CountryName,
                    ResidenceRegion = po.City?.CityName,
                    po.ReportingDate,
                    po.OutputAmount,
                    OutputType = po.OutputType?.TypeName,
                    po.Comments
                }).ToList(),

                DmsOutputs = project.DmsInfrastructureOutputs.Select(do_ => new
                {
                    do_.DmsInfrastructureOutputID,
                    do_.ReportingDate,
                    do_.ReportingPeriod,
                    do_.OutputAmount,
                    OutputType = do_.OutputType?.TypeName,
                    SpecificOutputType = do_.SpecificOutputType?.SpecificTypeName,
                    do_.AdultChild,
                    do_.Gender,
                    do_.ProgramTypeOfOutput,
                    do_.Comments
                }).ToList(),

                PlannedActualStats = plannedActual,
                
                PlannedRaw = project.PlannedOutputs.Select(po => new
                {
                    po.PlannedOutputID,
                    po.PlannedOutputTypeID,
                    OutputType = po.PlannedOutputType?.TypeName,
                    Category = po.PlannedOutputType?.Category,
                    po.PlannedAmount,
                    po.ReportingPeriod,
                    po.PlannedDateYear,
                    po.Comments
                }).ToList()
            });
        }

        // POST: api/projects
        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.ProjectID))
            {
                // Generate a custom ID if not provided
                project.ProjectID = $"PRJ-{DateTime.UtcNow.Year}-{new Random().Next(100, 999)}";
            }

            var duplicate = await _context.Projects.AnyAsync(p => p.ProjectID == project.ProjectID);
            if (duplicate)
            {
                return BadRequest($"Project ID '{project.ProjectID}' already exists.");
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.ProjectIDNumber }, project);
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, Project project)
        {
            if (id != project.ProjectIDNumber)
            {
                return BadRequest("Project ID mismatch.");
            }

            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // Check unique ProjectID constraints if changed
            if (existingProject.ProjectID != project.ProjectID)
            {
                var duplicate = await _context.Projects.AnyAsync(p => p.ProjectID == project.ProjectID && p.ProjectIDNumber != id);
                if (duplicate)
                {
                    return BadRequest($"Project ID '{project.ProjectID}' is already in use.");
                }
            }

            // Update fields
            existingProject.ProjectID = project.ProjectID;
            existingProject.ProjectTitle = project.ProjectTitle;
            existingProject.FrenchProjectName = project.FrenchProjectName;
            existingProject.ProgramID = project.ProgramID;
            existingProject.MasterStatsCategoryGroup = project.MasterStatsCategoryGroup;
            existingProject.MasterStatsCategory = project.MasterStatsCategory;
            existingProject.ProjectTypeID = project.ProjectTypeID;
            existingProject.ParticipantTypeID = project.ParticipantTypeID;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.InstructionDays = project.InstructionDays;
            existingProject.ShipID = project.ShipID;
            existingProject.PreShipPost = project.PreShipPost;
            existingProject.CountryID = project.CountryID;
            existingProject.CityID = project.CityID;
            existingProject.Venue = project.Venue;
            existingProject.ProjectComments = project.ProjectComments;
            existingProject.FinanceLocationID = project.FinanceLocationID;
            existingProject.FinanceProgramID = project.FinanceProgramID;
            existingProject.FinancePurposeID = project.FinancePurposeID;

            _context.Entry(existingProject).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Verify if there are any dependent participant outputs or patient outputs
            var hasParticipantOutputs = await _context.ProjectParticipantOutputs.AnyAsync(po => po.ProjectIDNumber == id);
            var hasPatientOutputs = await _context.PatientOutputs.AnyAsync(po => po.ProjectIDNumber == id);
            var hasDmsOutputs = await _context.DmsInfrastructureOutputs.AnyAsync(po => po.ProjectIDNumber == id);

            if (hasParticipantOutputs || hasPatientOutputs || hasDmsOutputs)
            {
                return BadRequest("Cannot delete project because it has associated outputs. Delete those outputs first.");
            }

            // Remove planned outputs
            var plannedOutputs = await _context.PlannedOutputs.Where(po => po.ProjectIDNumber == id).ToListAsync();
            _context.PlannedOutputs.RemoveRange(plannedOutputs);

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(e => e.ProjectIDNumber == id);
        }
    }
}
