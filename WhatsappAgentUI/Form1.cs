using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            InitializeSendingWorker();
            textBox1.AppendLine("Welcome to WhatsApp Campaign Tool!");
            textBox1.AppendLine("1. Click 'Start Driver' to launch browser.");
            textBox1.AppendLine("2. Click 'Login' and scan QR code.");
            textBox1.AppendLine("3. 'Upload Contact File' (CSV or TXT).");
            textBox1.AppendLine("4. Type message or select media, then click 'Send'.");
        }

        private void InitializeSendingWorker()
        {
            // This method configures the new background worker for sending campaigns.
            // Assumes 'backgroundWorkerSending' was added in the designer.
            backgroundWorkerSending.WorkerReportsProgress = true;
            backgroundWorkerSending.WorkerSupportsCancellation = true;
            backgroundWorkerSending.DoWork += SendCampaignWorker_DoWork;
            backgroundWorkerSending.ProgressChanged += SendCampaignWorker_ProgressChanged;
            backgroundWorkerSending.RunWorkerCompleted += SendCampaignWorker_RunWorkerCompleted;
        }

        #region Messenger Event Handlers
        private void Messegner_OnQRReady(Image qrbmp)
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new Action(() => pictureBox1.Image = qrbmp));
            }
            else
            {
                pictureBox1.Image = qrbmp;
            }
            textBox1.Invoke(() => textBox1.AppendLine("[QR] Please scan the QR code using your WhatsApp mobile app."));
        }

        private void Messegner_OnDisposed()
        {
            this.Invoke((MethodInvoker)delegate {
                textBox1.AppendLine("[STATUS] Messenger has been disposed. Please restart the driver.");
                button6.Enabled = true;
                checkBox1.Enabled = true;
                button5.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button7.Enabled = false;
            });
        }
        #endregion

        #region Button Click Logic
        // "Start Driver" button
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                KillChromiumProcesses();
                checkBox1.Enabled = false;
                button6.Enabled = false;
                textBox1.AppendLine("[INIT] Starting browser driver...");

                Messegner = new Messegner(checkBox1.Checked);
                Messegner.OnQRReady += Messegner_OnQRReady;
                Messegner.OnDisposed += Messegner_OnDisposed;

                textBox1.AppendLine("[INIT] Driver started. Ready for login.");
                button5.Enabled = true;
            }
            catch (Exception ex)
            {
                textBox1.AppendLine($"[ERROR] Failed to start driver: {ex.Message}");
                button6.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        // "Login" button
        private void button5_Click(object sender, EventArgs e)
        {
            if (Messegner == null)
            {
                textBox1.AppendLine("⚠️ Please start the driver first.");
                return;
            }
            button5.Enabled = false;
            textBox1.AppendLine("[STATUS] Attempting to log in... Please wait for the QR Code.");
            backgroundWorker1.RunWorkerAsync();
        }

        // "Upload Contacts" button
        private void button7_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Title = "Select Contact File (TXT or CSV)", Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    contacts.Clear();
                    textBox1.AppendLine($"[FILE] Loading contacts from: {ofd.FileName}");
                    try
                    {
                        var lines = File.ReadAllLines(ofd.FileName);
                        foreach (var (line, index) in lines.Select((value, i) => (value, i)))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = line.Split(',');
                            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                            {
                                // Using the new Contact class that holds all info for a message
                                var contact = new Contact(parts[0].Trim());
                                if (parts.Length > 1) contact.Message = parts[1].Trim();
                                if (parts.Length > 2 && Enum.TryParse(parts[2].Trim(), true, out MediaType mediaType)) contact.MediaType = mediaType;
                                if (parts.Length > 3) contact.FilePath = parts[3].Trim();
                                if (parts.Length > 4) contact.Caption = parts[4].Trim();
                                contacts.Add(contact);
                            }
                            else
                            {
                                textBox1.AppendLine($"[WARNING] Skipping invalid line {index + 1}: '{line}'.");
                            }
                        }
                        textBox1.AppendLine($"[FILE] Loaded {contacts.Count} contacts.");
                        lblcount.Text = $"{contacts.Count} contacts loaded.";
                    }
                    catch (Exception ex)
                    {
                        textBox1.AppendLine($"[ERROR] Failed to load contacts: {ex.Message}");
                        contacts.Clear();
                        lblcount.Text = "0 contacts loaded.";
                    }
                }
            }
        }

        // "Send Text Message" button
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textmsg.Text))
            {
                textBox1.AppendLine("⚠️ Please enter a message to send.");
                return;
            }
            if (contacts == null || !contacts.Any())
            {
                textBox1.AppendLine("⚠️ Please upload a contact list first.");
                return;
            }

            foreach (var contact in contacts)
            {
                contact.Message = ConvertRtfToWhatsAppFormat(textmsg); // Use the converter
                contact.MediaType = null;
                contact.FilePath = string.Empty;
                contact.Caption = string.Empty;
            }
            StartSendingCampaign();
        }

        // "Send Image" button
        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog { Title = "Select Image File", Filter = "Images (*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF", Multiselect = false })
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (contacts == null || !contacts.Any())
                    {
                        textBox1.AppendLine("⚠️ Please upload a contact list first.");
                        return;
                    }

                    foreach (var contact in contacts)
                    {
                        contact.MediaType = MediaType.IMAGE_OR_VIDEO;
                        contact.FilePath = openFileDialog1.FileName;
                        contact.Caption = ConvertRtfToWhatsAppFormat(textmsg); // Use the converter
                        contact.Message = string.Empty;
                    }
                    StartSendingCampaign();
                }
            }
        }

        // "Send Attachment" button
        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog { Title = "Select Attachment File", Multiselect = false })
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (contacts == null || !contacts.Any())
                    {
                        textBox1.AppendLine("⚠️ Please upload a contact list first.");
                        return;
                    }

                    foreach (var contact in contacts)
                    {
                        contact.MediaType = MediaType.ATTACHMENT;
                        contact.FilePath = openFileDialog1.FileName;
                        contact.Caption = ConvertRtfToWhatsAppFormat(textmsg); // Use the converter
                        contact.Message = string.Empty;
                    }
                    StartSendingCampaign();
                }
            }
        }

        // "Logout" button
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.AppendLine("[STATUS] Logging out...");
                Messegner?.Logout();
            }
            catch (Exception ex)
            {
                textBox1.AppendLine($"[ERROR] Logout failed: {ex.Message}");
            }
        }
        #endregion

        #region Login BackgroundWorker (backgroundWorker1)
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Messegner?.Login();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                textBox1.AppendLine($"[ERROR] Login failed: {e.Error.Message}");
                button5.Enabled = true;
                Messegner?.Dispose();
            }
            else
            {
                textBox1.AppendLine("[STATUS] Login successful!");
                pictureBox1.Image = null;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button7.Enabled = true;
            }
        }
        #endregion

        #region Sending Campaign BackgroundWorker (backgroundWorkerSending)
        private void StartSendingCampaign()
        {
            if (Messegner == null || Messegner.IsDisposed)
            {
                textBox1.AppendLine("⚠️ Messenger is not running. Please start and log in first.");
                return;
            }
            if (backgroundWorkerSending.IsBusy)
            {
                textBox1.AppendLine("⚠️ A sending campaign is already in progress.");
                return;
            }

            textBox1.AppendLine($"[CAMPAIGN] Starting campaign for {contacts.Count} contacts...");
            // Disable UI controls
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            button3.Enabled = false; // Disable logout during send
            button7.Enabled = false; // Disable contact upload during send

            // Pass a copy of the list to the worker to avoid thread issues
            backgroundWorkerSending.RunWorkerAsync(new List<Contact>(contacts));
        }

        private void SendCampaignWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var contactsToSend = e.Argument as List<Contact>;
            int totalCount = contactsToSend.Count;
            int successfulSends = 0;
            int failedSends = 0;

            for (int i = 0; i < totalCount; i++)
            {
                if (backgroundWorkerSending.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                Contact contact = contactsToSend[i];
                string logMessage = $"[PROCESS] ({i + 1}/{totalCount}) Sending to {contact.ContactNumber}...";
                backgroundWorkerSending.ReportProgress((int)(((double)(i + 1) / totalCount) * 100), logMessage);

                try
                {
                    if (string.IsNullOrEmpty(contact.ContactNumber)) throw new ArgumentException("Contact number is empty.");

                    if (contact.MediaType.HasValue && !string.IsNullOrEmpty(contact.FilePath))
                    {
                        Messegner?.SendMedia(contact.MediaType.Value, contact.ContactNumber, contact.FilePath, contact.Caption);
                    }
                    else if (!string.IsNullOrEmpty(contact.Message))
                    {
                        Messegner?.SendMessage(contact.ContactNumber, contact.Message);
                    }
                    else
                    {
                        throw new ArgumentException("No message or media specified for contact.");
                    }

                    successfulSends++;
                    backgroundWorkerSending.ReportProgress(e.ProgressPercentage, $"[SUCCESS] Sent to {contact.ContactNumber}.");
                }
                catch (Exception ex)
                {
                    failedSends++;
                    // Report a concise error message
                    backgroundWorkerSending.ReportProgress(e.ProgressPercentage, $"[FAILURE] Failed for {contact.ContactNumber}: {ex.Message.Split('\n').FirstOrDefault()}");
                }

                // Use the randomized wait from the Messegner class for anti-detection
                Messegner?.Wait(8, 20); // Random wait between 8 and 20 seconds
            }

            e.Result = new Tuple<int, int>(successfulSends, failedSends);
        }

        private void SendCampaignWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update UI with progress from the worker thread
            textBox1.AppendLine(e.UserState?.ToString());
            lblcount.Text = $"Progress: {e.ProgressPercentage}%";
        }

        private void SendCampaignWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                textBox1.AppendLine("[CAMPAIGN] Sending was cancelled.");
            }
            else if (e.Error != null)
            {
                textBox1.AppendLine($"[CAMPAIGN ERROR] An unexpected error occurred: {e.Error.Message}");
            }
            else if (e.Result is Tuple<int, int> results)
            {
                textBox1.AppendLine("\n------------------------------------------");
                textBox1.AppendLine("[CAMPAIGN COMPLETE] Bulk messaging finished.");
                textBox1.AppendLine($"  Successful sends: {results.Item1}");
                textBox1.AppendLine($"  Failed sends:     {results.Item2}");
                textBox1.AppendLine("------------------------------------------\n");
            }

            // Re-enable UI controls
            if (Messegner != null && !Messegner.IsDisposed)
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button7.Enabled = true;
            }
            lblcount.Text = "Campaign finished.";
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Converts the formatted text from a RichTextBox into WhatsApp's special character format.
        /// </summary>
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

                // Add closing characters if a style has just ended
                if (!isBold && wasBold) result.Append(boldChar);
                if (!isItalic && wasItalic) result.Append(italicChar);
                if (!isStrikethrough && wasStrikethrough) result.Append(strikeChar);

                // Add opening characters if a new style has just begun
                if (isBold && !wasBold) result.Append(boldChar);
                if (isItalic && !wasItalic) result.Append(italicChar);
                if (isStrikethrough && !wasStrikethrough) result.Append(strikeChar);

                result.Append(rtb.Text[i]);
            }

            // Append closing characters for the very last character if it was formatted
            rtb.Select(rtb.Text.Length - 1, 1);
            if (rtb.SelectionFont.Bold) result.Append(boldChar);
            if (rtb.SelectionFont.Italic) result.Append(italicChar);
            if (rtb.SelectionFont.Strikeout) result.Append(strikeChar);

            rtb.Select(0, 0); // Reset selection
            return result.ToString();
        }

        public void KillChromiumProcesses()
        {
            textBox1.AppendLine("[CLEANUP] Terminating residual Chrome/Chromium processes...");
            var processNames = new[] { "chromedriver", "chrome", "chromium" };
            foreach (var name in processNames)
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000); // Wait up to 3 seconds
                    }
                    catch
                    {
                        // Ignore errors if process is already gone or access is denied
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorkerSending.IsBusy)
            {
                backgroundWorkerSending.CancelAsync(); // Attempt to cancel gracefully
            }
            Messegner?.Dispose();
            KillChromiumProcesses(); // Final cleanup on exit
        }
        #endregion
    }
}