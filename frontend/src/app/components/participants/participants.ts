import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-participants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './participants.html',
  styleUrl: './participants.css'
})
export class Participants implements OnInit {
  protected readonly Math = Math;
  participants: any[] = [];
  totalCount = 0;
  loading = false;


  // Search & Filters
  filters: any = {
    search: '',
    countryId: '',
    institutionId: '',
    profession: '',
    page: 1,
    pageSize: 10
  };

  // References lookups
  references: any = {
    countries: [],
    cities: [],
    institutions: []
  };

  filteredCities: any[] = [];

  // Modal forms states
  showModal = false;
  isEditing = false;
  modalTitle = 'Register Participant';
  participantForm: any = {};

  // Details Overlay (Profile View)
  showDetailsOverlay = false;
  selectedParticipant: any = null;

  constructor(
    private apiService: ApiService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadReferences();
    this.loadParticipants();
  }

  loadReferences(): void {
    this.apiService.getReferences().subscribe({
      next: (refs) => this.references = refs,
      error: (err) => console.error('Failed to load lookup tables', err)
    });
  }

  loadParticipants(): void {
    this.loading = true;
    this.apiService.getParticipants(this.filters).subscribe({
      next: (res) => {
        this.participants = res.data;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load participants', err);
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.filters.page = 1;
    this.loadParticipants();
  }

  clearFilters(): void {
    this.filters = {
      search: '',
      countryId: '',
      institutionId: '',
      profession: '',
      page: 1,
      pageSize: 10
    };
    this.loadParticipants();
  }

  changePage(page: number): void {
    this.filters.page = page;
    this.loadParticipants();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.filters.pageSize);
  }

  // ==========================================
  // VIEW PARTICIPANT DETAILS OVERLAY
  // ==========================================
  viewParticipantDetails(p: any): void {
    this.loading = true;
    this.apiService.getParticipant(p.participantIDNumber).subscribe({
      next: (fullParticipant) => {
        this.selectedParticipant = fullParticipant;
        this.showDetailsOverlay = true;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load participant profile details', err);
        this.loading = false;
      }
    });
  }

  // ==========================================
  // PROFILE EDIT/CREATE FORM
  // ==========================================
  openCreateModal(): void {
    this.isEditing = false;
    this.modalTitle = 'Register New Participant / Trainee';
    this.participantForm = {
      participantID: '',
      title: '',
      firstName: '',
      lastName: '',
      gender: '',
      institutionID: '',
      mobilePhone: '',
      email: '',
      professionTitle: '',
      cityID: '',
      countryID: '',
      comments: ''
    };
    this.filteredCities = [];
    this.showModal = true;
  }

  openEditModal(p: any): void {
    this.isEditing = true;
    this.modalTitle = 'Edit Participant Profile';
    this.loading = true;
    this.showDetailsOverlay = false;

    this.apiService.getParticipant(p.participantIDNumber).subscribe({
      next: (fullPart) => {
        this.participantForm = {
          participantIDNumber: fullPart.participantIDNumber,
          participantID: fullPart.participantID,
          title: fullPart.title || '',
          firstName: fullPart.firstName,
          lastName: fullPart.lastName,
          gender: fullPart.gender || '',
          institutionID: fullPart.institutionID || '',
          mobilePhone: fullPart.mobilePhone || '',
          email: fullPart.email || '',
          professionTitle: fullPart.professionTitle || '',
          cityID: fullPart.cityID || '',
          countryID: fullPart.countryID || '',
          comments: fullPart.comments || ''
        };
        this.onCountryChange();
        this.loading = false;
        this.showModal = true;
      },
      error: (err) => {
        console.error('Failed to load profile details for edit', err);
        this.loading = false;
      }
    });
  }

  onCountryChange(): void {
    if (this.participantForm.countryID) {
      this.filteredCities = this.references.cities.filter(
        (c: any) => c.countryID === Number(this.participantForm.countryID)
      );
    } else {
      this.filteredCities = [];
    }
  }

  saveParticipant(): void {
    if (!this.participantForm.firstName || !this.participantForm.lastName) {
      alert('First Name and Last Name are required.');
      return;
    }

    this.loading = true;
    const body = { ...this.participantForm };
    
    // Normalize selections
    if (!body.institutionID) body.institutionID = null;
    if (!body.countryID) body.countryID = null;
    if (!body.cityID) body.cityID = null;

    if (this.isEditing) {
      this.apiService.updateParticipant(body.participantIDNumber, body).subscribe({
        next: () => {
          this.showModal = false;
          this.loadParticipants();
        },
        error: (err) => {
          alert('Error updating participant profile: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    } else {
      this.apiService.createParticipant(body).subscribe({
        next: () => {
          this.showModal = false;
          this.loadParticipants();
        },
        error: (err) => {
          alert('Error registering participant: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }

  deleteParticipant(p: any): void {
    if (confirm(`Are you sure you want to delete participant: ${p.firstName} ${p.lastName} (${p.participantID})?`)) {
      this.loading = true;
      this.apiService.deleteParticipant(p.participantIDNumber).subscribe({
        next: () => this.loadParticipants(),
        error: (err) => {
          alert('Failed to delete participant. Reason: ' + (err.error || err.message));
          this.loading = false;
        }
      });
    }
  }
}
