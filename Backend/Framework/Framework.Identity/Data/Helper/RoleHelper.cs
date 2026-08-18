using System;

namespace Framework.Identity.Data.Helper
{
    public static class RoleHelper
    {
        public static Guid CentralOperationsGM { get; set; } = new Guid("6f1b622f-1c3b-4fff-91c5-1b9c812a9e81");
        public static Guid DebtDepartmentManager { get; set; } = new Guid("6f1b622f-1c3b-4fff-91c5-1b9c812a9e8a");
        public static Guid Company { get; set; } = new Guid("3c3fe487-8a89-4483-bbf2-3dc8a5237fc3");
        public static Guid PortSupervisor { get; set; } = new Guid("558175a1-6a94-458d-94d7-4970680aa00b");
        public static Guid SectorHead { get; set; } = new Guid("154cab1c-59de-44a5-a58f-6462237fb579");
        public static Guid AuctionManager { get; set; } = new Guid("a4366143-d75f-4b2b-8098-6613e694be0e");
        public static Guid Individual { get; set; } = new Guid("ba1958cf-1712-4081-88ab-727aea87e1a9");
        public static Guid ExternalIndividual { get; set; } = new Guid("29E99891-1081-4D1D-90B9-0FCA0B45FAE1");
        public static Guid AuctionSupervisor { get; set; } = new Guid("42e4b4fa-366d-47a8-a72a-97450064acca");
        public static Guid Admin { get; set; } = new Guid("14e1d238-9886-459c-af9e-df62011f8153");
        public static Guid CentralOperationsOfficer { get; set; } = new Guid("86487c12-0903-42b7-af34-e6b2634629fc");
        public static Guid CommitmentOfficer { get; set; } = new Guid("941f0833-44f4-4766-85d1-f0c546972202");
        public static Guid CommitteeUser { get; set; } = new Guid("1f64eb96-40f2-4a55-91c0-ed6f8b887a93");
        public static Guid PortTechnicalSupportOfficer { get; set; } = new Guid("a19b49c1-4874-4286-94c8-cb3ffa156d7d");
        public static Guid DestructionSupervisor { get; set; } = new Guid("ef106849-6607-44eb-8be5-4a6356a01ead");
        public static Guid CorporateCommunicationMember { get; set; } = new Guid("f1032b5f-9ac0-486e-adad-40c7e0eeaec4");
        public static Guid LicenseTeam { get; set; } = new Guid("AC40739C-EA5A-4220-A39E-4514591D6CCF");
    }
}