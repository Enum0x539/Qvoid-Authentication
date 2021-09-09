using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Qvoid_Authentication
{
    public static class Encryption
    {
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        private static byte[] StringToByteArray(string hex)
        {
            //Haha belongs to the Shabak
            return (from x in Enumerable.Range(0, hex.Length)
                    where x % 2 == 0
                    select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray<byte>();
        }

        public static string StrXOR(string input, byte key, bool encrypt)
        {
            Thread.Sleep(20);

            string output = string.Empty;
            if (encrypt)
            {
                foreach (char c in input)
                    output += (c ^ key).ToString("X2");
            }
            else
            {
                try
                {
                    byte[] strBytes = StringToByteArray(input);
                    foreach (byte b in strBytes)
                        output += (char)(b ^ key);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return output;
        }

        public static string StrXOR(string input, bool encrypt, int Length = 1000)
        {
            Thread.Sleep(20);

            string key = string.Empty;
            string output = string.Empty;
            if (encrypt)
            {
                key = GenerateKey(Length);
                output = key;
                for (int i = 0; i < input.Length; ++i)
                    output += (input[i] ^ key[i % key.Length]).ToString("X2");
            }
            else
            {
                try
                {
                    key = input.Remove(Length);
                    byte[] strBytes = StringToByteArray(input.Substring(Length));
                    for (int i = 0; i < strBytes.Length; ++i)
                        output += (char)(strBytes[i] ^ key[i % key.Length]);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return output;
        }

        public static string StrXOR(string input, string key, bool encrypt)
        {
            Thread.Sleep(20);

            if (key.Length == 0)
                return string.Empty;

            string output = string.Empty;
            if (encrypt)
            {
                for (int i = 0; i < input.Length; ++i)
                    output += (input[i] ^ key[i % key.Length]).ToString("X2");
            }
            else
            {
                try
                {
                    byte[] strBytes = StringToByteArray(input);
                    for (int i = 0; i < strBytes.Length; ++i)
                        output += (char)(strBytes[i] ^ key[i % key.Length]);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return output;
        }

        public static string ROT(string value, int Type = 13)
        {
            return !string.IsNullOrEmpty(value) ? new string(value.Select(x => (x >= 'a' && x <= 'z') ? (char)((x - 'a' + Type) % 26 + 'a') : ((x >= 'A' && x <= 'Z') ? (char)((x - 'A' + Type) % 26 + 'A') : x)).ToArray()) : value;
        }

        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public static string Base64Decode(string base64EncodedData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
            }
        }

        public static string MD5(string Input)
        {
            return GetHexString(new MD5CryptoServiceProvider().ComputeHash(new ASCIIEncoding().GetBytes(Input)));
        }
        public static string SHA256CheckSum(string filePath)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }

        public static string GenerateKey(int size = 1000)
        {
            Random r = new Random();
            string output = "";

            for (int i = 0; i < size; ++i)
            {
                int[] rs = { r.Next('0', '9' + 1), r.Next('a', 'z' + 1), r.Next('A', 'Z' + 1) };
                output += (char)rs[r.Next(3)];
            }

            return output.ToUpper();
        }
    }

    public static class Strings
    {
        public static string MakeWarnString(string username)
        {
            return "**IP Address: **" + Security.GetPublicIpAddress() + "\r\n" +
                   "**Region: **" + RegionInfo.CurrentRegion.ThreeLetterISORegionName + "\r\n" +
                   "**HWID: **" + Security.GetMachineIdentifier() + "\r\n" +
                   "**Computer Name: **" + Environment.UserName + "\r\n" +
                   "**Desktop Name: **" + Environment.UserDomainName + "\r\n" +
                   "**Operating System: **" + Environment.OSVersion + "\r\n" +
                   "**Local Time: **" + Internals.GetNetworkTime().ToString("dd/MM/yyyy HH:mm:ss") + "\r\n" +

                   "\r\nTried accessing " + username + "'s account.\r\n" +
                   "Don't mess with the best (;";
        }

        public static string MakeLicenseString(string username, string license)
        {
            return "**IP Address: **" + Security.GetPublicIpAddress() + "\r\n" +
                   "**Region: **" + RegionInfo.CurrentRegion.ThreeLetterISORegionName + "\r\n" +
                   "**HWID: **" + Security.GetMachineIdentifier() + "\r\n" +
                   "**Computer Name: **" + Environment.UserName + "\r\n" +
                   "**Desktop Name: **" + Environment.UserDomainName + "\r\n" +
                   "**Operating System: **" + Environment.OSVersion + "\r\n" +
                   "**Local Time: **" + Internals.GetNetworkTime().ToString("dd/MM/yyyy HH:mm:ss") + "\r\n" +

                   "\r\n" + username + " created a new license key: " + license + ".";
        }

        public static string MakeDebugString(string debuggerName)
        {
            return "**Debugger: **" + debuggerName + "\r\n" +
                   "**IP Address: **" + Security.GetMachineIdentifier() + "\r\n" +
                   "**Region: **" + RegionInfo.CurrentRegion.ThreeLetterISORegionName + "\r\n" +
                   "**HWID: **" + Security.GetMachineIdentifier() + "\r\n" +
                   "**Computer Name: **" + Environment.UserName + "\r\n" +
                   "**Desktop Name: **" + Environment.UserDomainName + "\r\n" +
                   "**Operating System: **" + Environment.OSVersion + "\r\n" +
                   "**Local Time: **" + Internals.GetNetworkTime().ToString("dd/MM/yyyy HH:mm:ss") + "\r\n";
        }

        public static string MakeLoginString(Database.UserData userData)
        {
            return "**Username: **" + (userData == null ? "Trial" : userData.Username) + "\r\n" +
                   "**IP Address: **" + (userData == null ? Security.GetMachineIdentifier() : userData.LastIpAddress) + "\r\n" +
                   "**Region: **" + RegionInfo.CurrentRegion.ThreeLetterISORegionName + "\r\n" +
                   "**HWID: **" + (userData == null ? Security.GetMachineIdentifier() : userData.HWID) + "\r\n" +
                   "**Computer Name: **" + Environment.UserName + "\r\n" +
                   "**Local Time: **" + (userData == null ? Internals.GetNetworkTime() : userData.LastLogin).ToString("dd/MM/yyyy HH:mm:ss") + "\r\n";
        }

        public static string MakeRegisterString(Database.UserData userData)
        {
            return "**__Software Credentials__**\r\n" +
                   "**Username: **" + userData.Username + "\r\n" +
                   "**Password: **" + userData.Password + "\r\n" +
                   "**Administrator: **" + userData.Admin + "\r\n" +
                   "**HWID: **" + userData.HWID + "\r\n" +
                   "**Computer Name: **" + Environment.UserName + "\r\n" +
                   "**Desktop Name: **" + userData.DesktopName + "\r\n" +
                   "**Operating System: **" + Environment.OSVersion + "\r\n" +
                   "**Local Time: **" + Internals.GetNetworkTime().ToString("dd/MM/yyyy HH:mm:ss") + "\r\n";
        }
    }

    internal class Internals
    {
        public static byte[] Updater = new byte[] { };

        public static DateTime GetNetworkTime()
        {
            const string ntpServer = "time.windows.com";

            byte[] ntpData = new byte[48];

            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(ntpServer).AddressList;

                IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect(ipEndPoint);
                    socket.ReceiveTimeout = 3000;

                    try
                    {
                        socket.Send(ntpData);
                        socket.Receive(ntpData);
                        socket.Close();
                    }
                    catch
                    {
                        socket.Send(ntpData);
                        socket.Receive(ntpData);
                        socket.Close();
                    }
                }

                const byte serverReplyTime = 40;
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                //**UTC** time
                DateTime networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);

                return networkDateTime.ToLocalTime();
            }
            catch
            {
                return new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }

    public class Discord
    {
        public class Embed
        {
            public EmbedAuthor Author;
            public Color Color = Color.FromArgb(204, 0, 204);
            public string Content;
            public string Description;
            public List<EmbedField> Fields = new List<EmbedField>();
            public EmbedFooter Footer;
            public string Image;
            public string Thumbnail;
            public string Title;
            public bool Tts;
            public DateTime? Timestamp { get; internal set; }

            public void AddField(string name, string value, bool inline = false)
            {
                EmbedField field = new EmbedField();
                field.Inline = inline;
                field.Name = name;
                field.Value = value;
                this.Fields.Add(field);
            }

            public Embed Parse(string jsonObject)
            {
                Embed embed = new Embed();
                string[] fields = jsonObject.Split(',');
                foreach (string field in fields)
                {
                    int fieldLength = field.IndexOf('"', 2) - 2;
                    string fieldValue = field.Substring(field.IndexOf(':') + 2).Replace("\"", "");

                    switch (field.Substring(2, fieldLength))
                    {
                        case "title":
                            embed.Title = fieldValue;
                            break;

                        case "description":
                            embed.Description = fieldValue;
                            break;

                        case "color":
                            embed.Color = ColorTranslator.FromHtml(fieldValue);
                            break;

                        case "footer":
                            embed.Footer = new EmbedFooter(fieldValue);
                            break;

                        case "image":
                            embed.Image = fieldValue;
                            break;

                        case "thumbnail":
                            embed.Thumbnail = fieldValue;
                            break;

                        case "author":
                            embed.Author = new EmbedAuthor(fieldValue);
                            break;

                        case "fields":
                            if (fieldValue == "[]") break;
                            string[] fieldsValues = fieldValue.Replace("]", "").Replace("[", "").Split(',');
                            foreach (string value in fields)
                                embed.Fields.Add(new EmbedField(value.Replace("\"", "")));
                            break;
                    }
                }
                return embed;
            }

            public override string ToString()
            {
                string footer = "";
                if (this.Footer != null)
                {
                    footer = $"\"footer\":" + "{" + $"\"text\":" + "\"" + this.Footer.Text + "\"";
                    footer += String.IsNullOrEmpty(this.Footer.IconURL) ? "}" : ($",\"icon_url:" + "\"" + this.Footer.IconURL + "\"}");
                }

                string field = this.Fields == null ? "" : $"\"fields\":[";
                this.Fields.ForEach(item => field += "{" + $"\"name\": \"{item.Name}\"," +
                                                           $"\"value\": \"{item.Value}\"," +
                                                           $"\"inline\": {(item.Inline ? "true" : "false")}" + "},");
                field = (field[field.Length - 1] == ',' ? field.Remove(field.Length - 1, 1) : "") + "]";
                field = field == "\"fields\":[]" ? "" : field;
                field += (field != "" && footer != "\"footer\":[]") ? "," : "";
                field = field == "]," ? "" : field;

                string ColorHEX = this.Color.R.ToString("X2") + this.Color.G.ToString("X2") + this.Color.B.ToString("X2");
                string color = this.Color.IsEmpty ? "" : $"\"color\":\"{Convert.ToInt32(ColorHEX, 16)}\",";
                string content = String.IsNullOrEmpty(this.Content) ? "" : $"\"content\":" + $"\"{this.Content}\", \"tts\":{(this.Tts ? "true" : "false")},";
                string time = Timestamp != null ? $"\"timestamp\":\"{(((DateTime)Timestamp).ToString("yyyy-MM-ddTHH:mm:ssZ"))}\"," : "";
                string title = String.IsNullOrEmpty(this.Title) ? "" : $"\"title\":\"{this.Title}\",";

                string jsonObject = "{" + $"{content}" +
                                    "\"embed\":{" +
                                    $"\"description\":\"{this.Description}\"," +
                                    $"{title}" +
                                    //$"\"title\":\"{this.Title}\"" +
                                    $"{color}" +
                                    $"{time}" +
                                    $"{field}" +
                                    $"{footer}";

                jsonObject = jsonObject[jsonObject.Length - 1] == ',' ? jsonObject.Remove(jsonObject.Length - 1, 1) : jsonObject;
                jsonObject += "}}";
                return jsonObject;
            }

            public class EmbedAuthor
            {
                public string IconURL;
                public string Name;
                public string Url;

                public EmbedAuthor()
                {
                }

                public EmbedAuthor(string jsonObject)
                {
                }
            }

            public class EmbedField
            {
                public bool Inline;
                public string Name;
                public string Value;

                public EmbedField()
                {
                }

                public EmbedField(string jsonObject)
                {
                }
            }

            public class EmbedFooter
            {
                public string IconURL;
                public string Text;

                public EmbedFooter()
                {
                }

                public EmbedFooter(string jsonObject)
                {
                }
            }
        }

        public class Webhook
        {
            public Webhook(string url)
            {
                this.URL = url;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";

                try
                {
                    string responseStr = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()).ReadToEnd().Replace("}", "").Replace("{", " ");
                    string[] fields = responseStr.Split(',');
                    foreach (string field in fields)
                    {
                        int fieldLength = field.IndexOf('"', 2) - 2;
                        string fieldValue = field.Substring(field.IndexOf(':') + 2).Replace("\"", "");
                        switch (field.Substring(2, fieldLength))
                        {
                            case "id":
                                this.Id = ulong.Parse(fieldValue);
                                break;

                            case "name":
                                this.Name = fieldValue;
                                break;

                            case "avatar":
                                this.AvatarURL = string.Format("https://cdn.discordapp.com/avatars/{0}/{1}", this.Id, fieldValue);
                                break;

                            case "token":
                                this.Token = fieldValue;
                                break;

                            case "guild_id":
                                this.GuildID = ulong.Parse(fieldValue);
                                break;

                            case "channel_id":
                                this.ChannelID = ulong.Parse(fieldValue);
                                break;
                        }
                    }
                }
                catch { throw new Exception("Error while creating an object"); }
            }

            public string AvatarURL
            { get; private set; }

            public ulong ChannelID
            { get; private set; }

            public ulong GuildID
            { get; private set; }

            public ulong Id
            { get; private set; }

            public string Name
            { get; private set; }

            public string Token
            { get; private set; }

            public string URL
            { get; private set; }

            public static bool Delete(string url)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "DELETE";

                HttpStatusCode code = ((HttpWebResponse)request.GetResponse()).StatusCode;

                return code == HttpStatusCode.OK;
            }

            public void Send(string Message, bool TTS)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.URL);
                request.Headers.Set("Authorization", this.Token);
                request.ContentType = "application/json";
                request.Method = "POST";

                string message = "{" + $"\"content\":\"{Message}\",\"tts\":{(TTS ? "true" : "false")}" + "}";

                StreamWriter streamW = new StreamWriter(request.GetRequestStream());
                streamW.Write(message);
                streamW.Dispose();
                request.GetResponse();
            }

            public void Send(Discord.Embed embed, FileInfo file = null)
            {
                WebClient webhookRequest = new WebClient();
                string bound = "------------------------" + DateTime.Now.Ticks.ToString("x");

                using (var stream = new MemoryStream())
                {
                    webhookRequest.Headers.Add("Content-Type", "multipart/form-data; boundary=" + bound);
                    byte[] beginBodyBuffer = Encoding.UTF8.GetBytes("--" + bound + "\r\r\n");
                    stream.Write(beginBodyBuffer, 0, beginBodyBuffer.Length);
                    bool flag = file != null && file.Exists;
                    if (flag)
                    {
                        string fileBody = "Content-Disposition: form-data; name=\"file\"; filename=\"" + file.Name + "\"\r\r\nContent-Type: application/octet-stream\r\r\n\r\r\n";
                        byte[] fileBodyBuffer = Encoding.UTF8.GetBytes(fileBody);
                        stream.Write(fileBodyBuffer, 0, fileBodyBuffer.Length);
                        byte[] fileBuffer = File.ReadAllBytes(file.FullName);
                        stream.Write(fileBuffer, 0, fileBuffer.Length);
                        string fileBodyEnd = "\r\r\n--" + bound + "\r\r\n";
                        byte[] fileBodyEndBuffer = Encoding.UTF8.GetBytes(fileBodyEnd);
                        stream.Write(fileBodyEndBuffer, 0, fileBodyEndBuffer.Length);
                    }

                    string message = embed.ToString().Replace("\r\n", @"\r\n");
                    message = new Regex("embed").Replace(message, "embeds", 1);
                    message = new Regex("embeds\":{").Replace(message, "embeds\":[{", 1);
                    message = new Regex("\"}").Replace(message, "\"}]", 1);
                    string jsonBody = string.Concat(new string[]
                    {
                        "Content-Disposition: form-data; name=\"payload_json\"\r\r\nContent-Type: application/json\r\r\n\r\r\n",
                        string.Format("{0}\r\r\n", message),
                        "--",
                        bound,
                        "--"
                    });
                    byte[] jsonBodyBuffer = Encoding.UTF8.GetBytes(jsonBody);
                    stream.Write(jsonBodyBuffer, 0, jsonBodyBuffer.Length);
                    webhookRequest.UploadData(this.URL, stream.ToArray());
                }
            }
        }
    }

    public class Credentials
    {
        public Version Version;
        public string BaseAddress;
        public string Token;
    }

    public class Security
    {
        public class FingerPrint
        {
            private static string fingerPrint = string.Empty;

            public static string Value()
            {
                if (string.IsNullOrEmpty(fingerPrint))
                    fingerPrint = GetHash(Encryption.ComputeSha256Hash(/*"CPU >> " + CpuId() + "\r\nBIOS >> " + BiosId() +*/ "\r\nBASE >> " + BaseId() + /*"\r\nDISK >> "+ DiskId() + */"\r\nVIDEO >> " + VideoId() + "\r\nMAC >> " + MacId()));

                return fingerPrint;
            }

            private static string GetHash(string s)
            {
                MD5 sec = new MD5CryptoServiceProvider();
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] bt = enc.GetBytes(s);
                return GetHexString(sec.ComputeHash(bt));
            }

            private static string GetHexString(byte[] bt)
            {
                string s = string.Empty;
                for (int i = 0; i < bt.Length; i++)
                {
                    byte b = bt[i];
                    int n, n1, n2;
                    n = (int)b;
                    n1 = n & 15;
                    n2 = (n >> 4) & 15;
                    if (n2 > 9)
                        s += ((char)(n2 - 10 + (int)'A')).ToString();
                    else
                        s += n2.ToString();
                    if (n1 > 9)
                        s += ((char)(n1 - 10 + (int)'A')).ToString();
                    else
                        s += n1.ToString();
                    if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
                }
                return s;
            }

            #region Original Device ID Getting Code
            //Return a hardware identifier
            private static string identifier
            (string wmiClass, string wmiProperty, string wmiMustBeTrue)
            {
                string result = "";
                System.Management.ManagementClass mc =
                new System.Management.ManagementClass(wmiClass);
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo[wmiMustBeTrue].ToString() == "True")
                    {
                        //Only get the first one
                        if (result == "")
                        {
                            try
                            {
                                result = mo[wmiProperty].ToString();
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return result;
            }
            //Return a hardware identifier
            private static string identifier(string wmiClass, string wmiProperty)
            {
                string result = "";
                ManagementClass mc = new System.Management.ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (result == "")
                    {
                        try
                        {
                            var value = mo[wmiProperty];
                            result = value == null ? "" : value.ToString();
                            break;
                        }
                        catch { continue; }
                    }
                }
                return result;
            }
            private static string CpuId()
            {
                //Uses first CPU identifier available in order of preference
                //Don't get all identifiers, as it is very time consuming

                string retVal = /*identifier("Win32_Processor", "UniqueId")*/ "";
                if (retVal == "") //If no UniqueID, use ProcessorID
                {
                    retVal = identifier("Win32_Processor", "ProcessorId");
                    if (retVal == "") //If no ProcessorId, use Name
                    {
                        retVal = identifier("Win32_Processor", "Name");
                        if (retVal == "") //If no Name, use Manufacturer
                        {
                            retVal = identifier("Win32_Processor", "Manufacturer");
                        }
                        //Add clock speed for extra security
                        retVal += identifier("Win32_Processor", "MaxClockSpeed");
                    }
                }
                return retVal;
            }
            //BIOS Identifier
            private static string BiosId()
            {
                return identifier("Win32_BIOS", "Manufacturer")
                + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                + identifier("Win32_BIOS", "IdentificationCode")
                + identifier("Win32_BIOS", "SerialNumber")
                + identifier("Win32_BIOS", "ReleaseDate")
                + identifier("Win32_BIOS", "Version");
            }
            //Main physical hard drive ID
            private static string DiskId()
            {
                return identifier("Win32_DiskDrive", "Model")
                + identifier("Win32_DiskDrive", "Manufacturer")
                + identifier("Win32_DiskDrive", "Signature")
                + identifier("Win32_DiskDrive", "TotalHeads");
            }
            //Motherboard ID
            private static string BaseId()
            {
                return identifier("Win32_BaseBoard", "Model")
                + identifier("Win32_BaseBoard", "Manufacturer")
                + identifier("Win32_BaseBoard", "Name")
                + identifier("Win32_BaseBoard", "SerialNumber");
            }
            //Primary video controller ID
            private static string VideoId()
            {
                return identifier("Win32_VideoController", "DriverVersion")
                + identifier("Win32_VideoController", "Name");
            }
            //First enabled network card ID
            private static string MacId()
            {
                return identifier("Win32_NetworkAdapterConfiguration",
                    "MACAddress", "IPEnabled");
            }
            #endregion
        }

        public static string GetPublicIpAddress()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://icanhazip.com");
                return new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()).ReadToEnd();
            }
            catch { return "Error"; }
        }

        public static string GetInternalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "";
        }

        public static string GetMachineIdentifier()
        {
            return FingerPrint.Value();
        }
    }
}