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
        private List<Element> _selectedElements;
        private HashSet<ElementId> _visitedElements = new HashSet<ElementId>();

        private List<ElementInfo> elementInfos = new List<ElementInfo>();

        public ConnectedItemsCollector(UIDocument uiDoc, Document doc)
        {
            _uiDoc = uiDoc;
            _doc = doc;

            try
            {
                List<Element> selectedElements = uiDoc.GetSelectedElements();
                //List<Element> selectedElements = uiDoc.PickElements(e => e is FabricationPart, new OpenDocumentOption(), "Please, pick first element");
                if (selectedElements.Count > 0)
                {
                    _selectedElements = selectedElements;
                }
                //remove from selected elements list items were processed.
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public List<ElementInfo> GetAllConnectedElements()
        {
            //TraverseConnectedParts(_startElement, null, elementInfos, _doc);
            foreach (Element element in _selectedElements)
            {
                if (_visitedElements.Contains(element.Id)) continue;
                TraverseConnectedParts(element, elementInfos, _doc);
            }

            return elementInfos;
        }

        private void TraverseConnectedParts(Element startElement, List<ElementInfo> elementInfos, Document doc)
        {
            Stack<(Element, Connector)> stack = new Stack<(Element, Connector)>(); //stack is LIFO don't confuse with JS array`s methods
            stack.Push((startElement, null));

            Dictionary<ElementId, Connector> farthestConnectors = new Dictionary<ElementId, Connector>();

            while (stack.Count > 0)
            {
                var (element, referenceConnector) = stack.Pop();

                // Check if element has already been visited
                if (_visitedElements.Contains(element.Id)) continue;

                // Mark the element as visited
                _visitedElements.Add(element.Id);
                //if(element is FabricationPart fPart)
                //{
                //    var geometries = fPart.GetDimensions();
                //    foreach (var geom in geometries)
                //    {
                //        var name = geom.Name;
                //        var dim = fPart.GetDimensionValue(geom);
                //        var dimValue = true;
                //    }
                //}

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
                    continue; // If the element is neither a FabricationPart nor a DuctAccessory, continue to the next iteration
                }

                // Traverse to opposite connected item if the current item is a straight FabricationPart
                ElementId oppositeElementId = null;
                List<Connector> connectorList = connectors.Cast<Connector>().ToList();
                if (currentItem is FabricationPart
                    && currentItem.IsAStraight()
                    && connectorList.Count > 2
                    && referenceConnector != null)
                {
                    if (!farthestConnectors.TryGetValue(element.Id, out Connector oppositeConnector))
                    {
                        oppositeConnector = FindOppositeConnector(referenceConnector, connectorList);
                        farthestConnectors[element.Id] = oppositeConnector;
                    }

                    if (oppositeConnector != null)
                    {
                        foreach (Connector connector in oppositeConnector.AllRefs)
                        {
                            if (connector.Owner.Id != element.Id)
                            {
                                oppositeElementId = connector.Owner.Id;
                                break;
                            }
                        }
                    }
                }

                // Traverse to other connected items
                foreach (Connector connector in connectors)
                {
                    foreach (Connector connectedConnector in connector.AllRefs)
                    {
                        Element connectedElement = doc.GetElement(connectedConnector.Owner.Id);
                        if (connectedElement != null
                            && !_visitedElements.Contains(connectedElement.Id)
                            && connectedElement.Id != oppositeElementId)
                        {
                            // Check if the connected element is a FabricationPart or a DuctAccessory
                            if (connectedElement is FabricationPart
                                || (connectedElement is FamilyInstance connectedFamilyInstance
                                    && connectedFamilyInstance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory))
                            {
                                // Push the connected element onto the stack
                                stack.Push((connectedElement, connectedConnector));
                            }
                        }
                    }
                }

                // Push the opposite connected element onto the stack first
                if (oppositeElementId != null)
                    stack.Push((doc.GetElement(oppositeElementId), farthestConnectors[element.Id]));
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
