using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WhatsappAgent;
using WhatsappAgentUI.Model;

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

        #region New Button Logic (Bold, Italic, Underline, Emoji)
        
        private void btnEmoji_Click(object sender, EventArgs e)
        {
            using (frmEmojiPicker emojiForm = new frmEmojiPicker())
            {
                Point screenPoint = btnEmoji.PointToScreen(new Point(0, btnEmoji.Height));
                emojiForm.Location = screenPoint;
                if (emojiForm.ShowDialog(this) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(emojiForm.SelectedEmoji))
                    {
                        textmsg.Focus();
                        textmsg.SelectedText = emojiForm.SelectedEmoji;
                    }
                }
            }
        }
        
        private void btnBold_Click(object sender, EventArgs e)
        {
            textmsg.Focus();
            Font currentFont = textmsg.SelectionFont;
            FontStyle newStyle;
            if (textmsg.SelectionFont.Bold) { newStyle = currentFont.Style & ~FontStyle.Bold; } else { newStyle = currentFont.Style | FontStyle.Bold; }
            textmsg.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
        }

        private void btnItalic_Click(object sender, EventArgs e)
        {
            textmsg.Focus();
            Font currentFont = textmsg.SelectionFont;
            FontStyle newStyle;
            if (textmsg.SelectionFont.Italic) { newStyle = currentFont.Style & ~FontStyle.Italic; } else { newStyle = currentFont.Style | FontStyle.Italic; }
            textmsg.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
        }

        private void btnUnderline_Click(object sender, EventArgs e)
        {
            textmsg.Focus();
            Font currentFont = textmsg.SelectionFont;
            FontStyle newStyle;
            if (textmsg.SelectionFont.Underline) { newStyle = currentFont.Style & ~FontStyle.Underline; } else { newStyle = currentFont.Style | FontStyle.Underline; }
            textmsg.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
        }

        #endregion

        #region Existing Code
        private void Messegner_OnQRReady(Image qrbmp)
        {
            pictureBox1.Image = qrbmp;
            textBox1.Invoke(() => textBox1.AppendLine("please scan the QR code using your Whatsapp mobile app to continue login."));
        }

        private string ConvertRtfToWhatsAppFormat(RichTextBox rtb)
        {
            if (string.IsNullOrEmpty(rtb.Text)) return string.Empty;
            var result = new StringBuilder();
            const char boldChar = '*';
            const char italicChar = '_';
            const char strikeChar = '~';
            for (int i = 0; i < rtb.Text.Length; i++)
            {
                rtb.Select(i, 1);
                bool isBold = rtb.SelectionFont.Bold;
                bool isItalic = rtb.SelectionFont.Italic;
                bool isStrikethrough = rtb.SelectionFont.Strikeout;
                bool wasBold = false, wasItalic = false, wasStrikethrough = false;
                if (i > 0)
                {
                    rtb.Select(i - 1, 1);
                    wasBold = rtb.SelectionFont.Bold;
                    wasItalic = rtb.SelectionFont.Italic;
                    wasStrikethrough = rtb.SelectionFont.Strikeout;
                }
                if (!isBold && wasBold) result.Append(boldChar);
                if (!isItalic && wasItalic) result.Append(italicChar);
                if (!isStrikethrough && wasStrikethrough) result.Append(strikeChar);
                if (isBold && !wasBold) result.Append(boldChar);
                if (isItalic && !wasItalic) result.Append(italicChar);
                if (isStrikethrough && !wasStrikethrough) result.Append(strikeChar);
                result.Append(rtb.Text[i]);
            }
            rtb.Select(rtb.Text.Length - 1, 1);
            if (rtb.SelectionFont.Bold) result.Append(boldChar);
            if (rtb.SelectionFont.Italic) result.Append(italicChar);
            if (rtb.SelectionFont.Strikeout) result.Append(strikeChar);
            rtb.Select(0, 0);
            return result.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.AppendLine("sending text message...");
                var message = ConvertRtfToWhatsAppFormat(textmsg);
                if (string.IsNullOrWhiteSpace(message)) { textBox1.AppendLine("⚠️ Message cannot be empty."); return; }
                if (contacts == null || contacts.Count == 0) { textBox1.AppendLine("⚠️ Please upload the contact list before sending messages."); }
                else { lblcount.Text = contacts.Count.ToString(); SendMessageToMultiple(contacts, message); }
                textBox1.AppendLine("text message sent.");
            }
            catch (Exception ex) { textBox1.AppendLine(ex.Message); }
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
                    textBox1.AppendLine($"Message sent to {number.ContactNumber}");
                    lblcount.Text = $"{currentCount}/{totalCount}";
                }
                catch (Exception ex) { textBox1.AppendLine($"Failed to send to {number.ContactNumber}: {ex.Message}"); }
                Thread.Sleep(3000);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog { Title = "Select Image File", CheckFileExists = true, CheckPathExists = true, Filter = "Images (*.BMP;*.JPG,*.PNG)|*.BMP;*.JPG;*.PNG;", Multiselect = false };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.AppendLine("sending image...");
                    if (contacts == null || contacts.Count == 0) { textBox1.AppendLine("⚠️ Please upload the contact list before sending messages."); }
                    else { SendMediaMessageToMultiple(contacts, "dummy", openFileDialog1.FileName); }
                    textBox1.AppendLine("image sent.");
                }
            }
            catch (Exception ex) { textBox1.AppendLine(ex.Message); }
        }

        public void SendMediaMessageToMultiple(List<Contact> numbers, string message, string FileName)
        {
            int totalCount = numbers.Count;
            int currentCount = 0;
            foreach (var number in numbers)
            {
                try
                {
                    currentCount++;
                    Messegner?.SendMedia(MediaType.IMAGE_OR_VIDEO, number.ContactNumber.ToString(), FileName, ConvertRtfToWhatsAppFormat(textmsg));
                    textBox1.AppendLine($"Message sent to {number.ContactNumber}");
                    lblcount.Text = $"{currentCount}/{totalCount}";
                }
                catch (Exception ex) { textBox1.AppendLine($"Failed to send to {number.ContactNumber}: {ex.Message}"); }
                Thread.Sleep(3000);
            }
        }
        public void KillChromiumProcesses()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("chromedriver")) { process.Kill(); }
                foreach (var process in Process.GetProcessesByName("chrome")) { process.Kill(); }
                foreach (var process in Process.GetProcessesByName("chromium")) { process.Kill(); }
                Console.WriteLine("✅ All Chrome/Chromium processes terminated.");
            }
            catch (Exception ex) { Console.WriteLine("⚠️ Failed to kill browser processes: " + ex.Message); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog { Title = "Select Attachment", CheckFileExists = true, CheckPathExists = true, Multiselect = false };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.AppendLine("sending attachment...");
                    if (contacts != null && contacts.Count > 0)
                    {
                        foreach (var contact in contacts)
                        {
                            Messegner?.SendMedia(MediaType.ATTACHMENT, contact.ContactNumber.ToString(), openFileDialog1.FileName, ConvertRtfToWhatsAppFormat(textmsg));
                            textBox1.AppendLine($"Attachment sent to {contact.ContactNumber}");
                            Thread.Sleep(3000);
                        }
                    }
                    else { textBox1.AppendLine("⚠️ No contacts loaded to send attachment to."); }
                    textBox1.AppendLine("attachment sending finished.");
                }
            }
            catch (Exception ex) { textBox1.AppendLine(ex.Message); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.AppendLine("logging out...");
                Messegner?.Logout();
                textBox1.AppendLine("logged out.");
            }
            catch (Exception ex) { textBox1.AppendLine(ex.Message); }
            finally { button1.Enabled = false; button2.Enabled = false; button3.Enabled = false; button4.Enabled = false; }
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
            if (e.Error != null) { textBox1.AppendLine(e.Error.ToString()); }
            else { button1.Enabled = true; button2.Enabled = true; button3.Enabled = true; button4.Enabled = true; button7.Enabled = true; }
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
            catch (Exception ex) { textBox1.AppendLine(ex.Message); }
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
                contacts.Clear();
                contactFileData = File.ReadAllLines(ofd.FileName);
                try
                {
                    foreach (var item in contactFileData)
                    {
                        if (long.TryParse(item.Trim(), out long number))
                        {
                            contacts.Add(new Contact(number));
                        }
                    }
                    textBox1.AppendLine($"Loaded {contacts.Count} contacts.");
                    lblcount.Text = $"{contacts.Count} contacts loaded.";
                }
                catch (Exception ex) { textBox1.AppendLine("Error loading contacts: " + ex.Message); }
            }
        }
        #endregion
    }
}