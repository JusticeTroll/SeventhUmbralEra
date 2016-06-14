﻿using System;
using System.Collections.Generic;
using System.IO;

namespace FFXIVClassic_Map_Server.packets.send.Actor.inventory
{
    class InventoryRemoveX32Packet
    {
        public const ushort OPCODE = 0x0155;
        public const uint PACKET_SIZE = 0x60;

        public static SubPacket BuildPacket(uint playerActorID, List<ushort> slots, ref int listOffset)
        {
            byte[] data = new byte[PACKET_SIZE - 0x20];

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    int max;
                    if (slots.Count - listOffset <= 32)
                        max = slots.Count - listOffset;
                    else
                        max = 32;

                    for (int i = 0; i < max; i++)
                    {
                        binWriter.Write((UInt16)slots[listOffset]);
                        listOffset++;
                    }
                }
            }

            return new SubPacket(OPCODE, playerActorID, playerActorID, data);
        }
    }
}
