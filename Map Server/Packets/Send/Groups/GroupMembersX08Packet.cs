﻿/*
===========================================================================
Copyright (C) 2015-2019 Project Meteor Dev Team

This file is part of Project Meteor Server.

Project Meteor Server is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Project Meteor Server is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with Project Meteor Server. If not, see <https:www.gnu.org/licenses/>.
===========================================================================
*/

using Meteor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Meteor.Map.packets.send.group
{
    class GroupMembersX08Packet
    {
        public const ushort OPCODE = 0x017F;
        public const uint PACKET_SIZE = 0x1B8;

        public static SubPacket buildPacket(uint playerActorID, uint locationCode, ulong sequenceId, List<GroupMember> entries, ref int offset)
        {
            byte[] data = new byte[PACKET_SIZE - 0x20];

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    //Write List Header
                    binWriter.Write((UInt64)locationCode);
                    binWriter.Write((UInt64)sequenceId);
                    //Write Entries
                    int max = 8;
                    if (entries.Count-offset < 8)
                        max = entries.Count - offset;
                    for (int i = 0; i < max; i++)
                    {
                        binWriter.Seek(0x10 + (0x30 * i), SeekOrigin.Begin);

                        GroupMember entry = entries[i];
                        binWriter.Write((UInt32)entry.actorId);
                        binWriter.Write((Int32)entry.localizedName);
                        binWriter.Write((UInt32)entry.unknown2);
                        binWriter.Write((Byte)(entry.flag1? 1 : 0));
                        binWriter.Write((Byte)(entry.isOnline? 1 : 0));

                        binWriter.Write(Encoding.ASCII.GetBytes(entry.name), 0, Encoding.ASCII.GetByteCount(entry.name) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(entry.name));

                        offset++;
                    }
                    //Write Count
                    binWriter.Seek(0x10 + (0x30 * 8), SeekOrigin.Begin);
                    binWriter.Write(max);
                }
            }

            return new SubPacket(OPCODE, playerActorID, data);
        }
    }
}
