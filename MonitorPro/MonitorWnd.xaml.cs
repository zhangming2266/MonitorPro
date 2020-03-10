using GMap.NET;
using GMap.NET.MapProviders;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
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
using System.Windows.Shapes;

namespace MonitorPro
{
    /// <summary>
    /// MonitorWnd.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorWnd
    {
        public MonitorViewModel monitorViewModel { get; set; }
        public MonitorWnd()
        {
            InitializeComponent();
            monitorViewModel = new MonitorViewModel(this);
            DataContext = monitorViewModel;
        }

        
    }
}
