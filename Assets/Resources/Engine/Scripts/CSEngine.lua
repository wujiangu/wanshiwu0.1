-- 游戏程序刚启动时，必须加载的脚本
-- 都是同步加载，所以这里一定只放显示一个界面所必要的脚本

dofile("GameEventTable.lua")
dofile("Utility.lua")

SendEvent(EEventID.EngineScriptLoaded)