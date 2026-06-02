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
    public class ParticipantsController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public ParticipantsController(ProjectTrackingContext context)
        {
            _context = context;
        }

        // GET: api/participants
        [HttpGet]
        public async Task<IActionResult> GetParticipants(
            [FromQuery] string? search,
            [FromQuery] int? countryId,
            [FromQuery] int? institutionId,
            [FromQuery] string? profession,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Participants
                .Include(p => p.Country)
                .Include(p => p.City)
                .Include(p => p.Institution)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => 
                    p.FirstName.ToLower().Contains(lowerSearch) ||
                    p.LastName.ToLower().Contains(lowerSearch) ||
                    p.ParticipantID.ToLower().Contains(lowerSearch) ||
                    (p.Email != null && p.Email.ToLower().Contains(lowerSearch)) ||
                    (p.ProfessionTitle != null && p.ProfessionTitle.ToLower().Contains(lowerSearch))
                );
            }

            if (countryId.HasValue) query = query.Where(p => p.CountryID == countryId.Value);
            if (institutionId.HasValue) query = query.Where(p => p.InstitutionID == institutionId.Value);
            if (!string.IsNullOrWhiteSpace(profession))
            {
                query = query.Where(p => p.ProfessionTitle.ToLower().Contains(profession.ToLower()));
            }

            var totalCount = await query.CountAsync();
            var participants = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = participants.Select(p => new
            {
                p.ParticipantIDNumber,
                p.ParticipantID,
                p.Title,
                p.FirstName,
                p.LastName,
                p.Gender,
                p.MobilePhone,
                p.Email,
                p.ProfessionTitle,
                CountryName = p.Country?.CountryName,
                RegionName = p.City?.CityName,
                InstitutionName = p.Institution?.InstitutionName
            });

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = result
            });
        }

        // GET: api/participants/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetParticipant(int id)
        {
            var participant = await _context.Participants
                .Include(p => p.Country)
                .Include(p => p.City)
                .Include(p => p.Institution)
                .Include(p => p.AttendedProjects).ThenInclude(ap => ap.Project)
                .Include(p => p.AttendedProjects).ThenInclude(ap => ap.OutputType)
                .FirstOrDefaultAsync(p => p.ParticipantIDNumber == id);

            if (participant == null)
            {
                return NotFound($"Participant with ID {id} not found.");
            }

            // Calculate aggregations
            var totalTrainingHours = participant.AttendedProjects
                .Where(ap => ap.OutputType?.TypeName.Contains("Hours", StringComparison.OrdinalIgnoreCase) == true)
                .Sum(ap => ap.OutputAmount);

            var totalProjectsAttended = participant.AttendedProjects
                .Select(ap => ap.ProjectIDNumber)
                .Distinct()
                .Count();

            var projectHistory = participant.AttendedProjects.Select(ap => new
            {
                ap.ProjectParticipantOutputID,
                ap.ProjectIDNumber,
                ProjectCode = ap.Project?.ProjectID,
                ProjectTitle = ap.Project?.ProjectTitle,
                ap.ReportingDate,
                ap.ReportingPeriod,
                ap.OutputAmount,
                OutputType = ap.OutputType?.TypeName,
                ap.Comments
            }).ToList();

            return Ok(new
            {
                participant.ParticipantIDNumber,
                participant.ParticipantID,
                participant.Title,
                participant.FirstName,
                participant.LastName,
                participant.Gender,
                participant.MobilePhone,
                participant.Email,
                participant.ProfessionTitle,
                participant.Comments,
                
                // References details
                participant.CountryID,
                CountryName = participant.Country?.CountryName,
                participant.CityID,
                RegionName = participant.City?.CityName,
                participant.InstitutionID,
                InstitutionName = participant.Institution?.InstitutionName,

                // Aggregated stats
                TotalTrainingHours = totalTrainingHours,
                TotalProjectsAttended = totalProjectsAttended,
                
                // Details history
                AttendedProjects = projectHistory
            });
        }

        // POST: api/participants
        [HttpPost]
        public async Task<IActionResult> CreateParticipant(Participant participant)
        {
            if (string.IsNullOrWhiteSpace(participant.ParticipantID))
            {
                participant.ParticipantID = $"PAR-{new Random().Next(10000, 99999)}";
            }

            var duplicate = await _context.Participants.AnyAsync(p => p.ParticipantID == participant.ParticipantID);
            if (duplicate)
            {
                return BadRequest($"Participant ID '{participant.ParticipantID}' already exists.");
            }

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipant), new { id = participant.ParticipantIDNumber }, participant);
        }

        // PUT: api/participants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParticipant(int id, Participant participant)
        {
            if (id != participant.ParticipantIDNumber)
            {
                return BadRequest("Participant ID mismatch.");
            }

            var existingParticipant = await _context.Participants.FindAsync(id);
            if (existingParticipant == null)
            {
                return NotFound($"Participant with ID {id} not found.");
            }

            if (existingParticipant.ParticipantID != participant.ParticipantID)
            {
                var duplicate = await _context.Participants.AnyAsync(p => p.ParticipantID == participant.ParticipantID && p.ParticipantIDNumber != id);
                if (duplicate)
                {
                    return BadRequest($"Participant ID '{participant.ParticipantID}' is already in use.");
                }
            }

            existingParticipant.ParticipantID = participant.ParticipantID;
            existingParticipant.Title = participant.Title;
            existingParticipant.FirstName = participant.FirstName;
            existingParticipant.LastName = participant.LastName;
            existingParticipant.Gender = participant.Gender;
            existingParticipant.InstitutionID = participant.InstitutionID;
            existingParticipant.MobilePhone = participant.MobilePhone;
            existingParticipant.Email = participant.Email;
            existingParticipant.ProfessionTitle = participant.ProfessionTitle;
            existingParticipant.CityID = participant.CityID;
            existingParticipant.CountryID = participant.CountryID;
            existingParticipant.Comments = participant.Comments;

            _context.Entry(existingParticipant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ParticipantExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/participants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }

            // Verify if there are any project attendance outputs
            var hasAttendance = await _context.ProjectParticipantOutputs.AnyAsync(po => po.ParticipantIDNumber == id);
            if (hasAttendance)
            {
                return BadRequest("Cannot delete participant because they have attendance records.");
            }

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ParticipantExists(int id)
        {
            return await _context.Participants.AnyAsync(e => e.ParticipantIDNumber == id);
        }
    }
}
