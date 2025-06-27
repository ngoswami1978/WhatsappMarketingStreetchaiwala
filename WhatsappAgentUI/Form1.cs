using System;
using System.Collections.Generic;
using System.ComponentModel; // For BackgroundWorker
using System.Diagnostics;
using System.Drawing; // For Image
using System.IO; // For File operations
using System.Windows.Forms;
using WhatsappAgent;
using WhatsappAgentUI.Model;

// Note: System.Linq and System.Threading are also used but may be implicitly included.
// It's good practice to have them listed if you use methods from them.
using System.Linq;
using System.Threading;


namespace WhatsappAgentUI
{
    public partial class Form1 : Form
    {
        Messegner? Messegner = null;
        private List<Contact> contacts = new List<Contact>();

        public Form1()
        {
            InitializeComponent();
            InitializeSendingWorker(); // Call initialization for the new worker
            textBox1.AppendLine("Welcome to WhatsApp Campaign Tool!");
            textBox1.AppendLine("1. Click 'Start Driver' to launch browser.");
            textBox1.AppendLine("2. Click 'Login' and scan QR code.");
            textBox1.AppendLine("3. 'Upload Contact File' (CSV or TXT).");
            textBox1.AppendLine("4. Type message or select media, then click 'Send'.");
        }

        private void InitializeSendingWorker()
        {
            // backgroundWorker1 is for Login and is configured in the designer.

            // Configure the new BackgroundWorker for sending messages
            backgroundWorkerSending.WorkerReportsProgress = true; // Enable progress reporting
            backgroundWorkerSending.WorkerSupportsCancellation = true; // Allow cancellation
            backgroundWorkerSending.DoWork += SendCampaignWorker_DoWork;
            backgroundWorkerSending.ProgressChanged += SendCampaignWorker_ProgressChanged;
            backgroundWorkerSending.RunWorkerCompleted += SendCampaignWorker_RunWorkerCompleted;
        }

        // --- Messenger Event Handlers ---
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
            textBox1.Invoke(() => textBox1.AppendLine("[QR] Please scan the QR code using your WhatsApp mobile app to continue login."));
        }

        private void Messegner_OnDisposed()
        {
            this.Invoke((MethodInvoker)delegate {
                textBox1.AppendLine("[STATUS] Messenger has been disposed. Please restart the driver.");
                // Re-enable start driver button and disable others
                button6.Enabled = true;
                checkBox1.Enabled = true;
                button5.Enabled = false; // Login button
                button1.Enabled = false; // Send Text
                button2.Enabled = false; // Send Image
                button3.Enabled = false; // Logout
                button4.Enabled = false; // Send Attachment
                button7.Enabled = false; // Upload Contacts
            });
        }


        // --- Button Click Event Handlers ---

        // "Send Text" button click
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textmsg.Text))
            {
                textBox1.AppendLine("⚠️ Please enter a message to send.");
                return;
            }

            if (contacts == null || contacts.Count == 0)
            {
                textBox1.AppendLine("⚠️ Please upload the contact list before sending messages.");
                return;
            }

            // Set the message for all contacts in the current campaign
            foreach (var contact in contacts)
            {
                contact.Message = textmsg.Text;
                contact.MediaType = null; // Ensure no media type is set for text messages
                contact.FilePath = string.Empty;
                contact.Caption = string.Empty;
            }

            StartSendingCampaign();
        }

        // "Send Image" button click
        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Select Image File";
                openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF";
                openFileDialog1.Multiselect = false;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (contacts == null || contacts.Count == 0)
                    {
                        textBox1.AppendLine("⚠️ Please upload the contact list before sending media.");
                        return;
                    }

                    // Set media details for all contacts in the current campaign
                    foreach (var contact in contacts)
                    {
                        contact.MediaType = MediaType.IMAGE_OR_VIDEO;
                        contact.FilePath = openFileDialog1.FileName;
                        contact.Caption = textmsg.Text; // Use textmsg.Text as caption
                        contact.Message = string.Empty;
                    }
                    StartSendingCampaign();
                }
            }
        }

        // "Send Attachment" button click
        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Select Attachment File";
                openFileDialog1.Multiselect = false;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (contacts == null || contacts.Count == 0)
                    {
                        textBox1.AppendLine("⚠️ Please upload the contact list before sending attachment.");
                        return;
                    }
                    // Set media details for all contacts
                    foreach (var contact in contacts)
                    {
                        contact.MediaType = MediaType.ATTACHMENT;
                        contact.FilePath = openFileDialog1.FileName;
                        contact.Caption = textmsg.Text; // Use textmsg.Text as caption
                        contact.Message = string.Empty;
                    }
                    StartSendingCampaign();
                }
            }
        }

        // "Logout" button click
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.AppendLine("[STATUS] Logging out...");
                Messegner?.Logout(); // This will dispose the driver and trigger Messegner_OnDisposed
            }
            catch (Exception ex)
            {
                textBox1.AppendLine($"[ERROR] Logout failed: {ex.Message}");
            }
        }

        // "Start Driver" button click
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

                textBox1.AppendLine("[INIT] Driver started successfully. Ready for login.");
                button5.Enabled = true;
            }
            catch (Exception ex)
            {
                textBox1.AppendLine($"[ERROR] Failed to start driver: {ex.Message}");
                button6.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        // "Login" button click
        private void button5_Click(object sender, EventArgs e)
        {
            if (Messegner == null)
            {
                textBox1.AppendLine("⚠️ Please start the driver first.");
                return;
            }
            button5.Enabled = false;
            textBox1.AppendLine("[STATUS] Attempting to log in... Please wait for QR Code.");
            backgroundWorker1.RunWorkerAsync();
        }

        // "Upload Contact File" button click
        private void button7_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Contact File (TXT or CSV)";
                ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    contacts.Clear();
                    textBox1.AppendLine($"[FILE] Loading contacts from: {ofd.FileName}");
                    try
                    {
                        var lines = File.ReadAllLines(ofd.FileName);
                        int lineNumber = 0;
                        foreach (var line in lines)
                        {
                            lineNumber++;
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // CSV Format: Number,Message(optional),MediaType(optional),FilePath(optional),Caption(optional)
                            var parts = line.Split(',');
                            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                            {
                                var contact = new Contact(parts[0].Trim());

                                if (parts.Length > 1) contact.Message = parts[1].Trim();
                                if (parts.Length > 2 && Enum.TryParse(parts[2].Trim(), true, out MediaType mediaType)) contact.MediaType = mediaType;
                                if (parts.Length > 3) contact.FilePath = parts[3].Trim();
                                if (parts.Length > 4) contact.Caption = parts[4].Trim();

                                contacts.Add(contact);
                            }
                            else
                            {
                                textBox1.AppendLine($"[WARNING] Skipping invalid line {lineNumber}: '{line}'.");
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


        // --- Background Worker Logic for Login ---
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Messegner?.Login();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                textBox1.AppendLine($"[ERROR] Login failed: {e.Error.Message}");
                button5.Enabled = true; // Re-enable login on failure
                Messegner?.Dispose();
            }
            else
            {
                textBox1.AppendLine("[STATUS] Login successful!");
                pictureBox1.Image = null; // Clear QR code
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button7.Enabled = true;
            }
        }

        // --- Background Worker Logic for Sending Campaign ---
        private void StartSendingCampaign()
        {
            if (Messegner == null || Messegner.IsDisposed)
            {
                textBox1.AppendLine("⚠️ Messenger is not initialized or logged out. Please start and log in first.");
                return;
            }
            if (backgroundWorkerSending.IsBusy)
            {
                textBox1.AppendLine("⚠️ A sending campaign is already in progress.");
                return;
            }

            textBox1.AppendLine($"[CAMPAIGN] Starting campaign for {contacts.Count} contacts...");
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            button3.Enabled = false; // Disable logout during send
            button7.Enabled = false; // Disable contact upload during send

            backgroundWorkerSending.RunWorkerAsync(contacts);
        }

        private void SendCampaignWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Contact> contactsToSend = e.Argument as List<Contact>;
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
                        throw new ArgumentException("No message or media specified.");
                    }

                    successfulSends++;
                    backgroundWorkerSending.ReportProgress(e.ProgressPercentage, $"[SUCCESS] Sent to {contact.ContactNumber}.");
                }
                catch (Exception ex)
                {
                    failedSends++;
                    backgroundWorkerSending.ReportProgress(e.ProgressPercentage, $"[FAILURE] Failed for {contact.ContactNumber}: {ex.Message.Split('\n').FirstOrDefault()}");
                }

                // Use the randomized wait from the Messegner class
                Messegner?.Wait(8, 20); // Random wait between 8 and 20 seconds
            }

            e.Result = new Tuple<int, int>(successfulSends, failedSends);
        }

        private void SendCampaignWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
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
            else
            {
                var results = e.Result as Tuple<int, int>;
                textBox1.AppendLine("\n------------------------------------------");
                textBox1.AppendLine("[CAMPAIGN COMPLETE] Bulk messaging finished.");
                if (results != null)
                {
                    textBox1.AppendLine($"Successful sends: {results.Item1}");
                    textBox1.AppendLine($"Failed sends: {results.Item2}");
                }
                textBox1.AppendLine("------------------------------------------\n");
            }

            // Re-enable buttons if the driver is still active
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


        // --- Utility Methods ---
        public void KillChromiumProcesses()
        {
            textBox1.AppendLine("[CLEANUP] Terminating residual Chrome/Chromium processes...");
            try
            {
                var processNames = new[] { "chromedriver", "chrome", "chromium" };
                foreach (var name in processNames)
                {
                    foreach (var process in Process.GetProcessesByName(name))
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Wait up to 5 seconds
                        }
                        catch (Exception ex) 
                        {
                             // Ignore errors if process is already gone
                             Debug.WriteLine($"Could not kill {name}: {ex.Message}");
                        }
                    }
                }
                textBox1.AppendLine("✅ Cleanup complete.");
            }
            catch (Exception ex)
            {
                textBox1.AppendLine("⚠️ Failed to kill browser processes: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorkerSending.IsBusy)
            {
                backgroundWorkerSending.CancelAsync();
            }
            Messegner?.Dispose();
            KillChromiumProcesses(); // Final cleanup
        }
    }
}