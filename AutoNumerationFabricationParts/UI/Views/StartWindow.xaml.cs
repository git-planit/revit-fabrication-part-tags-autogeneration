using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace AutoNumerationFabricationParts_R2022.UI.Views
{
    public partial class StartWindow : Window, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        
        private string _branchName;
        public string BranchName
        {
            get { return _branchName; }
            set 
            { 
                _branchName = value;
                if (!string.IsNullOrEmpty(_firstNumber)) btnStart.IsEnabled = true;
                OnPropertyChanged();
            }
        }
        
        private string _firstNumber;
        public string FirstNumber
        {
            get { return _firstNumber; }
            set 
            {
                if (int.TryParse(value, out int result))
                {
                    _firstNumber = value;
                    if (!string.IsNullOrEmpty(_branchName)) btnStart.IsEnabled = true;

                    tbFirstNumber.Background = new SolidColorBrush(Colors.White);
                    tbFirstNumber.Foreground = new SolidColorBrush(Colors.Black);
                    tbFirstNumber.ToolTip = null;
                }
                else if (string.IsNullOrEmpty(value)) { 
                    btnStart.IsEnabled = false;

                    tbFirstNumber.Background = new SolidColorBrush(Colors.White);
                    tbFirstNumber.Foreground = new SolidColorBrush(Colors.Black);
                    tbFirstNumber.ToolTip = null;
                }
                else
                {
                    _firstNumber = string.Empty;
                    btnStart.IsEnabled = false;

                    tbFirstNumber.Background = new SolidColorBrush(Color.FromRgb(217, 84, 77));
                    tbFirstNumber.Foreground = new SolidColorBrush(Color.FromRgb(238, 232, 230));
                    tbFirstNumber.ToolTip = "Please enter a valid number";
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructors
        public StartWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        #endregion

        #region Handlers
        private void btnStart_Click(object sender, RoutedEventArgs e)
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

