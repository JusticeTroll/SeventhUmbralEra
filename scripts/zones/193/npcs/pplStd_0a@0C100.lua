function onInstantiate(npc)
	return "/Chara/Npc/Populace/PopulaceStandard", false, false, false, false, false, npc.getActorClassId(), false, false, 0, 1, "TEST";	
end

function onEventStarted(player, npc, triggerName)

	man0l0Quest = getStaticActor("Man0l0");

	if (triggerName == "talkDefault") then
		--player:runEventFunction("delegateEvent", player, man0l0Quest, "processTtrMini002", nil, nil, nil);
		player:runEventFunction("delegateEvent", player, man0l0Quest, "processEvent000_13", nil, nil, nil);
	else
		player:endEvent();
	end
	
end

function onEventUpdate(player, npc)	
	player:endEvent();
end