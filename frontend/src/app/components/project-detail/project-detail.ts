import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.css'
})
export class ProjectDetail implements OnInit {
  protected readonly Math = Math;
  projectIdNumber!: number;
  project: any = null;
  loading = false;

  activeTab = 'overview'; // 'overview', 'participants', 'patients', 'dms', 'targets'

  // Lookups & lists
  references: any = {
    countries: [],
    cities: [],
    outputTypes: [],
    specificOutputTypes: [],
    participants: [] // used to add attendees
  };
  
  filteredCities: any[] = [];
  filteredSpecificOutputs: any[] = [];

  // Modals visibility
  showParticipantModal = false;
  showPatientModal = false;
  showDmsModal = false;
  showTargetModal = false;

  // Add Participant Form State
  allParticipants: any[] = [];
  searchParticipantQuery = '';
  selectedParticipantId: any = '';
  participantForm: any = {};

  // Add Patient Form State
  patientForm: any = {};

  // Add DMS Form State
  dmsForm: any = {};

  // Add Target Form State
  targetForm: any = {};

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.projectIdNumber = Number(this.route.snapshot.paramMap.get('id'));
    this.loadReferences();
    this.loadProjectDetails();
    this.loadAllParticipants();
  }

  loadProjectDetails(): void {
    this.loading = true;
    this.apiService.getProject(this.projectIdNumber).subscribe({
      next: (data) => {
        this.project = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load project details', err);
        this.loading = false;
      }
    });
  }

  loadReferences(): void {
    this.apiService.getReferences().subscribe({
      next: (refs) => {
        this.references = refs;
      },
      error: (err) => console.error('Failed to load lookups', err)
    });
  }

  loadAllParticipants(): void {
    // Load first 100 participants for selection dropdown/search
    this.apiService.getParticipants({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.allParticipants = res.data;
      },
      error: (err) => console.error('Failed to load participants list', err)
    });
  }

  selectTab(tab: string): void {
    this.activeTab = tab;
  }

  // ==========================================
  // PARTICIPANT (ATTENDEES) ACTIONS
  // ==========================================
  openParticipantModal(): void {
    this.participantForm = {
      projectIDNumber: this.projectIdNumber,
      participantIDNumber: '',
      reportingDate: new Date().toISOString().split('T')[0],
      reportingPeriod: '',
      outputAmount: 0,
      outputTypeID: '',
      specificOutputTypeID: '',
      comments: ''
    };
    this.showParticipantModal = true;
  }

  onParticipantOutputTypeChange(): void {
    if (this.participantForm.outputTypeID) {
      this.filteredSpecificOutputs = this.references.specificOutputTypes.filter(
        (s: any) => s.outputTypeID === Number(this.participantForm.outputTypeID)
      );
    } else {
      this.filteredSpecificOutputs = [];
    }
  }

  saveParticipantOutput(): void {
    if (!this.participantForm.participantIDNumber || !this.participantForm.outputTypeID) {
      alert('Participant and Output Type are required.');
      return;
    }

    this.loading = true;
    const body = { ...this.participantForm };
    if (!body.specificOutputTypeID) body.specificOutputTypeID = null;

    this.apiService.addParticipantOutput(body).subscribe({
      next: () => {
        this.showParticipantModal = false;
        this.loadProjectDetails();
      },
      error: (err) => {
        alert('Error adding participant: ' + (err.error || err.message));
        this.loading = false;
      }
    });
  }

  deleteParticipantOutput(id: number): void {
    if (confirm('Are you sure you want to remove this participant attendance record?')) {
      this.loading = true;
      this.apiService.deleteParticipantOutput(id).subscribe({
        next: () => this.loadProjectDetails(),
        error: (err) => {
          alert('Error removing record: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  // ==========================================
  // PATIENT OUTPUT ACTIONS
  // ==========================================
  openPatientModal(): void {
    this.patientForm = {
      projectIDNumber: this.projectIdNumber,
      patientID: 'PAT-' + Math.floor(Math.random() * 90000 + 10000),
      sex: 'Female',
      ageGroup: '18-59',
      countryID: this.project?.countryID || '',
      cityID: this.project?.cityID || '',
      reportingDate: new Date().toISOString().split('T')[0],
      outputAmount: 1,
      outputTypeID: '',
      comments: ''
    };
    this.onPatientCountryChange();
    this.showPatientModal = true;
  }

  onPatientCountryChange(): void {
    if (this.patientForm.countryID) {
      this.filteredCities = this.references.cities.filter(
        (c: any) => c.countryID === Number(this.patientForm.countryID)
      );
    } else {
      this.filteredCities = [];
    }
  }

  savePatientOutput(): void {
    if (!this.patientForm.patientID || !this.patientForm.outputTypeID) {
      alert('Patient ID and Output Type are required.');
      return;
    }

    this.loading = true;
    const body = { ...this.patientForm };
    if (!body.countryID) body.countryID = null;
    if (!body.cityID) body.cityID = null;

    this.apiService.addPatientOutput(body).subscribe({
      next: () => {
        this.showPatientModal = false;
        this.loadProjectDetails();
      },
      error: (err) => {
        alert('Error adding patient output: ' + (err.error || err.message));
        this.loading = false;
      }
    });
  }

  deletePatientOutput(id: number): void {
    if (confirm('Are you sure you want to delete this patient record?')) {
      this.loading = true;
      this.apiService.deletePatientOutput(id).subscribe({
        next: () => this.loadProjectDetails(),
        error: (err) => {
          alert('Error deleting: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  // ==========================================
  // DMS / INFRASTRUCTURE ACTIONS
  // ==========================================
  openDmsModal(): void {
    this.dmsForm = {
      projectIDNumber: this.projectIdNumber,
      reportingDate: new Date().toISOString().split('T')[0],
      reportingPeriod: '',
      outputAmount: 1,
      outputTypeID: '',
      specificOutputTypeID: '',
      adultChild: 'Adult',
      gender: 'Both',
      programTypeOfOutput: '',
      comments: ''
    };
    this.filteredSpecificOutputs = [];
    this.showDmsModal = true;
  }

  onDmsOutputTypeChange(): void {
    if (this.dmsForm.outputTypeID) {
      this.filteredSpecificOutputs = this.references.specificOutputTypes.filter(
        (s: any) => s.outputTypeID === Number(this.dmsForm.outputTypeID)
      );
    } else {
      this.filteredSpecificOutputs = [];
    }
  }

  saveDmsOutput(): void {
    if (!this.dmsForm.outputTypeID) {
      alert('Output Type is required.');
      return;
    }

    this.loading = true;
    const body = { ...this.dmsForm };
    if (!body.specificOutputTypeID) body.specificOutputTypeID = null;

    this.apiService.addDmsOutput(body).subscribe({
      next: () => {
        this.showDmsModal = false;
        this.loadProjectDetails();
      },
      error: (err) => {
        alert('Error saving DMS output: ' + (err.error || err.message));
        this.loading = false;
      }
    });
  }

  deleteDmsOutput(id: number): void {
    if (confirm('Are you sure you want to delete this DMS record?')) {
      this.loading = true;
      this.apiService.deleteDmsOutput(id).subscribe({
        next: () => this.loadProjectDetails(),
        error: (err) => {
          alert('Error deleting record: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  // ==========================================
  // TARGETS (PLANNED) ACTIONS
  // ==========================================
  openTargetModal(): void {
    this.targetForm = {
      projectIDNumber: this.projectIdNumber,
      plannedOutputTypeID: '',
      plannedAmount: 0,
      reportingPeriod: '',
      plannedDateYear: new Date().getFullYear().toString(),
      comments: ''
    };
    this.showTargetModal = true;
  }

  saveTarget(): void {
    if (!this.targetForm.plannedOutputTypeID || this.targetForm.plannedAmount <= 0) {
      alert('Output Type and Target Amount are required.');
      return;
    }

    this.loading = true;
    this.apiService.addPlannedOutput(this.targetForm).subscribe({
      next: () => {
        this.showTargetModal = false;
        this.loadProjectDetails();
      },
      error: (err) => {
        alert('Error adding planned target: ' + (err.error || err.message));
        this.loading = false;
      }
    });
  }

  deleteTarget(id: number): void {
    if (confirm('Are you sure you want to delete this planned target?')) {
      this.loading = true;
      this.apiService.deletePlannedOutput(id).subscribe({
        next: () => this.loadProjectDetails(),
        error: (err) => {
          alert('Error deleting: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  // Filters output types by category
  getFilteredOutputTypes(category: string): any[] {
    return this.references.outputTypes.filter((o: any) => o.category === category);
  }
}
