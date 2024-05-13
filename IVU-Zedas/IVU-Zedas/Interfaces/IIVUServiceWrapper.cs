using IVUWS;
using System.Collections.Generic;

namespace ToIVUMultipleFromOracle.Interfaces
{
    public interface IIVUServiceWrapper
    {
        IEnumerable<employeeResult> ImportStaffMemberships(string request);
        IEnumerable<employeeResult> ImportPersonnelData(string request);
        //IEnumerable<employeeResult> ImportDepotAssignments(string request);
        //IEnumerable<employeeResult> ImportCostCenterAssignments(string request);
        IEnumerable<employeeResult> ImportQualificationAssignmentsRequest(string request);
        IEnumerable<employeeResult> ImportAttributeAssignmentsRequest(string request);
        IEnumerable<employeeResult> ImportEmployeeGroupAssignments(string request);
        IEnumerable<employeeResult> ImportAbsenceAndAttendanceAllocationsRequest(string request);

    }
}