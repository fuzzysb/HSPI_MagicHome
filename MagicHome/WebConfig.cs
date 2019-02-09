using System;
using System.Collections.Generic;
using System.Text;
using MagicHomeAPI;
using Scheduler;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralsWordIsNotInDictionary

namespace HSPI_MagicHome
{
    /// <summary>
    /// </summary>
    public class WebConfig : PageBuilderAndMenu.clsPageBuilder
    {
        private MagicHomeApp m_objApp = MagicHomeApp.GetInstance();

        public WebConfig(string pageName) : base(pageName)
        {
        }

        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            return GetPagePluginPriv(pageName, user, userRights, queryString);
        }

        private string GetPagePluginPriv(string pageName, string user, int userRights, string queryString)
        {
            try
            {
                var hs = m_objApp.Hs;
                var stringBuilder = new StringBuilder();
                reset();
                AddHeader(hs.GetPageHeader(pageName, "MagicHome Plugin", "", "", false, true, false, false, false));
                stringBuilder.AppendLine("<table class='full_width_table' cellspacing='0' width='100%' >");
                stringBuilder.AppendLine("<tr><td  colspan='1' >");
                stringBuilder.AppendLine(DivStart("bodydiv", ""));
                stringBuilder.AppendLine(BuildBody());
                stringBuilder.AppendLine(DivEnd());
                stringBuilder.AppendLine("</td></tr></table>");
                AddBody(stringBuilder.ToString());
                AddFooter(hs.GetPageFooter(false));
                suppressDefaultFooter = true;
                return BuildPage();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Logger.LogDebug(ex.ToString());
                return "Error";
            }
        }

        public string BuildBody()
        {
            return BuildBodyPriv();
        }

        private string BuildBodyPriv()
        {
            var hs = m_objApp.Hs;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table cellspacing='0' cellpadding='5'  width='100%'><tr>");
            stringBuilder.AppendLine("<tr><td class='tableheader' colspan='7'>MagicHome settings</td>");
            stringBuilder.AppendLine("</tr>");
            if (MagicHomeApp.deviceFindResults == null)
            {
                stringBuilder.AppendLine("<td class='tablecell' colspan='7'>");
                stringBuilder.AppendLine(
                    "No devices found, or the plugin has not completed the initial discovery, please view this page once the initial discovery has been completed to set device options.");
                stringBuilder.AppendLine("</td>");
            }
            else
            {
                if (MagicHomeApp.deviceFindResults.Length < 1)
                {
                    stringBuilder.AppendLine("<td class='tablecell' colspan='7'>");
                    stringBuilder.AppendLine(
                        "No devices found, or the plugin has not completed the initial discovery, please view this page once the initial discovery has been completed to set device options.");
                    stringBuilder.AppendLine("</td>");
                }
                else
                {
                    List<clsJQuery.jqDropList> deviceDropDowns = new List<clsJQuery.jqDropList>();
                    foreach (var discovery in MagicHomeApp.deviceFindResults)
                    {
                        stringBuilder.AppendLine("<tr><td class='tablecell' align='left' style='width:200px'>");
                        stringBuilder.AppendLine("Device: " + discovery.MacAddress);
                        stringBuilder.AppendLine("</td>");
                        stringBuilder.AppendLine("<td class='tablecell' align='left'>");
                        deviceDropDowns.Add(new clsJQuery.jqDropList(discovery.MacAddress.ToString(), PageName, false));
                        var dropvalues = Enum.GetValues(typeof(DeviceType));
                        foreach (DeviceType deviceType in dropvalues)
                        {
                            deviceDropDowns.Find(x => x.name == discovery.MacAddress.ToString()).AddItem(deviceType.ToString(), ((int)deviceType).ToString(),
                                deviceType.Description() == hs.GetINISetting("MAGICHOME", discovery.MacAddress.ToString(), "", m_objApp.IniFile));
                        }
                        stringBuilder.AppendLine(deviceDropDowns.Find(x => x.name == discovery.MacAddress.ToString()).Build());
                        stringBuilder.AppendLine("</td></tr>");

                    }
                }
            }

            stringBuilder.AppendLine("<tr><td class='tableheader' colspan='7'>Network Settings</td>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<td class='tablecell' align='left' style='width:200px'>");
            stringBuilder.AppendLine("Send/Receive Timeout");
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("<td class='tablecell' align='left'>");
            var jqDropListt = new clsJQuery.jqDropList("timeout", PageName, false);
            var tvalues = new int[] {100, 200, 500, 1000, 2000, 5000};
            foreach (var tOut in tvalues)
            {
                jqDropListt.AddItem(tOut.ToString(), tOut.ToString(),
                    tOut == m_objApp.SendRecieveTimeout);
            }
            stringBuilder.AppendLine(jqDropListt.Build());
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr><td class='tableheader' colspan='7'>Log settings</td>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<td class='tablecell' align='left' style='width:200px'>");
            stringBuilder.AppendLine("Log Level");
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("<td class='tablecell' align='left'>");
            var jqDropList2 = new clsJQuery.jqDropList("loglevel", PageName, false);
            var values = Enum.GetValues(typeof(Logger.ELogLevel));
            foreach (Logger.ELogLevel elogLevel in values)
                jqDropList2.AddItem(elogLevel.ToString(), ((int)elogLevel).ToString(),
                    elogLevel == Logger.LogLevel);
            stringBuilder.AppendLine(jqDropList2.Build());
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("</tr>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine(
                "<td class='tablecell' style='width:200px'>Log to File</td><td class='tablecell'>");
            stringBuilder.AppendLine(
                new clsJQuery.jqCheckBox("logtofileenabled", "", PageName, true, false)
                {
                    @checked = (Logger.LogToFileEnabled ? true : false)
                }.Build());
            stringBuilder.AppendLine("</td></tr>");
            if (Logger.LogToFileEnabled)
            {
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine("<td class='tablecell'>File Log Level</td><td class='tablecell'>");
                var jqDropList3 =
                    new clsJQuery.jqDropList("fileloglevel", PageName, false);
                foreach (Logger.ELogLevel elogLevel in values)
                    jqDropList3.AddItem(elogLevel.ToString(), ((int)elogLevel).ToString(),
                        elogLevel == Logger.FileLogLevel);
                stringBuilder.AppendLine(jqDropList3.Build());
                stringBuilder.AppendLine("</td></tr>");
            }
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("<tr><td class='tableheader' colspan='7'>Light Discovery</td>");
            stringBuilder.AppendLine("<tr>");
            stringBuilder.AppendLine("<td class='tablecell' align='left' colspan='7'>");
            var jqButton = new clsJQuery.jqButton("retrydiscovery", "Retry Discovery",
                PageName, false);
            stringBuilder.AppendLine(jqButton.Build());
            stringBuilder.AppendLine("</td>");
            stringBuilder.AppendLine("</table>");
            return stringBuilder.ToString();
        }
    }
}