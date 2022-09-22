
using Sosodotnet;

namespace VOIPService
{
    public class VOIPServer
    {
        private int ListenerId;

        public VOIPServer()
        {
            NetworkManager.PacketReceived += OnPacketReceived;
            ListenerId = NetworkManager.CreateListenerTCP(6969);
        }

        public IEnumerable<int> GetConnections()
        {
            return NetworkManager.Listeners[ListenerId].Connections;
        }

        private void OnPacketReceived(Packet packet)
        {
            NetworkManager.BroadcastPacket(ListenerId, packet);
        }
    }
}
