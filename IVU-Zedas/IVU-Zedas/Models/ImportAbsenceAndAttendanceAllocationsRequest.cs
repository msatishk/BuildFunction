using IVUWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{
    [XmlRoot(ElementName = "importTimeRange")]
    public class ImportTimeRange
    {
        [XmlElement(ElementName = "startTime")]
        public DateTime StartTime { get; set; }
        [XmlElement(ElementName = "endTime")]
        public DateTime EndTime { get; set; }
    }

    [XmlRoot(ElementName = "allocationTimeRange")]
    public class AllocationTimeRange
    {
        [XmlElement(ElementName = "startTime")]
        public DateTime StartTime { get; set; }
        [XmlElement(ElementName = "endTime")]
        public DateTime EndTime { get; set; }
    }

    [XmlRoot(ElementName = "allocation")]
    public class Allocation
    {
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "abbreviation")]
        public string Abbreviation { get; set; }
        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "planningLevel")]
        public string PlanningLevel { get; set; }
        [XmlElement(ElementName = "importTimeRange")]
        public ImportTimeRange ImportTimeRange { get; set; }
        [XmlElement(ElementName = "strategy")]
        public string Strategy { get; set; }
        [XmlElement(ElementName = "allocationTimeRange")]
        public List<AllocationTimeRange> AllocationTimeRange { get; set; }
        [XmlElement(ElementName = "paidTime")]
        public long PaidTime { get; set; }
        [XmlElement(ElementName = "workTime")]
        public long WorkTime { get; set; }
    }

    [XmlRoot(ElementName = "absenceAndAttendanceAllocations")]
    public class AbsenceAndAttendanceAllocations
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "allocation")]
        public List<Allocation> Allocation { get; set; }
    }

    [XmlRoot(ElementName = "importAbsenceAndAttendanceAllocationsRequest")]
    public class ImportAbsenceAndAttendanceAllocationsRequest
    {
        [XmlElement(ElementName = "absenceAndAttendanceAllocations")]
        public List<AbsenceAndAttendanceAllocations> AbsenceAndAttendanceAllocations { get; set; }
    }
}
