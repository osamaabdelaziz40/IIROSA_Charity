namespace Framework.Identity.Data
{
    public enum Roles
    {
        Admin,//مدير النظام
        Employee,//Default Role الموظف
        AdministrationAndFacilitiesDepartment,
        HostingDirectManager,
        VisaApplicationDirectManager,
        AdministrativeServicesManager,
        DirectorOfAdministrativeAffairsAndFacilitiesDepartment,
        DeputyJointServices
    }

    public enum RolesCategory
    {
        All,
        KMS,
        Referral
    }

    public enum RoleType
    {
        Dept = 1,
        Subdept = 2,
        Director = 3,
        Manager = 4
    }
}