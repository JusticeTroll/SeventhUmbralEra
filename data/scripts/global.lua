--[[

Globals referenced in all of the lua scripts

--]]

-- ACTOR STATES

ACTORSTATE_PASSIVE = 0;
ACTORSTATE_DEAD1 = 1;
ACTORSTATE_ACTIVE = 2;
ACTORSTATE_DEAD2 = 3;
ACTORSTATE_SITTING_ONOBJ = 11;
ACTORSTATE_SITTING_ONFLOOR = 13;
ACTORSTATE_MOUNTED = 15;


-- MESSAGE
MESSAGE_TYPE_NONE       = 0;
MESSAGE_TYPE_SAY        = 1;
MESSAGE_TYPE_SHOUT      = 2;
MESSAGE_TYPE_TELL       = 3;
MESSAGE_TYPE_PARTY      = 4;
MESSAGE_TYPE_LINKSHELL1 = 5;
MESSAGE_TYPE_LINKSHELL2 = 6;
MESSAGE_TYPE_LINKSHELL3 = 7;
MESSAGE_TYPE_LINKSHELL4 = 8;
MESSAGE_TYPE_LINKSHELL5 = 9;
MESSAGE_TYPE_LINKSHELL6 = 10;
MESSAGE_TYPE_LINKSHELL7 = 11;
MESSAGE_TYPE_LINKSHELL8 = 12;

MESSAGE_TYPE_SAY_SPAM       = 22;
MESSAGE_TYPE_SHOUT_SPAM     = 23;
MESSAGE_TYPE_TELL_SPAM      = 24;
MESSAGE_TYPE_CUSTOM_EMOTE   = 25;
MESSAGE_TYPE_EMOTE_SPAM     = 26;
MESSAGE_TYPE_STANDARD_EMOTE = 27;
MESSAGE_TYPE_URGENT_MESSAGE = 28;
MESSAGE_TYPE_GENERAL_INFO   = 29;
MESSAGE_TYPE_SYSTEM         = 32;
MESSAGE_TYPE_SYSTEM_ERROR   = 33;

-- INVENTORY
INVENTORY_NORMAL = 0x0000; --Max 0xC8
INVENTORY_LOOT = 0x0004; --Max 0xA
INVENTORY_MELDREQUEST = 0x0005; --Max 0x04
INVENTORY_BAZAAR = 0x0007; --Max 0x0A
INVENTORY_CURRENCY = 0x0063; --Max 0x140
INVENTORY_KEYITEMS = 0x0064; --Max 0x500
INVENTORY_EQUIPMENT = 0x00FE; --Max 0x23
INVENTORY_EQUIPMENT_OTHERPLAYER = 0x00F9; --Max 0x23

-- CHOCOBO APPEARANCE
CHOCOBO_NORMAL = 0;

CHOCOBO_LIMSA1 = 0x1;
CHOCOBO_LIMSA2 = 0x2;
CHOCOBO_LIMSA3 = 0x3;
CHOCOBO_LIMSA4 = 0x4;

CHOCOBO_GRIDANIA1 = 0x1F;
CHOCOBO_GRIDANIA2 = 0x20;
CHOCOBO_GRIDANIA3 = 0x21;
CHOCOBO_GRIDANIA4 = 0x22;

CHOCOBO_ULDAH1 = 0x3D;
CHOCOBO_ULDAH2 = 0x3E;
CHOCOBO_ULDAH3 = 0x3F;
CHOCOBO_ULDAH4 = 0x40;

--UTILS

function kickEventContinue(player, actor, trigger, ...)
	player:kickEvent(actor, trigger, ...);
	return coroutine.yield("_WAIT_EVENT", player);
end

function callClientFunction(player, functionName, ...)
	player:RunEventFunction(functionName, ...);	
	result = coroutine.yield("_WAIT_EVENT", player);
	return result;
end

function wait(seconds)
	return coroutine.yield("_WAIT_TIME", seconds);
end

function waitForSignal(signal)
	return coroutine.yield("_WAIT_SIGNAL", signal);
end

function sendSignal(signal)
	GetLuaInstance():OnSignal(signal);
end

function printf(s, ...)
    if ... then
        print(s:format(...));
    else
        print(s);
    end;
end;