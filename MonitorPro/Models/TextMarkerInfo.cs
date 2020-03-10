using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPro
{
    //标注信息
    public class TextMarkerInfo
    {
        public TextMarkerInfo()
        {
            Id = Guid.NewGuid().ToString();
        }
        private string Id;
        public string ID
        {
            get
            {
                return Id;
            }
        }
        private bool _DropDownShow = true;
        public bool DropDownShow
        {
            get
            {
                return _DropDownShow;
            }
            set
            {
                _DropDownShow = value;
            }
        }
        private bool _TextShow = false;
        public bool TextShow {
            get
            {
                return _TextShow;
            }
            set
            {
                _TextShow = value;
            }
        }
        private string _TextTitle = "";
        public string TextTitle {
            get
            {
                return _TextTitle;
            }
            set
            {
                _TextTitle = value;
            }
        }

        private string _TextContent = "";
        public string TextContent {
            get
            {
                return _TextContent;
            }
            set
            {
                _TextContent = value;
            }
        }
    }
}
