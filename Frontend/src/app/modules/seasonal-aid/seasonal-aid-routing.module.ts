import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

import { CampaignListComponent } from './campaign-list/campaign-list.component';
import { CampaignFormComponent } from './campaign-form/campaign-form.component';
import { CampaignDetailComponent } from './campaign-detail/campaign-detail.component';
import { BeneficiarySelectionComponent } from './beneficiary-selection/beneficiary-selection.component';
import { DistributionRecordComponent } from './distribution-record/distribution-record.component';

const routes: Routes = [
  {
    path: '',
    component: CampaignListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.title',
      permission: 'seasonalaid.view'
    }
  },
  {
    path: 'create',
    component: CampaignFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.createCampaign',
      permission: 'seasonalaid.create'
    }
  },
  {
    path: ':id',
    component: CampaignDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.campaignDetails',
      permission: 'seasonalaid.view'
    }
  },
  {
    path: ':id/edit',
    component: CampaignFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.editCampaign',
      permission: 'seasonalaid.edit'
    }
  },
  {
    path: ':id/beneficiaries',
    component: BeneficiarySelectionComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.manageBeneficiaries',
      permission: 'seasonalaid.managebeneficiaries'
    }
  },
  {
    path: ':id/distribution',
    component: DistributionRecordComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.recordDistribution',
      permission: 'seasonalaid.recorddistribution'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SeasonalAidRoutingModule {}
