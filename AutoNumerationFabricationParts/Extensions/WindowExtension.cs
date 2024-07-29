using System.Windows;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public static class WindowExtension
    {
        public static void ShowDialogWithOpacity(this Window window, Window ownerWindow, double opacityValue)
        {
            if (opacityValue < 0) opacityValue = 0;
            if (opacityValue > 1) opacityValue = 1;

            ownerWindow.Opacity = opacityValue;
            window.ShowDialog();
            ownerWindow.Opacity = 1;
        }
    }
}
