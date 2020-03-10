using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonitorPro
{
    public class TextMarkerViewModel : PropertyChangedBase
    {
        public event EventHandler<TextMarkerInfo> DeleteMarkerEvent;
        public TextMarkerViewModel()
        {
            _textMarkerInfo = new TextMarkerInfo();
        }
        private TextMarkerInfo _textMarkerInfo;

        public TextMarkerInfo textMarkerInfo
        {
            get
            {
                return _textMarkerInfo;
            }
            set
            {
                _textMarkerInfo = value;
            }
        }

        public bool DropDownShow
        {
            get
            {
                return _textMarkerInfo.DropDownShow;
            }
            set
            {
                _textMarkerInfo.DropDownShow = value;
                if (_textMarkerInfo.TextTitle == "" )
                {
                    TextShow = false;
                }
                else
                {
                    TextShow = !value;
                }
                NotifyPropertyChanged();
            }
        }

        public bool TextShow
        {
            get
            {
                return _textMarkerInfo.TextShow;
            }
            set
            {
                _textMarkerInfo.TextShow = value;
                NotifyPropertyChanged();
            }
        }

        public string TextTitle
        {
            get
            {
                return _textMarkerInfo.TextTitle;
            }
            set
            {
                _textMarkerInfo.TextTitle = value;
                if(_textMarkerInfo.TextTitle == "")
                {
                    TextShow = false;
                }
                if(DropDownShow == false && _textMarkerInfo.TextTitle != "")
                {
                    TextShow = true;
                }
                NotifyPropertyChanged();
            }
        }

        public string TextContent
        {
            get
            {
                return _textMarkerInfo.TextContent;
            }
            set
            {
                _textMarkerInfo.TextContent = value;
                NotifyPropertyChanged();
            }
        }

        #region  event
        public ICommand DeleteMarker
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    var result = MessageBoxX.Show("确定要删除当前标注，删除后无法恢复！", "Warning", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                    {
                        MessageBoxStyle = MessageBoxStyle.Classic,
                        MessageBoxIcon = MessageBoxIcon.Warning,
                        ButtonBrush = "#F1C825".ToColor().ToBrush(),
                    });
                    if (result == MessageBoxResult.Yes)
                    {
                        if (DeleteMarkerEvent != null)
                        {
                            DeleteMarkerEvent(this, textMarkerInfo);
                        }
                    }
                }));
            }
        }

        public ICommand OKMarker
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    textMarkerInfo.DropDownShow = false;
                }));
            }
        }

        public ICommand CancelMarker
        {
            get
            {
                return new RelayCommand(new Action<Object>(t =>
                {
                    textMarkerInfo.DropDownShow = false;
                }));
            }
        }
        #endregion
    }
}
