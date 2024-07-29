using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public class OpenDocumentOption : IPickElementsOption
    {
        public List<Element> PickElements(UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt = "")
        {
            var document = uiDocument.Document;
            var references = uiDocument.Selection.PickObjects(
                ObjectType.Element,
                new ElementSelectionFilter(validateElement), statusPrompt);
            var elements = references
                .Select(r => (document.GetElement(r.ElementId)))
                .ToList();
            return elements;
        }
        
        public Element PickElement(UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt = "")
        {
            var document = uiDocument.Document;
            var element = uiDocument.Selection.PickObject(
                ObjectType.Element,
                new ElementSelectionFilter(validateElement), statusPrompt);
            return document.GetElement(element.ElementId);
        }
    }
}
