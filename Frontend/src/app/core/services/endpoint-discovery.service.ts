import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EndpointDiscoveryService {
  constructor(private http: HttpClient) {}

  // Common endpoint patterns to test
  private commonPatterns = {
    auth: ['/api/auth/login', '/api/login', '/auth/login', '/login', '/token'],
    users: ['/api/users', '/api/usermanagement', '/users', '/usermanagement', '/api/user', '/user'],
    roles: ['/api/roles', '/api/rolemanagement', '/roles', '/rolemanagement']
  };

  discoverEndpoints() {
    console.log('=== Discovering Backend Endpoints ===');
    console.log('Testing common endpoint patterns...');

    this.testAuthEndpoints();
    this.testUserEndpoints();
    this.testRoleEndpoints();
  }

  private testAuthEndpoints() {
    console.log('\n🔐 Testing Auth Endpoints:');

    this.commonPatterns.auth.forEach(endpoint => {
      this.http.post(`${environment.apiUrl || 'https://localhost:60960'}${endpoint}`, {
        email: 'test@test.com',
        password: 'test123'
      }).subscribe({
        next: () => console.log(`✅ Found auth endpoint: ${endpoint}`),
        error: (err) => {
          if (err.status !== 404) {
            console.log(`✅ Found auth endpoint: ${endpoint} (responded with ${err.status})`);
          }
        }
      });
    });
  }

  private testUserEndpoints() {
    console.log('\n👥 Testing User Endpoints:');

    this.commonPatterns.users.forEach(endpoint => {
      this.http.get(`${environment.apiUrl || 'https://localhost:60960'}${endpoint}`, {
        params: { pageNumber: 1, pageSize: 10 }
      }).subscribe({
        next: () => console.log(`✅ Found users endpoint: ${endpoint}`),
        error: (err) => {
          if (err.status !== 404) {
            console.log(`✅ Found users endpoint: ${endpoint} (responded with ${err.status})`);
          } else {
            console.log(`❌ Not found: ${endpoint}`);
          }
        }
      });
    });
  }

  private testRoleEndpoints() {
    console.log('\n🛡️ Testing Role Endpoints:');

    this.commonPatterns.roles.forEach(endpoint => {
      this.http.get(`${environment.apiUrl || 'https://localhost:60960'}${endpoint}`).subscribe({
        next: () => console.log(`✅ Found roles endpoint: ${endpoint}`),
        error: (err) => {
          if (err.status !== 404) {
            console.log(`✅ Found roles endpoint: ${endpoint} (responded with ${err.status})`);
          } else {
            console.log(`❌ Not found: ${endpoint}`);
          }
        }
      });
    });
  }

  testSpecificEndpoint(endpoint: string, method: 'GET' | 'POST' = 'GET', body?: any) {
    const url = `${environment.apiUrl || 'https://localhost:60960'}${endpoint}`;
    console.log(`Testing: ${method} ${url}`);

    if (method === 'GET') {
      return this.http.get(url);
    } else {
      return this.http.post(url, body);
    }
  }
}