﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVClassic_Map_Server.packets.send.search
{
    class RetainerSearchHistoryResult
    {
        public uint timestamp;
        public ushort quanitiy;
        public uint gilCostPerItem;
        public byte numStack;
    }
}