using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HomeSeerAPI;

namespace HSPI_MagicHome
{
    public class Triggers
    {
        public int TriggerCount
        {
            get
            {
                return 0;
            }
        }

        public string TriggerFormatUI(IPlugInAPI.strTrigActInfo trigInfo)
        {
            if (trigInfo.DataIn == null)
                return "Error, trigInfo.DataIn is null";
            MagicHomeApp.GetInstance();
            // ISSUE: explicit non-virtual call
            var str1 = trigInfo.UID.ToString();
            var str2 = "";
            var queryString = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(trigInfo.DataIn));
            foreach (var allKey in queryString.AllKeys)
            {
                if (allKey.StartsWith("folder_" + str1))
                    str2 = queryString[allKey];
            }
            return "MagicHome Trigger" + "<br>Dropbox folder: " + str2;
        }

        public IPlugInAPI.strMultiReturn TriggerProcessPostUI(NameValueCollection postData, IPlugInAPI.strTrigActInfo trigInfoIN)
        {
            IPlugInAPI.strMultiReturn strMultiReturn;
            strMultiReturn.sResult = "";
            strMultiReturn.DataOut = trigInfoIN.DataIn;
            strMultiReturn.TrigActInfo = trigInfoIN;
            if (postData == null || postData.Count < 1)
                return strMultiReturn;
            var s = "";
            foreach (var allKey in postData.AllKeys)
            {
                if (s != "")
                    s += "&";
                s = s + allKey + "=" + HttpUtility.UrlEncode(postData[allKey]);
            }
            strMultiReturn.DataOut = Encoding.ASCII.GetBytes(s);
            return strMultiReturn;
        }

        public bool TriggerReferencesDevice(IPlugInAPI.strTrigActInfo trigInfo, int dvRef)
        {
            return false;
        }

        public int get_SubTriggerCount(int triggerNumber)
        {
            return 0;
        }

        public string get_SubTriggerName(int triggerNumber, int subTriggerNumber)
        {
            return "";
        }

        public bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo trigInfo)
        {
            if (trigInfo.DataIn == null)
                return false;
            var str1 = trigInfo.UID.ToString();
            var str2 = "";
            var queryString = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(trigInfo.DataIn));
            foreach (var allKey in queryString.AllKeys)
            {
                if (allKey.StartsWith("folder_" + str1))
                    str2 = queryString[allKey];
            }
            return str2 != "";
        }

        public string get_TriggerName(int triggerNumber)
        {
            var instance = MagicHomeApp.GetInstance();
            var str = "MagicHome Trigger";
            if (instance.Instance != "")
                str = str + " - " + instance.Instance;
            return str;
        }
    }
}
