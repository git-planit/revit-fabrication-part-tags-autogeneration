using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AutoNumerationFabricationParts_R2022.Entities;
using AutoNumerationFabricationParts_R2022.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022.Models
{
    internal class ConnectedItemsCollector
    {
        private UIDocument _uiDoc;
        private Document _doc;
        private Element _startElement;

        private List<ElementInfo> elementInfos = new List<ElementInfo>();

        public ConnectedItemsCollector(UIDocument uiDoc, Document doc)
        {
            _uiDoc = uiDoc;
            _doc = doc;

            try {
                Element startElement = uiDoc.PickElement(e => e is FabricationPart, new OpenDocumentOption(), "Please, pick first element");
                if (startElement != null) _startElement = startElement;
            } catch (Exception ex) {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public List<ElementInfo> GetAllConnectedElements()
        {
            TraverseConnectedParts(_startElement, null, elementInfos, _doc);

            return elementInfos;
        }

        private void TraverseConnectedParts(Element element, Connector referenceConnector, List<ElementInfo> elementInfos, Document doc)
        {
            // Check if elementInfos contains elementId. If not then plain return
            ElementInfo elementInfo = elementInfos.FirstOrDefault(e => e.ElementId == element.Id);
            if (elementInfo != null) return;

            // Store data about visited item
            ConnectorSet connectors;
            FabricationPart currentItem = null;
            if (element is FabricationPart fabricationPart)
            {
                string size = fabricationPart.Size;
                double centerlineLength = Math.Round(fabricationPart.CenterlineLength, 3); // Rounding to 3 decimal places
                string insulationType = fabricationPart.InsulationType;
                bool isAStraight = fabricationPart.IsAStraight();
                string angle = null;
                if (!isAStraight)
                {
                    Parameter angleParam = fabricationPart.get_Parameter(BuiltInParameter.FABRICATION_PART_ANGLE);
                    if (angleParam != null) angle = angleParam.AsValueString();
                }

                string elementCode = $"{size}_{centerlineLength}_{insulationType}_{isAStraight}_{angle}";
                ElementInfo elementData = new ElementInfo(fabricationPart.Id, elementCode);
                elementInfos.Add(elementData);

                connectors = fabricationPart.ConnectorManager.Connectors;
                currentItem = fabricationPart;
            }
            else if (element is FamilyInstance familyInstance && familyInstance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory)
            {
                ElementInfo elementData = new ElementInfo(familyInstance.Id, "NotFabricationPart"); // It means that the element is not a FabricationPart
                elementInfos.Add(elementData);
                connectors = familyInstance.MEPModel.ConnectorManager.Connectors;
            }
            else
            {
                return; // If the element is neither a FabricationPart nor a DuctAccessory, exit the method.
            }

            // Traverse to opposite connected item if the current item is a straight FabricationPart
            List<Connector> connectorList = connectors.Cast<Connector>().ToList();
            if (currentItem is FabricationPart
                && currentItem.IsAStraight()
                && connectorList.Count > 2
                && referenceConnector != null)
            {
                Connector oppositeConnector = FindOppositeConnector(referenceConnector, connectorList);
                if (oppositeConnector != null)
                {
                    ElementId nextItemElementId = null;
                    foreach (Connector connector in oppositeConnector.AllRefs)
                    {
                        if (connector.Owner.Id != element.Id)
                        {
                            nextItemElementId = connector.Owner.Id;
                            break;
                        }
                    }
                    Element ownerElement = doc.GetElement(nextItemElementId);
                    //traverse elements that are opposite to our current element connection
                    TraverseConnectedParts(ownerElement, oppositeConnector, elementInfos, doc);
                }
            }

            // Traverse to other connected items with recursive
            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    Element connectedElement = doc.GetElement(connectedConnector.Owner.Id);
                    if (connectedElement != null && elementInfos.FirstOrDefault(e => e.ElementId == connectedElement.Id) == null)
                    {
                        // Check if the connected element is a FabricationPart or a DuctAccessory
                        if (connectedElement is FabricationPart || (connectedElement is FamilyInstance connectedFamilyInstance
                            && connectedFamilyInstance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory))
                        {
                            // Recursively traverse connected elements
                            TraverseConnectedParts(connectedElement, connectedConnector, elementInfos, doc);
                        }
                    }
                }
            }
        }

        private Connector FindOppositeConnector(Connector referenceConnector, List<Connector> connectors)
        {
            Connector farthestConnector = null;
            double maxDistance = double.MinValue;

            foreach (Connector connector in connectors)
            {
                if (connector != referenceConnector)
                {
                    double distance = referenceConnector.Origin.DistanceTo(connector.Origin);

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestConnector = connector;
                    }
                }
            }

            return farthestConnector;
        }
    }
}
