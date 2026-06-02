using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferencesController : ControllerBase
    {
        private readonly ProjectTrackingContext _context;

        public ReferencesController(ProjectTrackingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReferences()
        {
            var countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync();
            var ships = await _context.Ships.OrderBy(s => s.ShipName).ToListAsync();
            var programs = await _context.ProgramNames.OrderBy(p => p.Name).ToListAsync();
            var projectTypes = await _context.ProjectTypes.OrderBy(pt => pt.TypeName).ToListAsync();
            var participantTypes = await _context.ParticipantTypes.OrderBy(pt => pt.TypeName).ToListAsync();
            
            var outputTypes = await _context.OutputTypes.OrderBy(ot => ot.TypeName).ToListAsync();
            var specificOutputTypes = await _context.SpecificOutputTypes
                .Include(s => s.OutputType)
                .OrderBy(s => s.SpecificTypeName)
                .ToListAsync();

            var financeCodes = await _context.FinanceCodes.OrderBy(f => f.Code).ToListAsync();
            
            var institutions = await _context.Institutions
                .Include(i => i.Country)
                .OrderBy(i => i.InstitutionName)
                .ToListAsync();

            var cities = await _context.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.CityName)
                .ToListAsync();

            return Ok(new
            {
                Countries = countries,
                Cities = cities,
                Ships = ships,
                Programs = programs,
                ProjectTypes = projectTypes,
                ParticipantTypes = participantTypes,
                OutputTypes = outputTypes,
                SpecificOutputTypes = specificOutputTypes,
                FinanceCodes = financeCodes,
                Institutions = institutions
            });
        }

        [HttpPost("countries")]
        public async Task<ActionResult<Country>> CreateCountry(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = country.CountryID }, country);
        }

        [HttpPost("cities")]
        public async Task<ActionResult<City>> CreateCity(City city)
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = city.CityID }, city);
        }

        [HttpPost("ships")]
        public async Task<ActionResult<Ship>> CreateShip(Ship ship)
        {
            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = ship.ShipID }, ship);
        }

        [HttpPost("programs")]
        public async Task<ActionResult<ProgramName>> CreateProgram(ProgramName program)
        {
            _context.ProgramNames.Add(program);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = program.ProgramID }, program);
        }

        [HttpPost("projecttypes")]
        public async Task<ActionResult<ProjectType>> CreateProjectType(ProjectType projectType)
        {
            _context.ProjectTypes.Add(projectType);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = projectType.ProjectTypeID }, projectType);
        }

        [HttpPost("financecodes")]
        public async Task<ActionResult<FinanceCode>> CreateFinanceCode(FinanceCode financeCode)
        {
            _context.FinanceCodes.Add(financeCode);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllReferences), new { id = financeCode.FinanceCodeID }, financeCode);
        }
    }
}
