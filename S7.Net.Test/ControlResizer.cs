using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S7.Net.Test
{
    public class ControlResizer
    {
        class ControlPosAndSize
        {
            public string Name { get; set; }
            public float FrmWidth { get; set; }
            public float FrmHeight { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float FontSize { get; set; }
        }

        private Control _form;

        //句柄,大小信息
        private Dictionary<int, ControlPosAndSize> _dic = new Dictionary<int, ControlPosAndSize>();
        public ControlResizer(Control form)
        {
            _form = form;
            _form.Resize += _form_Resize;//绑定窗体大小改变事件

            _form.ControlAdded += form_ControlAdded;  //窗体上新增控件的处理
            _form.ControlRemoved += form_ControlRemoved;

            SnapControlSize(_form);//记录控件和窗体大小
        }

        void form_ControlRemoved(object sender, ControlEventArgs e)
        {
            var key = e.Control.Handle.ToInt32();
            _dic.Remove(key);
        }

        //绑定控件添加事件
        private void form_ControlAdded(object sender, ControlEventArgs e)
        {
            var ctl = e.Control;
            var ps = new ControlPosAndSize
            {
                Name = ctl.Name,
                FrmHeight = _form.Height,
                FrmWidth = _form.Width,
                Width = ctl.Width,
                Height = ctl.Height,
                Left = ctl.Left,
                Top = ctl.Top,
                FontSize = ctl.Font.Size
            };
            var key = ctl.Handle.ToInt32();
            _dic[key] = ps;
        }

        void _form_Resize(object sender, EventArgs e)
        {
            ResizeControl(_form);
        }

        private void ResizeControl(Control control)
        {
            if (_form.Width > 0 && _form.Height > 0)
                foreach (Control ctl in control.Controls)
                {
                    var key = ctl.Handle.ToInt32();
                    if (_dic.ContainsKey(key))
                    {
                        var ps = _dic[key];
                        var newx = _form.Width / ps.FrmWidth;
                        var newy = _form.Height / ps.FrmHeight;
                        if (ctl is DataGridView)
                        {
                            var dgv = ctl as DataGridView;
                            dgv.RowsDefaultCellStyle.Font = new Font(ctl.Font.Name, ps.FontSize * newy, ctl.Font.Style, ctl.Font.Unit);
                        }

                        ctl.Top = (int)(ps.Top * newy);
                        ctl.Height = (int)(ps.Height * newy);

                        ctl.Left = (int)(ps.Left * newx);
                        ctl.Width = (int)(ps.Width * newx);

                        ctl.Font = new Font(ctl.Font.Name, ps.FontSize * newy, ctl.Font.Style, ctl.Font.Unit);

                        if (ctl.Controls.Count > 0)
                        {
                            ResizeControl(ctl);
                        }
                    }
                }
        }

        /// <summary>
        /// 创建控件的大小快照,参数为需要记录大小控件的 容器
        /// </summary>
        private void SnapControlSize(Control control)
        {
            foreach (Control ctl in control.Controls)
            {
                ControlPosAndSize ps;
                //if (ctl is DataGridView)
                //{
                //    var dgv = ctl as DataGridView;
                //    ps = new ControlPosAndSize
                //    {
                //        Name = ctl.Name,
                //        FrmHeight = _form.Height,
                //        FrmWidth = _form.Width,
                //        Width = ctl.Width,
                //        Height = ctl.Height,
                //        Left = ctl.Left,
                //        Top = ctl.Top,
                //        FontSize = dgv.RowsDefaultCellStyle.Font.Size
                //    };
                //}
                //else
                    ps = new ControlPosAndSize
                    {
                        Name = ctl.Name,
                        FrmHeight = _form.Height,
                        FrmWidth = _form.Width,
                        Width = ctl.Width,
                        Height = ctl.Height,
                        Left = ctl.Left,
                        Top = ctl.Top,
                        FontSize = ctl.Font.Size
                    };

                var key = ctl.Handle.ToInt32();

                _dic[key] = ps;

                //绑定添加事件
                ctl.ControlAdded += form_ControlAdded;
                ctl.ControlRemoved += form_ControlRemoved;

                if (ctl.Controls.Count > 0)
                {
                    SnapControlSize(ctl);
                }
            }

        }

    }
}
