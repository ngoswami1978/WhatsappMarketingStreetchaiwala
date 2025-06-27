using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using WhatsappAgent;
using WhatsappAgentUI.Model;
using System.Diagnostics;

namespace WhatsappAgentUI
{
    public partial class Form1 : Form
    {
        Messegner? Messegner = null;
        private List<Contact> contacts = new List<Contact>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Messegner_OnQRReady(Image qrbmp)
        {
            pictureBox1.Image = qrbmp;
            textBox1.Invoke(() => textBox1.AppendLine("please scan the QR code using your Whatsapp mobile app to continue login."));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.AppendLine("sending text message...");
                //Messegner?.SendMessage(txtmobile.Text, textmsg.Text);
                //var numbers = new List<string>
                //{
                //    "9899846293",
                //    "8860483590",
                //    "7982638240"
                //};

                var message = textmsg.Text;
                if (contacts == null || contacts.Count == 0)
                {
                    //textBox1.ForeColor = Color.Red;
                    textBox1.AppendLine("⚠️ Please upload the contact list before sending messages.");
                    //textBox1.ForeColor = Color.Black;
                }
                else
                {
                    lblcount.Text = contacts.Count.ToString();  
                    SendMessageToMultiple(contacts, message);
                }

                //SendMessageToMultiple(contacts, message);
                textBox1.AppendLine("text message sent.");
            }
            catch (Exception ex)
            {
                textBox1.AppendLine(ex.Message);
            }
        }
        public void SendMessageToMultiple(List<Contact> numbers, string message)
        {
            int totalCount = numbers.Count;
            int currentCount = 0;

            foreach (var number in numbers)
            {
                try
                {
                    currentCount++;
                    Messegner?.SendMessage(number.ContactNumber.ToString(), message);
                    //SendMessage(number, message);
                    textBox1.AppendLine($"Message sent to {number.ContactNumber}");
                    lblcount.Text = $"{currentCount}/{totalCount}";
                }
                catch (Exception ex)
                {
                    textBox1.AppendLine($"Failed to send to {number.ContactNumber}: {ex.Message}");
                }

                // Optional: Wait 3 seconds before next message to avoid rate limits
                Thread.Sleep(3000);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {              
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    Title = "Select Image File",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Images (*.BMP;*.JPG,*.PNG)|*.BMP;*.JPG;*.PNG;",
                    Multiselect = false
                };
                string message = "Street Chai wala";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.AppendLine("sending image...");
                    if (contacts == null || contacts.Count == 0)
                    {
                        //textBox1.ForeColor = Color.Red;
                        textBox1.AppendLine("⚠️ Please upload the contact list before sending messages.");
                        //textBox1.ForeColor = Color.Black;
                    }
                    else {
                        SendMediaMessageToMultiple(contacts, message, openFileDialog1.FileName);
                    }
                    
                    //Messegner?.SendMedia(MediaType.IMAGE_OR_VIDEO, txtmobile.Text, openFileDialog1.FileName, textmsg.Text);

                    textBox1.AppendLine("image sent.");
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendLine(ex.Message);
            }
        }

        public void SendMediaMessageToMultiple(List<Contact> numbers, string message,string FileName)
        {
            int totalCount = numbers.Count;
            int currentCount = 0;

            foreach (var number in numbers)
            {
                try
                {
                    currentCount++;
                    Messegner?.SendMedia(MediaType.IMAGE_OR_VIDEO, number.ContactNumber.ToString(), FileName, textmsg.Text);
                    //SendMessage(number, message);
                    textBox1.AppendLine($"Message sent to {number.ContactNumber}");
                    lblcount.Text = $"{currentCount}/{totalCount}";
                }
                catch (Exception ex)
                {
                    textBox1.AppendLine($"Failed to send to {number.ContactNumber}: {ex.Message}");
                }

                // Optional: Wait 3 seconds before next message to avoid rate limits
                Thread.Sleep(3000);
            }
        }
        public void KillChromiumProcesses()
        {
            try
            {
                // Kill chromedriver
                foreach (var process in Process.GetProcessesByName("chromedriver"))
                {
                    process.Kill();
                }

                // Kill chrome (or chromium) browser
                foreach (var process in Process.GetProcessesByName("chrome"))
                {
                    process.Kill();
                }

                // Optional: Kill chromium if you're using a separate Chromium build
                foreach (var process in Process.GetProcessesByName("chromium"))
                {
                    process.Kill();
                }

                Console.WriteLine("✅ All Chrome/Chromium processes terminated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Failed to kill browser processes: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    Title = "Select Attachment",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = false
                };

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.AppendLine("sending attachment...");
                    Messegner?.SendMedia(MediaType.ATTACHMENT, txtmobile.Text, openFileDialog1.FileName, textmsg.Text);
                    textBox1.AppendLine("attachment sent.");
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendLine(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {                

                textBox1.AppendLine("logging out...");
                Messegner?.Logout();
                textBox1.AppendLine("logged out.");
            }
            catch (Exception ex)
            {
                textBox1.AppendLine(ex.Message);
            }
            finally
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Messegner?.Dispose();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Messegner?.Login();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            textBox1.AppendLine("logging in...");
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                textBox1.AppendLine(e.Error.ToString());
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button7.Enabled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                KillChromiumProcesses();

                checkBox1.Enabled = false;
                button6.Enabled = false;
                textBox1.AppendLine("starting driver...");
                Messegner = new Messegner(checkBox1.Checked);
                Messegner.OnQRReady += Messegner_OnQRReady;
                textBox1.AppendLine("driver started.");
                button5.Enabled = true;
            }
            catch (Exception ex)
            {
                textBox1.AppendLine(ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String[] contactFileData = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                contactFileData = File.ReadAllLines(ofd.FileName);

                try
                {
                    foreach (var item in contactFileData)
                    {
                        contacts.Add(new Contact(long.Parse(item)));
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}