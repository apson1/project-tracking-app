using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Models;

namespace Backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(ProjectTrackingContext context)
        {
            // Only seed if there are no projects in the DB
            if (context.Projects.Any())
            {
                return;
            }

            Console.WriteLine("Fetching Reference Data...");

            // --- Reference Data (Already Seeded by EF Migration) ---
            var countries = context.Countries.ToList();
            var cities = context.Cities.ToList();
            var ships = context.Ships.ToList();
            var programs = context.ProgramNames.ToList();
            var projectTypes = context.ProjectTypes.ToList();
            var participantTypes = context.ParticipantTypes.ToList();
            var outputTypes = context.OutputTypes.ToList();
            var financeCodes = context.FinanceCodes.ToList();

            if (!countries.Any() || !financeCodes.Any()) 
            {
                Console.WriteLine("Reference data is missing, make sure migrations have run.");
                return;
            }

            Console.WriteLine("Seeding Projects and Participants...");

            // --- Projects ---
            var random = new Random(12345);
            var projects = new List<Project>();

            for (int i = 1; i <= 20; i++)
            {
                var prog = programs[random.Next(programs.Count)];
                var startDate = DateTime.UtcNow.AddMonths(-random.Next(1, 12)).AddDays(-random.Next(1, 30));
                
                var proj = new Project
                {
                    ProjectID = $"PRJ-2024-{i:D3}",
                    ProjectTitle = $"{prog.Name} Initiative {i}",
                    ProgramID = prog.ProgramID,
                    ProjectTypeID = projectTypes[random.Next(projectTypes.Count)].ProjectTypeID,
                    StartDate = startDate,
                    EndDate = startDate.AddDays(random.Next(10, 60)),
                    ShipID = ships[random.Next(ships.Count)].ShipID,
                    CountryID = countries[random.Next(countries.Count)].CountryID,
                    CityID = cities[random.Next(cities.Count)].CityID,
                    FinanceLocationID = financeCodes[0].FinanceCodeID,
                    FinanceProgramID = financeCodes[1].FinanceCodeID,
                    MasterStatsCategory = "Field Operations",
                    MasterStatsCategoryGroup = "Direct Services"
                };
                projects.Add(proj);
            }
            context.Projects.AddRange(projects);
            context.SaveChanges();

            // --- Participants ---
            var participants = new List<Participant>();
            for (int i = 1; i <= 50; i++)
            {
                var pType = participantTypes[random.Next(participantTypes.Count)];
                var part = new Participant
                {
                    ParticipantID = $"PAR-00{i:D3}",
                    FirstName = $"TestFirstName{i}",
                    LastName = $"TestLastName{i}",
                    Title = i % 2 == 0 ? "Dr." : "Mr.",
                    ProfessionTitle = pType.TypeName,
                    Email = $"participant{i}@example.com",
                    CountryID = countries[random.Next(countries.Count)].CountryID
                };
                participants.Add(part);
            }
            context.Participants.AddRange(participants);
            context.SaveChanges();

            Console.WriteLine("Seeding Outputs...");

            // --- Outputs ---
            var patientOutputsList = new List<PatientOutput>();
            var participantOutputsList = new List<ProjectParticipantOutput>();

            foreach (var proj in projects)
            {
                // Patient Outputs (Surgeries, Consults)
                int outputCount = random.Next(5, 20);
                for (int o = 0; o < outputCount; o++)
                {
                    patientOutputsList.Add(new PatientOutput
                    {
                        ProjectIDNumber = proj.ProjectIDNumber,
                        PatientID = $"PAT-{proj.ProjectIDNumber}-{o:D3}",
                        AgeGroup = random.Next(0, 2) == 0 ? "Child" : "Adult",
                        Sex = random.Next(0, 2) == 0 ? "M" : "F",
                        ReportingDate = proj.StartDate.AddDays(random.Next(1, 10)),
                        OutputAmount = random.Next(1, 3),
                        OutputTypeID = outputTypes[random.Next(0, 2)].OutputTypeID, // The patient ones
                        CountryID = proj.CountryID
                    });
                }

                // Participant Outputs (Training hours)
                int partCount = random.Next(2, 6);
                for (int p = 0; p < partCount; p++)
                {
                    participantOutputsList.Add(new ProjectParticipantOutput
                    {
                        ProjectIDNumber = proj.ProjectIDNumber,
                        ParticipantIDNumber = participants[random.Next(participants.Count)].ParticipantIDNumber,
                        ReportingDate = proj.StartDate.AddDays(random.Next(1, 10)),
                        OutputAmount = random.Next(4, 40), // hours
                        OutputTypeID = outputTypes[random.Next(2, 4)].OutputTypeID // The participant ones
                    });
                }
            }

            context.PatientOutputs.AddRange(patientOutputsList);
            context.ProjectParticipantOutputs.AddRange(participantOutputsList);
            
            context.SaveChanges();

            Console.WriteLine("Database Seeding Complete.");
        }
    }
}
