using Autodesk.Revit.DB;
using AutoNumerationFabricationParts_R2022.Entities;
using System.Collections.Generic;
using System.Text;

namespace AutoNumerationFabricationParts_R2022.Models
{
    public class NumerationSetter
    {
        private Dictionary<string, int> _processedGeometry = new Dictionary<string, int>();
        private List<ElementInfo> _elementsGeometry;
        private string _branchName;
        private int _startNumber;
        private int _precision = 1;
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
                if (elementCode == "NotFabricationPart") continue;
                if (_processedGeometry.TryGetValue(elementCode, out int value))
                {
                    string itemNumber = GetItemNumberByZeroPrefix(_processedGeometry[elementCode], _precision);

                    sb.AppendLine($"{_branchName}{itemNumber}");
                }
                else
                {
                    string itemNumber = GetItemNumberByZeroPrefix(_startNumber, _precision);

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

        public void CalculatePrecision(string userInput)
        {
            //extract zero prefix from user input from the beginning of the string
            string zeroPrefix = "";
            foreach (char c in userInput)
            {
                if (c == '0')
                {
                    zeroPrefix += c;
                }
                else
                {
                    break;
                }
            }

            //calculate precision
            int.TryParse(userInput, out int number);
            if (number > 0 && number < 10)
            {
                if (zeroPrefix.Length < 1) _precision = 1;
                else _precision = zeroPrefix.Length;
            }
            else if (number >= 10 || number < 100)
            {
                if (zeroPrefix.Length < 1)
                {
                    _precision = 1;
                }
                else
                {
                    _precision = 2;
                }
            }
            else if (number >= 100 || number < 1000)
            {
                if (zeroPrefix.Length < 1)
                {
                    _precision = 2;
                }
                else
                {
                    _precision = 3;
                }
            }
        }

        private string GetItemNumberByZeroPrefix(int number, int? presize = null)
        {
            string itemNumber = "";

            string strBelow_10 = "";
            string strBelow_100 = "";
            string strBelow_1000 = "";
            if (presize is null || presize == 1)
            {
                strBelow_10 = "0";
            }
            else if (presize == 2)
            {
                strBelow_10 = "00";
                strBelow_100 = "0";
            }
            else if (presize == 3)
            {
                strBelow_10 = "000";
                strBelow_100 = "00";
                strBelow_1000 = "0";
            }

            if (number > 0 && number < 10) itemNumber = $"{strBelow_10}{number}";
            else if (number >= 10 && number < 100) itemNumber = $"{strBelow_100}{number}";
            else if (number >= 100 && number < 1000) itemNumber = $"{strBelow_1000}{number}";
            else itemNumber = number.ToString();

            return itemNumber;
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
