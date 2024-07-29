using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Windows;
using System.Windows.Documents;
using Autodesk.Revit.UI;

namespace AutoNumerationFabricationParts_R2022.UI.Views
{

    public partial class SelectView : Window, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _viewNames;
        public List<string> ViewNames
        {
            get { return _viewNames; }
            set
            {
                _viewNames = value;
                OnPropertyChanged();
            }
        }

        private string _selectedView;
        public string SelectedView
        {
            get { return _selectedView; }
            set 
            { 
                if (value != null) btnSelect.IsEnabled = true;
                else btnSelect.IsEnabled = false;

                _selectedView = value; 
                OnPropertyChanged();
            }
        }

        public bool IsSelected = false;
        #endregion

        #region Constructor
        public SelectView(List<string> viewNames)
        {
            if (viewNames == null) throw new ArgumentNullException(nameof(viewNames));
            ViewNames = viewNames;

            InitializeComponent();
            DataContext = this;
        }
        #endregion

        #region Handlers
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            IsSelected = true;
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Utility methods
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
