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
using System.Collections.Generic;

namespace WhatsappAgent
{
    public enum MediaType
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

                if (!chromeDll.Exists && chromeZip.Exists)
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

                foreach (var handle in driver.WindowHandles.ToList())
                {
                    if (handle != null && !handle.Equals(driver.CurrentWindowHandle))
                    {
                        driver.SwitchTo().Window(handle);
                        driver.Close();
                    }
                }
                driver.SwitchTo().Window(driver.WindowHandles.First());
                this.handle = driver.CurrentWindowHandle;

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
                if (!CheckWindowState(false))
                {
                    Console.WriteLine("[LOGIN] Browser window closed or navigated away during login wait.");
                    return true;
                }
                var elms = x.FindElements(By.CssSelector("#side"));
                if (elms.Any() && elms.First().Displayed)
                {
                    Console.WriteLine("[LOGIN] Login successful, WhatsApp sidebar found.");
                    return true;
                }
                if (!foundQR)
                {
                    var qrCanvasElements = x.FindElements(By.CssSelector("canvas"));
                    if (qrCanvasElements.Any() && qrCanvasElements.First().Displayed)
                    {
                        var qrcanvas = qrCanvasElements.First();
                        var qrbmp = GetQRCodeAsImage(qrcanvas);
                        OnQRReady?.Invoke(qrbmp);
                        foundQR = true;
                        Console.WriteLine("[LOGIN] QR code displayed and captured.");
                    }
                    else if (foundQR)
                    {
                         Console.WriteLine("[LOGIN] QR code disappeared, re-scanning might be needed.");
                         foundQR = false;
                    }
                }
                return false;
            });
            CheckWindowState();
        }

        private Image GetQRCodeAsImage(IWebElement ele)
        {
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
            if (IsDisposed) return;
            Console.WriteLine("[CLEANUP] Disposing Messenger and quitting driver...");
            try { driver?.Quit(); } catch (Exception ex) { Console.WriteLine($"[ERROR] Error during driver quit: {ex.Message}"); }
            try { driver?.Dispose(); } catch (Exception ex) { Console.WriteLine($"[ERROR] Error during driver dispose: {ex.Message}"); }
            finally
            {
                IsDisposed = true;
                OnDisposed?.Invoke();
                Console.WriteLine("[CLEANUP] Messenger disposed successfully.");
            }
        }

        public void Wait(int minSeconds, int maxSeconds)
        {
            Random random = new Random();
            int randomDelayInSeconds = random.Next(minSeconds, maxSeconds + 1);
            Console.WriteLine($"[INFO] Waiting for {randomDelayInSeconds} seconds to mimic human behavior...");
            Thread.Sleep(randomDelayInSeconds * 1000);
        }

        public void SendMessageInCurrentChat(string message, bool useClipboard = false, uint ticks_timeout = 10)
        {
            try
            {
                var textbox = driver.FindElement(By.CssSelector("[data-testid=\"compose-input\"]"));
                if (useClipboard)
                {
                    ClipboardService.SetText(message);
                    Console.WriteLine("[INFO] Message copied to clipboard. Pasting...");
                    new Actions(driver).Click(textbox).KeyDown(Keys.Control).SendKeys("v").KeyUp(Keys.Control).Perform();
                }
                else
                {
                    Console.WriteLine("[INFO] Typing message into chat...");
                    var lines = message.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        textbox.SendKeys(lines[i]);
                        if (i < lines.Length - 1)
                        {
                            new Actions(driver).KeyDown(Keys.Shift).SendKeys(Keys.Enter).KeyUp(Keys.Shift).Perform();
                        }
                    }
                }
                textbox.SendKeys(Keys.Enter);
                Console.WriteLine("[INFO] Message typed, pressing Enter to send.");
                TryDismissAlert();
                WaitForLastMessage(ticks_timeout);
                Console.WriteLine("[INFO] Message sent successfully in current chat.");
                Wait(5, 10);
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
                throw;
            }
        }

        // =================================================================================
        // ===== START: UPDATED SendMessage METHOD =======================================
        // =================================================================================
        public void SendMessage(string number, string message, uint load_timeout = 30, uint ticks_timeout = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(number))
                    throw new ArgumentException("Recipient number is required.", nameof(number));
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentException("Message content is required.", nameof(message));

                CheckWindowState();

                var url = $"https://web.whatsapp.com/send?phone={number}&text={HttpUtility.UrlEncode(message)}&type=phone_number&app_absent=0";
                driver.Navigate().GoToUrl(url);
                Console.WriteLine($"[INFO] Navigating to chat with number: {number}");

                // --- NEW: LOGIC TO DETECT SLOW CHAT LOADING ---
                try
                {
                    var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var loadingElement = shortWait.Until(d => d.FindElements(By.XPath("//div[contains(text(), 'Loading chat')]")).FirstOrDefault());
                    if (loadingElement != null && loadingElement.Displayed)
                    {
                        Console.WriteLine("[INFO] Chat is loading slowly. Extending wait time...");
                        Wait(20, 30); // Wait for 20-30 seconds as requested
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    // This is the expected, good outcome. It means "Loading chat" did not appear.
                    Console.WriteLine("[INFO] Chat loaded quickly.");
                }
                // --- END: NEW LOGIC ---

                var textbox = WaitForCSSElemnt("[data-testid=\"compose-input\"]", load_timeout);
                if (textbox == null)
                {
                    throw new ElementNotVisibleException($"Message input box not found for {number}. The number may be invalid or chat did not load.");
                }
                
                // The URL already contains the message, so we just need to press Enter.
                // If you want to type it instead, uncomment the multi-line logic below.
                textbox.SendKeys(Keys.Enter);
                Console.WriteLine("[INFO] Pressing Enter to send pre-filled message.");
                TryDismissAlert();

                WaitForLastMessage(ticks_timeout);
                Console.WriteLine($"[SUCCESS] Message successfully sent to {number}.");
                Wait(1, 3);
            }
            catch (NoSuchWindowException)
            {
                Console.WriteLine($"[ERROR] Browser window closed during SendMessage to {number}.");
                Dispose();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while sending a message to {number}: {ex.Message}");
                throw;
            }
        }
        // =================================================================================
        // ===== END: UPDATED SendMessage METHOD =========================================
        // =================================================================================


        // =================================================================================
        // ===== START: UPDATED SendMedia METHOD =========================================
        // =================================================================================
        public void SendMedia(MediaType mediaType, string number, string path, string caption = null, uint load_timeout = 30, uint ticks_timeout = 20)
        {
            try
            {
                CheckWindowState();
                if (string.IsNullOrEmpty(number))
                    throw new ArgumentException("Recipient number is required.", nameof(number));
                if (!File.Exists(path))
                    throw new FileNotFoundException($"File not found at: {path}");
                var fi = new FileInfo(path);
                if (fi.Length == 0 || fi.Length > 16000000)
                    throw new ArgumentException($"File size ({fi.Length} bytes) out of allowed bounds [1 Byte, 16 MB].");

                driver.Url = $"https://web.whatsapp.com/send?phone={number}&text&type=phone_number&app_absent=1";
                Console.WriteLine($"[INFO] Navigating to chat with number: {number} to send media.");

                // --- NEW: LOGIC TO DETECT SLOW CHAT LOADING ---
                try
                {
                    var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var loadingElement = shortWait.Until(d => d.FindElements(By.XPath("//div[contains(text(), 'Loading chat')]")).FirstOrDefault());
                    if (loadingElement != null && loadingElement.Displayed)
                    {
                        Console.WriteLine("[INFO] Chat is loading slowly. Extending wait time...");
                        Wait(20, 30); // Wait for 20-30 seconds as requested
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("[INFO] Chat loaded quickly.");
                }
                // --- END: NEW LOGIC ---

                var messageInputCheck = WaitForCSSElemnt("[data-testid=\"compose-input\"]", load_timeout);
                if (messageInputCheck == null)
                {
                    throw new ElementNotVisibleException($"Message input not found for {number}. Chat may not be ready.");
                }

                var clip = WaitForCSSElemnt("[title='Attach']");
                if (clip == null) throw new ElementNotVisibleException("Attach icon not found.");
                clip.Click();
                Console.WriteLine("[INFO] Clicked attach button.");

                var fileinput = WaitForCSSElemnt($"{(mediaType == MediaType.IMAGE_OR_VIDEO ? "input[accept*='image'], input[accept*='video']" : "input[accept='*'][type='file']"), 5}");
                if (fileinput == null) throw new ElementNotVisibleException($"File input not found for {mediaType}.");
                fileinput.SendKeys(path);
                Console.WriteLine($"[INFO] File '{Path.GetFileName(path)}' selected for upload.");

                var sendButton = WaitForCSSElemnt("[data-testid=\"send\"]", 20);
                if (sendButton == null) throw new ElementNotVisibleException("Send button not found after media selection.");

                if (!string.IsNullOrEmpty(caption))
                {
                    Console.WriteLine("[INFO] Adding caption to media...");
                    var captionInput = WaitForCSSElemnt("[data-testid=\"caption-input\"]", 5);
                    if (captionInput != null)
                    {
                        captionInput.SendKeys(caption);
                        Console.WriteLine("[INFO] Caption added.");
                    }
                    else
                    {
                        Console.WriteLine("[WARNING] Caption input box not found, proceeding without caption.");
                    }
                }

                sendButton.Click();
                Console.WriteLine("[INFO] Media upload initiated.");
                TryDismissAlert();
                WaitForLastMessage(ticks_timeout);
                Console.WriteLine("[SUCCESS] Media sent successfully.");
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
                throw;
            }
        }
        // =================================================================================
        // ===== END: UPDATED SendMedia METHOD ===========================================
        // =================================================================================

        public void Logout()
        {
            try
            {
                Console.WriteLine("[STATUS] Attempting to logout...");
                var menuButton = WaitForCSSElemnt("header [title='Menu'], [data-testid='menu']", 10);
                if (menuButton == null) throw new ElementNotVisibleException("Menu button not found, unable to initiate logout.");
                menuButton.Click();
                Console.WriteLine("[INFO] Clicked menu button.");

                var logoutBtn = WaitForCSSElemnt("[aria-label='Log out']", 5);
                if (logoutBtn == null) throw new ElementNotVisibleException("Logout button not found.");
                logoutBtn.Click();
                Console.WriteLine("[INFO] Clicked logout button.");

                var confirmBtn = WaitForCSSElemnt("[data-testid='popup-controls-ok']", 5);
                 if (confirmBtn == null) throw new ElementNotVisibleException("Logout confirmation button not found.");
                confirmBtn.Click();
                Console.WriteLine("[INFO] Confirmed logout.");
                Wait(5, 10);
                Dispose();
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
                throw;
            }
        }

        private IWebElement WaitForCSSElemnt(string selector, uint timeoutInSeconds = 5)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                var element = wait.Until(driver =>
                {
                    IWebElement el = null;
                    try { el = driver.FindElement(By.CssSelector(selector)); } catch (NoSuchElementException) { }
                    return (el != null && el.Displayed && el.Enabled) ? el : null;
                });
                if (element != null) { Console.WriteLine($"[SUCCESS] Element '{selector}' found."); }
                else { Console.WriteLine($"[TIMEOUT] Element '{selector}' not found or interactive within {timeoutInSeconds}s."); }
                return element;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] Element '{selector}' not found within {timeoutInSeconds}s.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred while waiting for '{selector}': {ex.Message}");
                return null;
            }
        }

        private void WaitForLastMessage(uint seconds)
        {
            Console.WriteLine($"[WAIT] Waiting for last message status for up to {seconds}s...");
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(x => {
                    if (!CheckWindowState(false)) return true;
                    var elms = x.FindElements(By.CssSelector(".message-out"));
                    if (elms.Any())
                    {
                        var labels = elms.Last().FindElements(By.CssSelector("[data-icon='msg-dblcheck'], [data-icon='msg-check']"));
                        if (labels.Any() && labels.First().Displayed)
                        {
                            Console.WriteLine("[SUCCESS] Last message status icon found.");
                            return true;
                        }
                    }
                    return false;
                });
                CheckWindowState();
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"[TIMEOUT] Last message status not confirmed within {seconds}s.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred while waiting for last message status: {ex.Message}");
            }
        }

        private bool CheckWindowState(bool raiseError = true)
        {
            try
            {
                if (driver == null || !driver.WindowHandles.Any() || !driver.WindowHandles.Contains(handle))
                {
                    Console.WriteLine("[STATUS] Browser window handle not found or window was closed.");
                    Dispose();
                    if (raiseError) throw new NoSuchWindowException("Browser window was closed or became invalid.");
                    return false;
                }
                driver.SwitchTo().Window(handle);
                if (!driver.Url.StartsWith(BASE_URL, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"[STATUS] Browser navigated away from base URL: {driver.Url}");
                    Dispose();
                    if (raiseError) throw new NoSuchWindowException($"Browser navigated away from {BASE_URL}. Current URL: {driver.Url}");
                    return false;
                }
                return true;
            }
            catch (WebDriverException ex) when (ex is NoSuchWindowException || ex.Message.Contains("no such window"))
            {
                Console.WriteLine("[STATUS] Browser window already closed or detached during CheckWindowState.");
                Dispose();
                if (raiseError) throw new NoSuchWindowException("Browser window was already closed.", ex);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred during CheckWindowState: {ex.Message}");
                Dispose();
                if (raiseError) throw;
                return false;
            }
        }

        private void TryDismissAlert()
        {
            try
            {
                driver.SwitchTo().Alert().Accept();
                Console.WriteLine("[INFO] Browser alert dismissed.");
            }
            catch (NoAlertPresentException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Could not dismiss alert: {ex.Message}");
            }
        }
    }
}