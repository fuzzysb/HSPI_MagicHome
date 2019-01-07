using System;
using System.Threading;
using HomeSeerAPI;
using Hspi;
using HSCF.Communication.Scs.Communication;
using HSCF.Communication.Scs.Communication.EndPoints.Tcp;
using HSCF.Communication.ScsServices.Client;

namespace HSPI_MagicHome
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hspi = new HSPI();
            var instance = MagicHomeApp.GetInstance();
            //Connector.Connect<HSPI>(args);
            var server = "127.0.0.1";
            var port = 10400;
            var separator = new string[1] { "=" };
            foreach (var str2 in args)
            {

                var lower = args[0].ToLower();
                if (!(lower == "--server"))
                {
                    if (lower == "--instance")
                    {
                        try
                        {
                            instance.Instance = args[1];
                        }
                        catch
                        {
                            instance.Instance = "";
                        }
                    }
                }
                else
                {
                    server = args[1];
                }

                lower = args[2].ToLower();
                if (!(lower == "--port"))
                {
                    if (lower == "--instance")
                    {
                        try
                        {
                            instance.Instance = args[3];
                        }
                        catch
                        {
                            instance.Instance = "";
                        }
                    }
                }
                else
                {
                    port = Int32.Parse(args[3]);
                }
            }
            Console.WriteLine("Plugin: MagicHome Instance: " + instance.Instance + " starting...");
            Console.WriteLine($"Connecting to server at {server} on port {port}...");
            var iscsServiceClient1 = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(server, port), hspi);
            var iscsServiceClient2 = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(server, port), hspi);
            var num1 = 1;
            var num2 = 0.0;
            IHSApplication ihsApplication = null;
            while (num1 < 6)
            {
                if (num2 == 0.0)
                {
                    try
                    {
                        Console.WriteLine("Connection attempt #" + num1.ToString());
                        iscsServiceClient1.Connect();
                        iscsServiceClient2.Connect();
                        ihsApplication = iscsServiceClient1.ServiceProxy;
                        num2 = ihsApplication.APIVersion;
                        instance.HsCallback = iscsServiceClient2.ServiceProxy;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot connect, attempt " + num1.ToString() + ": " + ex.Message);
                        ++num1;
                        if (ex.Message.ToLower().Contains("timeout occured."))
                            Thread.Sleep(5000);
                    }
                }
                else
                    break;
            }
            if (num1 == 6)
            {
                if (iscsServiceClient1 != null)
                {
                    iscsServiceClient1.Dispose();
                    iscsServiceClient1 = null;
                }
                if (iscsServiceClient2 != null)
                {
                    iscsServiceClient2.Dispose();
                    iscsServiceClient2 = null;
                }
                Console.WriteLine("connection failed.");
                Environment.Exit(1);
            }
            try
            {
                ihsApplication.Connect("MagicHome", instance.Instance);
                instance.Hs = ihsApplication;
                hspi.OurInstanceFriendlyName = instance.Instance;
                Console.WriteLine("Connected (HomeSeer API " + num2.ToString() + "). Waiting to be initialized...");
                iscsServiceClient1.Disconnected += new EventHandler(client_Disconnected);
                do
                {
                    Thread.Sleep(10);
                }
                while (iscsServiceClient1.CommunicationState == CommunicationStates.Connected && !instance.WillShutDown);
                Console.WriteLine("Connection lost, exiting");
                iscsServiceClient1.Disconnect();
                iscsServiceClient2.Disconnect();
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect(2):" + ex.Message);
                Thread.Sleep(2000);
                Environment.Exit(1);
            }



        }

        private static void client_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from server - client");
        }
    }
}