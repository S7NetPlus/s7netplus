namespace PLC_client
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            ConnectButton = new Button();
            textBox1 = new TextBox();
            CleanButton = new Button();
            textBox2 = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(141, 25);
            button1.Name = "button1";
            button1.Size = new Size(124, 71);
            button1.TabIndex = 0;
            button1.Text = "ReadData";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ConnectButton
            // 
            ConnectButton.Location = new Point(11, 25);
            ConnectButton.Name = "ConnectButton";
            ConnectButton.Size = new Size(124, 71);
            ConnectButton.TabIndex = 1;
            ConnectButton.Text = "Connect";
            ConnectButton.UseVisualStyleBackColor = true;
            ConnectButton.Click += ConnectButton_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(11, 204);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(383, 79);
            textBox1.TabIndex = 2;
            // 
            // CleanButton
            // 
            CleanButton.Location = new Point(271, 25);
            CleanButton.Name = "CleanButton";
            CleanButton.Size = new Size(124, 71);
            CleanButton.TabIndex = 3;
            CleanButton.Text = "Clean";
            CleanButton.UseVisualStyleBackColor = true;
            CleanButton.Click += CleanButton_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 102);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(383, 79);
            textBox2.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(481, 295);
            Controls.Add(textBox2);
            Controls.Add(CleanButton);
            Controls.Add(textBox1);
            Controls.Add(ConnectButton);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button ConnectButton;
        private TextBox textBox1;
        private Button CleanButton;
        private TextBox textBox2;
    }
}