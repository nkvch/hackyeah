import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { AccessRequestDto } from '../models/access-request.model';

@Injectable({
  providedIn: 'root'
})
export class AccessRequestService {
  private readonly apiUrl = 'http://localhost:8080/api/access-requests';

  constructor(private http: HttpClient) {}

  getMyAccessRequest(): Observable<AccessRequestDto> {
    return this.http.get<AccessRequestDto>(`${this.apiUrl}/my-request`).pipe(
      catchError(error => {
        console.error('Error fetching access request:', error);
        throw error;
      })
    );
  }
}

