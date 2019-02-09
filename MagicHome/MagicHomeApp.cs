using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Xml;
using HomeSeerAPI;
using MagicHomeAPI;
using MoreLinq;
using Newtonsoft.Json;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_MagicHome
{
    internal partial class MagicHomeApp
    {
        internal static DeviceFindResult[] deviceFindResults { get; set; }
        private static MagicHomeApp _sObjSingletonInstance = (MagicHomeApp)null;
        private static readonly object SObjLock = new object();
        private string _mInstance = "";
        private int _mTimeOut { get; set; }
        private string _mIniFile = "MagicHome.ini";
        private string PluginVersion { get; set; }
        private readonly object _mUpdateLock = new object();
        private readonly object _mDiscoLock = new object();
        protected IAppCallbackAPI MHsCallback;
        protected IHSApplication MHs;
        private const string decp = "laksidhiuaysdfuiydfusdyf876ysd876sdf";
        private const string Iv = "llalkjlkjlkjljljlskdfhiudf";
        private volatile bool _mWillShutDown;
        private System.Timers.Timer _mPollingTimer;
        private System.Timers.Timer _mDiscoveryTimer;
        private readonly Dictionary<string, MagicHomeDevices> _mMagicHome = new Dictionary<string, MagicHomeDevices>();
        public Triggers Triggers { get; protected set; }
        public Actions Actions { get; protected set; }
        private WebConfig _mConfigPage;
        private bool isReg { get; set; }
        private bool isLic { get; set; }
        internal Enums.REGISTRATION_MODES regMod { get; set; }

        /// <summary>
        /// </summary>
        internal List<DevDetail> DevDetailsList { get; set; }

        private MagicHomeApp()
        {

        }

        public bool IsNull(object checkobject)
        {
            try
            {
                if (checkobject == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }

        internal int AccessLevel()
        {
            return int.Parse(AccessLevelPriv());
        }

        internal string AccessLevelPriv()
        {
            return DecryptCertificate("i0pxNChIi3DP8DCrLMhJdHHyLjSqcLS1BQA=");
        }

        public string IniFile => _mIniFile;

        private string GetPublicIp()
        {
            Classes.IpInfo ipInfo = new Classes.IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/json");
                ipInfo = JsonConvert.DeserializeObject<Classes.IpInfo>(info);
            }
            catch (Exception)
            {
                //ignored
            }

            return ipInfo.Ip;
        }

        private static string GetLocalhostFqdn()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return string.IsNullOrWhiteSpace(ipProperties.DomainName) ? ipProperties.HostName : $"{ipProperties.HostName}.{ipProperties.DomainName}";
        }

        private static string GetIPAddresses()
        {
            var iplist = new List<string>();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                if (network.OperationalStatus == OperationalStatus.Up)
                {
                    // Read the IP configuration for each network
                    IPInterfaceProperties properties = network.GetIPProperties();

                    // Each network interface may have multiple IP addresses
                    foreach (var address in properties.UnicastAddresses)
                    {
                        // We're only interested in IPv4 addresses for now
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        // Ignore loopback addresses (e.g., 127.0.0.1)
                        if (IPAddress.IsLoopback(address.Address))
                            continue;
                        iplist.Add(address.Address.ToString());
                    }
                }
            }
            var iparr = iplist.ToArray();
            var ips = string.Join(",", iparr);
            return ips;
        }

        private static string GetMacAddresses()
        {
            var maclist = new List<string>();
            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                if (network.OperationalStatus == OperationalStatus.Up)
                {
                    maclist.Add(network.GetPhysicalAddress().ToString());
                }
            }

            var macarr = maclist.ToArray();
            var macs = string.Join(",", macarr);
            return macs;
        }

        public string Init()
        {
            var str1 = "";
            try
            {
                isReg = MHs.IsRegistered();
                isLic = MHs.IsLicensed();
                regMod = MHs.PluginLicenseMode("MagicHome");
                PluginVersion = Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                                Assembly.GetEntryAssembly().GetName().Version.Minor + "." +
                                Assembly.GetEntryAssembly().GetName().Version.Build + "." +
                                Assembly.GetEntryAssembly().GetName().Version.Revision;
                Logger.Init();
                Logger.LogInfo("{0} version {1}", (object)"MagicHome", (object)Assembly.GetEntryAssembly().GetName().Version);
                Triggers = new Triggers();
                Actions = new Actions();
                var str2 = "MagicHome".ToLower() + "config";
                _mConfigPage = new WebConfig(str2 + (_mInstance != "" ? ":" + _mInstance : ""));
                _mTimeOut = Convert.ToInt32(MHs.GetINISetting("GENERAL", "send_receive_timeout", "100", IniFile));
                MHs.RegisterPage(str2, "MagicHome", _mInstance);
                var webPageDesc1 = new WebPageDesc
                {
                    link = str2,
                    linktext = "Config"
                };
                if (_mInstance != "")
                {
                    var webPageDesc2 = webPageDesc1;
                    var webPageDesc3 = webPageDesc1;
                    var str3 = webPageDesc2.linktext + " - " + _mInstance;
                    webPageDesc2.linktext = str3;
                    var str4 = webPageDesc3.linktext + " - " + _mInstance;
                    webPageDesc3.linktext = str4;
                }
                webPageDesc1.page_title = "MagicHome Plug-In Configuration";
                webPageDesc1.plugInName = "MagicHome";
                webPageDesc1.plugInInstance = _mInstance;
                MHsCallback.RegisterConfigLink(webPageDesc1);
                MHsCallback.RegisterLink(webPageDesc1);
                if (string.IsNullOrEmpty(_mInstance))
                {
                    var webPageDesc2 =
                        new WebPageDesc
                        {
                            link = "http://homeseer.com/guides/plugins/MagicHome%20plug-in%20documentation.pdf",
                            linktext = "User Guide",
                            page_title = "MagicHome Plug-in User Guide",
                            plugInName = "MagicHome",
                            plugInInstance = _mInstance
                        };
                    MHs.RegisterHelpLink(webPageDesc2);
                    MHsCallback.RegisterLink(webPageDesc2);
                    
                    var webPageDesc3 = new WebPageDesc()
                    {
                        link = "https://forums.homeseer.com/forum/lighting-primary-technology-plug-ins/lighting-primary-technology-discussion/magichome-fuzzysb",
                        linktext = "Forum",
                        page_title = "MagicHome Plug-in Forum",
                        plugInName = "MagicHome",
                        plugInInstance = _mInstance
                    };
                    
                    MHs.RegisterLinkEx(webPageDesc3);
                    MHsCallback.RegisterLink(webPageDesc3);
                }
                new Thread(() =>
                {
                    try
                    {
                        string hostname = null;
                        string ip = null;
                        string mac = null;
                        string pip = null;
                        try
                        {
                            hostname = GetLocalhostFqdn();
                        }
                        catch
                        {
                            //ignored
                        }

                        try
                        {
                            ip = GetIPAddresses();
                        }
                        catch
                        {
                            //ignored
                        }

                        try
                        {
                            mac = GetMacAddresses();
                        }
                        catch
                        {
                            //ignored
                        }

                        try
                        {
                            pip = GetPublicIp();
                        }
                        catch
                        {
                            //ignored
                        }

                        var HS3PluginDataEntry = new Classes.HS3PluginDataEntry()
                        {
                            TimeofEntry = DateTime.Now.ToFileTimeUtc().ToString(),
                            Plugin = "MagicHome",
                            Hostname = hostname,
                            ip = ip,
                            macAddress = mac,
                            publicIp = pip,
                            isRegistered = isReg ? "True" : "False",
                            isLicenced = isLic ? "True" : "False",
                            registrationMode = Enum.GetName(typeof(Enums.REGISTRATION_MODES), regMod),
                            pluginVersion = PluginVersion
                        };

                        SendPluginData.SendHS3Data(HS3PluginDataEntry);
                    }
                    catch
                    {
                        //ignored
                    }
                    try
                    {
                        _mPollingTimer = new System.Timers.Timer(120000.0) { AutoReset = true };
                        _mPollingTimer.Elapsed += PollDevices;
                        _mPollingTimer.Start();
                        PollDevices(null, null);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.Message);
                        Logger.LogDebug(ex.ToString());
                    }
                    try
                    {
                        _mDiscoveryTimer = new System.Timers.Timer(3600000.0) { AutoReset = true };
                        _mDiscoveryTimer.Elapsed += PollDiscoveryAsync;
                        _mDiscoveryTimer.Start();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.Message);
                        Logger.LogDebug(ex.ToString());
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                str1 = ex.ToString();
                Logger.LogError(ex.ToString());
                _mWillShutDown = true;
            }
            return str1;
        }

        public void Shutdown()
        {
            try
            {
                Logger.LogInfo("Shutting Down MagicHome plugin");
                _mPollingTimer?.Stop();
                _mDiscoveryTimer?.Stop();
                Logger.Shut();
                MHsCallback = null;
                MHs = null;
                _mWillShutDown = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        public IHSApplication Hs
        {
            get => MHs;
            set => MHs = value;
        }

        public void ButtonPress(string buttonName, double value, int dvref)
        {
            if (regMod == Enums.REGISTRATION_MODES.REG_REGISTERED || regMod == Enums.REGISTRATION_MODES.REG_TRIAL)
            {
                ButtonPressPriv(buttonName, value, dvref);
            }
            else
            {
                Logger.LogWarning("The MagicHome Plugin is not registered, or your trial has expired. please purchase and register the plugin. if you have just registered the plugin, you will need to restart the plugin.");
            }

        }

        private void ButtonPressPriv(string buttonName, double value, int dvref)
        {
            try
            {
                DeviceClass deviceByRef = (DeviceClass)this.MHs.GetDeviceByRef(dvref);
                if (deviceByRef == null)
                    return;
                string magicHomeId;
                var deviceId = this.GetDeviceId(deviceByRef, out magicHomeId);
                if (magicHomeId == null || deviceId == null)
                {
                    Logger.LogError("Cannot find Magic Home device {0}", (object)magicHomeId);
                }
                else
                {
                    var deviceAddress = deviceByRef.get_Address(MHs);
                    var devInfo = deviceAddress.Split('-');
                    var devMac = devInfo[0];
                    var devType = devInfo[1];
                    if (deviceFindResults.Length > 0)
                    {
                        foreach (var discovery in deviceFindResults)
                        {
                            try
                            {
                                var devdetail = DevDetailsList.Find(x => x.Mac == devMac);
                                if (devdetail.Mac == discovery.MacAddress.ToString())
                                {
                                    if (devdetail.Dev != null)
                                    {
                                        var devStatus = devdetail.DevStatus;
                                        switch (devType)
                                        {
                                            case "root":
                                                return;
                                            case "mode":
                                                if (buttonName == "On")
                                                {
                                                    Logger.LogDebug(
                                                        "Setting Device " + devdetail.Mac + "'s power state to On");
                                                    devdetail.Dev.SetPowerState(PowerState.PowerOn, SendRecieveTimeout);

                                                }

                                                if (buttonName == "Off")
                                                {
                                                    Logger.LogDebug(
                                                        "Setting Device " + devdetail.Mac + "'s power state to Off");
                                                    devdetail.Dev.SetPowerState(PowerState.PowerOff,SendRecieveTimeout);
                                                }
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "colour":
                                                return;
                                            case "red":
                                                var rred = devStatus.Red;
                                                var rgreen = devStatus.Green;
                                                var rblue = devStatus.Blue;
                                                var rwwhite = devStatus.White1;
                                                var rcwhite = devStatus.White2;
                                                if (buttonName == "Up")
                                                {
                                                    Logger.LogDebug(
                                                        "increasing Device " + devdetail.Mac + "'s Red Value");
                                                    rred = ((int)rred < 255) ? (byte)((int)rred + 1) : rred;
                                                }

                                                if (buttonName == "Down")
                                                {
                                                    Logger.LogDebug(
                                                        "decreasing Device " + devdetail.Mac + "'s Red Value");
                                                    rred = ((int)rred > 0) ? (byte)((int)rred - 1) : rred;
                                                }
                                                devdetail.Dev.SetColor((byte)rred, (byte)rgreen, (byte)rblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)rwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)rcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "green":
                                                var gred = devStatus.Red;
                                                var ggreen = devStatus.Green; 
                                                var gblue = devStatus.Blue;
                                                var gwwhite = devStatus.White1;
                                                var gcwhite = devStatus.White2;
                                                if (buttonName == "Up")
                                                {
                                                    Logger.LogDebug(
                                                        "increasing Device " + devdetail.Mac + "'s Green Value");
                                                    ggreen = ((int)ggreen < 255) ? (byte)((int)ggreen + 1) : ggreen;
                                                }

                                                if (buttonName == "Down")
                                                {
                                                    Logger.LogDebug(
                                                        "decreasing Device " + devdetail.Mac + "'s Green Value");
                                                    ggreen = ((int)ggreen > 0) ? (byte)((int)ggreen - 1) : ggreen;
                                                }
                                                devdetail.Dev.SetColor((byte)gred, (byte)ggreen, (byte)gblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)gwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)gcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "blue":
                                                var bred = devStatus.Red;
                                                var bgreen = devStatus.Green;
                                                var bblue = devStatus.Blue; 
                                                var bwwhite = devStatus.White1;
                                                var bcwhite = devStatus.White2;
                                                if (buttonName == "Up")
                                                {
                                                    Logger.LogDebug(
                                                        "increasing Device " + devdetail.Mac + "'s Blue Value");
                                                    bblue = ((int)bblue < 255) ? (byte)((int)bblue + 1) : bblue;
                                                }

                                                if (buttonName == "Down")
                                                {
                                                    Logger.LogDebug(
                                                        "decreasing Device " + devdetail.Mac + "'s Blue Value");
                                                    bblue = ((int)bblue > 0) ? (byte)((int)bblue - 1) : bblue;
                                                }
                                                devdetail.Dev.SetColor((byte)bred, (byte)bgreen, (byte)bblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)bwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)bcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "warmwhite":
                                                var w1red = devStatus.Red;
                                                var w1green = devStatus.Green;
                                                var w1blue = devStatus.Blue;
                                                var w1wwhite = devStatus.White1;
                                                var w1cwhite = devStatus.White2;
                                                if (buttonName == "Up")
                                                {
                                                    Logger.LogDebug(
                                                        "increasing Device " + devdetail.Mac + "'s Warm White Value");
                                                    w1wwhite = ((int)w1wwhite < 255) ? (byte)((int)w1wwhite + 1) : w1wwhite;
                                                }

                                                if (buttonName == "Down")
                                                {
                                                    Logger.LogDebug(
                                                        "decreasing Device " + devdetail.Mac + "'s Warm White Value");
                                                    w1wwhite = ((int)w1wwhite > 0) ? (byte)((int)w1wwhite - 1) : w1wwhite;
                                                }
                                                devdetail.Dev.SetColor((byte)w1red, (byte)w1green, (byte)w1blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)w1wwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)w1cwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "coolwhite":
                                                var w2red = devStatus.Red;
                                                var w2green = devStatus.Green;
                                                var w2blue = devStatus.Blue;
                                                var w2wwhite = devStatus.White1;
                                                var w2cwhite = devStatus.White2;
                                                if (buttonName == "Up")
                                                {
                                                    Logger.LogDebug(
                                                        "increasing Device " + devdetail.Mac + "'s Cool White Value");
                                                    w2cwhite = ((int)w2cwhite < 255) ? (byte)((int)w2cwhite + 1) : w2cwhite;
                                                }

                                                if (buttonName == "Down")
                                                {
                                                    Logger.LogDebug(
                                                        "decreasing Device " + devdetail.Mac + "'s Cool White Value");
                                                    w2cwhite = ((int)w2cwhite > 0) ? (byte)((int)w2cwhite - 1) : w2cwhite;
                                                }
                                                devdetail.Dev.SetColor((byte)w2red, (byte)w2green, (byte)w2blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)w2wwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)w2cwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "preset":
                                                switch (buttonName)
                                                    {
                                                        case "RGB Fade":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s RGB Fade Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.RgbFade,0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                        var p1devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p1devStatus);
                                                        return;
                                                        case "Red Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Red Pulse Preset");
                                                        devdetail.Dev.SetPreset(PresetMode.RedPulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                        var p2devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p2devStatus);
                                                        return;
                                                        case "Green Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Green Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.GreenPulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                        var p3devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p3devStatus);
                                                        return;
                                                        case "Blue Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Blue Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.BluePulse, 0, SendRecieveTimeout);
                                                            var p4devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p4devStatus);
                                                        return;
                                                        case "Yellow Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Yellow Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.YellowPulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p5devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p5devStatus);
                                                        return;
                                                        case "Cyan Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Cyan Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.CyanPulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p6devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p6devStatus);
                                                        return;
                                                        case "Violet Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Violet Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.VioletPulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p7devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p7devStatus);
                                                        return;
                                                        case "White Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s White Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.WhitePulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p8devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p8devStatus);
                                                        return;
                                                        case "Red Green Alternate Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Red Green Alternate Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.RedGreenAlternatePulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p9devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p9devStatus);
                                                        return;
                                                        case "Red Blue Alternate Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Red Blue Alternate Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.RedBlueAlternatePulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p10devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p10devStatus);
                                                        return;
                                                        case "Green Blue Alternate Pulse":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Green Blue Alternate Pulse Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.GreenBlueAlternatePulse, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p11devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p11devStatus);
                                                        return;
                                                        case "Disco Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Disco Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.DiscoFlash, 0, SendRecieveTimeout);
                                                            var p12devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p12devStatus);
                                                        return;
                                                        case "Red Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Red Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.RedFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p13devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p13devStatus);
                                                        return;
                                                        case "Green Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Green Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.GreenFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p14devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p14devStatus);
                                                        return;
                                                        case "Blue Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Blue Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.BlueFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p15devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p15devStatus);
                                                        return;
                                                        case "Yellow Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Yellow Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.YellowFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p16devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p16devStatus);
                                                        return;
                                                        case "Cyan Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Cyan Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.CyanFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p17devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p17devStatus);
                                                        return;
                                                        case "Violet Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Violet Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.VioletFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p18devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p18devStatus);
                                                        return;
                                                        case "White Flash":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s White Flash Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.WhiteFlash, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var p19devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p19devStatus);
                                                        return;
                                                        case "Colour Change":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Colour Change Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.ColorChange, 0, SendRecieveTimeout);
                                                            var p20devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, p20devStatus);
                                                        return;
                                                        case "Normal RGB":
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Normal RGB Mode Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.NormalRgb, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var pdevStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, pdevStatus);
                                                        return;
                                                        default:
                                                            Logger.LogDebug(
                                                                "Setting Device " + devdetail.Mac + "'s Normal RGB Mode Preset");
                                                            devdetail.Dev.SetPreset(PresetMode.NormalRgb, 0, SendRecieveTimeout);
                                                            Thread.Sleep(1000);
                                                            var dpdevStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                            UpdateMagicHomeDevice(discovery, devdetail.Dev, dpdevStatus);
                                                        return;
                                                    }
                                            default:
                                                Logger.LogError(
                                                    "Device Type  is not recognised, most likely because the device address has been changed, please restart the plugin to correct");
                                                return; 
                                        }
                                        

                                        
                                    }
                                    else
                                    {
                                        Logger.LogError("Device model " + discovery.Model + " is not currently supported by the plugin, please contact Broadband Tap via the plugin Homeseer forum and request support for your device.");
                                    }
                                }
                                
                                
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("An Error Occurred, the error reported was: " +
                                                ex.Message);
                            }

                        }

                    }

                }
            }
            catch
            {
                //ignore
            }
        }

        internal void SetControlValue(int dvref, double value)
        {
            try
            {
                DeviceClass deviceByRef = (DeviceClass)this.MHs.GetDeviceByRef(dvref);
                if (deviceByRef == null)
                    return;
                string magicHomeId;
                var deviceId = this.GetDeviceId(deviceByRef, out magicHomeId);
                if (magicHomeId == null || deviceId == null)
                {
                    Logger.LogError("Cannot find Magic Home device {0}", (object)magicHomeId);
                }
                else
                {
                    var deviceAddress = deviceByRef.get_Address(MHs);
                    var devInfo = deviceAddress.Split('-');
                    var devMac = devInfo[0];
                    var devType = devInfo[1];
                    if (deviceFindResults.Length > 0)
                    {
                        foreach (var discovery in deviceFindResults)
                        {
                            try
                            {
                                var devdetail = DevDetailsList.Find(x => x.Mac == devMac);
                                if (devdetail.Mac == discovery.MacAddress.ToString())
                                {
                                    if (devdetail.Dev != null)
                                    {
                                        var devStatus = devdetail.DevStatus;
                                        switch (devType)
                                        {
                                            case "root":
                                                return;
                                            case "mode":
                                                var mlastValue = deviceByRef.get_devValue(MHs);
                                                var mbyteValDiff = ((int)value == (int)mlastValue) ? 0 :  (int)value - (int)mlastValue;
                                                var mvalDiff = PercentToByte(mbyteValDiff);
                                                var mred = (devStatus.Red > 0) ? ((mvalDiff >= 0) ? (((devStatus.Red + mvalDiff) <= 255) ? (devStatus.Red + mvalDiff): 255) : (((devStatus.Red + mvalDiff) >= 0) ? (devStatus.Red + mvalDiff) : 1)) : 0;
                                                var mgreen = (devStatus.Green > 0) ? (mvalDiff >= 0) ? (((devStatus.Green + mvalDiff) <= 255) ? (devStatus.Green + mvalDiff) : 255) : (((devStatus.Green + mvalDiff) >= 0) ? (devStatus.Green + mvalDiff) : 1) : 0;
                                                var mblue = (devStatus.Blue > 0) ? (mvalDiff >= 0) ? (((devStatus.Blue + mvalDiff) <= 255) ? (devStatus.Blue + mvalDiff) : 255) : (((devStatus.Blue + mvalDiff) >= 0) ? (devStatus.Blue + mvalDiff) : 1) : 0;
                                                var mwwhite = (devStatus.White1 > 0) ? (mvalDiff >= 0) ? ((((int)devStatus.White1 + mvalDiff) <= 255) ? ((int)devStatus.White1 + mvalDiff) : 255) : ((((int)devStatus.White1 + mvalDiff) >= 0) ? ((int)devStatus.White1 + mvalDiff) : 1) : 0;
                                                var mcwhite = (devStatus.White2 > 0) ? (mvalDiff >= 0) ? ((((int)devStatus.White2 + mvalDiff) <= 255) ? ((int)devStatus.White2 + mvalDiff) : 255) : ((((int)devStatus.White2 + mvalDiff) >= 0) ? ((int)devStatus.White2 + mvalDiff) : 1) : 0;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s brightness to " + value);
                                                devdetail.Dev.SetColor((byte)mred, (byte)mgreen, (byte)mblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)mwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)mcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "colour":
                                                var intvalue = (int) value;
                                                string hexValue = intvalue.ToString("X6");
                                                var colours = ConvertToRgb(hexValue);
                                                var cwwhite = devStatus.White1;
                                                var ccwhite = devStatus.White2;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Colour to " +hexValue);
                                                devdetail.Dev.SetColor((byte)colours.red, (byte)colours.green, (byte)colours.blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)cwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)ccwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "red":
                                                var rred = (byte)value;
                                                var rgreen = devStatus.Green;
                                                var rblue = devStatus.Blue;
                                                var rwwhite = devStatus.White1;
                                                var rcwhite = devStatus.White2;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Red Value to " + value);
                                                devdetail.Dev.SetColor((byte)rred, (byte)rgreen, (byte)rblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)rwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)rcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "green":
                                                var gred = devStatus.Red;
                                                var ggreen = (byte)value;
                                                var gblue = devStatus.Blue;
                                                var gwwhite = devStatus.White1;
                                                var gcwhite = devStatus.White2;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Green Value to " + value);
                                                devdetail.Dev.SetColor((byte)gred, (byte)ggreen, (byte)gblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)gwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)gcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "blue":
                                                var bred = devStatus.Red;
                                                var bgreen = devStatus.Green;
                                                var bblue = (byte)value;
                                                var bwwhite = devStatus.White1;
                                                var bcwhite = devStatus.White2;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Blue Value to " + value);
                                                devdetail.Dev.SetColor((byte)bred, (byte)bgreen, (byte)bblue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)bwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)bcwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "warmwhite":
                                                var w1red = devStatus.Red;
                                                var w1green = devStatus.Green;
                                                var w1blue = devStatus.Blue;
                                                var w1wwhite = (byte)value;
                                                var w1cwhite = devStatus.White2;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Warm White Value to " + value);
                                                devdetail.Dev.SetColor((byte)w1red, (byte)w1green, (byte)w1blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)w1wwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)w1cwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "coolwhite":
                                                var w2red = devStatus.Red;
                                                var w2green = devStatus.Green;
                                                var w2blue = devStatus.Blue;
                                                var w2wwhite = devStatus.White1;
                                                var w2cwhite = (byte)value;
                                                Logger.LogDebug(
                                                    "Setting Device " + devdetail.Mac + "'s Cool White Value to " + value);
                                                devdetail.Dev.SetColor((byte)w2red, (byte)w2green, (byte)w2blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)w2wwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)w2cwhite : (byte?)null, true, true, SendRecieveTimeout);
                                                Thread.Sleep(1000);
                                                devStatus = devdetail.Dev.GetStatus(SendRecieveTimeout);
                                                UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                                return;
                                            case "preset":
                                                var curMode = devStatus.Mode;
                                                if (curMode == PresetMode.NormalRgb)
                                                {
                                                    Logger.LogDebug(
                                                        "Setting preset mode speed for Device " + devdetail.Mac + " is not valid for Normal RGB Mode");
                                                }
                                                else
                                                {
                                                    Logger.LogDebug(
                                                        "Setting Device " + devdetail.Mac + "'s Preset Speed to " + value + "%");
                                                    devdetail.Dev.SetPreset(curMode, (int)value, SendRecieveTimeout);
                                                }
                                                
                                                break;
                                            default:
                                                Logger.LogError(
                                                    "Device Type  is not recognised, most likely because the device address has been changed, please restart the plugin to correct");
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogError("Device model " + discovery.Model + " is not currently supported by the plugin, please contact Broadband Tap via the plugin Homeseer forum and request support for your device.");
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("An Error Occurred, the error reported was: " +
                                                ex.Message);
                            }

                        }

                    }

                }
            }
            catch
            {
                //ignore
            }
        }

        public void HsEvent(Enums.HSEvent eventType, object[] parms)
        {
            try
            {
                if (eventType != (Enums.HSEvent)32 || parms.Length < 2)
                    return;
                var parm = (int)parms[1];
            }
            catch (Exception ex)
            {
                Logger.LogError("HSEvent callback. " + ex.Message);
                Logger.LogDebug(ex.ToString());
            }
        }

        public string GetPagePlugin(string page, string user, int userRights, string queryString)
        {
            if (_mInstance != "")
                page = page + ":" + _mInstance;
            return _mConfigPage.GetPagePlugin(page, user, userRights, queryString);
        }

        public void PollDevices(object source, ElapsedEventArgs e)
        {
            Logger.LogDebug(
                "MagicHome Device Poll Inititated.");
            lock (_mUpdateLock)
            {
                var num =  UpdateMagicHomeDevices().Result ? 1 : 0;
            }
        }

        public void PollDiscoveryAsync(object source, ElapsedEventArgs e)
        {
            Logger.LogDebug(
                "MagicHome Device Discovery Inititated.");
            lock (_mDiscoLock)
            {
                var i = UpdateDiscoveredDevices().Result ? 1 : 0;
            }
        }

        private async Task<bool> UpdateDiscoveredDevices()
        {
            var returnValue = false;
            var deviceFindResultsList = new List<DeviceFindResult>();
            try
            {
                
                var nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in nics)
                {
                    int nicId = adapter.GetIPProperties().GetIPv4Properties().Index;
                    if (NetworkInterface.LoopbackInterfaceIndex == nicId)
                    {
                        Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is the Loopback Interface");
                        continue;
                    }

                    if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    {
                        Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is not an Ethernet or Wifi Adapter");
                        continue;
                    }

                    if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false)
                    {
                        Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as IPv4 is not Enabled on the Interface");
                        continue;
                    }
                    if (adapter.OperationalStatus == OperationalStatus.Up && adapter.SupportsMulticast &&
                        adapter.GetIPProperties().GetIPv4Properties() != null)
                    {
                        Logger.LogDebug("Discovery will run on on NIC Adapter " + adapter.Name);
                        try
                        {
                            var adapterProperties = adapter.GetIPProperties();
                            foreach (var ua in adapterProperties.UnicastAddresses)
                            {
                                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    var ipNet = IPNetwork.Parse(ua.Address, ua.IPv4Mask);
                                    Logger.LogDebug("Running Discovery on on NIC Adapter " + adapter.Name +
                                                    " and will target broadcast address " + ipNet.Broadcast);
                                    var nicEndPoint = new IPEndPoint(ipNet.Broadcast, 48899);
                                    var nicLocalEnpoint = new IPEndPoint(ua.Address, 48899);
                                    var allDeviceDisc = DeviceFinder.FindDevices(nicEndPoint, nicLocalEnpoint);
                                    deviceFindResultsList.AddRange(
                                        allDeviceDisc as List<DeviceFindResult> ?? allDeviceDisc.ToList());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Failed Discovery, the error is: " + ex.Message);
                        }

                    }
                    else
                    {
                        Logger.LogDebug("Discovery will not run on on NIC Adapter " + adapter.Name + " as it is either in a non operational state, Does not support Multicast, or we have failed to get IPv4 Properties ");
                    }
                }
                var devArray = deviceFindResultsList.DistinctBy(x => x.MacAddress).ToArray();
                if (devArray.Length > deviceFindResults.Length)
                {
                    Logger.LogDebug(devArray.Length - deviceFindResults.Length + " New Devices were discovered, Proceeding to Create Devices upon the next Device Poll");
                    deviceFindResults = deviceFindResultsList.DistinctBy(x => x.MacAddress).ToArray();
                }
                else
                {
                    Logger.LogDebug("No New Devices were discovered, not updating discovery list");
                }
                returnValue = true;
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(returnValue);
            }
        }
           
        private async Task<bool> UpdateMagicHomeDevices()
        {
            var returnValue = false;
            DeviceFindResult discResult = null;
            var updatedMagicHomeIds = new List<string>();
            try
            {
                var deviceFindResultsList = new List<DeviceFindResult>();
                if (deviceFindResults == null)
                {
                    var nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var adapter in nics)
                    {
                        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        {
                            Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is the Tunnel Interface");
                            continue;
                        }
                        int nicId = adapter.GetIPProperties().GetIPv4Properties().Index;
                        if (NetworkInterface.LoopbackInterfaceIndex == nicId)
                        {
                            Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is the Loopback Interface" );
                            continue;
                        }

                        if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                        {
                            Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is not an Ethernet or Wifi Adapter");
                            continue;
                        }

                        if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false)
                        {
                            Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as IPv4 is not Enabled on the Interface");
                            continue;
                        }
                        if (adapter.OperationalStatus == OperationalStatus.Up && adapter.SupportsMulticast &&
                            adapter.GetIPProperties().GetIPv4Properties() != null)
                        {
                            Logger.LogDebug("Discovery will run on on NIC Adapter " + adapter.Name);
                            try
                            {
                                var adapterProperties = adapter.GetIPProperties();
                                foreach (var ua in adapterProperties.UnicastAddresses)
                                {
                                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        var ipNet = IPNetwork.Parse(ua.Address, ua.IPv4Mask);
                                        Logger.LogDebug("Running Discovery on on NIC Adapter " + adapter.Name +
                                                        " and will target broadcast address " + ipNet.Broadcast);
                                        var nicEndPoint = new IPEndPoint(ipNet.Broadcast, 48899);
                                        var nicLocalEnpoint = new IPEndPoint(ua.Address, 48899);
                                        var allDeviceDisc = DeviceFinder.FindDevices(nicEndPoint, nicLocalEnpoint);
                                        deviceFindResultsList.AddRange(
                                            allDeviceDisc as List<DeviceFindResult> ?? allDeviceDisc.ToList());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("Failed Discovery, the error is: " + ex.Message);
                            }

                        }
                        else
                        {
                            Logger.LogDebug("Discovery will not run on on NIC Adapter " + adapter.Name + " as it is either in a non operational state, Does not support Multicast, or we have failed to get IPv4 Properties ");
                        }
                    }
                   deviceFindResults = deviceFindResultsList.DistinctBy(x => x.MacAddress).ToArray();
                }
                else
                {
                    if (deviceFindResults.Length == 0)
                    {
                        var nics = NetworkInterface.GetAllNetworkInterfaces();
                        foreach (var adapter in nics)
                        {
                            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                            {
                                Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is the Tunnel Interface");
                                continue;
                            }
                            int nicId = adapter.GetIPProperties().GetIPv4Properties().Index;
                            if (NetworkInterface.LoopbackInterfaceIndex == nicId)
                            {
                                Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is the Loopback Interface");
                                continue;
                            }

                            if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                            {
                                Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as it is not an Ethernet or Wifi Adapter");
                                continue;
                            }

                            if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false)
                            {
                                Logger.LogDebug("Discovery is not running on NIC Adapter " + adapter.Name + " as IPv4 is not Enabled on the Interface");
                                continue;
                            }
                            if (adapter.OperationalStatus == OperationalStatus.Up && adapter.SupportsMulticast &&
                                adapter.GetIPProperties().GetIPv4Properties() != null)
                            {
                                Logger.LogDebug("Discovery will run on on NIC Adapter " + adapter.Name);
                                try
                                {
                                    var adapterProperties = adapter.GetIPProperties();
                                    foreach (var ua in adapterProperties.UnicastAddresses)
                                    {
                                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            var ipNet = IPNetwork.Parse(ua.Address, ua.IPv4Mask);
                                            Logger.LogDebug("Running Discovery on on NIC Adapter " + adapter.Name +
                                                            " and will target broadcast address " + ipNet.Broadcast);
                                            var nicEndPoint = new IPEndPoint(ipNet.Broadcast, 48899);
                                            var nicLocalEnpoint = new IPEndPoint(ua.Address, 48899);
                                            var allDeviceDisc = DeviceFinder.FindDevices(nicEndPoint, nicLocalEnpoint);
                                            deviceFindResultsList.AddRange(
                                                allDeviceDisc as List<DeviceFindResult> ?? allDeviceDisc.ToList());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError("Failed Discovery, the error is: " + ex.Message);
                                }

                            }
                            else
                            {
                                Logger.LogDebug("Discovery will not run on on NIC Adapter " + adapter.Name + " as it is either in a non operational state, Does not support Multicast, or we have failed to get IPv4 Properties ");
                            }

                        }
                        deviceFindResults = deviceFindResultsList.DistinctBy(x => x.MacAddress).ToArray();
                    }

                    
                }

                if (deviceFindResults != null)
                {
                    if (deviceFindResults.Length > 0)
                    {
                        if (DevDetailsList == null)
                        {
                            DevDetailsList = new List<DevDetail>();
                        }
                        foreach (var discovery in deviceFindResults)
                        {
                            try
                            {
                                discResult = discovery;
                                Device dev = null;
                                if (DevDetailsList.Count > 0)
                                {
                                    try
                                    {
                                        dev =
                                            (Equals(
                                                 DevDetailsList.Find(x => x.Mac == discovery.MacAddress.ToString())
                                                     .Discovery.MacAddress, discovery.MacAddress) &&
                                             Equals(
                                                 DevDetailsList.Find(x => x.Mac == discovery.MacAddress.ToString())
                                                     .Discovery.IpAddress, discovery.IpAddress))
                                                ? DevDetailsList.Find(x => x.Mac == discovery.MacAddress.ToString()).Dev
                                                : GetDevice(discovery);
                                    }
                                    catch
                                    {
                                        //ignored
                                    }

                                }
                                if (dev != null)
                                {
                                    var devStatus = dev.GetStatus(SendRecieveTimeout);
                                    var devDetail = new DevDetail()
                                    {
                                        Mac = discovery.MacAddress.ToString(),
                                        Discovery = discovery,
                                        Dev = dev,
                                        DevStatus = devStatus
                                    };
                                    var itemToRemove = DevDetailsList.SingleOrDefault(x => x.Mac == discovery.MacAddress.ToString());
                                    if (itemToRemove != null)
                                    {
                                        DevDetailsList.Remove(itemToRemove);
                                    };
                                    DevDetailsList.Add(devDetail);
                                    Logger.LogDebug("Device model " + discovery.Model + " with Mac Address " +
                                                    discovery.MacAddress + " and Firmware Version " +
                                                    devStatus.VersionNumber + ", proceeding to update Homeseer devices.");
                                    UpdateMagicHomeDevice(discovery, dev, devStatus);
                                }
                                else
                                {
                                    if (DevDetailsList.Count < deviceFindResults.Length)
                                    {
                                        var initDev = GetDevice(discovery);
                                        var initDevStatus = initDev.GetStatus(SendRecieveTimeout);
                                        var initDevDetail = new DevDetail()
                                        {
                                            Mac = discovery.MacAddress.ToString(),
                                            Discovery = discovery,
                                            Dev = initDev,
                                            DevStatus = initDevStatus
                                        };
                                        DevDetailsList.Add(initDevDetail);
                                        Logger.LogDebug("Device model " + discovery.Model + " with Mac Address " +
                                                        discovery.MacAddress + " and Firmware Version " +
                                                        initDevStatus.VersionNumber + ", proceeding to update Homeseer devices.");
                                        UpdateMagicHomeDevice(discovery, initDev, initDevStatus);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (discResult != null)
                                {
                                    Logger.LogError("Unable to update the status of device with IP " + discResult.IpAddress +
                                                    " and Mac Address " + discResult.MacAddress + ", the error reported was: " +
                                                    ex.Message);
                                }
                            }

                        }
                    }
                    else
                    {
                        Logger.LogDebug(
                            "Unable to discover any Magic Home devices. please restart the plugin to try again");
                    }
                }
                else
                {
                    Logger.LogDebug("No response from any devices were found during discovery");
                }
                


                foreach (var magicHome in _mMagicHome)
                {
                    updatedMagicHomeIds.Add(magicHome.Key);
                }

                if (deviceFindResults.Length == updatedMagicHomeIds.Count)
                {
                    returnValue = true;
                }
                else
                {
                    Logger.LogError(
                        "Unable to update the status of all devices as some devices failed to respond to the status update, please check your devices. If a device has been powered down, please restart the plugin to rediscover devices");
                }
            }
            catch (Exception ex)
            {
                if (discResult != null)
                {
                    Logger.LogError("Unable to update the status of all devices the error reported was: " +
                                    ex.Message);
                }
                else
                {
                    Logger.LogError("Unable to discover any devices, the error was: " +
                                    ex.Message);
                }

                    
                returnValue = false;
            }

            return await Task.FromResult(returnValue);
        }

        internal void UpdateMagicHomeDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus)
        {
            if (devStatus != null)
            {
                DeviceClass deviceClass;
                DeviceClass dev1;
                DeviceClass dev2;
                DeviceClass dev3;
                DeviceClass dev4;
                DeviceClass dev5;
                DeviceClass dev6;
                DeviceClass dev7;
                DeviceClass dev8;

                if (_mMagicHome.ContainsKey(discovery.MacAddress.ToString()))
                {
                    deviceClass = _mMagicHome[discovery.MacAddress.ToString()].RootDevice;
                    dev1 = _mMagicHome[discovery.MacAddress.ToString()].Mode;
                    dev2 = _mMagicHome[discovery.MacAddress.ToString()].Colour;
                    dev3 = _mMagicHome[discovery.MacAddress.ToString()].Red;
                    dev4 = _mMagicHome[discovery.MacAddress.ToString()].Green;
                    dev5 = _mMagicHome[discovery.MacAddress.ToString()].Blue;
                    dev6 = _mMagicHome[discovery.MacAddress.ToString()].WarmWhite;
                    dev7 = _mMagicHome[discovery.MacAddress.ToString()].CoolWhite;
                    dev8 = _mMagicHome[discovery.MacAddress.ToString()].Preset;
                }
                else
                {
                    deviceClass = this.FindDeviceById("root", discovery.MacAddress.ToString());
                    if (deviceClass == null)
                    {
                        deviceClass = CreateRootDevice(discovery, dev, devStatus, false);
                    }
                    else
                    {
                        UpdateRootDeviceVsvgPairs(deviceClass, discovery, dev, devStatus, false);
                    }

                    dev1 = this.FindDeviceById(discovery.MacAddress.ToString(), discovery.MacAddress.ToString());
                    if (dev1 == null)
                    {
                        dev1 = CreateMagicHomeModeDevice(discovery, dev, devStatus, deviceClass);
                    }
                    else
                    {
                        UpdateMagicHomeModeVsvgPairs(dev1, discovery, dev, devStatus);
                    }

                    dev2 = this.FindDeviceById(discovery.MacAddress + " Colour", discovery.MacAddress.ToString());
                    if (dev2 == null)
                    {
                        dev2 = CreateMagicHomeColourDevice(discovery, dev, devStatus, deviceClass);
                    }
                    else
                    {
                        UpdateMagicHomeColourVsvgPairs(dev2, discovery, dev, devStatus);
                    }

                    if (dev._deviceType == DeviceType.Rgb || dev._deviceType == DeviceType.RgbWarmwhite ||
                        dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || dev._deviceType == DeviceType.LegacyBulb || dev._deviceType == DeviceType.Bulb)
                    {
                        dev3 = this.FindDeviceById(discovery.MacAddress + " Red", discovery.MacAddress.ToString());
                        if (dev3 == null)
                        {
                            dev3 = CreateMagicHomeRedDevice(discovery, dev, devStatus, deviceClass);
                        }
                        else
                        {
                            UpdateMagicHomeRedVsvgPairs(dev3, discovery, dev, devStatus);
                        }

                        dev4 = this.FindDeviceById(discovery.MacAddress + " Green", discovery.MacAddress.ToString());
                        if (dev4 == null)
                        {
                            dev4 = CreateMagicHomeGreenDevice(discovery, dev, devStatus, deviceClass);
                        }
                        else
                        {
                            UpdateMagicHomeGreenVsvgPairs(dev4, discovery, dev, devStatus);
                        }

                        dev5 = this.FindDeviceById(discovery.MacAddress + " Blue", discovery.MacAddress.ToString());
                        if (dev5 == null)
                        {
                            dev5 = CreateMagicHomeBlueDevice(discovery, dev, devStatus, deviceClass);
                        }
                        else
                        {
                            UpdateMagicHomeBlueVsvgPairs(dev5, discovery, dev, devStatus);
                        }
                    }
                    else
                    {
                        dev3 = null;
                        dev4 = null;
                        dev5 = null;
                    }



                    if (dev._deviceType == DeviceType.RgbWarmwhite ||
                        dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || dev._deviceType == DeviceType.LegacyBulb || dev._deviceType == DeviceType.Bulb)
                    {
                        dev6 = this.FindDeviceById(discovery.MacAddress + " Warm White",
                            discovery.MacAddress.ToString());
                        if (dev6 == null)
                        {
                            dev6 = CreateMagicHomeWarmWhiteDevice(discovery, dev, devStatus, deviceClass);
                        }
                        else
                        {
                            UpdateMagicHomeWarmWhiteVsvgPairs(dev6, discovery, dev, devStatus);
                        }
                    }
                    else
                    {
                        dev6 = null;
                    }

                    if (dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite)
                    {
                        dev7 = this.FindDeviceById(discovery.MacAddress + " Cool White", discovery.MacAddress.ToString());
                        if (dev7 == null)
                        {
                            dev7 = CreateMagicHomeCoolWhiteDevice(discovery, dev, devStatus, deviceClass);
                        }
                        else
                        {
                            UpdateMagicHomeCoolWhiteVsvgPairs(dev7, discovery, dev, devStatus);
                        }
                    }
                    else
                    {
                        dev7 = null;
                    }

                    
                    dev8 = this.FindDeviceById(discovery.MacAddress + " Preset", discovery.MacAddress.ToString());
                    if (dev8 == null)
                    {
                        dev8 = CreateMagicHomePresetDevice(discovery, dev, devStatus, deviceClass);
                    }
                    else
                    {
                        UpdateMagicHomePresetVsvgPairs(dev8, discovery, dev, devStatus);
                    }
                    this._mMagicHome.Add(discovery.MacAddress.ToString() ,new MagicHomeApp.MagicHomeDevices()
                        {
                            RootDevice = deviceClass,
                            Mode = dev1,
                            Colour = dev2,
                            Red = dev3,
                            Green = dev4,
                            Blue = dev5,
                            WarmWhite = dev6,
                            CoolWhite = dev7,
                            Preset = dev8
                        });
                }

                var num = dev1.get_Ref((IHSApplication) null);
                this.MHs.SetDeviceValueByRef(num, (devStatus.PowerState == PowerState.PowerOn)? (double)GetDimLevel(devStatus) : (double)0, true);
                num = dev2.get_Ref((IHSApplication)null);
                this.MHs.SetDeviceValueByRef(num, (double)ParseHexString(ConvertRgbToHex(devStatus)), true);
                if (dev._deviceType == DeviceType.Rgb || dev._deviceType == DeviceType.RgbWarmwhite ||
                    dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite)
                {
                    num = dev3.get_Ref((IHSApplication)null);
                    this.MHs.SetDeviceValueByRef(num, (double)devStatus.Red, true);
                    num = dev4.get_Ref((IHSApplication)null);
                    this.MHs.SetDeviceValueByRef(num, (double)devStatus.Green, true);
                    num = dev5.get_Ref((IHSApplication)null);
                    this.MHs.SetDeviceValueByRef(num, (double)devStatus.Blue, true);
                }

                if (dev._deviceType == DeviceType.RgbWarmwhite ||
                    dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite)
                {
                    num = dev6.get_Ref((IHSApplication)null);
                    if (devStatus.White1 != null) this.MHs.SetDeviceValueByRef(num, (double) devStatus.White1, true);
                }

                if (dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite)
                {
                    num = dev7.get_Ref((IHSApplication)null);
                    if (devStatus.White2 != null) this.MHs.SetDeviceValueByRef(num, (double) devStatus.White2, true);
                }  
                num = dev8.get_Ref((IHSApplication)null);
                this.MHs.SetDeviceValueByRef(num, (double)devStatus.Mode, true);

                var devDetail = new DevDetail()
                {
                    Mac = discovery.MacAddress.ToString(),
                    Discovery = discovery,
                    Dev = dev,
                    DevStatus = devStatus
                };
                var itemToRemove = DevDetailsList.SingleOrDefault(x => x.Mac == discovery.MacAddress.ToString());
                if (itemToRemove != null)
                {
                    DevDetailsList.Remove(itemToRemove);
                };
                DevDetailsList.Add(devDetail);
            }
        }

        private static string ConvertRgbToHex(DeviceStatus devStatus)
        {
            float red = (float)devStatus.Red;
            float green = (float)devStatus.Green;
            float blue = (float)devStatus.Blue;
            float num1 = Math.Max(red, Math.Max(green, blue));
            float num2 = Math.Min(red, Math.Min(green, blue));
            float single = Conversions.ToSingle(Interaction.IIf((double)num2 / (double)num1 < 0.5, (object)(float)((double)num2 * (double)num1 / ((double)num1 - (double)num2)), (object)num1));
            float maxValue = (float)byte.MaxValue;
            float num3 = (single + num1) / num1;
            float num4 = (float)Math.Floor(((double)num3 * (double)red - (double)single) / (double)maxValue);
            float num5 = (float)Math.Floor(((double)num3 * (double)green - (double)single) / (double)maxValue);
            float num6 = (float)Math.Floor(((double)num3 * (double)blue - (double)single) / (double)maxValue);
            return toHex(checked((int)Math.Round((double)red))) + toHex(checked((int)Math.Round((double)green))) + toHex(checked((int)Math.Round((double)blue)));
        }

        /// <summary>
        /// </summary>
        /// <param name="hexNumber"></param>
        /// <returns></returns>
        internal decimal ParseHexString(string hexNumber)
        {
            hexNumber = hexNumber.Replace("x", string.Empty);
            long result = 0;
            long.TryParse(hexNumber, System.Globalization.NumberStyles.HexNumber, null, out result);
            return result;
        }

        internal static ColourRGB ConvertToRgb(string HexColor)
        {
            ColourRGB oColorRgb = new ColourRGB();
            HexColor = Strings.Replace(HexColor, "#", "", 1, -1, CompareMethod.Binary);
            oColorRgb.red = checked((byte)Math.Round(Conversion.Val("&H" + Strings.Mid(HexColor, 1, 2))));
            oColorRgb.green = checked((byte)Math.Round(Conversion.Val("&H" + Strings.Mid(HexColor, 3, 2))));
            oColorRgb.blue = checked((byte)Math.Round(Conversion.Val("&H" + Strings.Mid(HexColor, 5, 2))));
            return oColorRgb;
        }

        private static int ByteToPercent(int byteValue)
        {
            if (byteValue > 255)
            {
                byteValue = 255;
            }

            if (byteValue < 0)
            {
                byteValue = 0;
            }
            return (int) ((byteValue * 100) / 255);
        }

        private static int ByteValueCheck(int byteValue)
        {
            if (byteValue > 255)
            {
                byteValue = 255;
            }

            if (byteValue < 0)
            {
                byteValue = 0;
            }
            return (int)byteValue;
        }

        private static int PercentToByte(int pctValue)
        {
            //if (pctValue > 100)
            //{
                //pctValue = 100;
            //}

            //if (pctValue < 0)
            //{
                //pctValue = 0;
            //}
            return (int)((pctValue * 255) / 100);
        }

        private static string toHex(int N)
        {
            return Strings.Right("00" + Conversion.Hex(N), 2);
        }

        private int GetDimLevel(DeviceStatus devStatus)
        {
            var rgbwValues = new List<int>();
            rgbwValues.Add((int)Math.Round((float)devStatus.Red / (float)255.0 * (float)100.0, MidpointRounding.AwayFromZero));
            rgbwValues.Add((int)Math.Round((float)devStatus.Green / (float)255.0 * (float)100.0, MidpointRounding.AwayFromZero));
            rgbwValues.Add((int)Math.Round((float)devStatus.Blue / (float)255.0 * (float)100.0, MidpointRounding.AwayFromZero));
            rgbwValues.Add((int)Math.Round((float)devStatus.White1 / (float)255.0 * (float)100.0, MidpointRounding.AwayFromZero));
            rgbwValues.Add((int)Math.Round((float)devStatus.White2 / (float)255.0 * (float)100.0, MidpointRounding.AwayFromZero));
            return rgbwValues.Max();
        }

        private string GetDeviceId(DeviceClass dev, out string MagicHomeId)
        {
            var str = "";
            MagicHomeId = "";
            var plugExtraDataGet = dev.get_PlugExtraData_Get(MHs);
            if (plugExtraDataGet != null)
            {
                str = (string)plugExtraDataGet.GetNamed("id");
                MagicHomeId = (string)plugExtraDataGet.GetNamed("MagicHome_id");
            }
            return str;
        }

        public string PostBackProc(string page, string data, string user, int userRights)
        {
            return PostBackProcPriv(page, data, user, userRights);
        }

        private string PostBackProcPriv(string page, string data, string user, int userRights)
        {
            var parts = HttpUtility.ParseQueryString(data);
            if (deviceFindResults != null)
            {
                if (deviceFindResults.Length > 0)
                {
                    foreach (var discovery in deviceFindResults)
                    {
                        if (parts["id"] == discovery.MacAddress.ToString())
                        {
                            var devPref = new DevIniSetting
                            {
                                Mac = discovery.MacAddress.ToString(),
                                deviceType = (DeviceType)Enum.Parse(typeof(DeviceType),
                                    parts[discovery.MacAddress.ToString()])
                            };
                            SaveDevPrefToIni(devPref);
                        }
                    }
                }
            }

            

            if (parts["id"] == "timeout")
            {
                SendRecieveTimeout = int.Parse(parts["timeout"]);
            }

            if (parts["id"] == "loglevel")
            {
                LogLevel = parts["loglevel"];
            }

            if (parts["id"] == "loglevel")
            {
                LogLevel = parts["loglevel"];
            }

            if (parts["id"] == "logtofileenabled")
            {
                LogToFile = parts["logtofileenabled"];
                ((PageBuilderAndMenu.clsPageBuilder.clsKeyValueCollection)_mConfigPage.divToUpdate).Add("bodydiv", _mConfigPage.BuildBody());
            }

            if (parts["id"] == "fileloglevel")
            {
                LogToFileLevel = parts["fileloglevel"];
            }

            if (parts["id"] == "retrydiscovery")
            {
                PollDiscoveryAsync(null, null);
            }

            

            return _mConfigPage.postBackProc(page, data, user, userRights);
        }

        public static MagicHomeApp GetInstance()
        {
            if (_sObjSingletonInstance == null)
            {
                lock (SObjLock)
                {
                    if (_sObjSingletonInstance == null)
                        _sObjSingletonInstance = new MagicHomeApp();
                }
            }
            return _sObjSingletonInstance;
        }

        public bool WillShutDown
        {
            get => _mWillShutDown;
            set => _mWillShutDown = value;
        }

        public IAppCallbackAPI HsCallback
        {
            get => MHsCallback;
            set => MHsCallback = value;
        }

        public string Instance
        {
            get => _mInstance;
            set
            {
                _mInstance = value;
                if (_mInstance == "")
                    return;
                _mIniFile = "MagicHome_" + RemoveNonAlphanum(_mInstance) + ".ini";
            }
        }

        public string LogLevel
        {
            get => Logger.LogLevel.ToString();
            set
            {
                Logger.LogLevel = (Logger.ELogLevel)Enum.Parse(typeof(Logger.ELogLevel), value);
                MHs.SaveINISetting("GENERAL", "log_level", Logger.LogLevel.ToString(), IniFile);
                Logger.LogDebug(
                    "Writing the Log Level " + Logger.LogLevel + " to the MagicHome config file " + IniFile);
            }
        }

        public string LogToFile
        {
            get => Logger.LogToFileEnabled.ToString();
            set
            {
                Logger.LogToFileEnabled = value == "checked";
                MHs.SaveINISetting("GENERAL", "log_to_file_enabled", Logger.LogToFileEnabled.ToString(), IniFile);
                Logger.LogDebug(
                    "Writing the Log to File boolean value " + Logger.LogToFileEnabled + " to the MagicHome config file " + IniFile);
            }
        }

        public void SaveDevPrefToIni(DevIniSetting devIni)
        {
                MHs.SaveINISetting("MAGICHOME", devIni.Mac, devIni.deviceType.Description(), IniFile);
                Logger.LogDebug(
                    "Writing the deviceType for " + devIni.Mac + " to the MagicHome config file " + IniFile);
        }

        public DeviceType GetDevPrefFromIni(string mac)
        {
            DeviceType devType;
            try
            {
                devType = (DeviceType) Enum.Parse(typeof(DeviceType), MHs.GetINISetting("MAGICHOME", mac, "", IniFile));
            }
            catch
            {
                devType = DeviceType.Rgb;
            }
            return devType;
        }

        public int SendRecieveTimeout
        {
            get => _mTimeOut;
            set
            {
                _mTimeOut =  value;
                MHs.SaveINISetting("GENERAL", "send_receive_timeout", _mTimeOut.ToString(), IniFile);
                Logger.LogDebug(
                    "Writing the Send/Receive Timeout to " + SendRecieveTimeout + " to the MagicHome config file " + IniFile);
            }
        }

        public string LogToFileLevel
        {
            get => Logger.FileLogLevel.ToString();
            set
            {
                Logger.FileLogLevel = (Logger.ELogLevel)Enum.Parse(typeof(Logger.ELogLevel), value);
                MHs.SaveINISetting("GENERAL", "file_log_level", Logger.FileLogLevel.ToString(), IniFile);
                Logger.LogDebug(
                    "Writing the Log to File Level " + Logger.FileLogLevel + " to the MagicHome config file " + IniFile);
            }
        }

        public static string RemoveNonAlphanum(string str)
        {
            return RemoveNonAlphaNumPriv(str);
        }

        private static string RemoveNonAlphaNumPriv(string str)
        {
            return new Regex("[^a-zA-Z0-9]").Replace(str, "");
        }

        internal DeviceClass FindDeviceById(string id, string MagicHomeId)
        {
            try
            {
                var deviceEnumerator = (clsDeviceEnumeration)MHs.GetDeviceEnumerator();
                if (deviceEnumerator != null)
                {
                    while (!deviceEnumerator.Finished)
                    {
                        var next = deviceEnumerator.GetNext();
                        if (next.get_Interface(null) == "MagicHome" && GetDeviceId(next, out var MagicHomeId1) == id && MagicHomeId1 == MagicHomeId)
                            return next;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("FindDeviceByID: " + ex.Message);
                Logger.LogDebug(ex.ToString());
            }
            return null;
        }

        private void SetDeviceAttributes(DeviceClass dev, string deviceName, bool showValues = false, bool root = false, DeviceClass rootDev = null)
        {
            var deviceTypeInfo =
                new DeviceTypeInfo_m.DeviceTypeInfo { Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In };
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Energy;
            dev.set_DeviceType_Set(MHs, deviceTypeInfo);
            dev.set_Address(MHs, "");
            dev.set_Interface(MHs, "MagicHome");
            dev.set_InterfaceInstance(MHs, _mInstance);
            dev.set_Last_Change(MHs, new DateTime());
            dev.set_Name(MHs, deviceName);
            dev.set_Location(MHs, "MagicHome");
            dev.set_Location2(MHs, "MagicHome");
            dev.set_Device_Type_String(MHs, "MagicHome");
            dev.set_ImageLarge(MHs, "images/MagicHome/MagicHome_500.png");
            dev.set_Image(MHs, "images/MagicHome/MagicHome_32.png");
            dev.MISC_Clear(MHs, Enums.dvMISC.AUTO_VOICE_COMMAND);
            if (showValues)
                dev.MISC_Set(MHs, Enums.dvMISC.SHOW_VALUES);
            if (root)
            {
                dev.set_Relationship(MHs, Enums.eRelationship.Parent_Root);
            }
            else
            {
                dev.set_Relationship(MHs, Enums.eRelationship.Child);
                if (rootDev == null)
                    return;
                rootDev.AssociatedDevice_Add(MHs, dev.get_Ref(MHs));
                dev.AssociatedDevice_ClearAll(MHs);
                dev.AssociatedDevice_Add(MHs, rootDev.get_Ref(MHs));
            }
        }

        private string DecryptCertificate(string input)
        {
            //if (Decrypted) return null;
            var output = PrivateDecryptRuntimeString(input);
            //Decrypted = true;
            return output;
        }

        private string PrivateDecryptRuntimeString(string input)
        {

            var dec = Convert.FromBase64String(input);                                                // create a byte array from encoded data
            var memoryStream = new MemoryStream();                                                          // create new memorystream object
            memoryStream.Write(dec, 0, dec.Length);                                                         // write the decoded data from 0 to the length into the memory stream
            memoryStream.Seek(0, 0);                                                                        // start at 0
            var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);                // create deflate object for memory stream
            var streamReader = new StreamReader(deflateStream);                                             // create a streamreader and pass it our deflated stream
            var zipoutput = streamReader.ReadToEnd();                                                          // read to end of stream and store in output
            var output = DecryptCert(zipoutput, Get256BitHash("MagicHomeApp"));
            return output;
        }

        private string DecryptCert(string input, string pass)
        {
            if (pass != Get256BitHash("MagicHomeApp")) return null;
            var result = PrivDecryptCert(input);
            return result;
        }

        private string PrivDecryptCert(string input)
        {
            try
            {
                var dec = Convert.FromBase64String(input);
                var memoryStream = new MemoryStream();                                                          // create new memorystream object
                memoryStream.Write(dec, 0, dec.Length);                                                         // write the decoded data from 0 to the length into the memory stream
                memoryStream.Seek(0, 0);
                RijndaelManaged prov = Ga();
                RijndaelManaged rmCrypto = new RijndaelManaged { Padding = PaddingMode.ISO10126 };
                CryptoStream cs = new CryptoStream(memoryStream,
                    rmCrypto.CreateDecryptor(prov.Key, prov.IV),
                    CryptoStreamMode.Read);
                MemoryStream fsOut = new MemoryStream();
                int data;
                while ((data = cs.ReadByte()) != -1)
                    fsOut.WriteByte((byte)data);
                fsOut.Close();
                cs.Close();
                string result = Encoding.UTF8.GetString(fsOut.ToArray());
                memoryStream.Close();
                return result;
            }
            catch
            {
                return null;
            }

        }

        private RijndaelManaged Ga()
        {
            RijndaelManaged prov = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 128,
                IV = Gi(),
                Key = Gk()
            };
            return prov;
        }

        private byte[] Gk()
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] pwba = Encoding.Unicode.GetBytes(decp.ToCharArray());
            byte[] shah = sha.ComputeHash(pwba);
            return shah;
        }

        private byte[] Gi()
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] pwba = Encoding.Unicode.GetBytes(Iv.ToCharArray());
            byte[] shah = sha.ComputeHash(pwba);
            byte[] ivend = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                ivend[i] = shah[i];
            }
            return ivend;
        }

        public string Get256BitHash(string value)
        {
            using (var hash = SHA256.Create())
            {
                var result = string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes(value + "trim9876"))
                    .Select(item => item.ToString("x2")));
                return result;
            }
        }

        private Version GetDeviceVersion(DeviceClass dev)
        {
            Version result = (Version)null;
            PlugExtraData.clsPlugExtraData plugExtraDataGet = dev.get_PlugExtraData_Get(this.MHs);
            if (plugExtraDataGet != null)
                Version.TryParse((string)plugExtraDataGet.GetNamed("version"), out result);
            return result;
        }

        private void SetDeviceVersion(DeviceClass dev, Version version)
        {
            PlugExtraData.clsPlugExtraData clsPlugExtraData = dev.get_PlugExtraData_Get(this.MHs) ?? new PlugExtraData.clsPlugExtraData();
            if (!(version != (Version)null))
                return;
            clsPlugExtraData.RemoveNamed(nameof(version));
            clsPlugExtraData.AddNamed(nameof(version), (object)version.ToString());
            dev.set_PlugExtraData_Set(this.MHs, clsPlugExtraData);
        }

        private void SetDeviceID(DeviceClass dev, string id, string MagicHomeId)
        {
            PlugExtraData.clsPlugExtraData clsPlugExtraData = dev.get_PlugExtraData_Get(this.MHs) ?? new PlugExtraData.clsPlugExtraData();
            clsPlugExtraData.AddNamed(nameof(id), (object)id);
            clsPlugExtraData.AddNamed("MagicHome_id", (object)MagicHomeId);
            dev.set_PlugExtraData_Set(this.MHs, clsPlugExtraData);
        }

        internal Device GetDevice(DeviceFindResult discovery)
        {
            try
            {
                Device dev = null;
                var devType = GetDevPrefFromIni(discovery.MacAddress.ToString());
                dev = new Device(IPAddress.Parse(discovery.IpAddress.ToString()),
                    devType);

                /*
                switch (discovery.Model)
                {
                    
                    case "AK001-ZJ100":
                        dev = new Device(IPAddress.Parse(discovery.IpAddress.ToString()),
                            DeviceType.Rgb);
                        break;
                    case "AK001-ZJ200":
                        dev = new Device(IPAddress.Parse(discovery.IpAddress.ToString()),
                            DeviceType.Rgb);
                        break;
                    case "HF-LPB100-ZJ200":
                        dev = new Device(IPAddress.Parse(discovery.IpAddress.ToString()),
                            DeviceType.LegacyBulb);
                        break;
                    default:
                        Logger.LogDebug("The Discovered Device Model " + discovery.Model +
                                        " Is not supported in this plugin, to get support for this model, please use the forum to request support.");
                        break;
                }
                */

                return dev;
            }
            catch
            {
                return null;
            }
        }

        internal class DevIniSetting
        {
            internal string Mac { get; set; }

            internal DeviceType deviceType { get; set;}
        }

        internal class MagicHomeDevices
        {
            internal DeviceClass RootDevice { get; set; }

            internal DeviceClass Mode { get; set; }

            internal DeviceClass Colour { get; set; }

            internal DeviceClass Red { get; set; }

            internal DeviceClass Green { get; set; }

            internal DeviceClass Blue { get; set; }

            internal DeviceClass WarmWhite { get; set; }

            internal DeviceClass CoolWhite { get; set; }

            internal DeviceClass Preset { get; set; }
        }

        internal class ColourRGB
        {
            internal byte red { get; set; }
            internal byte green { get; set; }
            internal byte blue { get; set; }
        }
    }
}