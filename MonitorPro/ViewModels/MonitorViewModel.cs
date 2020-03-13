using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.WindowsPresentation;
using Microsoft.Expression.Interactivity.Core;
using Microsoft.Win32;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonitorPro
{
    public class MonitorViewModel : PropertyChangedBase
    {

        private GMapOverlay TextMarker;
        private GmapGpsOverlay CameraMarker;
        private bool MarkerLayerLoaded = false;
        private MonitorWnd monitorWnd;
        private ObservableCollectionThreadSafe<GMapMarker> TextMarkerList;
        private EditMapConfigViewModel editMapConfigViewModel;
        public MonitorViewModel(MonitorWnd monitorWindow)
        {
            monitorWnd = monitorWindow;
            monitorWnd.Closed += MonitorWnd_Closed;
            MapConfigAttributes = new ObservableCollection<MapConfigAttribute>();
            TextMarkerList = new ObservableCollectionThreadSafe<GMapMarker>();
            monitorWnd.MapControl.Loaded += MapControl_Loaded;
            monitorWnd.MapControl.MouseUp += MapControl_MouseUp;
        }

        private ObservableCollection<MapConfigAttribute> _MapConfigAttributes;
        public ObservableCollection<MapConfigAttribute> MapConfigAttributes
        {
            get
            {
                return _MapConfigAttributes;
            }
            set
            {
                _MapConfigAttributes = value;
                NotifyPropertyChanged();
            }
        }

        private MapConfigAttribute _MapSelectedItem;
        public MapConfigAttribute MapSelectedItem
        {
            get
            {
                return _MapSelectedItem;
            }
            set
            {
                _MapSelectedItem = value;
                LoadMap();
            }
        }
        #region 窗口关闭
        private void MonitorWnd_Closed(object sender, EventArgs e)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "markers\\marker.txt";
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            foreach (var marker in TextMarkerList)
            {
                var textMarker = marker.Shape as TextMarker;
                var lng = marker.Position.Lat.ToString();
                var lat = marker.Position.Lng.ToString();
                var textTitle = textMarker.textMarkerViewModel.textMarkerInfo.TextTitle;
                var textContent = textMarker.textMarkerViewModel.textMarkerInfo.TextContent;
                textContent = textContent.Replace("\r\n","*#*#");
                var text = lng + ";" + lat + ";" + textTitle + ";" + textContent;
                sw.WriteLine(text);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
            monitorWnd.MapControl.Dispose();
            Notice.Dispose();
        }
        #endregion
        #region 地图事件
        private void MapControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MarkerLayerLoaded && monitorWnd.Marker.IsChecked == true)
            {
                var pt = e.GetPosition(monitorWnd.MapControl);
                var lngLatPt = monitorWnd.MapControl.FromLocalToLatLng((int)pt.X, (int)pt.Y);
                var textInfoMarker = new GMapMarker(lngLatPt);
                {
                    var textMarker = new TextMarker();
                    textInfoMarker.Shape = textMarker;
                    textInfoMarker.Tag = textMarker.textMarkerViewModel.textMarkerInfo.ID;
                    textInfoMarker.Offset = new System.Windows.Point(-15, -15);
                    textInfoMarker.ZIndex = int.MaxValue;
                    TextMarker.Markers.Add(textInfoMarker);
                    textMarker.textMarkerViewModel.DeleteMarkerEvent += TextMarkerViewModel_DeleteMarkerEvent;
                }
                TextMarkerList.Add(textInfoMarker);
            }
            if (editMapConfigViewModel != null && editMapConfigViewModel.PickCenter)
            {
                var pt = e.GetPosition(monitorWnd.MapControl);
                var lngLatPt = monitorWnd.MapControl.FromLocalToLatLng((int)pt.X, (int)pt.Y);
                editMapConfigViewModel.CenterLat = lngLatPt.Lng;
                editMapConfigViewModel.CenterLng = lngLatPt.Lat;
                editMapConfigViewModel.PickCenter = false;
            }
        }

        private void TextMarkerViewModel_DeleteMarkerEvent(object sender, TextMarkerInfo e)
        {
            if (MarkerLayerLoaded)
            {
                TextMarker.Markers.Clear();
                for (var i = 0; i < TextMarkerList.Count();)
                {
                    var marker = TextMarkerList[i];
                    if (e.ID == marker.Tag.ToString())
                    {
                        TextMarkerList.Remove(marker);
                    }
                    else
                    {
                        i++;
                        TextMarker.Markers.Add(marker);
                    }
                }


            }
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMapConfig();
            LoadMarkerConfig();
        }
        #endregion
        #region 加载地图
        private void LoadMap()
        {
            if (MapSelectedItem != null)
            {
                try
                {
                    monitorWnd.IsMaskVisible = true;
                    var providersList = GMapProviders.List;
                    foreach (var provider in GMapProviders.List)
                    {
                        if (provider.ToString() == MapSelectedItem.MapProvider)
                        {
                            monitorWnd.MapControl.MapProvider = provider;
                            break;
                        }
                    }
                    monitorWnd.MapControl.Manager.Mode = AccessMode.CacheOnly;
                    monitorWnd.MapControl.CacheLocation = AppDomain.CurrentDomain.BaseDirectory + "caches\\"; //缓存位置
                    monitorWnd.MapControl.CacheLocationFlieName = MapSelectedItem.MapName + ".gmdb";
                    monitorWnd.MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
                    var bounds = MapSelectedItem.MapBound.Split(',');
                    var boundsOfMap = RectLatLng.FromLTRB(double.Parse(bounds[0]), double.Parse(bounds[1]), double.Parse(bounds[2]), double.Parse(bounds[3]));
                    //monitorWnd.MapControl.BoundsOfMap = boundsOfMap;
                    monitorWnd.MapControl.ShowCenter = false;
                    monitorWnd.MapControl.MinZoom = MapSelectedItem.MinZoom;
                    monitorWnd.MapControl.MaxZoom = MapSelectedItem.MaxZoom;
                    monitorWnd.MapControl.Zoom = MapSelectedItem.Zoom;
                    var center = MapSelectedItem.Center.Split(',');
                    monitorWnd.MapControl.Position = new PointLatLng(double.Parse(center[1]), double.Parse(center[0]));
                    monitorWnd.MapControl.DragButton = MouseButton.Left;
                    if (!MarkerLayerLoaded)
                    {
                        CameraMarker = new GmapGpsOverlay("cameraMarkerLayer");
                        monitorWnd.MapControl.Overlays.Add(CameraMarker);
                        TextMarker = new GMapOverlay("textMarkerLayer");
                        monitorWnd.MapControl.Overlays.Add(TextMarker);
                        MarkerLayerLoaded = true;
                    }
                    Notice.Show("地图加载成功！", "地图消息", 3, MessageBoxIcon.Success);
                }
                finally
                {
                    monitorWnd.IsMaskVisible = false;
                }
            }
        }
        #endregion

        #region 加载地图文件配置
        public void LoadMapConfig()
        {

            var path = AppDomain.CurrentDomain.BaseDirectory + @"caches\TileDBv5\en";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var mapfiles = Directory.GetFiles(path);
            foreach (var mapfile in mapfiles)
            {
                var fileExtens = Path.GetExtension(mapfile);
                if (fileExtens == ".INI")
                {
                    IniHelper iniHelper = new IniHelper(mapfile);
                    if (iniHelper.ExistINIFile())
                    {
                        var mapName = Path.GetFileName(mapfile).Replace(fileExtens, "");
                        var minZoom = iniHelper.IniReadValue("地图配置", "minZoom");
                        var maxZoom = iniHelper.IniReadValue("地图配置", "maxZoom");
                        var zoom = iniHelper.IniReadValue("地图配置", "zoom");
                        var bounds = iniHelper.IniReadValue("地图配置", "bounds");
                        var mapProvider = iniHelper.IniReadValue("地图配置", "mapProvider");
                        var center = iniHelper.IniReadValue("地图配置", "center");
                        MapConfigAttribute mapConfigAttribute = new MapConfigAttribute();
                        mapConfigAttribute.MapName = mapName;
                        mapConfigAttribute.MinZoom = int.Parse(minZoom);
                        mapConfigAttribute.MaxZoom = int.Parse(maxZoom);
                        mapConfigAttribute.Zoom = int.Parse(zoom);
                        mapConfigAttribute.MapBound = bounds;
                        mapConfigAttribute.Center = center;
                        mapConfigAttribute.MapProvider = mapProvider;
                        MapConfigAttributes.Add(mapConfigAttribute);
                    }
                }
            }
            if (MapConfigAttributes.Count > 0)
            {
                MapSelectedItem = MapConfigAttributes[0];
            }
        }

        private void LoadMarkerConfig()
        {
            var dirPath = AppDomain.CurrentDomain.BaseDirectory + "markers";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var path = AppDomain.CurrentDomain.BaseDirectory + "markers\\marker.txt";
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    var text = line.ToString();
                    var textMarkerString = text.Split(';');
                    var textMarker = new TextMarker();
                    textMarker.textMarkerViewModel.DropDownShow = false;
                    textMarker.textMarkerViewModel.TextShow = true;
                    textMarker.textMarkerViewModel.TextTitle = textMarkerString[2];
                    textMarker.textMarkerViewModel.TextContent = textMarkerString[3].Replace("*#*#", "\r\n");
                    var lngLatPt = new PointLatLng(double.Parse(textMarkerString[0]), double.Parse(textMarkerString[1]));
                    var textInfoMarker = new GMapMarker(lngLatPt);
                    {
                        textInfoMarker.Shape = textMarker;
                        textInfoMarker.Tag = textMarker.textMarkerViewModel.textMarkerInfo.ID;
                        textInfoMarker.Offset = new System.Windows.Point(-15, -15);
                        textInfoMarker.ZIndex = int.MaxValue;
                        TextMarker.Markers.Add(textInfoMarker);
                        textMarker.textMarkerViewModel.DeleteMarkerEvent += TextMarkerViewModel_DeleteMarkerEvent;
                    }
                    TextMarkerList.Add(textInfoMarker);
                }
            }
        }
        #endregion

        #region  删除地图资源包
        public ICommand DeleteMap
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    var mapName = t.ToString();
                    var result = MessageBoxX.Show("确定要删除(" + mapName + ")地图文件，删除后无法恢复！", "Warning", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                    {
                        MessageBoxStyle = MessageBoxStyle.Classic,
                        MessageBoxIcon = MessageBoxIcon.Warning,
                        ButtonBrush = "#F1C825".ToColor().ToBrush(),
                    });
                    if (result == MessageBoxResult.Yes)
                    {
                        monitorWnd.IsMaskVisible = true;
                        var path = AppDomain.CurrentDomain.BaseDirectory + @"caches\TileDBv5\en";
                        var mapfiles = Directory.GetFiles(path);
                        foreach (var mapfile in mapfiles)
                        {
                            var fileExtens = Path.GetExtension(mapfile);
                            var fileName = Path.GetFileName(mapfile).Replace(fileExtens, "");
                            if (fileName == mapName)
                            {
                                foreach (var mapConfigAttribute in MapConfigAttributes)
                                {
                                    if (mapConfigAttribute.MapName == mapName)
                                    {
                                        MapConfigAttributes.Remove(mapConfigAttribute);
                                        if (MapSelectedItem == null || (MapSelectedItem.MapName == mapName && MapConfigAttributes.Count > 0))
                                        {
                                            MapSelectedItem = MapConfigAttributes[0];
                                        }
                                        break;
                                    }
                                }
                                Thread.Sleep(10000);
                                File.Delete(mapfile);
                            }
                        }

                        monitorWnd.IsMaskVisible = false;
                        Notice.Show("删除地图资源(" + mapName + ")成功！", "地图消息", 3, MessageBoxIcon.Success);
                    }
                }));
            }
        }
        #endregion

        #region  导入地图资源包
        public ICommand ImportMap
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    var monitorWnd = Application.Current.MainWindow as MonitorWnd;
                    monitorWnd.IsMaskVisible = true;
                    ImportMapDialog();
                    monitorWnd.IsMaskVisible = false;
                }));
            }
        }

        private void ImportMapDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "(config,map)|*.INI;*.gmdb;";
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\GMap.NET\TileDBv5\en";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            openFileDialog.InitialDirectory = path;
            var result = openFileDialog.ShowDialog(Application.Current.MainWindow);
            if (result == true)
            {
                var files = openFileDialog.FileNames;
                CopyMapFiles(files);
            }
        }

        private void CopyMapFiles(string[] files)
        {
            if (files.Length != 2)
            {
                MessageBoxX.Show("地图文件必须包含两个文件（.INI .gmdb）", "Warning", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxStyle = MessageBoxStyle.Classic,
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                });
                return;
            }
            var extens1 = Path.GetExtension(files[0]);
            var extens2 = Path.GetExtension(files[1]);
            if (extens1 == extens2)
            {
                MessageBoxX.Show("地图文件必须包含两个文件（.INI .gmdb）", "Warning", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxStyle = MessageBoxStyle.Classic,
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                });
                return;
            }

            var name1 = Path.GetFileName(files[0]).Replace(extens1, "");
            var name2 = Path.GetFileName(files[1]).Replace(extens2, "");
            if (name1 != name2)
            {
                MessageBoxX.Show("地图文件和配置文件的名称必须相同！", "Warning", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxStyle = MessageBoxStyle.Classic,
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                });
                return;
            }
            var path = AppDomain.CurrentDomain.BaseDirectory + @"caches\TileDBv5\en";
            var mapfiles = Directory.GetFiles(path, "*.gmdb");
            var desfile1 = path + "\\" + name1 + extens1;
            var desfile2 = path + "\\" + name2 + extens2;
            if (mapfiles.Contains(desfile1) || mapfiles.Contains(desfile2))
            {
                MessageBoxX.Show("当前离线地图文件中已包含相同名称的地图，请修改地图文件和地图配置文件名！", "Warning", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxStyle = MessageBoxStyle.Classic,
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                });
                return;
            }
            if (File.Exists(desfile1))
            {
                File.Delete(desfile1);
            }
            File.Copy(files[0], desfile1);
            if (File.Exists(desfile2))
            {
                File.Delete(desfile2);
            }
            File.Copy(files[1], desfile2);
            var configFile = desfile1;
            if (desfile2.IndexOf(".INI") > 0)
            {
                configFile = desfile2;
            }

            IniHelper iniHelper = new IniHelper(configFile);
            var fileExtens = Path.GetExtension(configFile);
            var mapName = Path.GetFileName(configFile).Replace(fileExtens, "");
            if (iniHelper.ExistINIFile())
            {
                var minZoom = iniHelper.IniReadValue("地图配置", "minZoom");
                var maxZoom = iniHelper.IniReadValue("地图配置", "maxZoom");
                var zoom = iniHelper.IniReadValue("地图配置", "zoom");
                var bounds = iniHelper.IniReadValue("地图配置", "bounds");
                var mapProvider = iniHelper.IniReadValue("地图配置", "mapProvider");
                MapConfigAttribute mapConfigAttribute = new MapConfigAttribute();
                mapConfigAttribute.MapName = mapName;
                mapConfigAttribute.MinZoom = int.Parse(minZoom);
                mapConfigAttribute.MaxZoom = int.Parse(maxZoom);
                mapConfigAttribute.Zoom = int.Parse(zoom);
                mapConfigAttribute.MapBound = bounds;
                mapConfigAttribute.MapProvider = mapProvider;
                MapConfigAttributes.Add(mapConfigAttribute);
                if(MapSelectedItem == null)
                {
                    MapSelectedItem = MapConfigAttributes[0];
                }
            }
            Notice.Show("离线地图(" + mapName + ")导入成功！", "地图消息", 3, MessageBoxIcon.Success);
        }
        #endregion

        #region 切换不同的地图文件
        public ICommand SelectedMap
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {

                }));
            }
        }
        #endregion

        #region 编辑地图配置文件
        public ICommand EditMapConfig
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    EditMapConfigWnd editMapConfigWnd = new EditMapConfigWnd(t.ToString());
                    editMapConfigWnd.editMapConfigViewModel.EditMapConfigHander += EditMapConfigViewModel_EditMapConfigHander;
                    editMapConfigWnd.editMapConfigViewModel.PickLatlngHander += EditMapConfigViewModel_PickLatlngHander;
                    editMapConfigWnd.Show();
                }));
            }
        }

        private void EditMapConfigViewModel_PickLatlngHander(object sender, bool e)
        {
            editMapConfigViewModel = sender as EditMapConfigViewModel;
        }

        private void EditMapConfigViewModel_EditMapConfigHander(object sender, MapConfigAttribute e)
        {
            if(MapSelectedItem != null && MapSelectedItem.MapName == e.MapName)
            {
                MapSelectedItem.Zoom = e.Zoom;
                MapSelectedItem.MaxZoom = e.MaxZoom;
                MapSelectedItem.MinZoom = e.MinZoom;
                MapSelectedItem.Center = e.Center;
                LoadMap();
            }
        }
        #endregion
    }
}
