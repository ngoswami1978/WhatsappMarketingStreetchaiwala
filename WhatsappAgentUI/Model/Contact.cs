using System;
using WhatsappAgent; // Required to access the MediaType enum

namespace WhatsappAgentUI.Model
{
    /// <summary>
    /// Represents a complete messaging task for a single contact.
    /// It holds the recipient's number and all content (text or media) to be sent.
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Default constructor, useful for data binding and initialization.
        /// </summary>
        public Contact() { }

        /// <summary>
        /// Creates a new contact task with a specified contact number.
        /// </summary>
        /// <param name="number">The recipient's phone number as a string.</param>
        public Contact(string number)
        {
            this.ContactNumber = number;
        }

        // All the properties from your new version go here...
        public string ContactNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public MediaType? MediaType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
    }
}