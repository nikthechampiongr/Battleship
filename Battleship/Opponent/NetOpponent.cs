using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Battleship.Model;

namespace Battleship.Opponent;

public class NetOpponent(Socket conn) : IOpponent
{
    private Socket _conn = conn;

    private const byte ShipBitmask = 0b0111_0000;
    private const byte HitBitmask = 0b0111_1111;
    private const byte HitMsgBitmask = 0b1000_0000;
    private const byte MsgTypeBitmask = 0b0000_1111;
    private const byte ShipShift = 4;

    public async Task SendMessage(OpponentMessage msg)
    {
        byte[] payload = [0];
        byte ship;
        switch (msg)
        {
            case SetupCompleteMessage:
                payload[0] = (byte) MessageType.SetupComplete;
                break;
            case SurrenderMessage surrender:
                payload[0] = (byte) MessageType.Surrender;
                ship = (byte) surrender.SunkShip;
                payload[0] |= (byte) (ship << ShipShift);
                break;
            case HitMissedMessage:
                payload[0] = (byte) MessageType.HitMiss;
                break;
            case ShipHitMessage shipHit:
                payload[0] = (byte) MessageType.HitSuccess;
                ship = shipHit.SunkShip != null ? (byte) shipHit.SunkShip.Value : (byte) 0;
                payload[0] |= (byte) (ship << ShipShift);
                break;
            case HitMessage hit:
                payload[0] = (byte) MessageType.Hit;
                payload[0] |= (byte)(hit.Position);
                break;
            default:
                throw new UnreachableException();
        }
        await _conn.SendAsync(payload);
    }

    public async Task<OpponentMessage> GetMessage()
    {
        var buf = new byte[1];
        var rcv = await _conn.ReceiveAsync(buf);
        if (rcv == 0)
        {
            _conn.Shutdown(SocketShutdown.Both);
            throw new Exception("Connection closed");
        }

        var msg = (buf[0] & HitMsgBitmask) == 0 ? (MessageType) (buf[0] & MsgTypeBitmask) : MessageType.Hit;
        ShipType ship;

        switch (msg)
        {
            case MessageType.SetupComplete:
                return new SetupCompleteMessage();
            case MessageType.Surrender:
                ship = (ShipType) ((buf[0] & ShipBitmask) >> ShipShift);
                return new SurrenderMessage(ship);
            case MessageType.HitMiss:
                return new HitMissedMessage();
            case MessageType.HitSuccess:
                if ((buf[0] & ShipBitmask) == 0)
                    return new ShipHitMessage(null);
                ship = (ShipType) ((buf[0] & ShipBitmask) >> ShipShift);
                return new ShipHitMessage(ship);
            case MessageType.Hit:
                var hit = buf[0] & HitBitmask;
                return new HitMessage(hit);
            default:
                throw new ArgumentOutOfRangeException();
        }

    }
}