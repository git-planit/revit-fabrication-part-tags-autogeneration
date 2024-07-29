using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public class LinkDocumentOption : IPickElementsOption
    {
        public List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt = "")
        {
            var document = uiDocument.Document;
            var references = uiDocument.Selection.PickObjects(
                ObjectType.LinkedElement,
                new LinkableSelectionFilter(document, validateElement), statusPrompt);
            var elements = references
                .Select(r => (document.GetElement(r.ElementId) as RevitLinkInstance)
                    .GetLinkDocument().GetElement(r.LinkedElementId))
                .ToList();
            return elements;

        }

        public Element PickElement(UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt = "")
        {
            var document = uiDocument.Document;
            var element = uiDocument.Selection.PickObject(
                ObjectType.LinkedElement,
                new LinkableSelectionFilter(document, validateElement), statusPrompt);
            return document.GetElement(element.ElementId);
        }
    }
}
