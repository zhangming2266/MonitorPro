using GMap.NET.WindowsPresentation;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPro
{
    public class LayerInfo: PropertyChangedBase
    {
        private bool _Checked = true;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                if (value)
                {
                    GMapOverlayLayer.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    GMapOverlayLayer.Visibility = System.Windows.Visibility.Collapsed;
                }
                NotifyPropertyChanged();
            }
        }
        public string _LayerName;
        public string LayerName
        {
            get
            {
                return _LayerName;
            }
            set
            {
                _LayerName = value;
                NotifyPropertyChanged();
            }
        }
        public GMapOverlay GMapOverlayLayer { get; set; }
    }
}
