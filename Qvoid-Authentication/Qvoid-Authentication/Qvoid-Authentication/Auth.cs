using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Qvoid.Authentication
{
    public abstract class FireClient
    {
        public string AuthSecret { get; private set; }
        public string BaseAddress { get; private set; }
        public bool Connected { get; private set; }

        public FireClient(string baseAddress = "", string Token = "")
        {
            if (String.IsNullOrEmpty(baseAddress))
                throw new Exception("The base address was null.");

            //Can be done with WebClient but I prefer HttpWebRequet.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseAddress}?{(String.IsNullOrEmpty(this.AuthSecret) ? "" : $"auth={this.AuthSecret}")}");
            request.Method = "GET";
            try
            {
                var response = ((HttpWebResponse)request.GetResponse());
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                Connected = true;
            }
#if DEBUG
            catch (WebException ex)
            {
                Connected = false;
                throw new Exception(((HttpWebResponse)ex.Response).StatusDescription);
            }
#else
            catch (WebException)
            {
                Connected = false;
            }
#endif

            this.BaseAddress = baseAddress;
            this.AuthSecret = Token;
        }

        public T GetData<T>(string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{this.BaseAddress}{path}.json?{(String.IsNullOrEmpty(this.AuthSecret) ? "" : $"auth={this.AuthSecret}")}");
            string responseStr = "";
            try
            {
                responseStr = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()).ReadToEnd();
            }
            catch { }

            return JsonConvert.DeserializeObject<T>(responseStr);
        }

        public bool SetData<T>(string path, T data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{this.BaseAddress}{path}.json?{(String.IsNullOrEmpty(this.AuthSecret) ? "" : $"auth={this.AuthSecret}")}");
            request.Method = "PUT";

            byte[] requestBytes = new ASCIIEncoding().GetBytes(JsonConvert.SerializeObject(data));
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();

                using (var response = ((HttpWebResponse)request.GetResponse()))
                    return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public bool DeleteData(string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{this.BaseAddress}{path}.json?{(String.IsNullOrEmpty(this.AuthSecret) ? "" : $"auth={this.AuthSecret}")}");
            request.Method = "DELETE";

            return ((HttpWebResponse)request.GetResponse()).StatusCode == HttpStatusCode.OK;
        }
    }

    public class Database
    {
        public class UserData
        {
            public bool Admin;
            public string Username;
            public string Password;
            public string DesktopName;
            public string HWID;
            public string LastIpAddress;
            public DateTime RegistrationDate;
            public DateTime LastLogin;
            public DateTime LicenseTime;
        }

        public class LicenseKey
        {
            public string Claimer;
            public DateTime LicenseTime;
        }
    }

    public class AuthSystem : FireClient
    {
        /// <summary>
        /// The webhooks structure
        /// </summary>
        internal static class Webhooks
        {
            static public Discord.Webhook Login        = null;
            static public Discord.Webhook Register     = null;
            static public Discord.Webhook Unauthorized = null;
            static public Discord.Webhook License      = null;
        }

        public Database.UserData LoggedUser { get; internal set; }

        /// <summary>
        /// Connecting into the database and checks for: new update, killswitch and the webhooks.
        /// </summary>
        /// <param name="Credentials"></param>
        public AuthSystem(Credentials Credentials) : base(Credentials.BaseAddress, Credentials.Token)
        {
            //Checking if the client has connected.
            if (!Connected)
                throw new Exception("It's seems like you haven't connected into the database.");

            //Taking the settings from the database.
            var Settings = GetData<JObject>("Settings");

            //Checking if there is an update.
            if (new Version($"{Settings["Version"]}").CompareTo(Credentials.Version) > 0)
            {
                //Update is required.
                var result = MessageBox.Show($"This distribution of the software is outdated; Would you like to install the new version?", "Qvoid Authentication", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    string tempPath = Path.GetTempPath() + "\\Updater.exe";
                    File.WriteAllBytes(tempPath, Internals.Updater);
                    Process.Start(tempPath, $"{Settings["Download"]} \"{Application.StartupPath.Replace("\\", "/")}\"");
                }

                Environment.Exit(0);
            }

            //Checking if the KillSwitch is active.
            if (bool.Parse(Settings["KillSwitch"].ToString()))
                Environment.Exit(1);

            //Updating the webhooks.
            this.UpdateWebhooks($"{Settings["Webhooks"]["Login"]}", $"{Settings["Webhooks"]["Register"]}", $"{Settings["Webhooks"]["Unautorized"]}", $"{Settings["Webhooks"]["License"]}");
        }

        /// <summary>
        /// Updating all the webhooks (used to synchronize the webhooks with the database)
        /// </summary>
        /// <param name="Login"></param>
        /// <param name="Register"></param>
        /// <param name="Warns"></param>
        /// <param name="License"></param>
        private void UpdateWebhooks(string Login, string Register, string Warns, string License)
        {
            Webhooks.Login = new Discord.Webhook(Login);
            Webhooks.Register = new Discord.Webhook(Register);
            Webhooks.Unauthorized = new Discord.Webhook(Warns);
            Webhooks.License = new Discord.Webhook(License);
            Webhooks.Login = new Discord.Webhook(Login);
        }

        /// <summary>
        /// Creating the database structure for the first time and registering a user which is random
        /// </summary>
        public void InitializeDatabase()
        {
            DateTime regTime = DateTime.UtcNow;
            string Username = "System";
            string Password = "System";

            DateTime licenseTime = DateTime.MinValue;
            licenseTime = licenseTime.AddDays(0).AddMonths(0).AddYears(3000);

            //Creating license for our system user.
            Database.LicenseKey license = new Database.LicenseKey() { Claimer = "System", LicenseTime = licenseTime };
            if (!SetData<Database.LicenseKey>($"License Keys/databaseInitKey", license))
            {
                MessageBox.Show("Internal database error.", "Databse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Formatting the random HWID to the regular format we use.
            string HWID = Encryption.MD5(Encryption.ComputeSha256Hash(Encryption.GenerateKey()));
            Database.UserData User = new Database.UserData
            {
                Admin = true,
                Username = Username,
                Password = Encryption.ComputeSha256Hash(Password),
                DesktopName = "Administrator",
                HWID = HWID,
                LastLogin = regTime,
                LastIpAddress = "137.0.0.0",
                RegistrationDate = regTime,
                LicenseTime = regTime.AddTicks(license.LicenseTime.Ticks)
            };

            SetData("DisabledUsers/", $"{User.Username}, ");

            SetData("Settings/Webhooks/License", "");
            SetData("Settings/Webhooks/Login", "");
            SetData("Settings/Webhooks/Register", "");
            SetData("Settings/Webhooks/Unautorized", "");

            SetData("Settings/Download", "");
            SetData("Settings/KillSwitch", false);
            SetData("Settings/Version", "1.0.0");

            SetData("License Keys/databaseInitKey/Claimer", User.Username);
            SetData("User Data/", User);
            SetData($"Registered HWIDs/{HWID}", User.Username);
        }

        /// <summary>
        /// Formmating the database.
        /// </summary>
        public void FormatDatabase() => DeleteData("");

        #region Blacklist
        /// <summary>
        /// Checks if the given HWID is blacklisted in the database.
        /// </summary>
        /// <param name="HWID"></param>
        /// <returns>User blacklist state</returns>
        public bool IsBlacklisted(string HWID)
        {
            string[] hwids = GetData<string>("Blacklist").Split(',');
            foreach (string hwid in hwids)
                if (hwid == HWID)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds HWID to the blacklist.
        /// </summary>
        /// <param name="HWID"></param>
        public void AddBlacklist(string HWID)
        {
            if (IsBlacklisted(HWID))
                return;

            string hwids = GetData<string>("Blacklist");
            if (String.IsNullOrEmpty(hwids))
                SetData("Blacklist", HWID);
            else
                SetData("Blacklist", hwids + "," + HWID);
        }

        /// <summary>
        /// Removes HWID from the blacklist.
        /// </summary>
        /// <param name="HWID"></param>
        public void RemoveBlacklist(string HWID)
        {
            if (!IsBlacklisted(HWID))
                return;

            string hwids = GetData<string>("Blacklist");
            if (hwids.Contains("," + HWID))
                hwids = hwids.Replace("," + HWID, "");
            else if (hwids.Contains(HWID + ","))
                hwids = hwids.Replace(HWID + ",", "");
            else
                hwids = hwids.Replace(HWID, "");

            SetData<string>("Blacklist", hwids);
        }
        #endregion

        #region Userstate
        public bool IsUserDisabled(string username)
        {
            string[] users = GetData<string>("DisabledUsers").Split(',');
            foreach (string user in users)
                if (user == username)
                    return true;

            return false;
        }

        public void ActivateUser(string username)
        {
            if (!IsUserDisabled(username))
                return;

            string users = GetData<string>("DisabledUsers");
            if (users.Contains("," + username))
                users = users.Replace("," + username, "");

            else if (users.Contains(username + ","))
                users = users.Replace(username + ",", "");

            else
                users = users.Replace(username, "");

            SetData<string>("DisabledUsers", users);
        }

        public void DisableUser(string username)
        {
            if (IsUserDisabled(username))
                return;

            string users = GetData<string>("DisabledUsers");
            if (String.IsNullOrEmpty(users))
                SetData<string>("DisabledUsers", username);
            else
                SetData<string>("DisabledUsers", users + "," + username);
        }
        #endregion

        #region General
        public Database.LicenseKey CreateLicense(string Code, int Days, int Months, int Years)
        {
            Database.LicenseKey fetchedLicense = GetData<Database.LicenseKey>($"License Keys/{Code}");
            if (fetchedLicense != null)
            {
                MessageBox.Show("Code already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            DateTime licenseTime = DateTime.MinValue;
            licenseTime = licenseTime.AddDays(Days).AddMonths(Months).AddYears(Years);

            Database.LicenseKey license = new Database.LicenseKey() { Claimer = "", LicenseTime = licenseTime };
            if (!SetData<Database.LicenseKey>($"License Keys/{Code}", license))
            {
                MessageBox.Show("Internal database error.", "Databse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            Discord.Embed embed = new Discord.Embed();
            embed.Title = "__New License Created__";
            embed.Description = Strings.MakeLicenseString(LoggedUser.Username, Code);

            Webhooks.License.Send(embed);
            return license;
        }

        public Database.UserData Login(string username, string password)
        {
            Database.UserData userData = GetData<Database.UserData>($"User Data/{username}");
            if (userData == null || userData.Password != Encryption.ComputeSha256Hash(password))
            {
                MessageBox.Show("Username or password is incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (userData.HWID != Security.GetMachineIdentifier())
            {
                Discord.Embed embed = new Discord.Embed();
                embed.Content = "@everyone";
                embed.Title = "__Unauthorized Access__";
                embed.Description = Strings.MakeWarnString(userData.Username);

                Webhooks.Unauthorized.Send(embed);

                MessageBox.Show("Username or password is incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }

            DateTime time = DateTime.UtcNow;
            if (DateTime.Compare(userData.LicenseTime, time) < 0)
            {
                MessageBox.Show("We are sorry but it seems like your license has been expired ):\r\nContact the owners to renew it.", "License expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            if (IsUserDisabled(username))
            {
                MessageBox.Show("Sorry.\r\nYour account has been disabled.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            if (IsBlacklisted(Security.GetMachineIdentifier()))
            {
                Environment.Exit(0);
                return null;
            }

            userData.LastIpAddress = Security.GetPublicIpAddress();
            LoggedUser = userData;
            Discord.Embed embed2 = new Discord.Embed();
            embed2.Title = "__New Login Detected__";
            embed2.Description = Strings.MakeLoginString(userData);

            if (!userData.Admin)
                Webhooks.Login.Send(embed2);

            SetData<string>($"User Data/{userData.Username}/LastIpAddress", userData.LastIpAddress);
            SetData<DateTime>($"User Data/{userData.Username}/LastLogin", time);

            return userData;
        }

        public Database.UserData Register(string username, string password, string licenseKey, bool Admin)
        {
            // hwid check

            string fetchedHwid = GetData<string>($"Registered HWIDs/{Security.GetMachineIdentifier()}");
            if (fetchedHwid != null)
            {
                MessageBox.Show("You already have a registred account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // key check
            Database.LicenseKey fetchedKey = GetData<Database.LicenseKey>($"License Keys/{licenseKey}");
            if (fetchedKey == null || !String.IsNullOrEmpty(fetchedKey.Claimer))
            {
                MessageBox.Show("Invalid License Key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // user check
            Database.UserData fetchedUserData = GetData<Database.UserData>($"User Data/{username}");
            if (fetchedUserData != null)
            {
                MessageBox.Show("Username already taken.", "Username taken", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // write to database
            else
            {
                DateTime regTime = DateTime.UtcNow;
                Database.UserData User = new Database.UserData
                {
                    Admin = Admin,
                    Username = username,
                    Password = Encryption.StrXOR(password, Encryption.ROT(username, 13), true),
                    DesktopName = Environment.UserDomainName,
                    HWID = Security.GetMachineIdentifier(),
                    LastLogin = regTime,
                    LastIpAddress = Security.GetPublicIpAddress(),
                    RegistrationDate = regTime,
                    LicenseTime = regTime.AddTicks(fetchedKey.LicenseTime.Ticks)
                };

                if (!SetData<string>($"Registered HWIDs/{User.HWID}", username) || !SetData<Database.UserData>($"User Data/{User.Username}", User))
                {
                    MessageBox.Show("Internal error please try again.", "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                fetchedKey.Claimer = username;
                SetData<Database.LicenseKey>($"License Keys/{licenseKey}", fetchedKey);

                User.Password = password;
                Discord.Embed embed = new Discord.Embed();
                embed.Content = "@everyone";
                embed.Title = "‎New Registeration Detected";
                embed.Description = Strings.MakeRegisterString(User);

                Webhooks.Register.Send(embed);

                MessageBox.Show("We successfully registered your new account!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.None);
                return User;
            }
        }
        #endregion
    }
}
