export interface DashboardStatistics {
  totalCharities: number;
  totalOrphans: number;
  totalFamilies: number;
  activeMissions: number;
  months: string[];
  charitiesCreatedByMonth: number[];
  orphansCreatedByMonth: number[];
  allocatedPaymentsByMonth: number[];
  receivedPaymentsByMonth: number[];
  familiesCreatedByMonth: number[];
  paymentStatusLabels: string[];
  paymentStatusValues: number[];
  missionQuarterLabels: string[];
  completedMissionsByQuarter: number[];
  pendingMissionsByQuarter: number[];
  activeCharityNames: string[];
  activeCharityInteractions: number[];
  familyCharityNames: string[];
  familiesByCharity: number[];
  orphanCharityNames: string[];
  orphansByCharity: number[];
  activeUserNames: string[];
  userInteractions: number[];
  maleOrphans: number;
  femaleOrphans: number;
}
