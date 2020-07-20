using System;
using System.Net;
using System.Net.Sockets;

namespace TcpForwarder
{
  public class SimpleTcpForwarder
  {
    private Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void Start(IPEndPoint lEndPoint, IPEndPoint rEndPoint)
    {
      sock.Bind(lEndPoint);
      sock.Listen(10);

      while (true)
      {
        Socket src = sock.Accept();
        SimpleTcpForwarder dst = new SimpleTcpForwarder();
        ForwardStruct state = new ForwardStruct(src, dst.sock);
        dst.Connect(rEndPoint, src);
        src.BeginReceive(state.Buf, 0, state.Buf.Length, 0, OnDataReceive, state);
      }
    }

    private void Connect(EndPoint srcEndPoint, Socket dstEndPoint)
    {
      var state = new ForwardStruct(sock, dstEndPoint);
      sock.Connect(srcEndPoint);
      sock.BeginReceive(state.Buf, 0, state.Buf.Length, SocketFlags.None, OnDataReceive, state);
    }

    private static void OnDataReceive(IAsyncResult data)

    {
      var state = (ForwardStruct)data.AsyncState;
      try
      {
        var bytesRead = state.SrcSocket.EndReceive(data);
        if (bytesRead > 0)
        {
          state.DstSocket.Send(state.Buf, bytesRead, SocketFlags.None);
          state.SrcSocket.BeginReceive(state.Buf, 0, state.Buf.Length, 0, OnDataReceive, state);
        }
      }
      catch
      {
        state.DstSocket.Close();
        state.SrcSocket.Close();
      }
    }
  }
}
