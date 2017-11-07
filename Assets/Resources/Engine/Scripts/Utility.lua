
-- ´´½¨GuiScene
function CreateGuiScene( val, fn )
	cs.GuiManager.Get():DestroyGuiScene(val)
	local scene = fn()
	return scene
end

function RegisterEvent(a_event, a_callback)
	cs.EventSystem.Get():RegisterEventHandler(a_event, a_callback)
end

function UnRegisterEvent(a_event, a_callback)
	cs.EventSystem.Get():UnRegisterEventHandler(a_event, a_callback)
end

function SendEvent(a_event, a_param1, a_param2, a_param3, a_param4)
	if a_param1 == nil then
		cs.EventSystem.Get():SendEvent(a_event)
	elseif a_param2 == nil then
		cs.EventSystem.Get():SendEvent(a_event, a_param1)
	elseif a_param3 == nil then
		cs.EventSystem.Get():SendEvent(a_event, a_param1, a_param2)
	elseif a_param4 == nil then
		cs.EventSystem.Get():SendEvent(a_event, a_param1, a_param2, a_param3)
	else
		cs.EventSystem.Get():SendEvent(a_event, a_param1, a_param2, a_param3, a_param4)
	end
end