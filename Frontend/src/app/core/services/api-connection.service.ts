import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiConnectionService {
  private connectionStatusSubject = new BehaviorSubject<boolean>(false);
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor(private http: HttpClient) {}

  testConnection(): Observable<any> {
    // Test the basic API connection
    return this.http.get(`${environment.apiUrl}/health`);
  }

  testAuthEndpoint(): Observable<any> {
    // Test the auth endpoint
    return this.http.post(`${environment.apiUrl}/auth/login`, {
      email: 'test@test.com',
      password: 'test123'
    });
  }

  testUsersEndpoint(): Observable<any> {
    // Test the users endpoint (should return 401 if auth is working)
    return this.http.get(`${environment.apiUrl}/api/usermanagement?pageNumber=1&pageSize=10`);
  }

  checkConnectionStatus() {
    this.testConnection().subscribe({
      next: () => this.connectionStatusSubject.next(true),
      error: () => this.connectionStatusSubject.next(false)
    });
  }
}
