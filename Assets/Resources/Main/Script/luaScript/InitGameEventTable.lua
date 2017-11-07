--[[
	定义进入主界面状态的游戏事件
    事件命名规则：
    1、首字母按(a,b,c等)排序；
    2、命名按照类型+功能(例如：打开/关闭背包----->PackageOpen/PackageClose);
]]
GameEventTable = {
	-- 进入活动界面
	"EnterGuiActivity",
	-- 进入建造界面
	"EnterGuiBuild",
	-- 进入料理界面
	"EnterCuisine",
	-- 进入委托界面
	"EnterGuiDelegate",
	-- 进入员工界面
	"EnterGuiEmployee",
	-- 进入首充界面
	"EnterGuiFirstCharge",
	-- 进入查看界面
	"EnterGuiLookover",
	-- 查看玩家信息界面
	"EnterGuiPlayerInfo",
	-- 进入制作界面
	"EnterGuiProduction",
	-- 进入设置界面
	"EnterGuiSetting",
	-- 进入签到界面
	"EnterGuiSignIn",

	-- 进入地图
	"EnterMap",

	-- 进入主界面
	"EnterMainScene",
}

EEventID.DefineGameEvent(GameEventTable)

print("进入加载游戏资源loading界面");
dofile("../Main/Script/luaScript/GuiEnterGameLoading.lua");