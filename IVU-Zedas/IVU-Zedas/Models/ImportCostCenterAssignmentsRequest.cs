//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

//namespace ToIVUMultipleFromOracle.Models
//{
//    [XmlRoot(ElementName = "importDateRange")]
//    public class ImportDateRangeCost
//    {
//        [XmlElement(ElementName = "startDate")]
//        public DateTime StartDate { get; set; }
//        [XmlElement(ElementName = "endDate")]
//        public DateTime EndDate { get; set; }

//        [XmlElement(ElementName = "startDateSpecified")]
//        public bool StartDateSpecified { get; set; }

//        [XmlElement(ElementName = "endDateSpecified")]
//        public bool EndDateSpecified { get; set; }

//    }

//    [XmlRoot(ElementName = "dateRange")]
//    public class DateRangeCost
//    {
//        [XmlElement(ElementName = "startDate")]
//        public DateTime StartDate { get; set; }
//        [XmlElement(ElementName = "endDate")]
//        public DateTime EndDate { get; set; }

//        [XmlElement(ElementName = "startDateSpecified")]
//        public bool StartDateSpecified { get; set; }

//        [XmlElement(ElementName = "endDateSpecified")]
//        public bool EndDateSpecified { get; set; }
//    }

//    [XmlRoot(ElementName = "costCenterAssignment")]
//    public class CostCenterAssignment
//    {
//        [XmlElement(ElementName = "abbreviation")]
//        public string Abbreviation { get; set; }
//        [XmlElement(ElementName = "dateRange")]
//        public DateRangeCost DateRange { get; set; }
//    }

//    [XmlRoot(ElementName = "costCenterAssignments")]
//    public class CostCenterAssignments
//    {
//        [XmlElement(ElementName = "personnelNumber")]
//        public string PersonnelNumber { get; set; }
//        [XmlElement(ElementName = "importDateRange")]
//        public ImportDateRangeCost ImportDateRange { get; set; }
//        [XmlElement(ElementName = "costCenterAssignment")]

//        public List<CostCenterAssignment> CostCenterAssignment { get; set; }
//    }

//    [XmlRoot(ElementName = "importCostCenterAssignmentsRequest")]
//    public class ImportCostCenterAssignmentsRequest
//    {
//        [XmlElement(ElementName = "costCenterAssignments")]
//        public List<CostCenterAssignments> CostCenterAssignments { get; set; }
//    }


//}
