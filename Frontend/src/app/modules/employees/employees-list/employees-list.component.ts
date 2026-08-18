import { Component, OnInit } from '@angular/core';
import { HttpService } from '../../../core/services/http.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PaginationComponent } from '../../../shared/components';

export interface Employee {
  id: string;
  code: string;
  fullName: string;
  nationalId?: string;
  dateOfBirth?: string;
  gender?: string;
  phoneNumber?: string;
  email?: string;
  address?: string;
  departmentId?: number;
  departmentName?: string;
  position?: string;
  hireDate?: string;
  salary?: number;
  isActive: boolean;
  notes?: string;
  createdDate: string;
  modifiedDate: string;
}

export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortBy?: string;
  sortDirection: string;
}

export interface PagedResponse {
  data: Employee[];
  totalRecords: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

@Component({
  selector: 'app-employees-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PaginationComponent],
  templateUrl: './employees-list.component.html',
  styleUrls: ['./employees-list.component.scss']
})
export class EmployeesListComponent implements OnInit {
  employees: Employee[] = [];
  loading: boolean = false;
  pagination: PagedResponse | null = null;
  currentPage: number = 1;
  pageSize: number = 10;
  searchTerm: string = '';

  columns = [
    { key: 'code', title: 'employees.employeeCode', sortable: true },
    { key: 'fullName', title: 'employees.fullName', sortable: true },
    { key: 'nationalId', title: 'employees.nationalId', sortable: false },
    { key: 'departmentName', title: 'employees.department', sortable: false },
    { key: 'position', title: 'employees.position', sortable: false },
    { key: 'isActive', title: 'employees.isActive', sortable: true }
  ];

  constructor(private httpService: HttpService) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading = true;
    const request: PagedRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      sortBy: 'fullName',
      sortDirection: 'asc'
    };

    this.httpService.post<PagedResponse>('/employees/paged', request).subscribe({
      next: (response: PagedResponse) => {
        this.employees = response.data;
        this.pagination = response;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadEmployees();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadEmployees();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadEmployees();
  }

  onSort(column: string): void {
    // TODO: Implement sorting
  }

  onAction(action: string, employee: Employee): void {
    switch (action) {
      case 'view':
        console.log('View employee:', employee);
        break;
      case 'edit':
        console.log('Edit employee:', employee);
        break;
      case 'delete':
        if (confirm('Are you sure you want to delete this employee?')) {
          this.deleteEmployee(employee.id);
        }
        break;
    }
  }

  addEmployee(): void {
    console.log('Add new employee');
    // TODO: Navigate to create page
  }

  exportToExcel(): void {
    console.log('Export to Excel');
    // TODO: Implement export
  }

  deleteEmployee(id: string): void {
    this.httpService.delete(`/employees/${id}`).subscribe({
      next: () => {
        this.loadEmployees();
      }
    });
  }
}
