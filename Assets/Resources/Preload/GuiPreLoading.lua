
-- 创建预加载状态的界面
function CreatePreLoadGui(a_sceneName)
	local m_guiScene = cs.GuiManager.Get():CreateGuiScene(a_sceneName)
	local m_guiTest = m_guiScene:LoadGuiObject("Main/Prefab/Test")

	local m_imgIcon = m_guiTest:GetControl(3):ToImage()
	local m_btnOpen = m_guiTest:GetControl(1):ToButton()	

	local _OnOpenClicked = nil
	local _OnGameStateChanged = nil
	local _OnOpen = nil
	local _OnHide = nil

	_OnOpenClicked = function()
		m_imgIcon:SetSprite("Main/Image/alloyCabinet")
	end
	
	_OnGameStateChanged = function(var1, var2)
		
	end
	
	_OnOpen = function()
		m_guiTest:Show()
	end

	_OnHide = function()
		m_guiTest:Hide()
	end

	
	m_btnOpen:AddListener(_OnOpenClicked)
	
	RegisterEvent(EEventID.GameStateChanged, _OnGameStateChanged)
	RegisterEvent(EEventID.GameStateChanged, _OnOpen)
	RegisterEvent(EEventID.GameStateChanged, _OnHide)
end

preLoadGui = CreateGuiScene( "preload", CreatePreLoadGui )

