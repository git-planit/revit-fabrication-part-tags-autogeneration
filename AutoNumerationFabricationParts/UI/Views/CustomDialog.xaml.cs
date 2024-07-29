using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoNumerationFabricationParts_R2022.UI.Views
{
    public partial class CustomDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool? DialogResult { get; private set; }

        private string _message;
        public string Message
        {
            get { return _message; }
            set 
            { 
                _message = value; 
                OnPropertyChanged();
            }
        }


        #region Constructor
        public CustomDialog(string title, string message)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException("title");
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            Title = title;
            Message = message;

            InitializeComponent();
            DataContext = this;
        }
        #endregion

        #region Handlers
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion

        #region Utility methods
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
