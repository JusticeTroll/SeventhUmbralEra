﻿using System;
using System.Collections.Generic;
using System.IO;

using FFXIVClassic.Common;

namespace  FFXIVClassic_Map_Server.packets.send.actor.inventory
{
    class InventoryRemoveX16Packet
    {
        public const ushort OPCODE = 0x154;
        public const uint PACKET_SIZE = 0x40;

        public static SubPacket BuildPacket(uint playerActorID, List<ushort> slots, ref int listOffset)
        {
            byte[] data = new byte[PACKET_SIZE - 0x20];

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    int max;
                    if (slots.Count - listOffset <= 16)
                        max = slots.Count - listOffset;
                    else
                        max = 16;

                    for (int i = 0; i < max; i++)
                    {
                        binWriter.Write((UInt16)slots[listOffset]);
                        listOffset++;
                    }
                }
            }

            return new SubPacket(OPCODE, playerActorID, data);
        }
    }
}
