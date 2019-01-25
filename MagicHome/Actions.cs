using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HomeSeerAPI;
using Scheduler;
using MagicHomeAPI;
using Scheduler.Classes;

namespace HSPI_MagicHome
{
    public class Actions
    {

        MagicHomeApp instance = MagicHomeApp.GetInstance();

        private static readonly List<ActionType> s_actionTypeList = new List<ActionType>()
        {
            new ActionType("Set Colour")
        };

        private const string m_pageName = "Events";

        public int ActionCount => 1;

        public string ActionBuildUI(string sUnique, IPlugInAPI.strTrigActInfo actInfo)
        {
            var stringBuilder = new StringBuilder();
            var str1 = actInfo.UID.ToString() + sUnique;
            var actionType = "";
            var str2 = "";
            var str3 = "";
            if (actInfo.DataIn != null)
            {
                var queryString = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(actInfo.DataIn));
                foreach (var allKey in queryString.AllKeys)
                {
                    if (allKey != null)
                    {
                        if (allKey.StartsWith("actiontype_"))
                            actionType = queryString[allKey];
                        else if (allKey.StartsWith("light_"))
                            str2 = queryString[allKey];
                        else if (allKey.StartsWith("colour_"))
                            str3 = queryString[allKey];
                    }
                }
            }
            var actionType1 = s_actionTypeList.Find(actType => actType.Name == actionType);
            var jqDropList1 = new clsJQuery.jqDropList("actiontype_" + str1, m_pageName, true);
            jqDropList1.AddItem("--Please Select--", "-1", false);
            foreach (var actionType2 in s_actionTypeList)
                jqDropList1.AddItem(actionType2.Name, actionType2.Name, actionType2.Name == actionType);
            stringBuilder.Append("Action Type:");
            stringBuilder.Append(jqDropList1.Build());
            if (actionType1 != null)
            {
                var jqDropList2 = new clsJQuery.jqDropList("light_" + str1, m_pageName, true);
                jqDropList2.AddItem("--Please Select--", "-1", false);

                var jqColorPicker3 = new clsJQuery.jqColorPicker("colour_" + str1, m_pageName, 40, "000000");

                if (MagicHomeApp.deviceFindResults != null)
                {
                    if (MagicHomeApp.deviceFindResults.Length > 0)
                    {
                        foreach (var discovery in MagicHomeApp.deviceFindResults)
                        {
                            var deviceClass = instance.FindDeviceById(discovery.MacAddress + " Colour", discovery.MacAddress.ToString());
                            jqDropList2.AddItem(deviceClass.get_Name(instance.Hs), deviceClass.get_Name(instance.Hs), deviceClass.get_Name(instance.Hs) == str2);
                        }
                    }
                }

                

                stringBuilder.Append("<br>Light:");
                stringBuilder.Append(jqDropList2.Build());
                var lightSelection = jqDropList2.items.Find(selected => selected.Name == str2);
                if (lightSelection.Name != null)
                {
                    stringBuilder.Append("<br>Colour:");
                    stringBuilder.Append(jqColorPicker3.Build());
                    var bcolor = new clsJQuery.jqButton("colorbutton", "Submit", m_pageName, true);
                    stringBuilder.Append(bcolor.Build());
                    str3 = jqColorPicker3.color;

                }
                stringBuilder.Append("<br>");
            }
            return stringBuilder.ToString();
        }

        public string get_ActionName(int actionNumber)
        {
            return "MagicHome Action";
        }

        public bool ActionConfigured(IPlugInAPI.strTrigActInfo actInfo)
        {
            
            if (actInfo.DataIn == null)
                return false;
            var act = "";
            var str1 = "";
            var str2 = "";
            var str3 = "";
            var queryString = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(actInfo.DataIn));
            foreach (var allKey in queryString.AllKeys)
            {
                if (allKey != null)
                {
                    if (allKey.StartsWith("actiontype_"))
                        act = queryString[allKey];
                    else if (allKey.StartsWith("light_"))
                        str2 = queryString[allKey];
                    else if (allKey.StartsWith("colour_"))
                        str3 = queryString[allKey];
                }
            }

            if (MagicHomeApp.deviceFindResults != null)
            {
                if (MagicHomeApp.deviceFindResults.Length > 0)
                {
                    foreach (var discovery in MagicHomeApp.deviceFindResults)
                    {
                        var deviceClass = instance.FindDeviceById("root", discovery.MacAddress.ToString());
                        if (deviceClass.get_Name(instance.Hs) == str2)
                        {
                            
                        }

                    }
                }
            }

            var result = s_actionTypeList.Find(actType => actType.Name == act) != null && str1 != "-1" &&
                         !string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3);
            return result;
        }

        public string ActionFormatUI(IPlugInAPI.strTrigActInfo actInfo)
        {
            if (actInfo.DataIn == null)
                return "Error, actInfo.DataIn is null";
            var str1 = "";
            var act = "";
            var key = "";
            var s1 = "";
            var queryString = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(actInfo.DataIn));
            foreach (var allKey in queryString.AllKeys)
            {
                if (allKey != null)
                {
                    if (allKey.StartsWith("actiontype_"))
                        act = queryString[allKey];
                    else if (allKey.StartsWith("light_"))
                        key = queryString[allKey];
                    else if (allKey.StartsWith("colour_"))
                        s1 = queryString[allKey];
                }
            }
            if (s_actionTypeList.Find(actType => actType.Name == act) == null)
                return "Error, cannot find action type " + act;
            string str2;
            if (string.IsNullOrEmpty(instance.Instance))
                str2 = str1 + "MagicHome: " + act;
            else
                str2 = str1 + "MagicHome - " + instance.Instance + ": " + act;
            //var st2 = key;
            
            var result = str2 + "<br>Light=" + HttpUtility.HtmlEncode(key) + "<br>Colour=" + HttpUtility.HtmlEncode(s1);

            return result;
        }

        public bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo actInfo, int dvRef)
        {
            return false;
        }

        public bool HandleAction(IPlugInAPI.strTrigActInfo actInfo)
        {
            ActionType actionType = null;
            var flag = false;
            var light = "";
            var colour = "";

            if (actInfo.DataIn != null)
            {
                var coll = HttpUtility.ParseQueryString(Encoding.ASCII.GetString(actInfo.DataIn));
                foreach (var allKey in coll.AllKeys)
                {
                    var s = allKey;
                    if (s != null)
                    {
                        if (s.StartsWith("actiontype_"))
                            actionType = s_actionTypeList.Find(actType => actType.Name == coll[s]);
                        else if (s.StartsWith("light_"))
                            light = coll[s];
                        else if (allKey.StartsWith("colour_"))
                            colour = coll[s];
                    }
                }
            }
            try
            {
                if (actionType != null)
                {
                    if (instance.regMod == Enums.REGISTRATION_MODES.REG_REGISTERED || instance.regMod == Enums.REGISTRATION_MODES.REG_TRIAL)
                    {
                        if (actionType.Name == "Set Colour")
                        {
                            if(MagicHomeApp.deviceFindResults != null)
                            {
                                if (MagicHomeApp.deviceFindResults.Length > 0)
                                {
                                    foreach (var discovery in MagicHomeApp.deviceFindResults)
                                    {
                                        var hsDev = instance.FindDeviceById(discovery.MacAddress + " Colour", discovery.MacAddress.ToString());
                                        if (light == hsDev.get_Name(instance.Hs))
                                        {
                                            var devdetail = instance.DevDetailsList.Find(x => x.Mac == discovery.MacAddress.ToString());
                                            var devStatus = devdetail.DevStatus;
                                            var colours = MagicHomeApp.ConvertToRgb(colour);
                                            var cwwhite = devStatus.White1;
                                            var ccwhite = devStatus.White2;
                                            devdetail.Dev.SetColor((byte)colours.red, (byte)colours.green, (byte)colours.blue, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhite || devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite || devdetail.Dev._deviceType == DeviceType.LegacyBulb || devdetail.Dev._deviceType == DeviceType.Bulb) ? (byte)cwwhite : (byte?)null, (devdetail.Dev._deviceType == DeviceType.RgbWarmwhiteCoolwhite) ? (byte)ccwhite : (byte?)null, true, true);
                                            devStatus = devdetail.Dev.GetStatus();
                                            instance.UpdateMagicHomeDevice(discovery, devdetail.Dev, devStatus);
                                        }
                                    }
                                }
                            }
                            //SetColor 
                            flag = true;
                        }
                        else
                            flag = false;
                    }
                    else
                    {
                        Logger.LogWarning("The MagicHome Plugin is not registered, or your trial has expired so actions are not allowed. please purchase and register the plugin. if you have just registered the plugin, you will need to restart the plugin.");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Logger.LogDebug(ex.StackTrace);
            }
            return flag;
        }

        public IPlugInAPI.strMultiReturn ActionProcessPostUI(NameValueCollection postData, IPlugInAPI.strTrigActInfo actInfo)
        {
            IPlugInAPI.strMultiReturn strMultiReturn;
            strMultiReturn.sResult = "";
            strMultiReturn.DataOut = actInfo.DataIn;
            strMultiReturn.TrigActInfo = actInfo;
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



        private class ActionType
        {
            public string Name { get; set; }

            public ActionType(string name)
            {
                Name = name;
            }
        }
    }
}
