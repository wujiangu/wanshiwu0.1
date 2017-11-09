
-- 创建预加载状态的界面
function CreatePreLoadGui(a_sceneName)
	local m_guiScene = cs.GuiManager.Get():CreateGuiScene(a_sceneName)
	local m_guiTest = m_guiScene:LoadGuiObject("Main/Prefab/Test")

	local m_imgIcon = m_guiTest:GetControl(3):ToImage()
	local m_btnOpen = m_guiTest:GetControl(1):ToButton()	

	local _OnOpenClicked = nil

	_OnOpenClicked = function()
		m_imgIcon:SetSprite("Main/Image/alloyCabinet", true)
	end

	m_guiTest:Show(true)
	m_btnOpen:AddListener(_OnOpenClicked)
end

preLoadGui = CreateGuiScene( "preload", CreatePreLoadGui )

