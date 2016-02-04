using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stability
{
    /// <summary>
    /// Interaction logic for TenzoRadioButton.xaml
    /// </summary>
    public partial class TenzoRadioButton : UserControl
    {

        public bool IsChecked { get { return _isChecked; } set { _isChecked = value; UpdateDot(); } }
        public Thickness DotMargin { get { return dot.Margin; } set { dot.Margin = value; } }
        private bool _isChecked;

        public List<TenzoRadioButton> GroupTenzoRadioButtons { get; set; }
        public TenzoRadioButton()
        {
            InitializeComponent();
            GroupTenzoRadioButtons = new List<TenzoRadioButton>();
        }

        private void ToggleState()
        {
            IsChecked = !_isChecked;
            UpdateDot();
        }

        private void UpdateDot()
        {
            dot.Visibility = _isChecked ? Visibility.Visible : Visibility.Collapsed;

            if(_isChecked)
                foreach (var groupTenzoRadioButton in GroupTenzoRadioButtons)
                {
                 groupTenzoRadioButton.IsChecked = false;
                }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ToggleState();
            IsChecked = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var mar_w = (ActualWidth / 2.0) / 5.0;
            var mar_h = (ActualHeight / 2.0) / 5.0;
            dot.Margin = new Thickness(mar_w, mar_h, mar_w, mar_h);
        }

    }
}
