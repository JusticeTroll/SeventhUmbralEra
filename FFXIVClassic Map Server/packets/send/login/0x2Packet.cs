﻿using System.IO;

namespace FFXIVClassic_Map_Server.packets.send.login
{
    class _0x2Packet
    {
        public const ushort OPCODE = 0x0002;
        public const uint PACKET_SIZE = 0x30;

        public static SubPacket BuildPacket(uint playerActorID)
        {
            byte[] data = new byte[PACKET_SIZE-0x20];

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    binWriter.BaseStream.Seek(0x8, SeekOrigin.Begin);
                    binWriter.Write((uint)playerActorID);
                }
            }

            return new SubPacket(OPCODE, playerActorID, playerActorID, data);
        }
    }
}
