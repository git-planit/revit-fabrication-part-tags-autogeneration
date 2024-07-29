using Autodesk.Revit.DB;
using AutoNumerationFabricationParts_R2022.Entities;
using AutoNumerationFabricationParts_R2022.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNumerationFabricationParts_R2022.Models
{
    public class NumerationSetter
    {
        private Dictionary<string, int> _processedGeometry = new Dictionary<string, int>();
        private List<ElementInfo> _elementsGeometry;
        private string _branchName;
        private int _startNumber;
        private Document _doc;

        public NumerationSetter(Document doc, List<ElementInfo> elementsGeometry, string branchName, int startNumber)
        {
            _elementsGeometry = elementsGeometry;
            _branchName = branchName;
            _startNumber = startNumber;
            _doc = doc;
        }

        public void SetNumeration()
        {
            foreach (ElementInfo elementData in _elementsGeometry)
            {
                StringBuilder sb = new StringBuilder();
                string elementCode = elementData.GeometryCode;
                if(elementCode == "NotFabricationPart") continue;
                if (_processedGeometry.TryGetValue(elementCode, out int value))
                {
                    string itemNumber = _GetItemNumberByZeroPrefix(_processedGeometry[elementCode]);

                    //sb.AppendLine($"{_branchName}{itemNumber}---{elementCode}"); //tested edition to track geometry
                    sb.AppendLine($"{_branchName}{itemNumber}");
                }
                else
                {
                    string itemNumber = _GetItemNumberByZeroPrefix(_startNumber);

                    //sb.AppendLine($"{_branchName}{itemNumber}---{elementCode}"); //tested edition to track geometry
                    sb.AppendLine($"{_branchName}{itemNumber}");
                    _processedGeometry.Add(elementCode, _startNumber);
                    _startNumber++;
                }

                //get element by id and set propery item number
                Element element = _doc.GetElement(elementData.ElementId);
                if (element == null) continue;

                // Set the shared parameter "Item Number" and "Comments"
                SetParameter(element, "Item Number", sb.ToString());
                SetParameter(element, "Comments", elementCode);
            }
        }

        private string _GetItemNumberByZeroPrefix(int number)
        {
            return number < 10 ? $"0{number}" : number.ToString();
        }

        private void SetParameter(Element element, string parameterName, string value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly)
            {
                parameter.Set(value);
            }
        }
    }
}
