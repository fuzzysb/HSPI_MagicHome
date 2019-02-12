using System;
using System.Reflection;
using HomeSeerAPI;
using MagicHomeAPI;
using Scheduler.Classes;

namespace HSPI_MagicHome
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class MagicHomeApp
    {
        private DeviceClass CreateRootDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, bool isSensor)
        {
            var device = CreateDevice(discovery.MacAddress + " Root Device", "root", discovery.MacAddress.ToString(), true, true, null);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            UpdateRootDeviceVsvgPairs(device, discovery, dev, devStatus, isSensor);
            return device;
        }

        private void UpdateRootDeviceVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devStatus, bool isSensor)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            if (isSensor)
            {
                deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                deviceTypeInfo.Device_Type = 999;
                device.MISC_Set(this.MHs, (Enums.dvMISC)131072);
            }
            else
            {
                deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                deviceTypeInfo.Device_Type =
                    (int) DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Plugin.Root;
            }

            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            int num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-root");
            if (!isSensor)
            {
                this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair((ePairStatusControl)1)
                {
                    PairType = VSVGPairs.VSVGPairType.SingleValue,
                    Value = 0.0,
                    IncludeValues = true,
                    RangeStatusDecimals = 0,
                    RangeStatusPrefix = "",
                    RangeStatusSuffix = "",
                    HasScale = false,
                    ControlUse = ePairControlUse.Not_Specified,
                    HasAdditionalData = false,
                    NewControlStatusUpdateForUtilityOnly = ePairStatusControl.Status
                });
                VSVGPairs.VGPair vgPair = new VSVGPairs.VGPair
                {
                    PairType = VSVGPairs.VSVGPairType.SingleValue,
                    Graphic = "images/HomeSeer/status/nostatus.gif",
                    Set_Value = 0.0
                };
                this.MHs.DeviceVGP_AddPair(num, vgPair);
            }
            else
            {
                this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair((ePairStatusControl)1)
                {
                    PairType = VSVGPairs.VSVGPairType.SingleValue,
                    Value = 0.0,
                    IncludeValues = true,
                    RangeStatusDecimals = 0,
                    RangeStatusPrefix = "",
                    RangeStatusSuffix = "",
                    HasScale = false,
                    ControlUse = ePairControlUse.Not_Specified,
                    HasAdditionalData = false,
                    NewControlStatusUpdateForUtilityOnly = ePairStatusControl.Status
                });
                VSVGPairs.VGPair vgPair = new VSVGPairs.VGPair
                {
                    PairType = VSVGPairs.VSVGPairType.SingleValue,
                    Graphic = "images/HomeSeer/status/nostatus.gif",
                    Set_Value = 0.0
                };
                this.MHs.DeviceVGP_AddPair(num, vgPair);
            }

            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeModeDevice(DeviceFindResult discovery, Device dev , DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress.ToString(),
                discovery.MacAddress.ToString(), discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeModeVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeModeVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus )
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            device.set_Can_Dim(MHs, true);
            device.set_Address(MHs, discovery.MacAddress + "-mode");
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Status)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Unreachable",
                Value = -1,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 0,
                    Column = 0,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Graphic = "images/HomeSeer/status/unknown.png",
                Set_Value = -1
            });

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Off",
                Value = 0,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse._Off,
                Render_Location = new Enums.CAPIControlLocation()
                {
                Row = 1,
                Column = 1,
                ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Graphic = "images/HomeSeer/status/off.gif",
                Set_Value = 0
            });

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 100,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse._Dim,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 2,
                    Column = 1,
                    ColumnSpan = 3
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-00.gif",
                RangeStart = 1,
                RangeEnd = 9
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-10.gif",
                RangeStart = 10,
                RangeEnd = 19
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-20.gif",
                RangeStart = 20,
                RangeEnd = 29
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-30.gif",
                RangeStart = 30,
                RangeEnd = 39
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-40.gif",
                RangeStart = 40,
                RangeEnd = 49
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-50.gif",
                RangeStart = 50,
                RangeEnd = 59
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-60.gif",
                RangeStart = 60,
                RangeEnd = 69
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-70.gif",
                RangeStart = 70,
                RangeEnd = 79
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-80.gif",
                RangeStart = 80,
                RangeEnd = 89
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/dim-90.gif",
                RangeStart = 90,
                RangeEnd = 99
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "On",
                Value = 100,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse._On,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Graphic = "images/HomeSeer/status/on.gif",
                Set_Value = 100
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeColourDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Colour",
                discovery.MacAddress + " Colour", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeColourVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeColourVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-colour");

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Colour Picker",
                Value = -1,
                Render = Enums.CAPIControlType.Color_Picker,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 2,
                    Column = 1,
                    ColumnSpan = 0
                },
                ControlUse = ePairControlUse._ColorControl
            });

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 16777215,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/custom-color.png",
                RangeStart = 1,
                RangeEnd = 16777215
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 16777216,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 16777217,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Value = 16777218,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 0,
                    Column = 0,
                    ColumnSpan = 0
                },
                ControlUse = ePairControlUse._ColorControl
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeRedDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Red",
                discovery.MacAddress + " Red", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeRedVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeRedVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-red");
            
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 255,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/red.png",
                RangeStart = 0,
                RangeEnd = 255
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 256,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 257,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeGreenDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Green",
                discovery.MacAddress + " Green", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeGreenVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeGreenVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-green");

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 255,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/green.png",
                RangeStart = 0,
                RangeEnd = 255
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 256,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 257,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeBlueDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Blue",
                discovery.MacAddress + " Blue", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeBlueVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeBlueVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-blue");

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 255,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/blue.png",
                RangeStart = 0,
                RangeEnd = 255
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 256,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 257,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeWarmWhiteDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Warm White",
                discovery.MacAddress + " Warm White", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeWarmWhiteVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeWarmWhiteVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-warmwhite");

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 255,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/white-warm.png",
                RangeStart = 0,
                RangeEnd = 255
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 256,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 257,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateMagicHomeCoolWhiteDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Cool White",
                discovery.MacAddress + " Cool White", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomeCoolWhiteVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomeCoolWhiteVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-coolwhite");

            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 0,
                RangeEnd = 255,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 1,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/white-cool.png",
                RangeStart = 0,
                RangeEnd = 255
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Up",
                Value = 256,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 3,
                    ColumnSpan = 0
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Control)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Down",
                Value = 257,
                Render = Enums.CAPIControlType.Button,
                ControlUse = ePairControlUse.Not_Specified,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 1,
                    Column = 2,
                    ColumnSpan = 0
                }
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }
        
        private DeviceClass CreateMagicHomePresetDevice(DeviceFindResult discovery, Device dev, DeviceStatus devStatus, DeviceClass rootDev)
        {
            DeviceClass device = this.CreateDevice(discovery.MacAddress + " Preset",
                discovery.MacAddress + " Preset", discovery.MacAddress.ToString(), true, false, rootDev);
            Logger.LogDebug("Existing Device Not Found Creating Device " + discovery.MacAddress + " in HomeSeer");
            this.UpdateMagicHomePresetVsvgPairs(device, discovery, dev, devStatus);
            return device;
        }

        private void UpdateMagicHomePresetVsvgPairs(DeviceClass device, DeviceFindResult discovery, Device dev, DeviceStatus devstatus)
        {
            if (device == null)
                return;
            Logger.LogDebug("Updating Device " + device.get_Name(MHs) + " Value Pairs");
            Version deviceVersion = this.GetDeviceVersion(device);
            if (!(deviceVersion == (Version)null) && !(deviceVersion < Assembly.GetEntryAssembly().GetName().Version))
                return;
            DeviceTypeInfo_m.DeviceTypeInfo deviceTypeInfo = new DeviceTypeInfo_m.DeviceTypeInfo();
            deviceTypeInfo.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            deviceTypeInfo.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.No_API;
            device.set_DeviceType_Set(this.MHs, deviceTypeInfo);
            var num = device.get_Ref(this.MHs);
            device.MISC_Set(MHs, Enums.dvMISC.CONTROL_POPUP);
            this.MHs.DeviceVSP_ClearAll(num, true);
            this.MHs.DeviceVGP_ClearAll(num, true);
            device.set_Address(MHs, discovery.MacAddress + "-preset");
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                RangeStart = 1,
                RangeEnd = 100,
                IncludeValues = true,
                RangeStatusDecimals = 0,
                RangeStatusPrefix = "",
                RangeStatusSuffix = "",
                HasScale = false,
                Render = Enums.CAPIControlType.ValuesRangeSlider,
                ControlUse = ePairControlUse.Not_Specified,
                ValueOffset = 0,
                Render_Location = new Enums.CAPIControlLocation()
                {
                    Row = 2,
                    Column = 1,
                    ColumnSpan = 3
                }
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "RGB Fade",
                Value = 37,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Red Pulse",
                Value = 38,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Green Pulse",
                Value = 39,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Blue Pulse",
                Value = 40,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Yellow Pulse",
                Value = 41,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Cyan Pulse",
                Value = 42,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Violet Pulse",
                Value = 43,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "White Pulse",
                Value = 44,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Red Green Alternate Pulse",
                Value = 45,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Red Blue Alternate Pulse",
                Value = 46,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Green Blue Alternate Pulse",
                Value = 47,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Disco Flash",
                Value = 48,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Red Flash",
                Value = 49,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Green Flash",
                Value = 50,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Blue Flash",
                Value = 51,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Yellow Flash",
                Value = 52,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Cyan Flash",
                Value = 53,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Violet Flash",
                Value = 54,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "White Flash",
                Value = 55,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Colour Change",
                Value = 56,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVSP_AddPair(num, new VSVGPairs.VSPair(ePairStatusControl.Both)
            {
                PairType = VSVGPairs.VSVGPairType.SingleValue,
                Status = "Normal RGB",
                Value = 97,
                Render = Enums.CAPIControlType.Button,
            });
            this.MHs.DeviceVGP_AddPair(num, new VSVGPairs.VGPair()
            {
                PairType = VSVGPairs.VSVGPairType.Range,
                Graphic = "images/HomeSeer/status/alarmheartbeat.png",
                RangeStart = 1,
                RangeEnd = 97
            });
            this.SetDeviceVersion(device, Assembly.GetEntryAssembly().GetName().Version);
        }

        private DeviceClass CreateDevice(string deviceName, string deviceID, string MagicHomeId, bool showValues = false,
            bool root = false, DeviceClass rootDev = null)
        {
            DeviceClass dev = null;
            try
            {
                var num = MHs.NewDeviceRef(deviceName);
                MHs.SaveEventsDevices();
                dev = (DeviceClass)MHs.GetDeviceByRef(num);
                SetDeviceAttributes(dev, deviceName, showValues, root, rootDev);
                SetDeviceID(dev, deviceID, MagicHomeId);
            }
            catch (Exception ex)
            {
                Logger.LogError("while creating device " + deviceName + ": " + ex.Message);
                Logger.LogDebug(ex.ToString());
            }

            return dev;
        }

        private void DeleteDevice(DeviceClass dev)
        {
            if (dev == null)
                return;
            Logger.LogDebug("Deleting Device " + dev.get_Name(MHs));
            this.MHs.DeleteDevice(dev.get_Ref(this.MHs));
        }

    }
}