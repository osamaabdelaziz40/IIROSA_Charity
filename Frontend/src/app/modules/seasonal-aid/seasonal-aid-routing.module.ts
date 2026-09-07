import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

import { CampaignListComponent } from './campaign-list/campaign-list.component';
import { CampaignFormComponent } from './campaign-form/campaign-form.component';
import { CampaignDetailComponent } from './campaign-detail/campaign-detail.component';
import { BeneficiarySelectionComponent } from './beneficiary-selection/beneficiary-selection.component';
import { DistributionRecordComponent } from './distribution-record/distribution-record.component';
import { EligibleFamiliesComponent } from './eligible-families/eligible-families.component';
import { CampaignReportComponent } from './campaign-report/campaign-report.component';

const routes: Routes = [
  {
    path: '',
    component: CampaignListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.title',
      permission: 'SeasonalAid.View'
    }
  },
  {
    path: 'create',
    component: CampaignFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.createCampaign',
      permission: 'SeasonalAid.Create'
    }
  },
  {
    path: ':id',
    component: CampaignDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.campaignDetails',
      permission: 'SeasonalAid.View'
    }
  },
  {
    path: ':id/edit',
    component: CampaignFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.editCampaign',
      permission: 'SeasonalAid.Edit'
    }
  },
  {
    path: ':id/beneficiaries',
    component: BeneficiarySelectionComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.manageBeneficiaries',
      permission: 'SeasonalAid.ManageBeneficiaries'
    }
  },
  {
    path: ':id/eligible-families',
    component: EligibleFamiliesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.eligibleFamilies',
      permission: 'SeasonalAid.View'
    }
  },
  {
    path: ':id/report',
    component: CampaignReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.campaignReport',
      permission: 'SeasonalAid.Reports'
    }
  },
  {
    path: ':id/distribution',
    component: DistributionRecordComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'seasonalAid.recordDistribution',
      permission: 'SeasonalAid.RecordDistribution'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SeasonalAidRoutingModule {}
