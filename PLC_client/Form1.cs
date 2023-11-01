using S7.Net;

namespace PLC_client
{
    public partial class Form1 : Form
    {
        private Plc plc;
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            try
            {
                //var readedBytes = await plc.ReadBytesAsync(, 0, 0, 200); hola
                var value = await plc.ReadAsync(DataType.DataBlock, 3, 0, VarType.S7String, 18);


                textBox2.Text =value?.ToString();
                // string result = System.Text.Encoding.UTF8.GetString(readedBytes);

                MessageBox.Show("result " + value);
            }
            catch (Exception exception)
            {
                textBox1.Text = exception.Message;
            }


        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            plc = new Plc(CpuType.S71200, "10.0.1.170", 0, 0);

            await plc.OpenAsync();
            MessageBox.Show("is connected " + plc.IsConnected);
        }

        private void CleanButton_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }
    }
}