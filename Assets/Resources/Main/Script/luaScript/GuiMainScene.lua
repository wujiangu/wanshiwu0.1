-- 主界面UI

function CreateGuiMainScene(a_sceneName)
	local m_guiScene = cs.GuiManager.Get():CreateGuiScene(a_sceneName)
	local m_guiMainScene = m_guiScene:LoadGuiObject("Main/Prefab/GuiMainScene")

	local btn_arr = {}

	local _OnOpen = nil

	local _BtnHandler = nil

	_BtnHandler = function()

	end

	for i = 8, 11 do
		local btn = m_guiMainScene:GetControl(i):ToButton()
		btn:AddListener(_BtnHandler)
		table.insert(btn_arr, btn)
	end

	_OnOpen = function()
		m_guiMainScene:Show()
	end

	RegisterEvent(EEventID.GameStateChanged, _OnOpen)
end

preLoadGui = CreateGuiScene( "login", CreateGuiMainScene )