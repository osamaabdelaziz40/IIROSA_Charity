import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DiagnosticService {
  constructor(private http: HttpClient) {}

  runDiagnostics() {
    console.log('=== Frontend Diagnostics ===');
    console.log('Environment:', environment);
    console.log('API URL:', environment.apiUrl);

    // Check localStorage
    const token = localStorage.getItem('accessToken');
    const user = localStorage.getItem('currentUser');

    console.log('Authentication Status:');
    console.log('- Token exists:', !!token);
    console.log('- Token length:', token?.length || 0);
    console.log('- Token preview:', token ? `${token.substring(0, 20)}...` : 'none');
    console.log('- User exists:', !!user);
    console.log('- User data:', user ? JSON.parse(user) : 'none');

    // Test API connection
    console.log('\n=== Testing API Connection ===');

    this.testBackendConnection();
  }

  testBackendConnection() {
    // Test 1: Simple GET request to health endpoint
    console.log('Test 1: Backend Health Check');
    this.http.get(`${environment.apiUrl}/health`).subscribe({
      next: (response) => console.log('✅ Backend is reachable:', response),
      error: (error) => console.log('❌ Backend health check failed:', error.message)
    });

    // Test 2: Test login endpoint (will fail with bad credentials, but shows connection)
    console.log('\nTest 2: Auth Endpoint Connection');
    this.http.post(`${environment.apiUrl}/auth/login`, {
      email: 'test@test.com',
      password: 'wrongpassword'
    }).subscribe({
      next: (response) => console.log('✅ Auth endpoint response:', response),
      error: (error) => {
        if (error.status === 401) {
          console.log('✅ Auth endpoint is reachable (401 = bad credentials expected)');
        } else {
          console.log('❌ Auth endpoint error:', error.message, error.status);
        }
      }
    });

    // Test 3: Test users endpoint (will fail with 401 if no auth, but shows connection)
    console.log('\nTest 3: Users Endpoint Connection');
    this.http.get(`${environment.apiUrl}/api/usermanagement?pageNumber=1&pageSize=10`).subscribe({
      next: (response) => console.log('✅ Users endpoint response:', response),
      error: (error) => {
        if (error.status === 401) {
          console.log('✅ Users endpoint is reachable (401 = authentication required)');
        } else {
          console.log('❌ Users endpoint error:', error.message, error.status);
        }
      }
    });
  }

  checkAuthentication() {
    console.log('=== Authentication Check ===');

    const token = localStorage.getItem('accessToken');
    if (!token) {
      console.log('❌ No authentication token found');
      console.log('Please login first');
      return;
    }

    console.log('✅ Token found, testing API calls with authentication...');

    this.http.get(`${environment.apiUrl}/api/usermanagement?pageNumber=1&pageSize=10`).subscribe({
      next: (response) => {
        console.log('✅ Authenticated API call successful:', response);
      },
      error: (error) => {
        console.log('❌ Authenticated API call failed:', {
          status: error.status,
          message: error.message,
          error: error.error
        });
      }
    });
  }
}
