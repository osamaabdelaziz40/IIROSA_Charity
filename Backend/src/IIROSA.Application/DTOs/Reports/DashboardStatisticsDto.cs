namespace IIROSA.Application.DTOs.Reports;

/// <summary>Database-backed chart and headline data for the home page.</summary>
public sealed class DashboardStatisticsDto
{
    public int TotalCharities { get; set; }
    public int TotalOrphans { get; set; }
    public int TotalFamilies { get; set; }
    public int ActiveMissions { get; set; }
    public List<string> Months { get; set; } = new();
    public List<int> CharitiesCreatedByMonth { get; set; } = new();
    public List<int> OrphansCreatedByMonth { get; set; } = new();
    public List<decimal> AllocatedPaymentsByMonth { get; set; } = new();
    public List<decimal> ReceivedPaymentsByMonth { get; set; } = new();
    public List<int> FamiliesCreatedByMonth { get; set; } = new();
    public List<string> PaymentStatusLabels { get; set; } = new();
    public List<int> PaymentStatusValues { get; set; } = new();
    public List<string> MissionQuarterLabels { get; set; } = new();
    public List<int> CompletedMissionsByQuarter { get; set; } = new();
    public List<int> PendingMissionsByQuarter { get; set; } = new();
    public List<string> ActiveCharityNames { get; set; } = new();
    public List<int> ActiveCharityInteractions { get; set; } = new();
    public List<string> FamilyCharityNames { get; set; } = new();
    public List<int> FamiliesByCharity { get; set; } = new();
    public List<string> OrphanCharityNames { get; set; } = new();
    public List<int> OrphansByCharity { get; set; } = new();
    public List<string> ActiveUserNames { get; set; } = new();
    public List<int> UserInteractions { get; set; } = new();
    public int MaleOrphans { get; set; }
    public int FemaleOrphans { get; set; }
}
