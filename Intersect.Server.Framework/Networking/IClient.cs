using Intersect.Config;
using Intersect.Core;
using Intersect.Network;
using Intersect.Server.Framework.Database.PlayerData;
using Intersect.Server.Framework.Database.PlayerData.Security;
using Intersect.Server.Framework.Entities;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Networking
{
    public interface IClient : IPacketSender
    {
        long AccountAttempts { get; set; }
        bool Banned { get; set; }
        List<IPlayer> Characters { get; }
        Guid EditorMap { get; set; }
        IPlayer Entity { get; set; }
        long FloodDetects { get; set; }
        bool FloodKicked { get; set; }
        Guid Id { get; }
        bool IsEditor { get; set; }
        long LastPacketDesyncForgiven { get; set; }
        long LastPing { get; set; }
        string Name { get; }
        long PacketCount { get; set; }
        bool PacketFloodDetect { get; set; }
        FloodThreshholds PacketFloodingThreshholds { get; set; }
        bool PacketHandlingQueued { get; set; }
        bool PacketSendingQueued { get; set; }
        long PacketTimer { get; set; }
        long Ping { get; }
        IUserRights Power { get; set; }
        Dictionary<Guid, Tuple<long, int>> SentMaps { get; set; }
        int TimedBufferPacketsRemaining { get; set; }
        long TimeoutMs { get; set; }
        long TotalFloodDetects { get; set; }
        IUser User { get; }

        void Disconnect(string reason = "", bool shutdown = false);
        void FailedAttempt();
        void HandlePackets();
        bool IsConnected();
        void LoadCharacter(IPlayer character);
        void Logout(bool force = false);
        void Pinged();
        void ResetTimeout();
        bool Send(IPacket packet, TransmissionMode mode);
        void SendPackets();
        void SetUser(IUser user);
    }
}