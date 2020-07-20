using System.Net.Sockets;

namespace TcpForwarder
{
  public class ForwardStruct
  {
    public Socket SrcSocket { get; set; }
    public Socket DstSocket { get; set; }
    public byte[] Buf { get; set; }

    public ForwardStruct(Socket source, Socket destination)
    {
      SrcSocket = source;
      DstSocket = destination;
      Buf = new byte[8192];
    }
  }
}
