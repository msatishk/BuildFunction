using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{
    [XmlRoot(ElementName = "attributeAssignment")]
    public class AttributeAssignment
    {
        [XmlElement(ElementName = "attributeName")]
        public string AttributeName { get; set; }
        [XmlElement(ElementName = "attributeValue")]
        public string AttributeValue { get; set; }
    }

    [XmlRoot(ElementName = "attributeAssignments")]
    public class AttributeAssignments
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "attributeAssignment")]
        public List<AttributeAssignment> AttributeAssignment { get; set; }
    }

    [XmlRoot(ElementName = "importAttributeAssignmentsRequest")]
    public class ImportAttributeAssignmentsRequest
    {
        [XmlElement(ElementName = "attributeAssignments")]
        public List<AttributeAssignments> AttributeAssignments { get; set; }
    }

}
