using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022.Models
{
    public class ElementsDisplaySetter
    {
        private UIDocument _uiDoc;
        private List<Element> _elements;
        private View3D _activeView;

        public ElementsDisplaySetter(List<Element> elements, UIDocument uiDoc)
        {
            _elements = elements
                ?? throw new ArgumentNullException(nameof(elements));
            _uiDoc = uiDoc
                ?? throw new ArgumentNullException(nameof(uiDoc));

            View view = uiDoc.ActiveView;
            if (view.ViewType != ViewType.ThreeD) return;
            else _activeView = (View3D)view;
        }

        public void SetElementsDisplay()
        {
            BoundingBoxXYZ boundingBox = CalculateBoundingBox();
            AdjustViewToBoundingBox(boundingBox);
        }

        private BoundingBoxXYZ CalculateBoundingBox()
        {
            BoundingBoxXYZ boundingBox = null;
            foreach (Element element in _elements)
            {
                BoundingBoxXYZ elementBoundingBox = element.get_BoundingBox(null);

                if (elementBoundingBox != null)
                {
                    if (boundingBox == null)
                    {
                        boundingBox = new BoundingBoxXYZ
                        {
                            Min = elementBoundingBox.Min,
                            Max = elementBoundingBox.Max,
                            //Transform = elementBoundingBox.Transform
                        };
                    }
                    else
                    {
                        boundingBox.Min = new XYZ(
                            Math.Min(boundingBox.Min.X, elementBoundingBox.Min.X),
                            Math.Min(boundingBox.Min.Y, elementBoundingBox.Min.Y),
                            Math.Min(boundingBox.Min.Z, elementBoundingBox.Min.Z)
                        );

                        boundingBox.Max = new XYZ(
                            Math.Max(boundingBox.Max.X, elementBoundingBox.Max.X),
                            Math.Max(boundingBox.Max.Y, elementBoundingBox.Max.Y),
                            Math.Max(boundingBox.Max.Z, elementBoundingBox.Max.Z)
                        );
                    }
                }
            }

            if (boundingBox != null)
            {
                // Calculate the expansion amount (5% of the current size)
                XYZ expansion = new XYZ(
                    0.05 * (boundingBox.Max.X - boundingBox.Min.X),
                    0.05 * (boundingBox.Max.Y - boundingBox.Min.Y),
                    0.05 * (boundingBox.Max.Z - boundingBox.Min.Z)
                );

                // Expand the bounding box by 5% in each direction
                boundingBox.Min -= expansion;
                boundingBox.Max += expansion;
            }

            return boundingBox;
        }

        private void AdjustViewToBoundingBox(BoundingBoxXYZ boundingBox)
        {
            if (boundingBox == null) return;

            //set view orientation to be from top and see all bounding box
            XYZ center = (boundingBox.Min + boundingBox.Max) / 2;
            XYZ viewDirection = new XYZ(0, 1, 0);
            XYZ upDirection = new XYZ(0, 0, -1);
            ViewOrientation3D viewOrientation = new ViewOrientation3D(center, viewDirection, upDirection);
            _activeView.SetOrientation(viewOrientation);

            _uiDoc.GetOpenUIViews().First().ZoomAndCenterRectangle(boundingBox.Min, boundingBox.Max);
        }
    }
}
