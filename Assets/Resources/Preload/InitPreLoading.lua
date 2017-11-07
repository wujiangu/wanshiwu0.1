

-- 定义预加载阶段的GameEventID
PreLoadEventTable =
{
	"PreLoadingProgressChanged",
	"PreLoadingNoticeChanged",
}
EEventID.DefineGameEvent(PreLoadEventTable)

dofile("GuiPreLoading.lua")