
using System.Xml.Serialization;

namespace ToIVUDeploymentRestrictions
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [XmlElement(ElementName = "createDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
        public CreateDeploymentRestriction CreateDeploymentRestriction { get; set; }

        [XmlElement(ElementName = "modifyDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
        public ModifyDeploymentRestriction ModifyDeploymentRestriction { get; set; }

        [XmlElement(ElementName = "deleteDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
        public DeleteDeploymentRestriction DeleteDeploymentRestriction { get; set; }
    }

    [XmlRoot(ElementName = "createDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
    public class CreateDeploymentRestriction
    {
        public string division { get; set; }
        public string vehicle { get; set; }
        public DateTimeDTO startTime { get; set; }
        public DateTimeDTO endTime { get; set; }
        public string reason { get; set; }
        public string comment { get; set; }
        public string user { get; set; }
        public string serializedKey { get; set; }
    }

    [XmlRoot(ElementName = "modifyDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
    public class ModifyDeploymentRestriction
    {
        public string division { get; set; }
        public string vehicle { get; set; }
        public DateTimeDTO startTime { get; set; }
        public DateTimeDTO endTime { get; set; }
        public string reason { get; set; }
        public string comment { get; set; }
        public string user { get; set; }
        public string serializedKey { get; set; }
    }

    [XmlRoot(ElementName = "deleteDeploymentRestriction", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
    public class DeleteDeploymentRestriction
    {
        public string division { get; set; }
        public string vehicle { get; set; }
        public DateTimeDTO startTime { get; set; }
        public DateTimeDTO endTime { get; set; }
        public string reason { get; set; }
        public string comment { get; set; }
        public string user { get; set; }
        public string serializedKey { get; set; }
    }

    [XmlRoot(ElementName = "DateTime", Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]
    public class DateTimeDTO
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
    }

    [XmlRoot(ElementName = "realTimeInfoTO")]
    public class RealTimeInfoTODTO
    {
        [XmlElement(ElementName = "division")]
        public string Division { get; set; }
        [XmlElement(ElementName = "tripNumber")]
        public string TripNumber { get; set; }
        [XmlElement(ElementName = "tripIdentificationDate")]
        public long TripIdentificationDate { get; set; }
        [XmlElement(ElementName = "stopArea")]
        public string StopArea { get; set; }
        [XmlElement(ElementName = "distanceToStopArea")]
        public int DistanceToStopArea { get; set; }
        [XmlElement(ElementName = "eventCode")]
        public int EventCode { get; set; }
        [XmlElement(ElementName = "timeStamp")]
        public long TimeStamp { get; set; }
    }

    [XmlRoot(ElementName = "importRealTimeInfo", Namespace = "http://web.facade.ejb.fzd.mb.ivu.de/jaws")]
    public class ImportRealTimeInfoDTO
    {
        [XmlElement(ElementName = "realTimeInfoTO")]
        public RealTimeInfoTODTO[] RealTimeInfoTODTO { get; set; }
    }
}
