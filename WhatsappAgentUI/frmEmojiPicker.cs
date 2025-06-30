using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhatsappAgentUI
{
    // This partial class defines the LOGIC for our popup form.
    public partial class frmEmojiPicker : Form
    {
        public string SelectedEmoji { get; private set; }

        public frmEmojiPicker()
        {
            InitializeComponent(); 
            PopulateEmojiPicker();
        }
        
        private void PopulateEmojiPicker()
        {
            var emojiCategories = new Dictionary<string, List<string>>
            {
                ["Smileys"] = new List<string> { "😀", "😁", "😂", "🤣", "😃", "😄", "😅", "😆", "😉", "😊", "😋", "😎", "😍", "😘", "🥰", "😗", "😙", "😚", "🙂", "🤗", "🤩", "🤔", "🤨", "😐", "😑", "😶", "🙄", "😏", "😣", "😥", "😮", "🤐", "😯", "😪", "😫", "🥱", "😴", "🤤", "😒", "🥴", "😵", "🤯", "🤠", "🥳", "🥺", "😭", "😱", "🤪", "😇", "🥳", "👋", "👍", "👎", "👌", "🙏", "❤️" },
                ["Animals"] = new List<string> { "🐶", "🐱", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯", "🦁", "🐮", "🐷", "🐸", "🐵", "🐔", "🐧", "🐦", "🐤", "🦆", "🦅", "🦉", "🦇", "🐺", "🐗", "🐴", "🦄", "🐝", "🐛", "🦋", "🐌", "🐞", "🐜", "🦟", "🦗", "🕷️", "🐢", "🐍", "🦎", "🦖", "🦕", "🐙", "🦑", "🦐", "🦞", "🦀", "🐡", "🐠", "🐟", "🐬", "🐳", "🐋", "🦈" },
                ["Food"] = new List<string> { "🍏", "🍎", "🍐", "🍊", "🍋", "🍌", "🍉", "🍇", "🍓", "🍈", "🍒", "🍑", "🥭", "🍍", "🥥", "🥝", "🍅", "🍆", "🥑", "🥦", "🥬", "🥒", "🌶️", "🌽", "🥕", "🧄", "🧅", "🥔", "🍠", "🥐", "🥯", "🍞", "🥖", "🥨", "🧀", "🥚", "🍳", "🧈", "🥞", "🧇", "🥓", "🥩", "🍗", "🍖", "🦴", "🌭", "🍔", "🍟", "🍕" },
                ["Objects"] = new List<string> { "⌚", "📱", "💻", "⌨️", "🖥️", "🖨️", "🖱️", "💾", "💿", "📀", "📼", "📷", "📸", "📹", "📞", "☎️", "📟", "📠", "📺", "📻", "💡", "🔦", "🔋", "🔌", "💰", "💵", "💴", "💶", "💷", "💳", "💎", "⚖️", "🔧", "🔨", "⚒️", "🛠️", "⛏️", "🔩", "⚙️", "🧱", "⛓️", "🧰", "🧲", "🔫", "💣", "🔪", "🗡️", "🛡️", "🚬", "⚰️", "⚱️", "🏺", "🔮", "📿", "💈", "⚗️", "🔭", "🔬", "💊", "💉", "🩸" },
                ["Symbols"] = new List<string> { "☮️", "✝️", "☪️", "🕉️", "☸️", "✡️", "🔯", "🕎", "☯️", "☦️", "🛐", "⛎", "♈", "♉", "♊", "♋", "♌", "♍", "♎", "♏", "♐", "♑", "♒", "♓", "⚛️", "☢️", "☣️", "️", "❤️‍🔥", "❤️‍🩹", "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "🤎", "💔", "❣️", "💕", "💞", "💓", "💗", "💖", "💘", "💝", "💟", "💌", "💤", "💢", "💥", "💦", "💨", "💫", "💬", "💭", "🌀", "✅", "✔️", "☑️", "🔘", "🔗", "✖️", "❌", "❎", "➕", "➖", "➗", "➰", "➿", "⤴️", "⤵️", "⬅️", "⬆️", "⬇️", "➡️" }
            };

            foreach (var category in emojiCategories)
            {
                var panelName = "flp" + category.Key;
                var panel = this.tabControlEmojis.Controls.Find(panelName, true).FirstOrDefault() as FlowLayoutPanel;

                if (panel != null)
                {
                    foreach (var emoji in category.Value)
                    {
                        var emojiButton = new Button();
                        emojiButton.Text = emoji;
                        emojiButton.Font = new Font("Segoe UI Emoji", 12);
                        emojiButton.Size = new Size(40, 40);
                        emojiButton.Margin = new Padding(2);
                        emojiButton.Cursor = Cursors.Hand;
                        emojiButton.Click += (sender, args) =>
                        {
                            this.SelectedEmoji = emoji;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        };
                        panel.Controls.Add(emojiButton);
                    }
                }
            }
        }
    }
}