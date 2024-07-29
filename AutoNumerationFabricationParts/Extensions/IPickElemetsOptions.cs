using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public interface IPickElementsOption
    {
        List<Element> PickElements(
            UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt);

        Element PickElement(
            UIDocument uiDocument, Func<Element, bool> validateElement, string statusPrompt);
    }
}
