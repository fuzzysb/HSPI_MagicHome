using System.Collections.Generic;
using System.Collections.Specialized;
using HomeSeerAPI;
using Hspi;

namespace HSPI_MagicHome
{
    // ReSharper disable once InconsistentNaming
    public class HSPI : HspiBase
    {
        public string OurInstanceFriendlyName;

        private readonly MagicHomeApp _mObjApp = MagicHomeApp.GetInstance();

        public override string InstanceFriendlyName()
        {
            return _mObjApp.Instance;
        }
        
        public override IPlugInAPI.strInterfaceStatus InterfaceStatus()
        {
            return new IPlugInAPI.strInterfaceStatus { intStatus = IPlugInAPI.enumInterfaceStatus.OK };
        }

        public override int Capabilities()
        {
            return (int)Enums.eCapabilities.CA_IO;
        }

        public override int AccessLevel()
        {
            return _mObjApp.AccessLevel();
        }

        public override bool SupportsAddDevice()
        {
            return false;
        }

        public override bool SupportsConfigDevice()
        {
            return false;
        }

        public override bool SupportsConfigDeviceAll()
        {
            return false;
        }

        public override bool SupportsMultipleInstances()
        {
            return false;
        }

        public override bool SupportsMultipleInstancesSingleEXE()
        {
            return false;
        }

        public override bool RaisesGenericCallbacks()
        {
            return false;
        }

        public override void HSEvent(Enums.HSEvent eventType, object[] parms)
        {
            _mObjApp.HsEvent(eventType, parms);
        }

        public override string InitIO(string port)
        {
            return _mObjApp.Init();          
        }

        public override IPlugInAPI.PollResultInfo PollDevice(int deviceId)
        {
            // return the value of a device on demand

            return new IPlugInAPI.PollResultInfo
            {
                Result = IPlugInAPI.enumPollResult.Device_Not_Found,
                Value = 0
            };
        }

        public override void SetIOMulti(List<CAPI.CAPIControl> colSend)
        {
            using (var enumerator = colSend.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current != null && current.ControlType == Enums.CAPIControlType.Button)
                    {
                        this._mObjApp.ButtonPress(current.Label, current.ControlValue, current.Ref);
                    }
                    else if (current != null && current.ControlType == Enums.CAPIControlType.Color_Picker)
                    {
                        this._mObjApp.SetControlValue(current.Ref, (double)_mObjApp.ParseHexString(current.ControlString));
                    }
                    else if (current != null)
                    {
                        if (current.ControlValue > 0)
                        {
                            this._mObjApp.SetControlValue(current.Ref, current.ControlValue);
                        }
                    }
                }
            }
        }

        public override void ShutdownIO()
        {
            // debug
            //HS.WriteLog(Name, "Entering ShutdownIO");

            // shut everything down here
            _mObjApp.Shutdown();

            // let our console wrapper know we are finished
            Shutdown = true;

            // debug
            //HS.WriteLog(Name, "Completed ShutdownIO");
        }

        public override SearchReturn[] Search(string searchString, bool regEx)
        {
            return null;
        }

        public override string ActionBuildUI(string uniqueControlId, IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Actions.ActionBuildUI(uniqueControlId, actionInfo);
        }

        public override bool ActionConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Actions.ActionConfigured(actionInfo);
        }

        public override int ActionCount()
        {
            return _mObjApp.Actions.ActionCount;
        }

        public override string ActionFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Actions.ActionFormatUI(actionInfo);
        }

        public override IPlugInAPI.strMultiReturn ActionProcessPostUI(NameValueCollection postData,
            IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Actions.ActionProcessPostUI(postData, actionInfo);
        }

        public override bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo actionInfo, int deviceId)
        {
            return _mObjApp.Actions.ActionReferencesDevice(actionInfo, deviceId);
        }

        public override string get_ActionName(int actionNumber)
        {
            return _mObjApp.Actions.get_ActionName(actionNumber);
        }

        public override bool get_Condition(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return false;
        }

        public override bool get_HasConditions(int triggerNumber)
        {
            return false;
        }

        public override string TriggerBuildUI(string uniqueControlId, IPlugInAPI.strTrigActInfo triggerInfo)
        {
            return "";
        }

        public override string TriggerFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Triggers.TriggerFormatUI(actionInfo);
        }

        public override IPlugInAPI.strMultiReturn TriggerProcessPostUI(NameValueCollection postData,
            IPlugInAPI.strTrigActInfo trigInfoIn)
        {
            return _mObjApp.Triggers.TriggerProcessPostUI(postData, trigInfoIn);
        }

        public override bool TriggerReferencesDevice(IPlugInAPI.strTrigActInfo actionInfo, int deviceId)
        {
            return _mObjApp.Triggers.TriggerReferencesDevice(actionInfo, deviceId);
        }

        public override bool TriggerTrue(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return false;
        }

        public override int get_SubTriggerCount(int triggerNumber)
        {
            return _mObjApp.Triggers.get_SubTriggerCount(triggerNumber);
        }

        public override string get_SubTriggerName(int triggerNumber, int subTriggerNumber)
        {
            return _mObjApp.Triggers.get_SubTriggerName(triggerNumber, subTriggerNumber);
        }

        public override bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Triggers.get_TriggerConfigured(actionInfo);
        }

        public override string get_TriggerName(int triggerNumber)
        {
            return _mObjApp.Triggers.get_TriggerName(triggerNumber);
        }

        public override bool HandleAction(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return _mObjApp.Actions.HandleAction(actionInfo);
        }

        public override void set_Condition(IPlugInAPI.strTrigActInfo actionInfo, bool value)
        {
        }

        public override void SpeakIn(int deviceId, string txt, bool w, string host)
        {
        }

        public override string GenPage(string link)
        {
            return "";
        }

        public override string PagePut(string data)
        {
            return "";
        }

        public override string GetPagePlugin(string page, string user, int userRights, string queryString)
        {
            return _mObjApp.GetPagePlugin(page, user, userRights, queryString);
        }

        public override string PostBackProc(string page, string data, string user, int userRights)
        {
            return _mObjApp.PostBackProc(page, data, user, userRights);
        }

        public override string ConfigDevice(int deviceId, string user, int userRights, bool newDevice)
        {
            return "";
        }

        public override Enums.ConfigDevicePostReturn ConfigDevicePost(int deviceId, string data, string user, int userRights)
        {
            return Enums.ConfigDevicePostReturn.DoneAndCancel;
        }

        public override object PluginFunction(string functionName, object[] parameters)
        {
            return null;
        }

        public override object PluginPropertyGet(string propertyName, object[] parameters)
        {
            return null;
        }

        public override void PluginPropertySet(string propertyName, object value)
        {
        }

        protected override bool GetHasTriggers()
        {
            return false;
        }

        protected override int GetTriggerCount()
        {
            return _mObjApp.Triggers.TriggerCount;
        }

        protected override string GetName()
        {
            return "MagicHome";
        }

        protected override bool GetHscomPort()
        {
            return false;
        }

        public override void SetDeviceValue(int deviceId, double value, bool trigger = true)
        {
        }
    }
}