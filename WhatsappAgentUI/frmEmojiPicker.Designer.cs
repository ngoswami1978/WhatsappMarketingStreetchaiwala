namespace WhatsappAgentUI
{
    // This partial class defines the LOOK of our popup form.
    partial class frmEmojiPicker
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.tabControlEmojis = new System.Windows.Forms.TabControl();
            this.SuspendLayout();

            this.tabControlEmojis.Name = "tabControlEmojis";
            this.tabControlEmojis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlEmojis.Location = new System.Drawing.Point(0, 0);
            this.tabControlEmojis.SelectedIndex = 0;

            // --- Create Categories and Panels ---
            var tabPageSmileys = new System.Windows.Forms.TabPage();
            this.flpSmileys = new System.Windows.Forms.FlowLayoutPanel();
            tabPageSmileys.Controls.Add(this.flpSmileys);
            tabPageSmileys.Text = "Smileys";
            this.flpSmileys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSmileys.AutoScroll = true;
            this.flpSmileys.Name = "flpSmileys";

            var tabPageAnimals = new System.Windows.Forms.TabPage();
            this.flpAnimals = new System.Windows.Forms.FlowLayoutPanel();
            tabPageAnimals.Controls.Add(this.flpAnimals);
            tabPageAnimals.Text = "Animals";
            this.flpAnimals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAnimals.AutoScroll = true;
            this.flpAnimals.Name = "flpAnimals";

            var tabPageFood = new System.Windows.Forms.TabPage();
            this.flpFood = new System.Windows.Forms.FlowLayoutPanel();
            tabPageFood.Controls.Add(this.flpFood);
            tabPageFood.Text = "Food";
            this.flpFood.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpFood.AutoScroll = true;
            this.flpFood.Name = "flpFood";
            
            var tabPageObjects = new System.Windows.Forms.TabPage();
            this.flpObjects = new System.Windows.Forms.FlowLayoutPanel();
            tabPageObjects.Controls.Add(this.flpObjects);
            tabPageObjects.Text = "Objects";
            this.flpObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpObjects.AutoScroll = true;
            this.flpObjects.Name = "flpObjects";

            var tabPageSymbols = new System.Windows.Forms.TabPage();
            this.flpSymbols = new System.Windows.Forms.FlowLayoutPanel();
            tabPageSymbols.Controls.Add(this.flpSymbols);
            tabPageSymbols.Text = "Symbols";
            this.flpSymbols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSymbols.AutoScroll = true;
            this.flpSymbols.Name = "flpSymbols";

            this.tabControlEmojis.Controls.Add(tabPageSmileys);
            this.tabControlEmojis.Controls.Add(tabPageAnimals);
            this.tabControlEmojis.Controls.Add(tabPageFood);
            this.tabControlEmojis.Controls.Add(tabPageObjects);
            this.tabControlEmojis.Controls.Add(tabPageSymbols);

            // --- Form Properties ---
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.tabControlEmojis);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmEmojiPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.Text = "Select an Emoji";
            this.ResumeLayout(false);
        }
        #endregion

        // Declaration of all the controls
        private System.Windows.Forms.TabControl tabControlEmojis;
        private System.Windows.Forms.FlowLayoutPanel flpSmileys;
        private System.Windows.Forms.FlowLayoutPanel flpAnimals;
        private System.Windows.Forms.FlowLayoutPanel flpFood;
        private System.Windows.Forms.FlowLayoutPanel flpObjects;
        private System.Windows.Forms.FlowLayoutPanel flpSymbols;
    }
}