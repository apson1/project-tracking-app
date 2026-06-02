import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './projects.html',
  styleUrl: './projects.css'
})
export class Projects implements OnInit {
  protected readonly Math = Math;
  projects: any[] = [];
  totalCount = 0;
  loading = false;


  // Search & Filter Parameters
  filters: any = {
    search: '',
    countryId: '',
    programId: '',
    projectTypeId: '',
    shipId: '',
    status: '',
    missingFinanceCodes: undefined,
    page: 1,
    pageSize: 10
  };

  // Lookups data
  references: any = {
    countries: [],
    cities: [],
    ships: [],
    programs: [],
    projectTypes: [],
    participantTypes: [],
    financeCodes: [],
    institutions: []
  };

  filteredCities: any[] = [];
  filteredFinanceLocations: any[] = [];
  filteredFinancePrograms: any[] = [];
  filteredFinancePurposes: any[] = [];

  // Modal Form State
  showModal = false;
  isEditing = false;
  modalTitle = 'Create Project';
  projectForm: any = {};

  constructor(
    private apiService: ApiService, 
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadReferences();
    this.route.queryParams.subscribe(params => {
      if (params['missingFinanceCodes'] === 'true') {
        this.filters.missingFinanceCodes = true;
      } else {
        this.filters.missingFinanceCodes = undefined;
      }
      this.loadProjects();
    });
  }

  loadReferences(): void {
    this.apiService.getReferences().subscribe({
      next: (refs) => {
        this.references = refs;
        // Group finance codes by type
        this.filteredFinanceLocations = refs.financeCodes.filter((f: any) => f.codeType === 'Location');
        this.filteredFinancePrograms = refs.financeCodes.filter((f: any) => f.codeType === 'Program');
        this.filteredFinancePurposes = refs.financeCodes.filter((f: any) => f.codeType === 'Purpose');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load lookup tables', err);
      }
    });
  }

  loadProjects(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.apiService.getProjects(this.filters).subscribe({
      next: (res) => {
        this.projects = res.data;
        this.totalCount = res.totalCount;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load projects list', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    this.filters.page = 1;
    this.loadProjects();
  }

  clearFilters(): void {
    this.filters = {
      search: '',
      countryId: '',
      programId: '',
      projectTypeId: '',
      shipId: '',
      status: '',
      missingFinanceCodes: undefined,
      page: 1,
      pageSize: 10
    };
    this.loadProjects();
  }

  changePage(page: number): void {
    this.filters.page = page;
    this.loadProjects();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.filters.pageSize);
  }

  // ==========================================
  // MODAL FORM METHODS
  // ==========================================
  openCreateModal(): void {
    this.isEditing = false;
    this.modalTitle = 'Create New Project';
    this.projectForm = {
      projectID: '',
      projectTitle: '',
      frenchProjectName: '',
      programID: '',
      masterStatsCategoryGroup: '',
      masterStatsCategory: '',
      projectTypeID: '',
      participantTypeID: '',
      startDate: '',
      endDate: '',
      instructionDays: 0,
      shipID: '',
      preShipPost: '',
      countryID: '',
      cityID: '',
      venue: '',
      projectComments: '',
      financeLocationID: '',
      financeProgramID: '',
      financePurposeID: ''
    };
    this.filteredCities = [];
    this.showModal = true;
  }

  openEditModal(projectSummary: any): void {
    this.isEditing = true;
    this.modalTitle = 'Edit Project Details';
    this.loading = true;

    // Load full details for editing
    this.apiService.getProject(projectSummary.projectIDNumber).subscribe({
      next: (fullProj) => {
        // Format dates for html input type="date"
        const sd = fullProj.startDate ? fullProj.startDate.split('T')[0] : '';
        const ed = fullProj.endDate ? fullProj.endDate.split('T')[0] : '';

        this.projectForm = {
          projectIDNumber: fullProj.projectIDNumber,
          projectID: fullProj.projectID,
          projectTitle: fullProj.projectTitle,
          frenchProjectName: fullProj.frenchProjectName,
          programID: fullProj.programID || '',
          masterStatsCategoryGroup: fullProj.masterStatsCategoryGroup || '',
          masterStatsCategory: fullProj.masterStatsCategory || '',
          projectTypeID: fullProj.projectTypeID || '',
          participantTypeID: fullProj.participantTypeID || '',
          startDate: sd,
          endDate: ed,
          instructionDays: fullProj.instructionDays,
          shipID: fullProj.shipID || '',
          preShipPost: fullProj.preShipPost || '',
          countryID: fullProj.countryID || '',
          cityID: fullProj.cityID || '',
          venue: fullProj.venue || '',
          projectComments: fullProj.projectComments || '',
          financeLocationID: fullProj.financeLocation?.financeCodeID || '',
          financeProgramID: fullProj.financeProgram?.financeCodeID || '',
          financePurposeID: fullProj.financePurpose?.financeCodeID || ''
        };

        this.onCountryChange();
        this.loading = false;
        this.showModal = true;
      },
      error: (err) => {
        console.error('Failed to load project details for editing', err);
        this.loading = false;
      }
    });
  }

  onCountryChange(): void {
    if (this.projectForm.countryID) {
      this.filteredCities = this.references.cities.filter((c: any) => c.countryID === Number(this.projectForm.countryID));
    } else {
      this.filteredCities = [];
    }
  }

  saveProject(): void {
    // Basic validation
    if (!this.projectForm.projectTitle || !this.projectForm.startDate || !this.projectForm.endDate) {
      alert('Title, Start Date, and End Date are required fields.');
      return;
    }

    this.loading = true;
    const body = { ...this.projectForm };
    
    // Normalize empty selections to null for EF core
    if (!body.programID) body.programID = null;
    if (!body.projectTypeID) body.projectTypeID = null;
    if (!body.participantTypeID) body.participantTypeID = null;
    if (!body.shipID) body.shipID = null;
    if (!body.countryID) body.countryID = null;
    if (!body.cityID) body.cityID = null;
    if (!body.financeLocationID) body.financeLocationID = null;
    if (!body.financeProgramID) body.financeProgramID = null;
    if (!body.financePurposeID) body.financePurposeID = null;

    if (this.isEditing) {
      this.apiService.updateProject(body.projectIDNumber, body).subscribe({
        next: () => {
          this.showModal = false;
          this.loadProjects();
        },
        error: (err) => {
          alert('Error updating project: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    } else {
      this.apiService.createProject(body).subscribe({
        next: () => {
          this.showModal = false;
          this.loadProjects();
        },
        error: (err) => {
          alert('Error creating project: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  deleteProject(project: any): void {
    if (confirm(`Are you sure you want to delete project: ${project.projectID} - ${project.projectTitle}?`)) {
      this.loading = true;
      this.apiService.deleteProject(project.projectIDNumber).subscribe({
        next: () => {
          this.loadProjects();
        },
        error: (err) => {
          alert('Failed to delete project. Reason: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }
}
