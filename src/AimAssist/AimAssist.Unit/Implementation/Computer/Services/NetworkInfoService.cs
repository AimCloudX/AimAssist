using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class NetworkInfoService
    {
        public Dictionary<string, string> GetIpAddresses()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result["IPv4"] = ip.ToString();
                    }
                    else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result["IPv6"] = ip.ToString();
                    }
                }
            }
            catch
            {
                // 何もしない
            }
            
            return result;
        }

        public string GetMacAddress()
        {
            string macAddress = string.Empty;
            
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up && 
                        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        PhysicalAddress address = adapter.GetPhysicalAddress();
                        byte[] bytes = address.GetAddressBytes();
                        
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            macAddress += bytes[i].ToString("X2");
                            
                            if (i != bytes.Length - 1)
                            {
                                macAddress += "-";
                            }
                        }
                        
                        break;
                    }
                }
            }
            catch
            {
                macAddress = "取得できませんでした";
            }
            
            return macAddress;
        }

        public string GetNetworkStatus()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                string connectionInfo = "";
                
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up && 
                        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        string speed = (adapter.Speed / 1000000).ToString() + " Mbps";
                        string type = adapter.NetworkInterfaceType.ToString();
                        
                        connectionInfo += $"{adapter.Name} ({type}), 速度: {speed}, 状態: {adapter.OperationalStatus}\n";
                    }
                }
                
                return connectionInfo;
            }
            catch (Exception ex)
            {
                return $"接続情報を取得できませんでした: {ex.Message}";
            }
        }
    }
}
