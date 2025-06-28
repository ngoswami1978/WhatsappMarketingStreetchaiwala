Here’s a detailed and professional-style **`README.md`** content for your GitHub project named **WhatsAppAgent - Street Chai Wala**, tailored for campaign automation:

---

# 📲 WhatsAppAgent – Street Chai Wala 🍕

An intelligent desktop tool to automate **offer campaigns** for Street Chai Wala using **WhatsApp messaging**. Whether you're sending text offers, images, or media attachments, this tool simplifies customer engagement directly from your system.

---

## 🚀 Project Overview

The **WhatsAppAgent** is a .NET-based Windows application designed to streamline marketing campaigns for small and local businesses through WhatsApp Web. It helps send **bulk offers**, **images**, and **attachments** with ease by reading customer contact lists from a file.

This tool is tailored for **Street Chai Wala** but can be reused and extended for other businesses with similar needs.

---

## 🔧 Features

* ✅ One-click startup and driver initialization
* 📤 Upload contact file (.txt format) for bulk messaging
* 💬 Send plain **text message campaigns**
* 🖼️ Send **image or media-based offers**
* 📎 Send **attachments** to customer WhatsApp numbers
* 🔁 Live logging of message delivery status
* 🛑 Robust error handling during driver or message failure

---

## 🧩 Getting Started

Follow these steps to run your WhatsAppAgent successfully:

### 1. Start the Driver

Click the **“Start Driver”** button.
This will launch and initialize the browser driver necessary to interact with WhatsApp Web.

### 2. Login to WhatsApp

Click the **“Login”** button.
You will be prompted to scan the QR code in the browser window. Ensure your phone is connected to the internet.

> The login becomes active once your system connects with the initialized driver.

### 3. Upload Contact List

Click **“Add Contact File”** and select your `.txt` file.
Each line of this text file must contain a valid customer WhatsApp number (without country code formatting symbols like `+` or `-`).

Example `contacts.txt`:

```
919876543210
918888777666
919595959595
```

### 4. Send Campaign Message

In the **message text box**, type your offer (e.g., "Buy 1 Get 1 Free Pizza today!")
Then click **“Send Message Offer”** to begin sending text messages to the uploaded contact list.

### 5. Send Image / Media Campaign

To send media offers:

* Click **“Send Offer As Image”** and select an image (`.jpg`, `.png`, etc.).
* You may also click **“Send Attachment”** to send documents or other media.

---

## 📁 Project Structure

```bash
WhatsAppAgent-StreetChaiWala/
│
├── /bin/
├── /obj/
├── MainForm.cs           # Main application window and UI logic
├── Messenger.cs          # WhatsApp interaction logic
├── Program.cs            # Entry point
├── contacts.txt          # Sample contact file
├── README.md             # Project documentation
└── resources/            # Images or assets used in campaign
```

---

## 📝 Requirements

* .NET Framework 4.6 or higher
* Chromium Driver or Selenium WebDriver (included in the driver startup)
* WhatsApp Web (requires active phone connection)

---

## 🛠️ Future Enhancements

* Scheduled messaging support
* Campaign reporting dashboard
* Message templates with variable substitution (e.g., customer name)

---

## 📣 Credits

Built by **Neeraj & Team – Street Chai Wala**
For queries or improvements, please open an issue or contact us.
