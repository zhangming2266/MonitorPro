using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitorPro
{
    public class EditMapConfigViewModel : PropertyChangedBase
    {
        public event EventHandler<MapConfigAttribute> EditMapConfigHander;
        public event EventHandler<bool> PickLatlngHander;
        private WindowX windowX;
        private string MapName = "";
        public EditMapConfigViewModel(WindowX wnd, string mapConfigName)
        {
            this.windowX = wnd;
            this.MapName = mapConfigName;
            LoadMapConfig(mapConfigName);
        }

        #region 属性
        private int _Zoom;
        public int Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                _Zoom = value;
                NotifyPropertyChanged();
            }
        }

        private int _Max;
        public int Max
        {
            get
            {
                return _Max;
            }
            set
            {
                _Max = value;
                NotifyPropertyChanged();
            }
        }

        private int _Min;
        public int Min
        {
            get
            {
                return _Min;
            }
            set
            {
                _Min = value;
                NotifyPropertyChanged();
            }
        }

        private double _CenterLng;
        public double CenterLng
        {
            get
            {
                return _CenterLng;
            }
            set
            {
                _CenterLng = value;
                NotifyPropertyChanged();
            }
        }

        private double _CenterLat;
        public double CenterLat
        {
            get
            {
                return _CenterLat;
            }
            set
            {
                _CenterLat = value;
                NotifyPropertyChanged();
            }
        }

        private bool _PickCenter;
        public bool PickCenter
        {
            get
            {
                return _PickCenter;
            }
            set
            {
                _PickCenter = value;
                NotifyPropertyChanged();
            }
        }
        #endregion


        #region 方法
        private void LoadMapConfig(string mapName)
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + @"caches\TileDBv5\en\" + mapName + ".INI";
            IniHelper iniHelper = new IniHelper(fileName);
            if (iniHelper.ExistINIFile())
            {
                var zoom = iniHelper.IniReadValue("地图配置", "zoom");
                var minzoom = iniHelper.IniReadValue("地图配置", "minZoom");
                var maxzoom = iniHelper.IniReadValue("地图配置", "maxZoom");
                var center = iniHelper.IniReadValue("地图配置", "center").Split(',');
                this.Zoom = int.Parse(zoom);
                this.Min = int.Parse(minzoom);
                this.Max = int.Parse(maxzoom);
                this.CenterLng = double.Parse(center[1]);
                this.CenterLat = double.Parse(center[0]);
            }
        }
        #endregion

        #region 事件
        public ICommand SetMapConfig
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    var fileName = AppDomain.CurrentDomain.BaseDirectory + @"caches\TileDBv5\en\" + this.MapName + ".INI";
                    IniHelper iniHelper = new IniHelper(fileName);
                    if (iniHelper.ExistINIFile())
                    {
                        var center = CenterLat.ToString() + "," + CenterLng.ToString();
                        iniHelper.IniWriteValue("地图配置", "zoom", Zoom.ToString());
                        iniHelper.IniWriteValue("地图配置", "minZoom", Min.ToString());
                        iniHelper.IniWriteValue("地图配置", "maxZoom", Max.ToString());
                        iniHelper.IniWriteValue("地图配置", "center", CenterLat.ToString() + "," + CenterLng.ToString());
                        if(EditMapConfigHander != null)
                        {
                            MapConfigAttribute mapConfigAttribute = new MapConfigAttribute();
                            mapConfigAttribute.MapName = MapName;
                            mapConfigAttribute.Zoom = Zoom;
                            mapConfigAttribute.MinZoom = Min;
                            mapConfigAttribute.MaxZoom = Max;
                            mapConfigAttribute.Center = center;
                            EditMapConfigHander(this, mapConfigAttribute);
                        }
                        Notice.Show("地图配置文件修改成功！", "地图消息", 3, MessageBoxIcon.Success);
                    }
                    this.windowX.Close();
                }));
            }
        }

        public ICommand CancelMapConfig
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    this.windowX.Close();
                }));
            }
        }

        #endregion

        public ICommand PickLatlng
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    if (PickLatlngHander != null)
                    {
                        PickLatlngHander(this,PickCenter);
                    }
                }));
            }
        }
    }
}
