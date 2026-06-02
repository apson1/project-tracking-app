import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-output-entry',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './output-entry.html',
  styleUrl: './output-entry.css'
})
export class OutputEntry implements OnInit {
  projects: any[] = [];
  participants: any[] = [];
  references: any = {
    countries: [],
    cities: [],
    outputTypes: [],
    specificOutputTypes: []
  };

  loading = false;
  successMessage = '';
  errorMessage = '';

  // Selector fields
  selectedProjectIDNumber = '';
  selectedCategory = 'Participant'; // 'Participant', 'Patient', 'DMS', 'Planned'

  // Dynamic lists
  filteredCities: any[] = [];
  filteredSpecificOutputs: any[] = [];
  filteredOutputTypes: any[] = [];

  // Active form data
  form: any = {};

  constructor(
    private apiService: ApiService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
    this.loadReferences();
    this.loadParticipants();
    this.resetForm();
  }

  loadProjects(): void {
    this.apiService.getProjects({ page: 1, pageSize: 200 }).subscribe({
      next: (res) => this.projects = res.data,
      error: (err) => console.error('Failed to load projects', err)
    });
  }

  loadReferences(): void {
    this.apiService.getReferences().subscribe({
      next: (refs) => {
        this.references = refs;
        this.updateFilteredOutputTypes();
      },
      error: (err) => console.error('Failed to load references', err)
    });
  }

  loadParticipants(): void {
    this.apiService.getParticipants({ page: 1, pageSize: 200 }).subscribe({
      next: (res) => this.participants = res.data,
      error: (err) => console.error('Failed to load participants', err)
    });
  }

  updateFilteredOutputTypes(): void {
    if (!this.references.outputTypes) return;
    
    if (this.selectedCategory === 'Planned') {
      this.filteredOutputTypes = this.references.outputTypes;
    } else {
      this.filteredOutputTypes = this.references.outputTypes.filter(
        (o: any) => o.category === this.selectedCategory
      );
    }
  }

  onCategoryChange(): void {
    this.updateFilteredOutputTypes();
    this.resetForm();
  }

  onCountryChange(): void {
    if (this.form.countryID) {
      this.filteredCities = this.references.cities.filter(
        (c: any) => c.countryID === Number(this.form.countryID)
      );
    } else {
      this.filteredCities = [];
    }
  }

  onOutputTypeChange(): void {
    if (this.form.outputTypeID) {
      this.filteredSpecificOutputs = this.references.specificOutputTypes.filter(
        (s: any) => s.outputTypeID === Number(this.form.outputTypeID)
      );
    } else {
      this.filteredSpecificOutputs = [];
    }
  }

  resetForm(): void {
    this.successMessage = '';
    this.errorMessage = '';

    const todayStr = new Date().toISOString().split('T')[0];

    if (this.selectedCategory === 'Participant') {
      this.form = {
        participantIDNumber: '',
        reportingDate: todayStr,
        reportingPeriod: '',
        outputAmount: 0,
        outputTypeID: '',
        specificOutputTypeID: '',
        comments: ''
      };
    } else if (this.selectedCategory === 'Patient') {
      this.form = {
        patientID: 'PAT-' + Math.floor(Math.random() * 90000 + 10000),
        sex: 'Female',
        ageGroup: '18-59',
        countryID: '',
        cityID: '',
        reportingDate: todayStr,
        outputAmount: 1,
        outputTypeID: '',
        comments: ''
      };
    } else if (this.selectedCategory === 'DMS') {
      this.form = {
        reportingDate: todayStr,
        reportingPeriod: '',
        outputAmount: 1,
        outputTypeID: '',
        specificOutputTypeID: '',
        adultChild: 'Adult',
        gender: 'Both',
        programTypeOfOutput: '',
        comments: ''
      };
    } else if (this.selectedCategory === 'Planned') {
      this.form = {
        plannedOutputTypeID: '',
        plannedAmount: 0,
        reportingPeriod: '',
        plannedDateYear: new Date().getFullYear().toString(),
        comments: ''
      };
    }

    this.filteredCities = [];
    this.filteredSpecificOutputs = [];
  }

  submitEntry(): void {
    if (!this.selectedProjectIDNumber) {
      this.errorMessage = 'Please select a Project/Event first.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload = { 
      ...this.form, 
      projectIDNumber: Number(this.selectedProjectIDNumber) 
    };

    // Normalize empty dropdowns
    if (payload.specificOutputTypeID === '') payload.specificOutputTypeID = null;
    if (payload.countryID === '') payload.countryID = null;
    if (payload.cityID === '') payload.cityID = null;

    if (this.selectedCategory === 'Participant') {
      if (!payload.participantIDNumber || !payload.outputTypeID) {
        this.errorMessage = 'Trainee Participant and Output Type are required fields.';
        this.loading = false;
        return;
      }
      this.apiService.addParticipantOutput(payload).subscribe(this.handleResponse());
    } 
    else if (this.selectedCategory === 'Patient') {
      if (!payload.patientID || !payload.outputTypeID) {
        this.errorMessage = 'Patient ID and Output Type are required fields.';
        this.loading = false;
        return;
      }
      this.apiService.addPatientOutput(payload).subscribe(this.handleResponse());
    } 
    else if (this.selectedCategory === 'DMS') {
      if (!payload.outputTypeID) {
        this.errorMessage = 'Output Type is required.';
        this.loading = false;
        return;
      }
      this.apiService.addDmsOutput(payload).subscribe(this.handleResponse());
    } 
    else if (this.selectedCategory === 'Planned') {
      if (!payload.plannedOutputTypeID || payload.plannedAmount <= 0) {
        this.errorMessage = 'Target Output Type and Target Amount are required.';
        this.loading = false;
        return;
      }
      this.apiService.addPlannedOutput(payload).subscribe(this.handleResponse());
    }
  }

  private handleResponse() {
    return {
      next: () => {
        this.successMessage = `Successfully logged output record to project database.`;
        this.resetForm();
        this.loading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to submit data entry: ' + (err.error || err.message);
        this.loading = false;
      }
    };
  }
}
