import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReportsService } from '../../../core/services/reports.service';
import { Entity, REPORT_TYPES, SubmitReportRequest } from '../../../core/models/report.model';

/**
 * Component for submitting regulatory reports with file upload
 */
@Component({
  selector: 'app-submit-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './submit-report.component.html',
  styleUrl: './submit-report.component.scss'
})
export class SubmitReportComponent implements OnInit {
  private fb = inject(FormBuilder);
  private reportsService = inject(ReportsService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  reportForm!: FormGroup;
  entities: Entity[] = [];
  reportTypes = REPORT_TYPES;
  
  // File upload state
  selectedFile: File | null = null;
  selectedFileName: string = '';
  fileSizeFormatted: string = '';
  
  // Upload progress
  uploadProgress: number = 0;
  isUploading: boolean = false;
  
  // UI state
  isSubmitting: boolean = false;
  error: string = '';
  success: string = '';
  showMultipleEntities: boolean = false;

  // File validation constants
  readonly MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MB
  readonly ALLOWED_FILE_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
  readonly ALLOWED_EXTENSION = '.xlsx';

  ngOnInit(): void {
    this.initializeForm();
    this.loadUserEntities();
  }

  private initializeForm(): void {
    this.reportForm = this.fb.group({
      entityId: [null, Validators.required],
      reportType: ['', Validators.required],
      reportingPeriod: ['', [Validators.required, Validators.pattern(/^(Q[1-4]_\d{4}|Annual_\d{4}|Monthly_\d{2}_\d{4})$/)]]
    });
  }

  private loadUserEntities(): void {
    this.reportsService.getUserEntities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entities) => {
          this.entities = entities;
          
          if (entities.length === 1) {
            // Auto-select if user has only one entity
            this.reportForm.patchValue({ entityId: entities[0].entityId });
            this.showMultipleEntities = false;
          } else {
            this.showMultipleEntities = true;
          }
        },
        error: (error) => {
          this.error = 'Failed to load entities. Please refresh the page.';
          console.error('Failed to load entities:', error);
        }
      });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    
    if (!input.files || input.files.length === 0) {
      this.clearFileSelection();
      return;
    }

    const file = input.files[0];
    
    // Validate file
    const validation = this.validateFile(file);
    if (!validation.valid) {
      this.error = validation.error!;
      this.clearFileSelection();
      input.value = ''; // Reset file input
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    this.fileSizeFormatted = this.formatFileSize(file.size);
    this.error = '';
  }

  private validateFile(file: File): { valid: boolean; error?: string } {
    // Check file extension
    const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (extension !== this.ALLOWED_EXTENSION) {
      return { valid: false, error: 'Only .xlsx files are allowed' };
    }

    // Check content type
    if (file.type !== this.ALLOWED_FILE_TYPE) {
      return { valid: false, error: 'Invalid file type. Please upload a valid XLSX file.' };
    }

    // Check file size
    if (file.size === 0) {
      return { valid: false, error: 'File is empty' };
    }

    if (file.size > this.MAX_FILE_SIZE) {
      return { valid: false, error: `File too large. Maximum size is 100 MB.` };
    }

    return { valid: true };
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  clearFileSelection(): void {
    this.selectedFile = null;
    this.selectedFileName = '';
    this.fileSizeFormatted = '';
  }

  onSubmit(): void {
    // Clear previous messages
    this.error = '';
    this.success = '';

    // Validate form
    if (this.reportForm.invalid) {
      this.error = 'Please fill in all required fields';
      this.markFormGroupTouched(this.reportForm);
      return;
    }

    // Validate file
    if (!this.selectedFile) {
      this.error = 'Please select a file to upload';
      return;
    }

    // Prepare request
    const request: SubmitReportRequest = {
      entityId: this.reportForm.value.entityId,
      reportType: this.reportForm.value.reportType,
      reportingPeriod: this.reportForm.value.reportingPeriod,
      file: this.selectedFile
    };

    // Submit report
    this.isSubmitting = true;
    this.isUploading = true;
    this.uploadProgress = 0;

    this.reportsService.submitReport(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (result.progress !== undefined) {
            // Update progress
            this.uploadProgress = result.progress;
          }
          
          if (result.response) {
            // Upload complete
            this.isUploading = false;
            this.isSubmitting = false;
            this.success = `Report submitted successfully! Report ID: ${result.response.reportId}. Status: ${result.response.status}. Your report is being validated.`;
            
            // Reset form after 3 seconds
            setTimeout(() => {
              this.resetForm();
              // TODO: Navigate to reports list or report details page
              // this.router.navigate(['/reports']);
            }, 3000);
          }
        },
        error: (error) => {
          this.isUploading = false;
          this.isSubmitting = false;
          this.error = error.message || 'Failed to submit report. Please try again.';
          console.error('Submit report error:', error);
        }
      });
  }

  resetForm(): void {
    this.reportForm.reset();
    this.clearFileSelection();
    this.uploadProgress = 0;
    this.error = '';
    this.success = '';
    
    // Re-select entity if user has only one
    if (this.entities.length === 1) {
      this.reportForm.patchValue({ entityId: this.entities[0].entityId });
    }
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  // Getter methods for template
  get entityId() { return this.reportForm.get('entityId'); }
  get reportType() { return this.reportForm.get('reportType'); }
  get reportingPeriod() { return this.reportForm.get('reportingPeriod'); }
}

