using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public abstract class BaseSelectionFilter : ISelectionFilter
    {
        protected Func<Element, bool> ValidateElement { get; }

        protected BaseSelectionFilter(Func<Element, bool> validateElement)
        {
            ValidateElement = validateElement;
        }

        public abstract bool AllowElement(Element elem);

        public abstract bool AllowReference(Reference reference, XYZ position);

    }
}
