using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutoNumerationFabricationParts_R2022.Models
{
    public class TagsOnViewCreator
    {
        private Document _doc;
        private View _view;
        private List<ElementId> _elementIds;
        private string _sourceParameterName;

        public TagsOnViewCreator(Document doc, View view, List<ElementId> elementIds, string sourceParameterName)
        {
            _doc = doc;
            _view = view;
            _elementIds = elementIds;
            _sourceParameterName = sourceParameterName;

        }

        //TODO: Implement this method
        public void CreateTagsOnView()
        {
            foreach (ElementId elementId in _elementIds)
            {
                Element element = _doc.GetElement(elementId);
                if (element == null) continue;
                if (!(element is FabricationPart)) continue; //add tags only for fabrication parts

                XYZ location = GetElementLocation(element);
                if (location == null) continue;

                string tagText = GetTagText(element);

                if(elementId.ToString() == "18181730")
                {
                    bool t = true;
                }

                //Check if a tag already exists for the element
                List<Element> rawTagCollector = new FilteredElementCollector(_doc, _view.Id)
                    .OfClass(typeof(IndependentTag))
                    .ToList();
                
                List<Element> tagCollector = rawTagCollector
                    .Where(tag => ((IndependentTag)tag).GetTaggedLocalElementIds().Contains(elementId))
                    .ToList();

                Element existingTag = tagCollector.FirstOrDefault();

                if (existingTag == null)
                {
                    // Create a new tag if it doesn't exist
                    Reference elementRef = new Reference(element);
                    bool hasLeader = false;
                    IndependentTag newTag = IndependentTag.Create(
                        _doc,
                        _view.Id,
                        elementRef,
                        hasLeader,
                        TagMode.TM_ADDBY_CATEGORY,
                        TagOrientation.Horizontal,
                        location);

                    bool shouldAddLeader = isTagBiggerThanElement(newTag, element); //check if tag is bigger than element and adjust its position if necessary
                    if (shouldAddLeader)
                    {
                        newTag.HasLeader = true;
                        newTag.LeaderEndCondition = LeaderEndCondition.Free;
                        newTag.SetLeaderEnd(elementRef, location);
                    }
                }
            }
        }

        private string GetTagText(Element element)
        {
            Parameter parameter = element.LookupParameter(_sourceParameterName);
            return parameter != null ? parameter.AsString() : string.Empty;
        }

        private XYZ GetElementLocation(Element element)
        {
            if (element is FabricationPart fabricationPart)
            {
                if (fabricationPart.IsAStraight())
                {
                    Location location = element.Location;
                    if (location is LocationPoint locationPoint)
                    {
                        return locationPoint.Point;
                    }
                    else if (location is LocationCurve locationCurve)
                    {
                        Curve curve = locationCurve.Curve;
                        return curve.Evaluate(0.5, true);
                    }
                }
                else
                {
                    //return the center of the bounding box
                    BoundingBoxXYZ boundingBox = element.get_BoundingBox(_view);
                    if (boundingBox != null)
                    {
                        XYZ min = boundingBox.Min;
                        XYZ max = boundingBox.Max;
                        return (min + max) / 2;
                    }
                }
            }
            else
            {
                Location location = element.Location;
                if (location is LocationPoint locationPoint)
                {
                    return locationPoint.Point;
                }
                else if (location is LocationCurve locationCurve)
                {
                    Curve curve = locationCurve.Curve;
                    return curve.Evaluate(0.5, true);
                }
            }

            return null;
        }

        private bool isTagBiggerThanElement(IndependentTag tag, Element element)
        {
            BoundingBoxXYZ tagBoundingBox = tag.get_BoundingBox(_view);
            BoundingBoxXYZ elementBoundingBox = element.get_BoundingBox(_view);

            if (tagBoundingBox == null || elementBoundingBox == null) return false; //by default we assume that the tag is not bigger than the element

            XYZ tagSize = tagBoundingBox.Max - tagBoundingBox.Min;
            XYZ elementSize = elementBoundingBox.Max - elementBoundingBox.Min;

            // true if tag is bigger in X/Y directions than element false otherwise
            return (tagSize.X > elementSize.X || tagSize.Y > elementSize.Y);
        }
    }
}
