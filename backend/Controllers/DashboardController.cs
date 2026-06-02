using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public DashboardController(ProjectTrackingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var now = DateTime.UtcNow;

            // 1. KPI Aggregations
            var totalProjects = await _context.Projects.CountAsync();
            var activeProjects = await _context.Projects.CountAsync(p => p.StartDate <= now && p.EndDate >= now);
            var totalParticipants = await _context.Participants.CountAsync();
            
            var totalPatientOutputs = await _context.PatientOutputs.SumAsync(po => po.OutputAmount);
            
            var totalTrainingHours = await _context.ProjectParticipantOutputs
                .Where(po => po.OutputType != null && (po.OutputType.TypeName.Contains("Hours") || po.OutputType.TypeName.Contains("Hours Completed")))
                .SumAsync(po => po.OutputAmount);

            // 2. Outputs by Country (Combined Patient + Participant outputs)
            var patientOutputsByCountry = await _context.PatientOutputs
                .Include(po => po.Country)
                .GroupBy(po => po.Country!.CountryName)
                .Select(g => new { Name = g.Key ?? "Unknown", Count = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            var participantOutputsByCountry = await _context.ProjectParticipantOutputs
                .Include(po => po.Project).ThenInclude(p => p!.Country)
                .GroupBy(po => po.Project!.Country!.CountryName)
                .Select(g => new { Name = g.Key ?? "Unknown", Count = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            // Merge countries dictionary
            var countryStats = new Dictionary<string, decimal>();
            foreach (var item in patientOutputsByCountry)
            {
                countryStats[item.Name] = item.Count;
            }
            foreach (var item in participantOutputsByCountry)
            {
                if (countryStats.ContainsKey(item.Name))
                    countryStats[item.Name] += item.Count;
                else
                    countryStats[item.Name] = item.Count;
            }

            var formattedCountryStats = countryStats.Select(kv => new { Country = kv.Key, Value = kv.Value }).ToList();

            // 3. Outputs by Program
            var patientOutputsByProgram = await _context.PatientOutputs
                .Include(po => po.Project).ThenInclude(p => p!.Program)
                .GroupBy(po => po.Project!.Program!.Name)
                .Select(g => new { Program = g.Key ?? "Unassigned Program", Value = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            var participantOutputsByProgram = await _context.ProjectParticipantOutputs
                .Include(po => po.Project).ThenInclude(p => p!.Program)
                .GroupBy(po => po.Project!.Program!.Name)
                .Select(g => new { Program = g.Key ?? "Unassigned Program", Value = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            var programStats = new Dictionary<string, decimal>();
            foreach (var item in patientOutputsByProgram)
            {
                programStats[item.Program] = item.Value;
            }
            foreach (var item in participantOutputsByProgram)
            {
                if (programStats.ContainsKey(item.Program))
                    programStats[item.Program] += item.Value;
                else
                    programStats[item.Program] = item.Value;
            }
            var formattedProgramStats = programStats.Select(kv => new { Program = kv.Key, Value = kv.Value }).ToList();

            // 4. Outputs by Year
            var patientByYear = await _context.PatientOutputs
                .GroupBy(po => po.ReportingDate.Year)
                .Select(g => new { Year = g.Key, Count = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            var participantByYear = await _context.ProjectParticipantOutputs
                .GroupBy(po => po.ReportingDate.Year)
                .Select(g => new { Year = g.Key, Count = g.Sum(x => x.OutputAmount) })
                .ToListAsync();

            var yearStats = new Dictionary<int, decimal>();
            foreach (var item in patientByYear) yearStats[item.Year] = item.Count;
            foreach (var item in participantByYear)
            {
                if (yearStats.ContainsKey(item.Year)) yearStats[item.Year] += item.Count;
                else yearStats[item.Year] = item.Count;
            }
            var formattedYearStats = yearStats.OrderBy(kv => kv.Key).Select(kv => new { Year = kv.Key.ToString(), Value = kv.Value }).ToList();

            // 5. Upcoming Projects
            var upcomingProjects = await _context.Projects
                .Include(p => p.Country)
                .Include(p => p.Program)
                .Where(p => p.StartDate > now)
                .OrderBy(p => p.StartDate)
                .Take(5)
                .Select(p => new
                {
                    p.ProjectIDNumber,
                    p.ProjectID,
                    p.ProjectTitle,
                    p.StartDate,
                    p.EndDate,
                    CountryName = p.Country != null ? p.Country.CountryName : "N/A",
                    ProgramName = p.Program != null ? p.Program.Name : "N/A"
                })
                .ToListAsync();

            // 6. Data Warnings & Missing Data Auditing
            var warnings = new List<object>();

            // Past projects with no outputs
            var pastProjectsWithNoOutputs = await _context.Projects
                .Where(p => p.EndDate < now)
                .Where(p => !p.ParticipantOutputs.Any() && !p.PatientOutputs.Any() && !p.DmsInfrastructureOutputs.Any())
                .Select(p => new { Id = p.ProjectIDNumber, Code = p.ProjectID, Title = p.ProjectTitle, Message = "Completed project has no output records logged." })
                .Take(5)
                .ToListAsync();
            
            foreach (var p in pastProjectsWithNoOutputs) warnings.Add(p);

            // Active / Upcoming projects missing finance codes
            var projectsMissingFinance = await _context.Projects
                .Where(p => p.FinanceLocationID == null || p.FinanceProgramID == null || p.FinancePurposeID == null)
                .Select(p => new { Id = p.ProjectIDNumber, Code = p.ProjectID, Title = p.ProjectTitle, Message = "Project is missing one or more Finance codes (Location, Program, or Purpose)." })
                .Take(5)
                .ToListAsync();
            
            foreach (var p in projectsMissingFinance) warnings.Add(p);

            // Participants missing contact details
            var participantsMissingContact = await _context.Participants
                .Where(p => (p.Email == null || p.Email == "") && (p.MobilePhone == null || p.MobilePhone == ""))
                .Select(p => new { Id = p.ParticipantIDNumber, Code = p.ParticipantID, Title = $"{p.FirstName} {p.LastName}", Message = "Participant is missing both Email and Mobile Phone details." })
                .Take(5)
                .ToListAsync();
            
            foreach (var p in participantsMissingContact) warnings.Add(p);

            return Ok(new
            {
                Kpis = new
                {
                    TotalProjects = totalProjects,
                    ActiveProjects = activeProjects,
                    TotalParticipants = totalParticipants,
                    TotalPatientOutputs = totalPatientOutputs,
                    TotalTrainingHours = totalTrainingHours
                },
                OutputsByCountry = formattedCountryStats,
                OutputsByProgram = formattedProgramStats,
                OutputsByYear = formattedYearStats,
                UpcomingProjects = upcomingProjects,
                Warnings = warnings
            });
        }
    }
}
