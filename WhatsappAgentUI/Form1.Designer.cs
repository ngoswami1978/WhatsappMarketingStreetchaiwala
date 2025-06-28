namespace WhatsappAgentUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            textBox1 = new TextBox();
            button4 = new Button();
            pictureBox1 = new PictureBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerSending = new System.ComponentModel.BackgroundWorker();
            button5 = new Button();
            textmsg = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtmobile = new TextBox();
            button6 = new Button();
            checkBox1 = new CheckBox();
            button7 = new Button();
            lblcount = new Label();
            statusStrip1 = new StatusStrip();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button1.Enabled = false;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Location = new Point(818, 11);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(334, 28);
            button1.TabIndex = 0;
            button1.Text = "Send Message Offer";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button2.Enabled = false;
            button2.ForeColor = SystemColors.ButtonHighlight;
            button2.Location = new Point(17, 207);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(172, 28);
            button2.TabIndex = 1;
            button2.Text = "Send Offer as Image";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button3.Enabled = false;
            button3.ForeColor = SystemColors.ButtonHighlight;
            button3.Location = new Point(15, 328);
            button3.Margin = new Padding(4);
            button3.Name = "button3";
            button3.Size = new Size(172, 28);
            button3.TabIndex = 2;
            button3.Text = "logout";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.White;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.ForeColor = SystemColors.WindowText;
            textBox1.Location = new Point(17, 371);
            textBox1.Margin = new Padding(4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(545, 239);
            textBox1.TabIndex = 3;
            // 
            // button4
            // 
            button4.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button4.Enabled = false;
            button4.ForeColor = SystemColors.ButtonHighlight;
            button4.Location = new Point(17, 235);
            button4.Margin = new Padding(4);
            button4.Name = "button4";
            button4.Size = new Size(172, 28);
            button4.TabIndex = 4;
            button4.Text = "send attachment";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(195, 14);
            pictureBox1.Margin = new Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Padding = new Padding(13, 12, 13, 12);
            pictureBox1.Size = new Size(366, 341);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            // 
            // button5
            // 
            button5.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button5.Enabled = false;
            button5.ForeColor = SystemColors.ButtonHighlight;
            button5.Location = new Point(15, 72);
            button5.Margin = new Padding(4);
            button5.Name = "button5";
            button5.Size = new Size(172, 28);
            button5.TabIndex = 6;
            button5.Text = "login";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // textmsg
            // 
            textmsg.Location = new Point(568, 44);
            textmsg.Margin = new Padding(4);
            textmsg.Multiline = true;
            textmsg.Name = "textmsg";
            textmsg.Size = new Size(584, 566);
            textmsg.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Image = Properties.Resources.StreetChaiwalaLogos06;
            label1.Location = new Point(568, 15);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(223, 23);
            label1.TabIndex = 8;
            label1.Text = "Type Offer Message below";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(35, 118);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(127, 18);
            label2.TabIndex = 10;
            label2.Text = "Mobile Numbers";
            label2.Visible = false;
            // 
            // txtmobile
            // 
            txtmobile.Location = new Point(35, 148);
            txtmobile.Margin = new Padding(4);
            txtmobile.Name = "txtmobile";
            txtmobile.Size = new Size(153, 26);
            txtmobile.TabIndex = 9;
            txtmobile.Text = "70434962";
            txtmobile.Visible = false;
            // 
            // button6
            // 
            button6.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button6.ForeColor = SystemColors.ButtonHighlight;
            button6.Location = new Point(15, 44);
            button6.Margin = new Padding(4);
            button6.Name = "button6";
            button6.Size = new Size(172, 28);
            button6.TabIndex = 11;
            button6.Text = "Start Driver";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.BackColor = SystemColors.ActiveBorder;
            checkBox1.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            checkBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBox1.ForeColor = SystemColors.ButtonHighlight;
            checkBox1.Location = new Point(15, 14);
            checkBox1.Margin = new Padding(4);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(125, 24);
            checkBox1.TabIndex = 12;
            checkBox1.Text = "Hide Browser";
            checkBox1.UseVisualStyleBackColor = false;
            // 
            // button7
            // 
            button7.BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            button7.Enabled = false;
            button7.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            button7.ForeColor = SystemColors.ButtonHighlight;
            button7.Location = new Point(17, 118);
            button7.Margin = new Padding(4);
            button7.Name = "button7";
            button7.Size = new Size(172, 65);
            button7.TabIndex = 13;
            button7.Text = "Add Customer List";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // lblcount
            // 
            lblcount.AutoSize = true;
            lblcount.Location = new Point(954, 11);
            lblcount.Margin = new Padding(4, 0, 4, 0);
            lblcount.Name = "lblcount";
            lblcount.Size = new Size(0, 18);
            lblcount.TabIndex = 14;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Location = new Point(0, 614);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1165, 22);
            statusStrip1.TabIndex = 16;
            statusStrip1.Text = "statusStrip1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.StreetChaiwalaLogos06;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1165, 636);
            Controls.Add(statusStrip1);
            Controls.Add(lblcount);
            Controls.Add(button7);
            Controls.Add(pictureBox1);
            Controls.Add(checkBox1);
            Controls.Add(button6);
            Controls.Add(label2);
            Controls.Add(txtmobile);
            Controls.Add(label1);
            Controls.Add(textmsg);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(textBox1);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "STREET CHAI WALA  WHATSAPP CAMPAIGN";
            FormClosing += Form1_FormClosing;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private TextBox textBox1;
        private Button button4;
        private PictureBox pictureBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSending;

        private Button button5;
        private TextBox textmsg;
        private Label label1;
        private Label label2;
        private TextBox txtmobile;
        private Button button6;
        private CheckBox checkBox1;
        private Button button7;
        private Label lblcount;
        private StatusStrip statusStrip1;
    }
}