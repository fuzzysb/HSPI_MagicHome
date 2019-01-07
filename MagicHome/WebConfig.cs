﻿using System;
using System.Text;
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
            stringBuilder.AppendLine(
                    "The MagicHome plugin requires no configuration, all MagicHome Lights will be discovered and devices will be created.");
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
            stringBuilder.AppendLine("</table>");
            return stringBuilder.ToString();
        }
    }
}