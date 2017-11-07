-- È«¾ÖµÄGameEventID

CreateEventIDTable = function()
	local evtids = {
		currentMaxID = 0;
		DefineGameEvent = nil;
	}
	
	evtids.currentMaxID = cs.EventSystem.Get():GetBuildInEventCount()

	evtids.DefineGameEvent = function(evt_names_table)
		for i,v in ipairs(evt_names_table) do
			evtids[v] = i+evtids.currentMaxID
			cs.EventSystem.Get():DefineEvent(i+evtids.currentMaxID, v)
		end
		evtids.currentMaxID = evtids.currentMaxID + #evt_names_table
	end
	
	evtids.Invalid = 0
	evtids.EngineScriptLoaded = 1
	evtids.GameStateChanged = 2

	
	return evtids
end

EEventID = CreateEventIDTable()