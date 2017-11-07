-- 登陆成功后，进入游戏需要加载的资源loading界面


-- 加载资源
local function loadResources()
	
	dofile("../Main/Script/luaScript/GuiMainScene.lua");
end

-- 创建资源加载界面
local function CreateGuiEnterGameLoading()
	

	local _OnHide = nil;

	_OnHide = function()
		
	end

	loadResources();
end

CreateGuiEnterGameLoading();
-- enterGameLoadingGui = CreateGuiScene("login", )