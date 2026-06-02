import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService, User } from '../../services/auth.service';

@Component({
  selector: 'app-import-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-admin.html',
  styleUrl: './import-admin.css'
})
export class ImportAdmin implements OnInit {
  importType = 'Projects'; // 'Projects', 'Participants'
  csvData = '';
  fileName = '';
  
  loading = false;
  isValidated = false;
  isCommitted = false;
  errorMessage = '';
  successMessage = '';

  // Security Sub-tabs & custom user fields
  activeTab = 'import'; // 'import' | 'users'
  newUser = { name: '', role: 'viewer' as 'admin' | 'reporter' | 'viewer', initials: '' };
  userSuccessMessage = '';
  userErrorMessage = '';

  // Dynamic status & history
  warningCount = 0;
  importHistory: any[] = [];

  // Reference Tables Management
  references: any = {
    countries: [],
    cities: [],
    ships: [],
    programs: [],
    projectTypes: [],
    financeCodes: []
  };
  activeRefTable: string | null = null;
  showRefModal = false;
  newRefData: any = {};

  // Validation Preview Lists
  projectPreviewRows: any[] = [];
  participantPreviewRows: any[] = [];

  // Filter Preview Grid
  previewFilter = 'all'; // 'all', 'errors', 'duplicates', 'valid'

  // Import stats
  stats = {
    total: 0,
    valid: 0,
    warnings: 0,
    errors: 0,
    duplicates: 0
  };

  constructor(
    private apiService: ApiService, 
    private cdr: ChangeDetectorRef,
    private router: Router,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadReferences();
    this.loadWarnings();
    this.loadImportHistory();
  }

  loadWarnings(): void {
    this.apiService.getDashboard().subscribe({
      next: (res) => {
        this.warningCount = res.warnings ? res.warnings.length : 0;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load warnings from dashboard', err)
    });
  }

  navigateToFlagged(): void {
    this.router.navigate(['/projects'], { queryParams: { missingFinanceCodes: 'true' } });
  }

  loadImportHistory(): void {
    const historyJson = localStorage.getItem('import_history');
    if (historyJson) {
      this.importHistory = JSON.parse(historyJson);
    } else {
      // Seed default/initial history
      const seedHistory = [
        {
          fileName: 'Database_Initial_Seed.sql',
          importType: 'Projects & Participants',
          timestamp: new Date(Date.now() - 4 * 3600000).toISOString(), // 4 hours ago
          user: 'system@impacttracker.org',
          created: 70,
          updated: 0,
          rows: 70
        },
        {
          fileName: 'March_2026_Outputs.csv',
          importType: 'Projects',
          timestamp: new Date(Date.now() - 2 * 3600000).toISOString(), // 2 hours ago
          user: 'admin@example.com',
          created: 20,
          updated: 5,
          rows: 25
        }
      ];
      localStorage.setItem('import_history', JSON.stringify(seedHistory));
      this.importHistory = seedHistory;
    }
    this.cdr.detectChanges();
  }

  loadReferences(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.apiService.getReferences().subscribe({
      next: (refs) => {
        this.references = refs;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load references', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  selectRefTable(tableName: string): void {
    this.activeRefTable = tableName;
    this.newRefData = {};
    this.showRefModal = true;
    this.cdr.detectChanges();
  }

  addRefRecord(): void {
    if (!this.activeRefTable) return;

    this.loading = true;
    this.cdr.detectChanges();

    let obs;
    if (this.activeRefTable === 'Countries') {
      if (!this.newRefData.countryName) { 
        alert('Country Name is required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createCountry({ 
        countryName: this.newRefData.countryName, 
        countryCode: this.newRefData.countryCode || '' 
      });
    } else if (this.activeRefTable === 'Cities') {
      if (!this.newRefData.cityName || !this.newRefData.countryID) { 
        alert('City Name and Country are required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createCity({ 
        cityName: this.newRefData.cityName, 
        countryID: Number(this.newRefData.countryID), 
        regionName: this.newRefData.regionName || '' 
      });
    } else if (this.activeRefTable === 'Ships') {
      if (!this.newRefData.shipName) { 
        alert('Ship Name is required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createShip({ 
        shipName: this.newRefData.shipName, 
        isActive: true 
      });
    } else if (this.activeRefTable === 'Programs') {
      if (!this.newRefData.name) { 
        alert('Program Name is required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createProgram({ 
        name: this.newRefData.name, 
        description: this.newRefData.description || '' 
      });
    } else if (this.activeRefTable === 'ProjectTypes') {
      if (!this.newRefData.typeName) { 
        alert('Type Name is required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createProjectType({ 
        typeName: this.newRefData.typeName 
      });
    } else if (this.activeRefTable === 'FinanceCodes') {
      if (!this.newRefData.code || !this.newRefData.codeType) { 
        alert('Code and Code Type are required.'); 
        this.loading = false; 
        this.cdr.detectChanges(); 
        return; 
      }
      obs = this.apiService.createFinanceCode({ 
        code: this.newRefData.code, 
        codeType: this.newRefData.codeType, 
        description: this.newRefData.description || '' 
      });
    }

    if (obs) {
      obs.subscribe({
        next: () => {
          this.newRefData = {};
          this.loadReferences();
          alert('Reference item successfully created!');
        },
        error: (err: any) => {
          alert('Failed to save reference: ' + (err.error || err.message));
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.fileName = file.name;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.csvData = e.target.result;
        this.validateImport(); // Trigger validation automatically once file is fully read
      };
      reader.readAsText(file);
    }
  }

  validateImport(): void {
    if (!this.csvData) {
      this.errorMessage = 'Please paste CSV content or upload a CSV file.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.isValidated = false;
    this.isCommitted = false;

    if (this.importType === 'Projects') {
      this.apiService.validateProjectsImport(this.csvData).subscribe({
        next: (rows) => {
          this.projectPreviewRows = rows;
          this.calculateStats(rows);
          this.isValidated = true;
          this.loading = false;
        },
        error: (err) => {
          this.errorMessage = 'Validation failed: ' + (err.error || err.message);
          this.loading = false;
        }
      });
    } else {
      this.apiService.validateParticipantsImport(this.csvData).subscribe({
        next: (rows) => {
          this.participantPreviewRows = rows;
          this.calculateStats(rows);
          this.isValidated = true;
          this.loading = false;
        },
        error: (err) => {
          this.errorMessage = 'Validation failed: ' + (err.error || err.message);
          this.loading = false;
        }
      });
    }
  }

  calculateStats(rows: any[]): void {
    this.stats = {
      total: rows.length,
      valid: rows.filter(r => r.isValid && !r.isDuplicate).length,
      warnings: rows.filter(r => r.warnings.length > 0).length,
      errors: rows.filter(r => r.errors.length > 0).length,
      duplicates: rows.filter(r => r.isDuplicate).length
    };
  }

  getFilteredRows(): any[] {
    const rows = this.importType === 'Projects' ? this.projectPreviewRows : this.participantPreviewRows;
    
    if (this.previewFilter === 'errors') {
      return rows.filter(r => r.errors.length > 0);
    }
    if (this.previewFilter === 'duplicates') {
      return rows.filter(r => r.isDuplicate);
    }
    if (this.previewFilter === 'valid') {
      return rows.filter(r => r.isValid);
    }
    return rows;
  }

  commitImport(): void {
    const rows = this.importType === 'Projects' ? this.projectPreviewRows : this.participantPreviewRows;
    const uploadableRows = rows.filter(r => r.isValid); // filter out rows with structural errors

    if (uploadableRows.length === 0) {
      alert('There are no valid rows to import. Correct validation errors in your CSV sheet.');
      return;
    }

    if (confirm(`Do you approve importing ${uploadableRows.length} records into the program database? (Duplicates will overwrite corresponding rows).`)) {
      this.loading = true;
      this.errorMessage = '';

      if (this.importType === 'Projects') {
        this.apiService.commitProjectsImport(uploadableRows).subscribe(this.handleCommitResponse());
      } else {
        this.apiService.commitParticipantsImport(uploadableRows).subscribe(this.handleCommitResponse());
      }
    }
  }

  private handleCommitResponse() {
    const fileNameCopy = this.fileName || 'Pasted_CSV_Data.csv';
    const typeCopy = this.importType;
    return {
      next: (res: any) => {
        this.successMessage = `Import Approved! Successfully inserted ${res.created} new records, and updated ${res.updated} existing records.`;
        
        // Log to history
        const newLog = {
          fileName: fileNameCopy,
          importType: typeCopy,
          timestamp: new Date().toISOString(),
          user: 'admin@example.com',
          created: res.created,
          updated: res.updated,
          rows: res.created + res.updated
        };
        const currentHistory = JSON.parse(localStorage.getItem('import_history') || '[]');
        currentHistory.unshift(newLog); // Prepend so it shows up at the top
        localStorage.setItem('import_history', JSON.stringify(currentHistory));

        this.isValidated = false;
        this.isCommitted = true;
        this.csvData = '';
        this.fileName = '';
        this.projectPreviewRows = [];
        this.participantPreviewRows = [];
        this.loading = false;
        
        this.loadImportHistory();
        this.loadWarnings();
      },
      error: (err: any) => {
        this.errorMessage = 'Commit failed: ' + (err.error || err.message);
        this.loading = false;
        this.cdr.detectChanges();
      }
    };
  }

  downloadTemplate(): void {
    let headers = '';
    let fileName = '';

    if (this.importType === 'Projects') {
      headers = 'Project ID,Project Title,Start Date,End Date,Country,French Project Name,Program Name,Project Type,Participant Type,Ship,Pre/Ship/Post,City,Venue,Finance Location Code,Finance Program Code,Finance Purpose Code,Project Comments';
      fileName = 'Projects_Import_Template.csv';
    } else {
      headers = 'First Name,Last Name,Participant ID,Title,Gender,Mobile Phone,Email,Profession Title,Institution / Facility,Region,Country,Comments';
      fileName = 'Participants_Import_Template.csv';
    }

    const blob = new Blob([headers], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    if (link.download !== undefined) { // feature detection
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', fileName);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  switchSubTab(tab: string): void {
    if (tab === 'users' && !this.authService.isAdmin()) {
      alert('Access Denied: Only System Administrators can access user management.');
      return;
    }
    this.activeTab = tab;
    this.cdr.detectChanges();
  }

  registerUser(): void {
    this.userErrorMessage = '';
    this.userSuccessMessage = '';

    if (!this.newUser.name || !this.newUser.role || !this.newUser.initials) {
      this.userErrorMessage = 'All fields (Name, Initials, Role) are required.';
      return;
    }

    if (this.newUser.initials.length > 3) {
      this.userErrorMessage = 'Initials must be 3 characters or less.';
      return;
    }

    // Check if name is unique
    const exists = this.authService.users.some(u => u.name.toLowerCase() === this.newUser.name.toLowerCase());
    if (exists) {
      this.userErrorMessage = `A user with the name "${this.newUser.name}" already exists.`;
      return;
    }

    let roleName = 'Guest Viewer';
    if (this.newUser.role === 'admin') roleName = 'System Administrator';
    else if (this.newUser.role === 'reporter') roleName = 'Reporting Unit';

    const userToCreate: User = {
      name: this.newUser.name,
      role: this.newUser.role,
      roleName: roleName,
      initials: this.newUser.initials.toUpperCase()
    };

    this.authService.addUser(userToCreate);
    this.userSuccessMessage = `Successfully registered user "${userToCreate.name}".`;
    
    // Reset form
    this.newUser = { name: '', role: 'viewer', initials: '' };
    this.cdr.detectChanges();
  }

  deleteUser(name: string): void {
    if (name === 'Admin User') {
      alert('Cannot delete the default Admin User.');
      return;
    }
    if (confirm(`Are you sure you want to delete user "${name}"?`)) {
      this.authService.deleteUser(name);
      this.cdr.detectChanges();
    }
  }
}
