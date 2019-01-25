namespace MagicHomeAPI
{
    public class DevDetail
    {
        public string Mac { get; set; }
        public DeviceFindResult Discovery { get; set; }
        public Device Dev { get; set; }
        public DeviceStatus DevStatus { get; set; }
    }
}