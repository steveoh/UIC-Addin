using System;
using System.Collections.Generic;
using System.Linq;
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


namespace UIC_testing_two
{
    /// <summary>
    /// Interaction logic for WorkFlowPaneView.xaml
    /// </summary>
    public partial class WorkFlowPaneView : UserControl
    {
        readonly WorkFlowPaneViewModel _taskTrackingViewModel;
        public WorkFlowPaneView()
        {
            InitializeComponent();
            base.DataContext = _taskTrackingViewModel;
        }
    }
}
