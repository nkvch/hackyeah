import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { SubmitReportRequest, SubmitReportResponse, Entity } from '../models/report.model';

/**
 * Service for report submission and management
 */
@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api';

  /**
   * Submit a regulatory report with file upload
   * Returns progress updates during upload
   */
  submitReport(request: SubmitReportRequest): Observable<{ progress?: number; response?: SubmitReportResponse }> {
    const formData = new FormData();
    formData.append('entityId', request.entityId.toString());
    formData.append('reportType', request.reportType);
    formData.append('reportingPeriod', request.reportingPeriod);
    formData.append('file', request.file);

    return this.http.post<SubmitReportResponse>(`${this.apiUrl}/reports/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      map((event: HttpEvent<SubmitReportResponse>) => {
        switch (event.type) {
          case HttpEventType.UploadProgress:
            const progress = event.total ? Math.round(100 * event.loaded / event.total) : 0;
            return { progress };
          case HttpEventType.Response:
            return { response: event.body! };
          default:
            return {};
        }
      }),
      catchError(this.handleSubmitReportError)
    );
  }

  /**
   * Submit report without progress tracking (simpler version)
   */
  submitReportSimple(request: SubmitReportRequest): Observable<SubmitReportResponse> {
    const formData = new FormData();
    formData.append('entityId', request.entityId.toString());
    formData.append('reportType', request.reportType);
    formData.append('reportingPeriod', request.reportingPeriod);
    formData.append('file', request.file);

    return this.http.post<SubmitReportResponse>(`${this.apiUrl}/reports/upload`, formData)
      .pipe(
        catchError(this.handleSubmitReportError)
      );
  }

  /**
   * Get entities for current user (for entity selection dropdown)
   * TODO: Replace with real endpoint when Epic 2 is complete
   */
  getUserEntities(): Observable<Entity[]> {
    // For MVP, return hardcoded test entities
    // In production, this would call: GET /api/entities/my-entities
    const testEntities: Entity[] = [
      { entityId: 1001, name: 'Test Bank S.A.', type: 'Bank', isActive: true },
      { entityId: 1002, name: 'Test Insurance Company', type: 'Insurance', isActive: true },
      { entityId: 1003, name: 'Example Credit Union', type: 'Credit Union', isActive: true }
    ];
    
    return new Observable(observer => {
      setTimeout(() => {
        observer.next(testEntities);
        observer.complete();
      }, 100);
    });
  }

  private handleSubmitReportError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Failed to submit report';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = error.error?.error || 'Invalid file or request data. Please check your input.';
          break;
        case 401:
          errorMessage = 'You must be logged in to submit reports';
          break;
        case 403:
          errorMessage = error.error?.error || 'You don\'t have permission to submit reports for this entity';
          break;
        case 404:
          errorMessage = error.error?.error || 'Entity not found';
          break;
        case 409:
          errorMessage = error.error?.error || 'A report for this period already exists';
          break;
        case 413:
          errorMessage = 'File too large. Maximum size is 100 MB.';
          break;
        case 500:
          errorMessage = 'Server error. Please try again later.';
          break;
        default:
          if (error.error?.error) {
            errorMessage = error.error.error;
          }
      }
    }

    console.error('Submit report error:', error);
    return throwError(() => new Error(errorMessage));
  }
}

