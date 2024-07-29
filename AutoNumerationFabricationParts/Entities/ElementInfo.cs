using Autodesk.Revit.DB;

namespace AutoNumerationFabricationParts_R2022.Entities
{
    public class ElementInfo
    {
        public ElementId ElementId { get; set; }

        //(Size)_(Length)_(InsulationType)_(IsAStraigth)_(AngleValue) OR "NotFabricationPart" if value is another type
        public string GeometryCode { get; set; } 

        public ElementInfo(ElementId elementId, string geometryChecksum)
        {
            ElementId = elementId;
            GeometryCode = geometryChecksum;
        }
    }
}
