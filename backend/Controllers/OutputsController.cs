using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutputsController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public OutputsController(ProjectTrackingContext context)
        {
            _context = context;
        }

        // ==========================================
        // PARTICIPANT OUTPUTS
        // ==========================================

        [HttpPost("participant")]
        public async Task<IActionResult> CreateParticipantOutput(ProjectParticipantOutput output)
        {
            _context.ProjectParticipantOutputs.Add(output);
            await _context.SaveChangesAsync();
            return Ok(output);
        }

        [HttpPut("participant/{id}")]
        public async Task<IActionResult> UpdateParticipantOutput(int id, ProjectParticipantOutput output)
        {
            if (id != output.ProjectParticipantOutputID) return BadRequest("ID mismatch");

            _context.Entry(output).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("participant/{id}")]
        public async Task<IActionResult> DeleteParticipantOutput(int id)
        {
            var output = await _context.ProjectParticipantOutputs.FindAsync(id);
            if (output == null) return NotFound();

            _context.ProjectParticipantOutputs.Remove(output);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // PATIENT OUTPUTS
        // ==========================================

        [HttpPost("patient")]
        public async Task<IActionResult> CreatePatientOutput(PatientOutput output)
        {
            _context.PatientOutputs.Add(output);
            await _context.SaveChangesAsync();
            return Ok(output);
        }

        [HttpPut("patient/{id}")]
        public async Task<IActionResult> UpdatePatientOutput(int id, PatientOutput output)
        {
            if (id != output.PatientOutputID) return BadRequest("ID mismatch");

            _context.Entry(output).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("patient/{id}")]
        public async Task<IActionResult> DeletePatientOutput(int id)
        {
            var output = await _context.PatientOutputs.FindAsync(id);
            if (output == null) return NotFound();

            _context.PatientOutputs.Remove(output);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // DMS / INFRASTRUCTURE OUTPUTS
        // ==========================================

        [HttpPost("dms")]
        public async Task<IActionResult> CreateDmsOutput(DmsInfrastructureOutput output)
        {
            _context.DmsInfrastructureOutputs.Add(output);
            await _context.SaveChangesAsync();
            return Ok(output);
        }

        [HttpPut("dms/{id}")]
        public async Task<IActionResult> UpdateDmsOutput(int id, DmsInfrastructureOutput output)
        {
            if (id != output.DmsInfrastructureOutputID) return BadRequest("ID mismatch");

            _context.Entry(output).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("dms/{id}")]
        public async Task<IActionResult> DeleteDmsOutput(int id)
        {
            var output = await _context.DmsInfrastructureOutputs.FindAsync(id);
            if (output == null) return NotFound();

            _context.DmsInfrastructureOutputs.Remove(output);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // PLANNED OUTPUTS (TARGETS)
        // ==========================================

        [HttpPost("planned")]
        public async Task<IActionResult> CreatePlannedOutput(PlannedOutput output)
        {
            _context.PlannedOutputs.Add(output);
            await _context.SaveChangesAsync();
            return Ok(output);
        }

        [HttpPut("planned/{id}")]
        public async Task<IActionResult> UpdatePlannedOutput(int id, PlannedOutput output)
        {
            if (id != output.PlannedOutputID) return BadRequest("ID mismatch");

            _context.Entry(output).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("planned/{id}")]
        public async Task<IActionResult> DeletePlannedOutput(int id)
        {
            var output = await _context.PlannedOutputs.FindAsync(id);
            if (output == null) return NotFound();

            _context.PlannedOutputs.Remove(output);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
