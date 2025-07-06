C#
using WhatsappAgent;
using System;
using System.Collections.Generic;
// Define a simple class to hold contact information.
// You can put this in a separate file like 'Contact.cs' for better organization.
public class Contact
{
public string Number { get; set; }
public string Message { get; set; }
}
// Initialize driver and start browser (you can hide or show browser window)
Messegner Messegner = new Messegner(hideWindow: false);
void Messegner_OnQRReady(System.Drawing.Image qrbmp)
{
// This is helpful when 'hideWindow' option is set to true
Console.WriteLine("Login QR code has been received as image, you can save it to disk or show it
somewhere for the user so the user can scan it to continue login.");
}
void Messegner_OnDisposed()
{
Console.WriteLine("Messenger has been disposed, you can't use it anymore.");
}
Messegner.OnDisposed += Messegner_OnDisposed;
Messegner.OnQRReady += Messegner_OnQRReady;
// Open web.whatsapp.com and try to login
Messegner.Login();
// --- SUGGESTION: USE A BULK MESSAGING LOOP ---
// Instead of sending messages one by one, create a list of contacts.
// This makes your code scalable for a marketing campaign.
// You can also load these contacts from a file (e.g., CSV or text file).
var contactsToSend = new List<Contact>
{
// NOTE: Use full international numbers with country codes (e.g., "91" for India).
// The number '70434962' is incomplete and may not work.
// Example: new Contact { Number = "919876543210", Message = "Hello! This is a test message 1." },
new Contact { Number = "70434962", Message = "This is a text message with random delays." },
new Contact { Number = "70434963", Message = "This is another test message." },
new Contact { Number = "70434964", Message = "One more message to send." }
// Add more contacts here...
};
// Create a method to handle bulk sending.
// This method will call your existing SendMessage method for each contact.
// It also includes robust error handling and logging.
void SendBulkMessages(List<Contact> contacts)
{
Console.WriteLine($"[CAMPAIGN] Starting bulk messaging campaign for {contacts.Count}
contacts.");
int successfulSends = 0;
int failedSends = 0;
foreach (var contact in contacts)
{
Console.WriteLine($"[PROCESS] Attempting to send message to {contact.Number}...");
try
{
// Call the enhanced SendMessage method from your Messegner class.
Messegner.SendMessage(contact.Number, contact.Message);
successfulSends++;
Console.WriteLine($"[SUCCESS] Message sent to {contact.Number}. Moving to the next
contact.");
}
catch (Exception ex)
{
// The SendMessage method should already log specific errors,
// but this catch block ensures the loop continues even on unexpected failures.
Console.WriteLine($"[FAILURE] Failed to send message to {contact.Number}. Error:
{ex.Message}");
failedSends++;
}
// --- SUGGESTION: ADD A HUMAN-LIKE, RANDOM DELAY BETWEEN MESSAGES ---
// This is crucial for avoiding detection.
// The enhanced Messegner.Wait() method (as suggested previously) should be used here.
// For example, if you modified your Wait() method to accept a range:
// Messegner.Wait(minSeconds: 10, maxSeconds: 20); // Wait randomly between 10 and 20
seconds.
}
Console.WriteLine("\n------------------------------------------
");
Console.WriteLine("[SUMMARY] Bulk messaging campaign finished.");
Console.WriteLine($"Total contacts processed: {contacts.Count}");
Console.WriteLine($"Successful sends: {successfulSends}");
Console.WriteLine($"Failed sends: {failedSends}");
Console.WriteLine("
------------------------------------------
");
}
// Call the new bulk messaging method.
SendBulkMessages(contactsToSend);
// You can still send media after the loop if needed.
// Messegner.SendMedia(MediaType.IMAGE_OR_VIDEO, "70434962",
"C:\\Users\\96170\\Desktop\\WhatsApp Image 2022-11-28 at 19.20.48.jpg", "this is an image with
caption");
// Messegner.SendMedia(MediaType.ATTACHMENT, "70434962",
"C:\\Users\\96170\\Desktop\\WhatsApp Image 2022-11-28 at 19.20.48.jpg", "");
// Logout from whatsapp and dispose the Messenger object
Messegner.Logout();