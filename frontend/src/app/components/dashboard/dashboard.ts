import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { ApiService } from '../../services/api.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  loading = true;
  currentChartType: 'year' | 'country' | 'program' = 'year';
  private rawResponse: any;
  
  monthChart: any;
  kpis: any = {};
  programOutputs: any[] = [];
  upcomingProjects: any[] = [];
  warnings: any[] = [];

  constructor(
    private apiService: ApiService, 
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  navigateToAddOutput(): void {
    this.router.navigate(['/output-entry']);
  }

  generatePdf(): void {
    window.print();
  }

  switchChart(type: 'year' | 'country' | 'program'): void {
    this.currentChartType = type;
    this.cdr.detectChanges();
    setTimeout(() => {
      try {
        this.renderChart(type);
        this.cdr.detectChanges();
      } catch (e) {
        console.error('Failed to render chart: ' + type, e);
      }
    }, 50);
  }

  ngOnInit(): void {
    console.log('Dashboard component initialized, calling API...');
    this.apiService.getDashboard().subscribe({
      next: (res) => {
        console.log('API response received!', res);
        try {
          if (!res) {
            console.warn('Dashboard API returned null/empty response');
            this.loading = false;
            this.cdr.detectChanges();
            return;
          }

          this.rawResponse = res;
          this.kpis = res.kpis || {};
          this.upcomingProjects = res.upcomingProjects || [];
          this.warnings = res.warnings || [];
          
          // Map program outputs to the progress bar structure
          if (res.outputsByProgram && Array.isArray(res.outputsByProgram) && res.outputsByProgram.length > 0) {
            // Find max value to calculate percentages
            const maxValue = Math.max(...res.outputsByProgram.map((p: any) => p.value || 0));
            const safeMax = maxValue > 0 ? maxValue : 1;
            const colors = ['#6366f1', '#10b981', '#f59e0b', '#06b6d4', '#ec4899', '#8b5cf6'];
            
            this.programOutputs = res.outputsByProgram
              .sort((a: any, b: any) => (b.value || 0) - (a.value || 0))
              .map((p: any, index: number) => ({
                name: p.program,
                percent: Math.round(((p.value || 0) / safeMax) * 100),
                color: colors[index % colors.length]
              }));
          }

          this.loading = false;
          this.cdr.detectChanges();
          
          // Render initial chart
          this.switchChart('year');
        } catch (e) {
          console.error('Error processing dashboard data', e);
          this.loading = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Failed to load dashboard data', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  renderChart(type: 'year' | 'country' | 'program'): void {
    const canvas = document.getElementById('monthChart') as HTMLCanvasElement;
    if (!canvas) return;

    if (this.monthChart) this.monthChart.destroy();

    let chartConfig: any = {};

    if (type === 'year') {
      const outputsByYear = this.rawResponse?.outputsByYear || [];
      const labels = outputsByYear.map((o: any) => o.year) || [];
      const data = outputsByYear.map((o: any) => o.value) || [];

      chartConfig = {
        type: 'bar',
        data: {
          labels: labels.length > 0 ? labels : ['No Data'],
          datasets: [{
            label: 'Outputs',
            data: data.length > 0 ? data : [0],
            backgroundColor: '#6366f1',
            hoverBackgroundColor: '#4f46e5',
            borderRadius: { topLeft: 4, topRight: 4, bottomLeft: 0, bottomRight: 0 },
            barThickness: 32
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              backgroundColor: '#1e293b',
              titleColor: '#f3f4f6',
              bodyColor: '#f3f4f6',
              borderColor: 'rgba(255,255,255,0.1)',
              borderWidth: 1,
              padding: 12
            }
          },
          scales: {
            y: { 
              grid: { color: 'rgba(255, 255, 255, 0.03)' }, 
              ticks: { color: '#6b7280', font: { size: 11 }, padding: 10 },
              border: { display: false }
            },
            x: { 
              grid: { display: false }, 
              ticks: { color: '#9ca3af', font: { size: 12 }, padding: 10 },
              border: { display: false }
            }
          }
        }
      };
    } else if (type === 'country') {
      const outputsByCountry = this.rawResponse?.outputsByCountry || [];
      // Sort and take top 8 countries for clean display
      const sortedCountries = [...outputsByCountry].sort((a, b) => b.value - a.value).slice(0, 8);
      const labels = sortedCountries.map((o: any) => o.country) || [];
      const data = sortedCountries.map((o: any) => o.value) || [];

      chartConfig = {
        type: 'bar',
        data: {
          labels: labels.length > 0 ? labels : ['No Data'],
          datasets: [{
            label: 'Outputs',
            data: data.length > 0 ? data : [0],
            backgroundColor: '#10b981',
            hoverBackgroundColor: '#059669',
            borderRadius: { topLeft: 0, topRight: 4, bottomLeft: 0, bottomRight: 4 },
            barThickness: 16
          }]
        },
        options: {
          indexAxis: 'y',
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              backgroundColor: '#1e293b',
              titleColor: '#f3f4f6',
              bodyColor: '#f3f4f6',
              borderColor: 'rgba(255,255,255,0.1)',
              borderWidth: 1,
              padding: 12
            }
          },
          scales: {
            x: { 
              grid: { color: 'rgba(255, 255, 255, 0.03)' }, 
              ticks: { color: '#6b7280', font: { size: 11 }, padding: 10 },
              border: { display: false }
            },
            y: { 
              grid: { display: false }, 
              ticks: { color: '#9ca3af', font: { size: 11 }, padding: 10 },
              border: { display: false }
            }
          }
        }
      };
    } else if (type === 'program') {
      const outputsByProgram = this.rawResponse?.outputsByProgram || [];
      const sortedPrograms = [...outputsByProgram].sort((a, b) => b.value - a.value).slice(0, 6);
      const labels = sortedPrograms.map((o: any) => o.program) || [];
      const data = sortedPrograms.map((o: any) => o.value) || [];
      const colors = ['#6366f1', '#10b981', '#f59e0b', '#06b6d4', '#ec4899', '#8b5cf6'];

      chartConfig = {
        type: 'doughnut',
        data: {
          labels: labels.length > 0 ? labels : ['No Data'],
          datasets: [{
            data: data.length > 0 ? data : [0],
            backgroundColor: colors.slice(0, labels.length),
            borderWidth: 1,
            borderColor: '#1e293b'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { 
              display: true, 
              position: 'right', 
              labels: { 
                color: '#9ca3af',
                font: { size: 11 },
                boxWidth: 12
              } 
            },
            tooltip: {
              backgroundColor: '#1e293b',
              titleColor: '#f3f4f6',
              bodyColor: '#f3f4f6',
              borderColor: 'rgba(255,255,255,0.1)',
              borderWidth: 1,
              padding: 12
            }
          },
          scales: {
            x: { display: false },
            y: { display: false }
          }
        }
      };
    }

    this.monthChart = new Chart(canvas, chartConfig);
  }
}
