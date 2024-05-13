//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

//namespace ToIVUMultipleFromOracle.Models
//{
//    [XmlRoot(ElementName = "importDateRange")]
//    public class ImportDateRangeDepot 
//    {
//        [XmlElement(ElementName = "startDate")]
//        public DateTime StartDate { get; set; }
//        [XmlElement(ElementName = "startDateSpecified")]
//        public bool StartDateSpecified { get; set; }

//        [XmlElement(ElementName = "endDateSpecified")]
//        public bool EndDateSpecified { get; set; }

//        [XmlElement(ElementName = "endDate")]
//        public DateTime EndDate { get; set; }
//    }

//    [XmlRoot(ElementName = "dateRange")]
//    public class DateRangeDepot
//    {
//        [XmlElement(ElementName = "startDate")]
//        public DateTime StartDate { get; set; }
//        [XmlElement(ElementName = "endDate")]
//        public DateTime EndDate { get; set; }
//    }

//    [XmlRoot(ElementName = "depotAssignment")]
//    public class DepotAssignment
//    {
//        [XmlElement(ElementName = "abbreviation")]
//        public string Abbreviation { get; set; }
//        [XmlElement(ElementName = "externalNumber")]
//        public string ExternalNumber { get; set; }
//        [XmlElement(ElementName = "dateRange")]
//        public DateRangeDepot DateRange { get; set; }
//        [XmlElement(ElementName = "endDateSpecified")]
//        public bool EndDateSpecified { get; set; }
//        [XmlElement(ElementName = "startDateSpecified")]
//        public bool StartDateSpecified { get; set; }

//    }

//    [XmlRoot(ElementName = "depotAssignments")]
//    public class DepotAssignments
//    {
//        [XmlElement(ElementName = "personnelNumber")]
//        public string PersonnelNumber { get; set; }
//        [XmlElement(ElementName = "importDateRange")]
//        public ImportDateRangeDepot ImportDateRangeDepot { get; set; }
//        [XmlElement(ElementName = "depotAssignment")]
//        public List<DepotAssignment> DepotAssignment { get; set; }
//    }

//    [XmlRoot(ElementName = "importDepotAssignmentsRequest")]
//    public class ImportDepotAssignmentsRequest
//    {
//        [XmlElement(ElementName = "depotAssignments")]
//        public List<DepotAssignments> DepotAssignments { get; set; }
//        [XmlElement(ElementName = "ignoreLock")]
//        public string IgnoreLock { get; set; }
//    }

  
//}
