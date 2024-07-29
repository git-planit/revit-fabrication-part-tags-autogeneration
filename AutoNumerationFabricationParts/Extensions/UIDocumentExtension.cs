using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public static class UIDocumentExtensions
    {
        public static List<Element> GetSelectedElements(this UIDocument uiDocument)
        {
            return uiDocument.Selection
                .GetElementIds()
                .Select(id => uiDocument.Document.GetElement(id))
                .ToList();
        }
        
        public static List<T> GetSelectedElements<T>(this UIDocument uiDocument)
        {
            return uiDocument.Selection
                .GetElementIds()
                .Select(id => uiDocument.Document.GetElement(id))
                .OfType<T>()
                .ToList();
        }

        public static List<Element> PickElements(
            this UIDocument uiDocument,
            Func<Element, bool> validateElement,
            IPickElementsOption pickElementsOption,
            string statusPrompt = "")
        {
            return pickElementsOption.PickElements(uiDocument, validateElement, statusPrompt);
        }
        
        public static Element PickElement(
            this UIDocument uiDocument,
            Func<Element, bool> validateElement,
            IPickElementsOption pickElementsOption,
            string statusPrompt = "")
        {
            return pickElementsOption.PickElement(uiDocument, validateElement, statusPrompt);
        }

        public static void Highlight(this UIDocument uiDocument, Element element)
        {
            uiDocument.Selection.SetElementIds(new List<ElementId>() { element.Id });
        }
        public static void Highlight(this UIDocument uiDocument, List<Element> elements)
        {
            uiDocument.Selection.SetElementIds(elements.Select(e => e.Id).ToList());
        }
        public static void Highlight(this UIDocument uiDocument, List<ElementId> elementIds)
        {
            uiDocument.Selection.SetElementIds(elementIds);
        }

        public static void Highlight(this UIDocument uiDocument, IEnumerable<Element> elements)
        {
            uiDocument.Selection.SetElementIds(elements.Select(e => e.Id).ToList());
        }
    }
}
