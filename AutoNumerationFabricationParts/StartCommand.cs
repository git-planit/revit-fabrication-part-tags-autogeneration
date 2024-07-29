using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AutoNumerationFabricationParts_R2022.Entities;
using AutoNumerationFabricationParts_R2022.Extensions;
using AutoNumerationFabricationParts_R2022.Models;
using AutoNumerationFabricationParts_R2022.UI.Views;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StartCommand : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApplication = commandData.Application;
            Application application = uiApplication.Application;
            UIDocument uiDoc = uiApplication.ActiveUIDocument;
            Document doc = uiDoc.Document;

            View activeView = uiDoc.ActiveView;
            if (activeView.ViewType != ViewType.ThreeD) {
                TaskDialog wrongViewTypeDialog = new TaskDialog("Wrong active view")
                {
                    MainInstruction = "Active view is not a 3D View.\nOpen desired 3D view and try again.",
                    CommonButtons = TaskDialogCommonButtons.Ok,
                    DefaultButton = TaskDialogResult.Ok
                };
                wrongViewTypeDialog.Show();

                return Result.Failed;
            }

            //get all items to be processed
            ConnectedItemsCollector connectedItemsCollector = new ConnectedItemsCollector(uiDoc, doc);
            List<ElementInfo> elementData = connectedItemsCollector.GetAllConnectedElements();
            List<ElementId> processedElementIds = elementData.Select(e => e.ElementId).ToList();

            if (processedElementIds.Count == 0)
            {
                TaskDialog noElementsSelectedDialog = new TaskDialog("Element selection")
                {
                    MainInstruction = "There is no selected elements.",
                    CommonButtons = TaskDialogCommonButtons.Ok,
                    DefaultButton = TaskDialogResult.Ok
                };
                noElementsSelectedDialog.Show();

                return Result.Failed;
            }

            //higlight modified items
            doc.Run(() => uiDoc.Highlight(processedElementIds));

            //show all items to be modified
            doc.Run(() =>
            {
                ElementsDisplaySetter elementsDisplaySetter = new ElementsDisplaySetter(
                    elementData.Select(e => doc.GetElement(e.ElementId)).ToList(),
                    uiDoc);
                elementsDisplaySetter.SetElementsDisplay();
            }, "Show all selected items");

            //confirm items selection
            TaskDialog taskDialog = new TaskDialog("Set numeration")
            {
                MainInstruction = "Do you want to set numeration for selected items?",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };
            TaskDialogResult result = taskDialog.Show();
            if (result == TaskDialogResult.No)
            {
                return Result.Cancelled;
            }

            //get user input for tags numeration
            StartWindow window = new StartWindow();
            window.ShowDialog();
            string branchName = window.BranchName;
            int.TryParse(window.FirstNumber, out int startNumber);
            if (string.IsNullOrEmpty(branchName) || startNumber == 0)
            {
                return Result.Cancelled;
            }

            //set 'Item Number' property
            NumerationSetter numerationSetter = new NumerationSetter(doc, elementData, branchName, startNumber);
            numerationSetter.CalculatePrecision(window.FirstNumber);
            doc.Run(() => numerationSetter.SetNumeration(), "Set properties");

            
            //1. notify user about successful operation
            TaskDialog successSetPropertiesDialog = new TaskDialog("Set Item Number")
            {
                MainInstruction = "Item numbers have been set successfully for all highlighted items.",
                CommonButtons = TaskDialogCommonButtons.Ok,
                DefaultButton = TaskDialogResult.Ok
            };
            successSetPropertiesDialog.Show();

            //2. ask user if he wants to add tags to the selected items and take view from user
            TaskDialog extraStepDialog = new TaskDialog("Create tags on a view?")
            {
                MainInstruction = "Do you want to create tags for these items on a view?",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };
            TaskDialogResult extraStepDialogResult = extraStepDialog.Show();
            if (extraStepDialogResult == TaskDialogResult.No)
            {
                return Result.Succeeded;
            }

            //3. get desired view for placing tags
            List<View> availableViews = doc.GetElementsByType<View>()
                .Where(v => v.ViewType == ViewType.FloorPlan)
                .ToList();
            List<string> viewNames = availableViews.Select(v => v.Name).ToList();
            viewNames.Sort();

            SelectView viewPicker = new SelectView(viewNames);
            viewPicker.ShowDialog();
            if(viewPicker.IsSelected == false)
            {
                return Result.Cancelled;
            }

            string selectedViewName = viewPicker.SelectedView;
            View view = availableViews.FirstOrDefault(v => v.Name == selectedViewName);

            //4. add tags to the selected items
            TagsOnViewCreator tagsOnViewCreator = new TagsOnViewCreator(doc, view, processedElementIds, "Item Number");
            doc.Run(() => tagsOnViewCreator.CreateTagsOnView(), "Create tags on view");

            //5. notify user about successful operation
            TaskDialog successTagGenerationDialog = new TaskDialog("Add tags on view")
            {
                MainInstruction = $"Tags have been created successfully for selected view ({selectedViewName}).",
                CommonButtons = TaskDialogCommonButtons.Ok,
                DefaultButton = TaskDialogResult.Ok
            };
            successTagGenerationDialog.Show();

            return Result.Succeeded;
        }
    }
}
