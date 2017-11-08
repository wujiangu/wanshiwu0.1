-- 主界面UI

function CreateGuiMainScene(a_sceneName)
	local m_guiScene = cs.GuiManager.Get():CreateGuiScene(a_sceneName)
	local m_guiMainScene = m_guiScene:LoadGuiObject("Main/Prefab/GuiMainScene")

	local m_imagePath = "Main/Image/Gui/";
	local btn_arr = {}

	local _OnOpen = nil

	-- 改变按钮内部的图片(多图片组合按钮)
	local function changeImageSprite(id, name)
		local path = m_imagePath..name
		local image = m_guiMainScene:GetControl(id):ToImage()
		image:SetSprite(path)
	end

	-----------------------------------按钮--------------------------------------
	-- 签到按钮
	local _BtnSignIn = m_guiMainScene:GetControl(8):ToButton()
	local _OnBtnSignInHandler = nil
	_OnBtnSignInHandler = function()
		print("点击签到")
	end
	_BtnSignIn:AddListener(_OnBtnSignInHandler)

	-- 活动按钮
	local _BtnActive = m_guiMainScene:GetControl(9):ToButton()
	local _OnBtnActiveHandler = nil
	_OnBtnActiveHandler = function()
		print("点击活动")
	end
	_BtnActive:AddListener(_OnBtnActiveHandler)


	-- 设置按钮
	local _BtnSetting = m_guiMainScene:GetControl(10):ToButton()
	local _OnBtnSettingHandler = nil
	_OnBtnSettingHandler = function()
		print("点击设置")
	end
	_BtnSetting:AddListener(_OnBtnSettingHandler)


	-- 首充按钮
	local _BtnFirCharge = m_guiMainScene:GetControl(11):ToButton()
	local _OnBtnFirChargeHandler = nil
	_OnBtnFirChargeHandler = function()
		print("点击首充")
	end
	_BtnFirCharge:AddListener(_OnBtnFirChargeHandler)



	-- 委托按钮
	local _BtnDelegate = m_guiMainScene:GetControl(12):ToButton()
	local _OnBtnDelegateDownHandler = nil
	local _OnBtnDelegateUpHandler = nil
	_OnBtnDelegateDownHandler = function()
		changeImageSprite(19, "tubiao-5-1")
	end
	_OnBtnDelegateUpHandler = function()
		changeImageSprite(19, "tubiao-5")
	end
	_BtnDelegate:AddMouseDownListener(_OnBtnDelegateDownHandler)
	_BtnDelegate:AddMouseUpListener(_OnBtnDelegateUpHandler)

	-- 员工按钮
	local _BtnEmployee = m_guiMainScene:GetControl(13):ToButton()
	local _OnBtnEmployeeDownHandler = nil
	local _OnBtnEmployeeUpHandler = nil
	_OnBtnEmployeeDownHandler = function()
		changeImageSprite(20, "tubiao-4-1")
	end
	_OnBtnEmployeeUpHandler = function()
		changeImageSprite(20, "tubiao-4")
	end
	_BtnEmployee:AddMouseDownListener(_OnBtnEmployeeDownHandler)
	_BtnEmployee:AddMouseUpListener(_OnBtnEmployeeUpHandler)

	-- 制作按钮
	local _BtnProduct = m_guiMainScene:GetControl(14):ToButton()
	local _OnBtnProductDownHandler = nil
	local _OnBtnProductUpHandler = nil
	_OnBtnProductDownHandler = function()
		changeImageSprite(21, "tubiao-3-1")
	end
	_OnBtnProductUpHandler = function()
		changeImageSprite(21, "tubiao-3")
	end
	_BtnProduct:AddMouseDownListener(_OnBtnProductDownHandler)
	_BtnProduct:AddMouseUpListener(_OnBtnProductUpHandler)

	-- 地图按钮
	local _BtnMap = m_guiMainScene:GetControl(15):ToButton()
	local _OnBtnMapDownHandler = nil
	local _OnBtnMapUpHandler = nil
	_OnBtnMapDownHandler = function()
		changeImageSprite(22, "tubiao-1-1")
	end
	_OnBtnMapUpHandler = function()
		changeImageSprite(22, "tubiao-1")
	end
	_BtnMap:AddMouseDownListener(_OnBtnMapDownHandler)
	_BtnMap:AddMouseUpListener(_OnBtnMapUpHandler)

	-- 建造按钮
	local _BtnBuild = m_guiMainScene:GetControl(16):ToButton()
	local _OnBtnBuildDownHandler = nil
	local _OnBtnBuildUpHandler = nil
	_OnBtnBuildDownHandler = function()
		changeImageSprite(23, "tubiao-2-1")
	end
	_OnBtnBuildUpHandler = function()
		changeImageSprite(23, "tubiao-2")
	end
	_BtnBuild:AddMouseDownListener(_OnBtnBuildDownHandler)
	_BtnBuild:AddMouseUpListener(_OnBtnBuildUpHandler)

	-- 料理按钮
	local _BtnCuisine = m_guiMainScene:GetControl(17):ToButton()
	local function _OnBtnCuisineDownHandler()
		changeImageSprite(24, "tubiao-7-1")
	end
	local function _OnBtnCuisineUpHandler()
		changeImageSprite(24, "tubiao-7")
	end
	_BtnCuisine:AddMouseDownListener(_OnBtnCuisineDownHandler)
	_BtnCuisine:AddMouseUpListener(_OnBtnCuisineUpHandler)

	-- 料理按钮
	local _BtnFind = m_guiMainScene:GetControl(18):ToButton()
	local function _OnBtnFindDownHandler()
		changeImageSprite(25, "tubiao-6-1")
	end
	local function _OnBtnFindUpHandler()
		changeImageSprite(25, "tubiao-6")
	end
	_BtnFind:AddMouseDownListener(_OnBtnFindDownHandler)
	_BtnFind:AddMouseUpListener(_OnBtnFindUpHandler)



	-- 玩家信息按钮
	local _BtnPlayerInfo = m_guiMainScene:GetControl(29):ToButton()
	local function _OnBtnPlayerInfoClick()
		print("进入玩家信息")
	end
	_BtnPlayerInfo:AddListener(_OnBtnPlayerInfoClick)
	-------------------------------------------------------------------------

	-------------------------------文字--------------------------------------
	-- 玩家名字
	local _LabPlayerName = m_guiMainScene:GetControl(30):ToLabel()

	-- 玩家等级
	local _LabLv = m_guiMainScene:GetControl(31):ToLabel()

	-- 日期
	local _LabDate = m_guiMainScene:GetControl(32):ToLabel()

	-- 时间
	local _LabTime = m_guiMainScene:GetControl(33):ToLabel()

	-- 进度条
	local _LabProgress = m_guiMainScene:GetControl(34):ToLabel()

	-- 钻石
	local _LabDiamond = m_guiMainScene:GetControl(35):ToLabel()

	-- 金币
	local _LabMoney = m_guiMainScene:GetControl(36):ToLabel()

	-- 好感度
	local _LabFastic = m_guiMainScene:GetControl(37):ToLabel()


	---------------------------------------------------------------------------

	_OnOpen = function()
		m_guiMainScene:Show()
	end

	RegisterEvent(EEventID.GameStateChanged, _OnOpen)
end

preLoadGui = CreateGuiScene( "login", CreateGuiMainScene )