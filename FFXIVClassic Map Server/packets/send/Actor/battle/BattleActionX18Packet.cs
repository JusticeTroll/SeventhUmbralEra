﻿using FFXIVClassic.Common;
using System;
using System.IO;

using FFXIVClassic.Common;

namespace  FFXIVClassic_Map_Server.packets.send.actor.battle
{
    class BattleActionX18Packet
    {
        public const ushort OPCODE = 0x013B;
        public const uint PACKET_SIZE = 0x148;

        public static SubPacket BuildPacket(uint playerActorID, uint sourceActorId, uint animationId, ushort commandId, BattleAction[] actionList)
        {
            byte[] data = new byte[PACKET_SIZE - 0x20];

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    binWriter.Write((UInt32)sourceActorId);
                    binWriter.Write((UInt32)animationId);

                    //Missing... last value is float, string in here as well?

                    binWriter.Seek(0x20, SeekOrigin.Begin);
                    binWriter.Write((UInt32) actionList.Length); //Num actions (always 1 for this)
                    binWriter.Write((UInt16)commandId);
                    binWriter.Write((UInt16)810); //?

                    binWriter.Seek(0x58, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((UInt32)action.targetId);

                    binWriter.Seek(0xA0, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((UInt16)action.amount);

                    binWriter.Seek(0xC4, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((UInt16)action.worldMasterTextId);

                    binWriter.Seek(0xE8, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((UInt32)action.effectId);

                    binWriter.Seek(0x130, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((Byte)action.param);

                    binWriter.Seek(0x142, SeekOrigin.Begin);
                    foreach (BattleAction action in actionList)
                        binWriter.Write((Byte)action.unknown);
                }
            }

            return new SubPacket(OPCODE, sourceActorId, data);
        }
    }
}
