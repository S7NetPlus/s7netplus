using S7.Net;
using S7.Net.Types;
using System;
using System.Collections;
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
    public partial class Form1 : Form
    {
        private Plc plc;
        private byte[] cards = new byte[3];
        private Dictionary<int, BitArray> cardBits = new Dictionary<int, BitArray>();
        private List<Tuple<int, int, IoToggleSwitch>> toggles = new List<Tuple<int, int, IoToggleSwitch>>();
        private bool m_updating = false;


        public Form1()
        {
            InitializeComponent();
            this.Text = "SpecMetrix S7 - I/O Test (0.1-Alpha)";
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;

            var c = GetAll(this, typeof(IoToggleSwitch));
            foreach (var control in c)
            {
                IoToggleSwitch ctrl = control as IoToggleSwitch;
                toggles.Add(new Tuple<int, int, IoToggleSwitch>(ctrl.IoCardNumber, ctrl.Bit, ctrl));
            }

            cardBits.Add(0, new BitArray(6));
            cardBits.Add(1, new BitArray(8));
            cardBits.Add(2, new BitArray(8));
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                plc = new Plc(CpuType.S71200, "172.20.1.30", 0, 1);
                plc.Open();
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            catch (Exception)
            {
                readTimer.Enabled = false;
                MessageBox.Show($"Failed to Connect to PLC.");
                return;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                plc.Close();
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
            }
            catch (Exception)
            {
                MessageBox.Show($"Failed to Disconnect from PLC.");
                btnConnect.Enabled = true;
            }
        }

        private void toggleSwitch_Toggled(object sender, EventArgs e)
        {
            try
            {
                IoToggleSwitch sw = (IoToggleSwitch)sender;
                if (sender is IoToggleSwitch)
                {
                    UpdateIo(sw.IoCardNumber, sw.Bit, sw.IsOn);
                    //cardBits[sw.IoCardNumber].Set(sw.Bit, sw.IsOn);
                    //BitArraytoByteArray();
                    //byte[] val = new byte[1];
                    //val[0] = cards[sw.IoCardNumber];
                    //plc.WriteBytes(DataType.Output, 0, sw.IoCardNumber, val);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Failure when setting IO.");
                btnConnect.Enabled = true;
            }
        }

        private void UpdateIo(int card, int bit, bool state) => plc.WriteBit(DataType.Output, 0, card, bit, state);

        private void BitArraytoByteArray()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = Convert.ToByte(getIntFromBitArray(cardBits[i]));
            }
        }

        private BitArray ByteToBitArray(byte b)
        {
            return new BitArray(new byte[] { b });
        }

        private int getIntFromBitArray(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

        private void readTimer_Tick(object sender, EventArgs e)
        {
            UpdateState();
        }

        private async Task UpdateState()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (m_updating)
                        return;

                    m_updating = true;

                    cards = plc?.ReadBytes(DataType.Output, 0, 0, 3);
                    foreach (var card in cards)
                    {
                        BitArray ba = ByteToBitArray(card);

                    }

                    foreach (var item in toggles)
                    {
                        //item.Item3.IsOn = 
                    }
                }
                finally
                {
                    m_updating = false;
                }
            });
        }
        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

    }
}
