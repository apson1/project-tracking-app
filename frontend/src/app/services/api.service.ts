import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  // ==========================================
  // DASHBOARD
  // ==========================================
  getDashboard(): Observable<any> {
    return this.http.get(`${this.baseUrl}/dashboard`);
  }

  // ==========================================
  // REFERENCES
  // ==========================================
  getReferences(): Observable<any> {
    return this.http.get(`${this.baseUrl}/references`);
  }

  createCountry(country: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/countries`, country);
  }

  createCity(city: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/cities`, city);
  }

  createInstitution(institution: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/institutions`, institution);
  }

  createShip(ship: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/ships`, ship);
  }

  createProgram(program: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/programs`, program);
  }

  createProjectType(projectType: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/projecttypes`, projectType);
  }

  createFinanceCode(financeCode: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/references/financecodes`, financeCode);
  }

  // ==========================================
  // PROJECTS
  // ==========================================
  getProjects(params: any): Observable<any> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.countryId) httpParams = httpParams.set('countryId', params.countryId.toString());
    if (params.cityId) httpParams = httpParams.set('cityId', params.cityId.toString());
    if (params.programId) httpParams = httpParams.set('programId', params.programId.toString());
    if (params.projectTypeId) httpParams = httpParams.set('projectTypeId', params.projectTypeId.toString());
    if (params.shipId) httpParams = httpParams.set('shipId', params.shipId.toString());
    if (params.year) httpParams = httpParams.set('year', params.year.toString());
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.missingFinanceCodes !== undefined) httpParams = httpParams.set('missingFinanceCodes', params.missingFinanceCodes.toString());
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get(`${this.baseUrl}/projects`, { params: httpParams });
  }

  getProject(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/projects/${id}`);
  }

  createProject(project: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/projects`, project);
  }

  updateProject(id: number, project: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/projects/${id}`, project);
  }

  deleteProject(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/projects/${id}`);
  }

  // ==========================================
  // PARTICIPANTS
  // ==========================================
  getParticipants(params: any): Observable<any> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.countryId) httpParams = httpParams.set('countryId', params.countryId.toString());
    if (params.institutionId) httpParams = httpParams.set('institutionId', params.institutionId.toString());
    if (params.profession) httpParams = httpParams.set('profession', params.profession);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get(`${this.baseUrl}/participants`, { params: httpParams });
  }

  getParticipant(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/participants/${id}`);
  }

  createParticipant(participant: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/participants`, participant);
  }

  updateParticipant(id: number, participant: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/participants/${id}`, participant);
  }

  deleteParticipant(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/participants/${id}`);
  }

  // ==========================================
  // OUTPUTS CRUD
  // ==========================================
  addParticipantOutput(output: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/outputs/participant`, output);
  }
  updateParticipantOutput(id: number, output: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/outputs/participant/${id}`, output);
  }
  deleteParticipantOutput(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/outputs/participant/${id}`);
  }

  addPatientOutput(output: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/outputs/patient`, output);
  }
  updatePatientOutput(id: number, output: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/outputs/patient/${id}`, output);
  }
  deletePatientOutput(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/outputs/patient/${id}`);
  }

  addDmsOutput(output: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/outputs/dms`, output);
  }
  updateDmsOutput(id: number, output: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/outputs/dms/${id}`, output);
  }
  deleteDmsOutput(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/outputs/dms/${id}`);
  }

  addPlannedOutput(output: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/outputs/planned`, output);
  }
  updatePlannedOutput(id: number, output: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/outputs/planned/${id}`, output);
  }
  deletePlannedOutput(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/outputs/planned/${id}`);
  }

  // ==========================================
  // EXCEL / CSV IMPORT
  // ==========================================
  validateProjectsImport(csvData: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/import/projects/validate`, { csvData });
  }

  commitProjectsImport(rows: any[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/import/projects/commit`, rows);
  }

  validateParticipantsImport(csvData: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/import/participants/validate`, { csvData });
  }

  commitParticipantsImport(rows: any[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/import/participants/commit`, rows);
  }
}
