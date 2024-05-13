using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{

    [XmlRoot(ElementName = "staffMemberships")]
    public class StaffMemberships 
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "hireDate")]
        public DateTime HireDate { get; set; }
    }

    [XmlRoot(ElementName = "importStaffMembershipsRequest")]//, Namespace = "http://www.ivu.de/pdp/PersonnelImportWebService")]
    public class ImportStaffMembershipsRequest
    {
        [XmlElement(ElementName = "staffMemberships", Namespace = "")]
        public List<StaffMemberships> StaffMemberships { get; set; }
    }

   

}

