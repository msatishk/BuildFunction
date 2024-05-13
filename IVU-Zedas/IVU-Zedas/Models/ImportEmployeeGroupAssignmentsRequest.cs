using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{
    [XmlRoot(ElementName = "importDateRange")]
    public class EmpImportDateRange
    {
        [XmlElement(ElementName = "startDate")]
        public DateTime StartDate { get; set; }
        [XmlElement(ElementName = "endDate")]
        public DateTime EndDate { get; set; }
    }

    [XmlRoot(ElementName = "assignmentDateRange")]
    public class EmpAssignmentDateRange
    {
        [XmlElement(ElementName = "startDate")]
        public DateTime StartDate { get; set; }
        [XmlElement(ElementName = "endDate")]
        public DateTime EndDate { get; set; }
    }

    [XmlRoot(ElementName = "employeeGroupAssignment")]
    public class EmployeeGroupAssignment
    {
        [XmlElement(ElementName = "group")]
        public string Group { get; set; }
        [XmlElement(ElementName = "groupType")]
        public string GroupType { get; set; }
        [XmlElement(ElementName = "importDateRange")]
        public EmpImportDateRange EmpImportDateRange { get; set; }
        [XmlElement(ElementName = "assignmentDateRange")]
        public List<EmpAssignmentDateRange> EmpAssignmentDateRange { get; set; }
    }

    [XmlRoot(ElementName = "employeeGroupAssignments")]
    public class EmployeeGroupAssignments
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "employeeGroupAssignment")]
        public List<EmployeeGroupAssignment> EmployeeGroupAssignment { get; set; }
    }

    [XmlRoot(ElementName = "importEmployeeGroupAssignmentsRequest")]
    public class ImportEmployeeGroupAssignmentsRequest
    {
        [XmlElement(ElementName = "employeeGroupAssignments")]
        public List<EmployeeGroupAssignments> EmployeeGroupAssignments { get; set; }
    }
}
