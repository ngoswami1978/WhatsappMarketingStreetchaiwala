using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using TextCopy;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using System.Threading;
using System.Collections.Generic; // Added for Thread.Sleep in the new Wait method

namespace WhatsappAgent
{
    public enum MediaType // Ensure this enum is accessible
    {
        IMAGE_OR_VIDEO,
        ATTACHMENT
    }

    public class Messegner
    {
        private const string BASE_URL = "https://web.whatsapp.com";
        private readonly string codebase = Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName;
        private readonly string tempPath = Path.GetTempPath();
        private readonly IWebDriver driver;
        private string handle;

        public bool IsDisposed { get; set; } = false;
        public delegate void OnDisposedEventHandler();
        public event OnDisposedEventHandler OnDisposed;

        public delegate void OnQRReadyEventHandler(Image qrbmp);
        public event OnQRReadyEventHandler OnQRReady;

        public Messegner(bool hideWindow = false)
        {
            try
            {
                var options = new ChromeOptions()
                {
                    LeaveBrowserRunning = false,
                    UnhandledPromptBehavior = UnhandledPromptBehavior.Accept,
                };

                var chromeDir = Path.Combine(codebase, "chrome");
                var chromeExe = new FileInfo(Path.Combine(codebase, "chrome\\chrome.exe")); 
                var chromeDll = new FileInfo(Path.Combine(codebase, "chrome\\chrome.dll"));
                var chromeZip = new FileInfo(Path.Combine(codebase, "chrome\\chrome.zip"));

                // Extract Chrome binaries if not already present
                if (!chromeDll.Exists && chromeZip.Exists) // Check if zip exists before attempting extraction
                {
                    try
                    {
                        Console.WriteLine("[INIT] Extracting Chrome binaries...");
                        System.IO.Compression.ZipFile.ExtractToDirectory(chromeZip.FullName, chromeDir);
                        Console.WriteLine("[INIT] Chrome binaries extracted.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to extract Chrome binaries: {ex.Message}");
                        throw new Exception("Failed to extract Chrome binaries. Ensure chrome.zip is valid and accessible.", ex);
                    }
                }
                else if (!chromeDll.Exists)
                {
                    throw new FileNotFoundException("chrome.dll not found and chrome.zip is missing for extraction.");
                }


                options.BinaryLocation = chromeExe.FullName;

                options.AddArgument($"--user-data-dir={tempPath.Replace("\\","\\\\")}\\\\Chrome\\\\UserData");
                if (hideWindow) {
                    options.AddArgument("--headless");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--no-sandbox");
                    options.AddArgument($"user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{FileVersionInfo.GetVersionInfo(chromeExe.FullName).ProductVersion.Split('.')[0]}.0.0.0 Safari/537.36");
                }
                var chromeDriverService = ChromeDriverService.CreateDefaultService(chromeDir);
                chromeDriverService.HideCommandPromptWindow = true;
                driver = new ChromeDriver(chromeDriverService, options, TimeSpan.FromSeconds(100));
                Console.WriteLine("[INIT] ChromeDriver initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] Failed to initialize Messenger: {ex.Message}");
                Dispose();
                throw;
            }
        }

        public void Login(uint login_timeout = 100)
        {
            try
            {
                driver.Url = BASE_URL;
                Console.WriteLine($"[INFO] Navigating to {BASE_URL} for login.");

                // Close any extra tabs that might open
                foreach (var handle in driver.WindowHandles)
                {
                    if (handle != null && !handle.Equals(driver.CurrentWindowHandle))
                    {
                        driver.SwitchTo().Window(handle);
                        driver.Close();
                    }
                }
                this.handle = driver.CurrentWindowHandle; // Store the handle of the main WhatsApp window

                WaitForQRAndLogin(login_timeout);
                Console.WriteLine("[INFO] Login process completed (QR scanned or already logged in).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login failed: {ex.Message}");
                Dispose();
                throw;
            }
        }

        private void WaitForQRAndLogin(uint login_timeout)
        {
            var foundQR = false;
            Console.WriteLine("[LOGIN] Waiting for QR code or successful login...");
            new WebDriverWait(driver, TimeSpan.FromSeconds(login_timeout)).Until(x => {
                // Check if the window is still valid and URL is correct
                if (!CheckWindowState(false))
                {
                    Console.WriteLine("[LOGIN] Browser window closed or navigated away during login wait.");
                    return true; // Stop waiting if window is invalid
                }

                // Check for successful login (sidebar usually indicates logged in state)
                var elms = x.FindElements(By.CssSelector("#side"));
                if (elms.Count() > 0 && elms.First().Displayed)
                {
                    Console.WriteLine("[LOGIN] Login successful, WhatsApp sidebar found.");
                    return true; // Login successful
                }

                // If not logged in and QR code not yet found, try to get QR code
                if (!foundQR)
                {
                    var qrCanvasElements = x.FindElements(By.CssSelector("canvas"));
                    if (qrCanvasElements.Count() > 0 && qrCanvasElements.First().Displayed)
                    {
                        var qrcanvas = qrCanvasElements.First();
                        var qrbmp = GetQRCodeAsImage(qrcanvas);
                        OnQRReady?.Invoke(qrbmp); // Invoke event to provide QR image to UI
                        foundQR = true; // Mark QR as found to avoid re-invoking
                        Console.WriteLine("[LOGIN] QR code displayed and captured.");
                    }
                    else if (foundQR) // If QR was found but now disappeared (e.g., timed out on WhatsApp side)
                    {
                         Console.WriteLine("[LOGIN] QR code disappeared, re-scanning might be needed.");
                         foundQR = false; // Reset to re-attempt QR capture
                    }
                }

                return false; // Continue waiting
            });

            // Final check to ensure state is correct after waiting ends
            CheckWindowState();
        }

        private Image GetQRCodeAsImage(IWebElement ele)
        {
            // Execute JavaScript to get the base64 data URL of the canvas
            var base64Img = driver.ExecuteJavaScript<string>("return arguments[0].toDataURL('image/png').substring(22);", ele);
            
            Image img = null;
            using (var stream = new MemoryStream(Convert.FromBase64String(base64Img)))
            {
                img = Image.FromStream(stream);
            }
            Console.WriteLine("[INFO] Extracted QR code image from canvas.");
            return img;
        }

        public void Dispose()
        {
            if (IsDisposed) return; // Prevent multiple disposals

            Console.WriteLine("[CLEANUP] Disposing Messenger and quitting driver...");
            try
            {
                driver?.Quit(); // Closes all browser windows managed by the driver
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during driver quit: {ex.Message}");
            }
            try
            {
                driver?.Dispose(); // Disposes the ChromeDriver instance
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during driver dispose: {ex.Message}");
            }
            finally
            {
                IsDisposed = true;
                OnDisposed?.Invoke(); // Notify subscribers that Messenger is disposed
                Console.WriteLine("[CLEANUP] Messenger disposed successfully.");
            }
        }

        // --- DEPRECATED/REMOVED: Old fixed Wait method ---
        // private void Wait(uint seconds)
        // {
        //     var timenow = DateTime.Now;
        //     new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(x => DateTime.Now - timenow >= TimeSpan.FromMilliseconds(seconds * 1000));
        // }

        /// <summary>
        /// Waits for a random duration between a specified minimum and maximum number of seconds.
        /// This mimics human-like behavior to avoid bot detection.
        /// </summary>
        /// <param name="minSeconds">The minimum number of seconds to wait.</param>
        /// <param name="maxSeconds">The maximum number of seconds to wait.</param>
        public void Wait(int minSeconds, int maxSeconds)
        {
            Random random = new Random();
            int randomDelayInSeconds = random.Next(minSeconds, maxSeconds + 1); // +1 because Next is exclusive on upper bound
            Console.WriteLine($"[INFO] Waiting for {randomDelayInSeconds} seconds to mimic human behavior...");
            Thread.Sleep(randomDelayInSeconds * 1000); // Thread.Sleep expects milliseconds
        }

        /// <summary>
        /// Sends a text message within the currently active WhatsApp chat.
        /// </summary>
        /// <param name="message">The message text to send.</param>
        /// <param name="useClipboard">If true, uses clipboard (Ctrl+V) for pasting. More robust for long text.</param>
        /// <param name="ticks_timeout">Timeout for waiting for last message status.</param>
        public void SendMessageInCurrentChat(string message, bool useClipboard = false, uint ticks_timeout = 10) // Removed wait_after_send as it's handled externally
        {
            try
            {
                // Using a more stable selector for the message input box
                // [data-testid="compose-input"] is a common alternative. Verify this in browser dev tools.
                var textbox = driver.FindElement(By.CssSelector("[data-testid=\"compose-input\"]"));

                if (useClipboard)
                {
                    ClipboardService.SetText(message);
                    Console.WriteLine("[INFO] Message copied to clipboard. Pasting...");
                    Actions actions = new Actions(driver);
                    actions.Click(textbox)
                       .KeyDown(Keys.Control)
                       .SendKeys("v") // Paste text
                       .KeyUp(Keys.Control)
                       .Perform();
                }
                else
                {
                    Console.WriteLine("[INFO] Typing message into chat...");
                    // Split message by lines and send with Shift+Enter for multiline input
                    var lines = message.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        textbox.SendKeys(lines[i]);
                        if (i < lines.Length - 1) // If not the last line, add Shift+Enter
                        {
                            new Actions(driver).KeyDown(Keys.Shift).SendKeys(Keys.Enter).KeyUp(Keys.Shift).Perform();
                        }
                    }
                }

                // Send the message by pressing Enter
                textbox.SendKeys(Keys.Enter);
                Console.WriteLine("[INFO] Message typed, pressing Enter to send.");
                TryDismissAlert(); // Dismiss any potential alerts/popups

                WaitForLastMessage(ticks_timeout);
                Console.WriteLine("[INFO] Message sent successfully in current chat.");
                Wait(5, 10); // Random delay after sending in current chat (e.g., 5-10 seconds)
            }
            catch (NoSuchWindowException)
            {
                Console.WriteLine("[ERROR] Browser window closed during SendMessageInCurrentChat.");
                Dispose();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send message in current chat: {ex.Message}");
                throw; // Re-throw to inform calling method
            }
        }

        /// <summary>
        /// Navigates to a specific WhatsApp chat and sends a text message.
        /// </summary>
        /// <param name="number">The recipient's phone number (full international format, e.g., "919876543210").</param>
        /// <param name="message">The text message to send.</param>
        /// <param name="load_timeout">Timeout for page elements to load.</param>
        /// <param name="ticks_timeout">Timeout for message status checks.</param>
        public void SendMessage(string number, string message, uint load_timeout = 30, uint ticks_timeout = 10) // Removed wait_after_send as it's handled by bulk loop or internal wait
        {
            try
            {
                if(string.IsNullOrEmpty(number))
                    throw new ArgumentException("Recipient number is required.", nameof(number));

                if (string.IsNullOrEmpty(message))
                    throw new ArgumentException("Message content is required.", nameof(message));

                CheckWindowState(); // Ensure browser window is active

                var url = $"https://web.whatsapp.com/send?phone={number}&text={HttpUtility.UrlEncode(message)}&type=phone_number&app_absent=0";
                driver.Navigate().GoToUrl(url);
                Console.WriteLine($"[INFO] Navigating to chat with number: {number}");

                // Wait for the message input box using a more stable selector
                // [data-testid="compose-input"] is a common alternative. Verify this in browser dev tools.
                //var textbox = WaitForCSSElemnt("[data-testid=\"compose-input\"]", load_timeout); // Replaced original line and duplicate
                var textbox = WaitForCSSElemnt("[aria-placeholder=\"Type a message\"]", load_timeout);


                // --- SUGGESTION: ADD ROBUST NULL CHECK HERE ---
                if (textbox == null)
                {
                    Console.WriteLine($"[ERROR] Could not find message input box for number: {number}. The chat may not have loaded or the number is invalid/unreachable.");
                    throw new ElementNotVisibleException($"Message input box not found for {number}."); // Throw exception to indicate failure
                }
                // --- END OF SUGGESTION ---

                Console.WriteLine("[INFO] Typing message into chat...");
                // Handle multiline messages by sending lines with Shift+Enter
                var lines = message.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    //textbox.SendKeys(lines[i]);
                    if (i < lines.Length - 1) // If not the last line, add Shift+Enter for a new line within the message
                    {
                        new Actions(driver).KeyDown(Keys.Shift).SendKeys(Keys.Enter).KeyUp(Keys.Shift).Perform();
                    }
                }
                
                // Send the message by pressing Enter
                textbox.SendKeys(Keys.Enter);
                Console.WriteLine("[INFO] Message typed, pressing Enter to send.");
                TryDismissAlert(); // Dismiss any potential alerts/popups

                WaitForLastMessage(ticks_timeout);
                Console.WriteLine($"[SUCCESS] Message successfully sent to {number}.");
                // The main application loop (e.g., SendBulkMessages) should handle the primary wait between contacts.
                // A small internal wait might still be useful for immediate post-send stability.
                Wait(1, 3); // Random small delay for post-send stability
            }
            catch (NoSuchWindowException)
            {
                Console.WriteLine($"[ERROR] Browser window closed during SendMessage to {number}.");
                Dispose(); // Dispose if window is gone
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while sending a message to {number}: {ex.Message}");
                throw; // Re-throw to inform calling method
            }
        }

        /// <summary>
        /// Sends an image, video, or attachment to a specific WhatsApp chat.
        /// </summary>
        /// <param name="mediaType">Type of media (IMAGE_OR_VIDEO or ATTACHMENT).</param>
        /// <param name="number">The recipient's phone number.</param>
        /// <param name="path">Full path to the media file.</param>
        /// <param name="caption">Optional caption for the media.</param>
        /// <param name="load_timeout">Timeout for page elements to load.</param>
        /// <param name="ticks_timeout">Timeout for message status checks.</param>
        public void SendMedia(MediaType mediaType, string number, string path, string caption=null, uint load_timeout = 30, uint ticks_timeout = 20)
        {
            try
            {                
                CheckWindowState(); // Ensure browser window is active

                if (string.IsNullOrEmpty(number))
                    throw new ArgumentException("Recipient number is required.", nameof(number));

                if (!File.Exists(path))
                    throw new FileNotFoundException($"File not found at: {path}");

                var fi = new FileInfo(path);
                if (fi.Length == 0 || fi.Length > 16000000) // WhatsApp limit ~16MB
                    throw new ArgumentException($"File size ({fi.Length} bytes) out of allowed bounds [1 Byte, 16 MB].");

                driver.Url = $"https://web.whatsapp.com/send?phone={number}&text&type=phone_number&app_absent=1";
                Console.WriteLine($"[INFO] Navigating to chat with number: {number} to send media.");

                // Wait for the message input box (as an indicator of chat readiness)
                // Use a more stable selector
                var messageInputCheck = WaitForCSSElemnt("[aria-placeholder=\"Type a message\"]", load_timeout); ;
                if (messageInputCheck == null)
                {
                    Console.WriteLine($"[ERROR] Message input element not found for {number}. Cannot proceed with media sending.");
                    throw new ElementNotVisibleException($"Message input not found for {number}. Chat may not be ready.");
                }

                // Locate and click the attach icon
                // [data-testid="clip"] is a common alternative for attach icon. Verify this.
                var clip = WaitForCSSElemnt("[title='Attach']"); 
                if (clip == null)
                {
                    Console.WriteLine("[ERROR] Attach icon not found.");
                    throw new ElementNotVisibleException("Attach icon not found.");
                }
                clip.Click();
                Console.WriteLine("[INFO] Clicked attach button.");

                // Locate the file input element based on media type
                // input[accept*='image'] for images/videos, input[accept='*'] for any file (document)
                //var fileinput = WaitForCSSElemntImg($"{(mediaType == MediaType.IMAGE_OR_VIDEO ? "input[accept*='image'], input[accept*='video']" : "input[accept='*'][type='file']"), 5}"); // Added type='file' for robustness, and explicit 5s timeout

                var fileinput = WaitForCSSElemntImg($"{(mediaType == MediaType.IMAGE_OR_VIDEO ? "input[accept*='image']" : "input[accept='*']")}");
                if (fileinput == null)
                {
                    Console.WriteLine($"[ERROR] File input element for {mediaType} not found.");
                    throw new ElementNotVisibleException($"File input not found for {mediaType}.");
                }
                fileinput.SendKeys(path); // Upload the file
                Console.WriteLine($"[INFO] File '{Path.GetFileName(path)}' selected for upload.");

                // Wait for the send button to appear and become clickable after file selection
                // [data-testid="send"] is a common alternative for send button after file selection
                var sendButton = WaitForCSSElemnt("div[aria-label='Send'][role='button']");  // Give more time for large files to process
                if (sendButton == null)
                {
                    Console.WriteLine("[ERROR] Send button after media selection not found.");
                    throw new ElementNotVisibleException("Send button not found after media selection.");
                }
                
                Wait(3, 7); // Random delay after clicking send button for media (e.g., 3-7 seconds)

                // Add caption if provided
                if (!string.IsNullOrEmpty(caption))
                {
                    Console.WriteLine("[INFO] Adding caption to media...");
                    // The caption input box often appears after file selection. Its selector might be different.
                    // Common selectors: [aria-label="Add a caption"], [data-testid="caption-input"]
                    var captionInput = WaitForCSSElemnt("[aria-label=\"Add a caption\"], [data-testid=\"caption-input\"]", 5);
                    if (captionInput != null)
                    {
                        var captionLines = caption.Split('\n');
                        for (int i = 0; i < captionLines.Length; i++)
                        {
                            captionInput.SendKeys(captionLines[i]);
                            if (i < captionLines.Length - 1)
                            {
                                new Actions(driver).KeyDown(Keys.Shift).SendKeys(Keys.Enter).KeyUp(Keys.Shift).Perform();
                            }
                        }
                        Console.WriteLine("[INFO] Caption added.");
                    }
                    else
                    {
                        Console.WriteLine("[WARNING] Caption input box not found, proceeding without caption.");
                    }
                }
                
                // Click the send button to finalize media sending
                sendButton.Click(); 
                Console.WriteLine("[INFO] Media upload initiated.");
                
                TryDismissAlert(); // Dismiss any potential alerts/popups

                WaitForLastMessage(ticks_timeout);
                Console.WriteLine("[SUCCESS] Media sent successfully.");
                // A small internal wait for post-send stability
                Wait(1, 3);
            }
            catch (NoSuchWindowException)
            {
                Console.WriteLine($"[ERROR] Browser window closed during SendMedia to {number}.");
                Dispose();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while sending media to {number}: {ex.Message}");
                throw; // Re-throw to inform calling method
            }
        }

        public void Logout()
        {
            try
            {
                Console.WriteLine("[STATUS] Attempting to logout...");
                // Locate and click the menu button (usually has title='Menu' or data-testid='menu')
                var menuButton = WaitForCSSElemnt("header [title='Menu'], [data-testid='menu']", 10);
                if (menuButton == null)
                {
                    Console.WriteLine("[ERROR] Menu button not found. Already logged out or element changed?");
                    throw new ElementNotVisibleException("Menu button not found, unable to initiate logout.");
                }
                menuButton.Click();
                Console.WriteLine("[INFO] Clicked menu button.");

                // Locate and click the logout option (usually has aria-label='Log out' or text 'Log out')
                var logoutBtn = WaitForCSSElemnt("[aria-label='Log out'], [role='button'][tabindex='0'] span[data-icon='log-out']", 5);
                if (logoutBtn == null)
                {
                    Console.WriteLine("[ERROR] Logout button not found.");
                    throw new ElementNotVisibleException("Logout button not found.");
                }
                logoutBtn.Click();
                Console.WriteLine("[INFO] Clicked logout button.");

                // Locate and click the confirm button in the dialog (usually the second button in a dialog role)
                var confirmBtn = WaitForCSSElemnt("[role='dialog'] button:nth-child(2), [data-testid='popup-controls'] button:last-child", 5);
                 if (confirmBtn == null)
                {
                    Console.WriteLine("[ERROR] Logout confirmation button not found.");
                    throw new ElementNotVisibleException("Logout confirmation button not found.");
                }
                confirmBtn.Click();
                Console.WriteLine("[INFO] Confirmed logout.");
                Wait(5, 10); // Random delay after logout confirmation (e.g., 5-10 seconds)
                
                Dispose(); // Dispose the driver after successful logout
            }
            catch (NoSuchWindowException)
            {
                Console.WriteLine("[ERROR] Browser window closed during logout process.");
                Dispose();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred during logout: {ex.Message}");
                throw; // Re-throw to inform calling method
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        //private IWebElement WaitForCSSElemntImg(string selector, uint timeout = 3)
        //{
        //    new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(x => !CheckWindowState(false) || x.FindElements(By.CssSelector(selector)).Count > 0);
        //    CheckWindowState();
        //    return driver.FindElement(By.CssSelector(selector));
        //}

        /// <summary>
        /// Waits for a CSS element to be present and displayed.
        /// Includes error handling for timeouts and not found elements.
        /// </summary>
        /// <param name="selector">The CSS selector of the element.</param>
        /// <param name="timeoutInSeconds">Maximum time to wait for the element.</param>
        /// <returns>The found IWebElement, or null if not found or timed out.</returns>
        private IWebElement WaitForCSSElemnt(string selector, uint timeoutInSeconds = 5)
        {
            Console.WriteLine($"[WAIT] Waiting for element with selector '{selector}' for up to {timeoutInSeconds} seconds...");
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                var element = wait.Until(driver =>
                {
                    IWebElement el = null;
                    try
                    {
                        el = driver.FindElement(By.CssSelector(selector));
                    }
                    catch (NoSuchElementException)
                    {
                        // Element not found yet, continue waiting
                    }
                    return (el != null && el.Displayed && el.Enabled) ? el : null; // Ensure element is visible and enabled
                });
                if (element != null)
                {
                    Console.WriteLine($"[SUCCESS] Element with selector '{selector}' found and is interactive.");
                }
                else
                {
                    Console.WriteLine($"[TIMEOUT] Element with selector '{selector}' not found or not interactive within {timeoutInSeconds} seconds.");
                }
                return element;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] Element with selector '{selector}' not found within {timeoutInSeconds} seconds.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred while waiting for selector '{selector}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Waits for a CSS element (specifically for image inputs) to be present.
        /// This method might need further refinement based on specific WhatsApp UI behavior for file dialogs.
        /// </summary>
        /// <param name="selector">The CSS selector.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <returns>The found IWebElement.</returns>
        private IWebElement WaitForCSSElemntImg(string selector, uint timeout = 3)
        {
            Console.WriteLine($"[WAIT_IMG] Waiting for image/file input element with selector '{selector}' for up to {timeout} seconds...");
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(x =>
                    !CheckWindowState(false) || x.FindElements(By.CssSelector(selector)).Count > 0
                );
                CheckWindowState(); // Re-check window state after wait
                var element = driver.FindElement(By.CssSelector(selector));
                Console.WriteLine($"[SUCCESS] Image/file input element '{selector}' found.");
                return element;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] Image/file input element '{selector}' not found within {timeout} seconds.");
                throw; // Re-throw as it's critical for media sending
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"[ERROR] Image/file input element '{selector}' not found immediately.");
                throw; // Re-throw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred while waiting for image/file input '{selector}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Waits for the last sent message to show a "sent" or "delivered" status icon.
        /// </summary>
        /// <param name="seconds">Timeout in seconds.</param>
        private void WaitForLastMessage(uint seconds)
        {
            Console.WriteLine($"[WAIT] Waiting for last message to be sent/delivered for up to {seconds} seconds...");
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(x => {
                    if (!CheckWindowState(false))
                    {
                        Console.WriteLine("[WAIT] Browser window closed during WaitForLastMessage.");
                        return true;
                    }

                    // Look for outgoing messages
                    var elms = x.FindElements(By.CssSelector(".message-out"));
                    if (elms.Count > 0)
                    {
                        // Check the last outgoing message for the sent/delivered icon
                        var lastMessage = elms.Last();
                        // Look for data-icon='msg-dblcheck' (delivered) or data-icon='msg-check' (sent)
                        var labels = lastMessage.FindElements(By.CssSelector("[data-icon='msg-dblcheck'], [data-icon='msg-check']"));
                        if (labels.Count > 0 && labels.First().Displayed)
                        {
                            Console.WriteLine("[SUCCESS] Last message status icon found.");
                            return true; // Icon found, message likely sent/delivered
                        }
                    }
                    return false; // Keep waiting
                });
                CheckWindowState(); // Final check of window state
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] Last message status not confirmed within {seconds} seconds. Message might still be sending or failed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while waiting for last message status: {ex.Message}");
            }
        }

        /// <summary>
        /// Waits for multiple CSS elements to be present.
        /// </summary>
        /// <param name="selector">The CSS selector.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <returns>A ReadOnlyCollection of found IWebElements.</returns>
        private ReadOnlyCollection<IWebElement> WaitForCSSElemnts(string selector, int timeout = 3)
        {
            Console.WriteLine($"[WAIT_MULTIPLE] Waiting for multiple elements with selector '{selector}' for up to {timeout} seconds...");
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(x =>
                    !CheckWindowState(false) || x.FindElements(By.CssSelector(selector)).Count > 0
                );
                CheckWindowState();
                var elements = driver.FindElements(By.CssSelector(selector));
                Console.WriteLine($"[SUCCESS] Found {elements.Count} elements with selector '{selector}'.");
                return elements;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] No elements found with selector '{selector}' within {timeout} seconds.");
                return new ReadOnlyCollection<IWebElement>(new List<IWebElement>()); // Return empty collection on timeout
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while waiting for multiple elements '{selector}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if the browser window is still open and on the correct URL.
        /// Disposes the driver if window is invalid.
        /// </summary>
        /// <param name="raiseError">If true, throws NoSuchWindowException if window is invalid.</param>
        /// <returns>True if window is valid, false otherwise.</returns>
        private bool CheckWindowState(bool raiseError = true)
        {
            try
            {
                // Ensure driver is not null and has window handles
                if (driver == null || !driver.WindowHandles.Any() || !driver.WindowHandles.Contains(handle))
                {
                    Console.WriteLine("[STATUS] Browser window handle not found or window was closed.");
                    Dispose();
                    if (raiseError)
                        throw new NoSuchWindowException("Browser window was closed or became invalid.");
                    return false;
                }
                
                // Switch to the main window handle to ensure correct context
                driver.SwitchTo().Window(handle);

                if (!driver.Url.StartsWith(BASE_URL, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"[STATUS] Browser navigated away from base URL: {driver.Url}");
                    Dispose();
                    if (raiseError)
                        throw new NoSuchWindowException($"Browser navigated away from {BASE_URL}. Current URL: {driver.Url}");
                    return false;
                }
                return true; // Window is valid and on correct URL
            }
            catch (WebDriverException ex) when (ex is NoSuchWindowException || ex.Message.Contains("no such window"))
            {
                Console.WriteLine("[STATUS] Browser window already closed or detached during CheckWindowState.");
                Dispose(); // Ensure cleanup
                if (raiseError)
                    throw new NoSuchWindowException("Browser window was already closed.", ex);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred during CheckWindowState: {ex.Message}");
                Dispose(); // Attempt cleanup on other errors
                if (raiseError)
                    throw; // Re-throw unknown exceptions
                return false;
            }
        }

        /// <summary>
        /// Attempts to dismiss any browser alert pop-ups.
        /// </summary>
        private void TryDismissAlert()
        {
            try
            {
                driver.SwitchTo().Alert().Accept();
                Console.WriteLine("[INFO] Browser alert dismissed.");
            }
            catch (NoAlertPresentException)
            {
                // No alert present, which is expected most of the time
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Could not dismiss alert: {ex.Message}");
            }
        }
    }
}