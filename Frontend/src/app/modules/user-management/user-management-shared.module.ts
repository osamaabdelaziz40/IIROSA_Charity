import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserManagementComponent } from './user-management.component';

@NgModule({
  declarations: [UserManagementComponent],
  imports: [CommonModule],
  exports: [UserManagementComponent]
})
export class UserManagementSharedModule {}
