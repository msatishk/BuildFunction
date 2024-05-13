using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{
    [XmlRoot(ElementName = "importDateRange")]
    public class ImportDateRange
    {
        [XmlElement(ElementName = "startDate")]
        public DateTime StartDate { get; set; }
        [XmlElement(ElementName = "endDate")]
        public DateTime EndDate { get; set; }

        [XmlElement(ElementName = "startDateSpecified")]
        public bool StartDateSpecified { get; set; }

        [XmlElement(ElementName = "endDateSpecified")]
        public bool EndDateSpecified { get; set; }

    }

    [XmlRoot(ElementName = "assignmentDateRange")]
    public class AssignmentDateRange
    {
        [XmlElement(ElementName = "startDate")]
        public DateTime StartDate { get; set; }

        [XmlElement(ElementName = "endDate")]
        public DateTime EndDate { get; set; }

        [XmlElement(ElementName = "startDateSpecified")]
        [XmlIgnore()]
        public bool StartDateSpecified { get; set; }
        [XmlIgnore()]
        [XmlElement(ElementName = "endDateSpecified")]
        public bool EndDateSpecified { get; set; }
    }

    [XmlRoot(ElementName = "qualificationAssignment")]
    public class QualificationAssignment
    {
        [XmlElement(ElementName = "importDateRange")]
        public ImportDateRange ImportDateRange { get; set; }
        [XmlElement(ElementName = "assignmentDateRange", IsNullable = true)]
        public List<AssignmentDateRange> AssignmentDateRange { get; set; }
        [XmlElement(ElementName = "externalNumber")]
        public string ExternalNumber { get; set; }

        [XmlElement(ElementName = "qualification")]
        public string Qualification { get; set; }

        [XmlElement(ElementName = "qualificationClass")]
        public string QualificationClass { get; set; }

        [XmlElement(ElementName = "qualificationGroup")]
        public string QualificationGroup{ get; set; }
    }

    [XmlRoot(ElementName = "qualificationAssignments")]
    public class QualificationAssignments
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "qualificationAssignment")]
        public List<QualificationAssignment> QualificationAssignment { get; set; }
    }

    [XmlRoot(ElementName = "importQualificationAssignmentsRequest")]
    public class ImportQualificationAssignmentsRequest
    {
        [XmlElement(ElementName = "qualificationAssignments")]
        public List<QualificationAssignments> QualificationAssignments { get; set; }
    }




}
