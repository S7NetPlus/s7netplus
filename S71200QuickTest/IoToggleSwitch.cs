using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S71200QuickTest
{
    public partial class IoToggleSwitch : DevExpress.XtraEditors.ToggleSwitch
    {
        public int IoCardNumber { get; set; }

        public int Bit { get; set; }


        public IoToggleSwitch()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
