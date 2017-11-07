﻿namespace ResData
{
    using System;

    public enum RES_GLOBAL_CONF_TYPE
    {
        RES_GLOBAL_CONF_BUY_HERO_ID = 70,
        RES_GLOBAL_CONF_DIRECT_BUY_ITEM_CNT = 0x56,
        RES_GLOBAL_CONF_GET_PVE_TASK_REWARD_TASKID = 80,
        RES_GLOBAL_CONF_GET_PVP_TASK_REWARD_TASKID = 0x47,
        RES_GLOBAL_CONF_INIT_COMBAT_ABILITY = 0x45,
        RES_GLOBAL_CONF_MAX_OPEN_ADVENTURE_DIFFICULT = 0x57,
        RES_GLOBAL_CONF_REWARD_MTCH_PLAYER_NUM_SHOW_LIMIT = 0xab,
        RES_GLOBAL_CONF_TEHUIGIFT_1 = 0xac,
        RES_GLOBAL_CONF_TEHUIGIFT_2 = 0xad,
        RES_GLOBAL_CONF_TYPE_1V1_GUIDE_HERO = 0x72,
        RES_GLOBAL_CONF_TYPE_1V1_GUIDE_LEVEL = 0x74,
        RES_GLOBAL_CONF_TYPE_3V3_GUIDE_HERO = 0x90,
        RES_GLOBAL_CONF_TYPE_3V3_GUIDE_LEVEL = 0x75,
        RES_GLOBAL_CONF_TYPE_5V5_GUIDE_HERO = 0x91,
        RES_GLOBAL_CONF_TYPE_5V5_GUIDE_LEVEL = 0x76,
        RES_GLOBAL_CONF_TYPE_ACTIVATE_ADV_VIDEO_LINK = 0x9b,
        RES_GLOBAL_CONF_TYPE_AFKTIME_TO_HANGUP = 0xb0,
        RES_GLOBAL_CONF_TYPE_APBUY_LIMIT = 0x17,
        RES_GLOBAL_CONF_TYPE_APINCVALUE = 2,
        RES_GLOBAL_CONF_TYPE_APINTERVAL = 1,
        RES_GLOBAL_CONF_TYPE_ARENA_FIGHT_CDTIME = 0x31,
        RES_GLOBAL_CONF_TYPE_ARENA_FIGHT_LIMIT = 0x30,
        RES_GLOBAL_CONF_TYPE_ARENA_SETTLE_BEGINTIME = 50,
        RES_GLOBAL_CONF_TYPE_ARENA_SETTLE_ENDTIME = 0x33,
        RES_GLOBAL_CONF_TYPE_ATTACK_LIMIT = 12,
        RES_GLOBAL_CONF_TYPE_AUTO_SINGLE_MATCH_LEVEL_LIMIT = 0x6a,
        RES_GLOBAL_CONF_TYPE_AUTO_SINGLE_MATCH_TIMER = 0x69,
        RES_GLOBAL_CONF_TYPE_BALANCE_LOWLIMIT_LVL = 0x1f,
        RES_GLOBAL_CONF_TYPE_BALANCE_TRANSFORM_LVL = 0x20,
        RES_GLOBAL_CONF_TYPE_BATTLECHAT_CD = 0x62,
        RES_GLOBAL_CONF_TYPE_BURNING_HERO_LV_LIMIT = 0x2d,
        RES_GLOBAL_CONF_TYPE_BURNING_RESET_MAX_NUM = 0x2b,
        RES_GLOBAL_CONF_TYPE_CASTING_GUIDE_HERO = 0x92,
        RES_GLOBAL_CONF_TYPE_CASTING_GUIDE_LEVEL = 0x77,
        RES_GLOBAL_CONF_TYPE_CHAT_DOOR_LEVEL = 0x7f,
        RES_GLOBAL_CONF_TYPE_CHAT_FREE_CNT = 0x7c,
        RES_GLOBAL_CONF_TYPE_CHAT_PRICE_ONCE = 0x7d,
        RES_GLOBAL_CONF_TYPE_CHAT_PRICE_TYPE = 0x7e,
        RES_GLOBAL_CONF_TYPE_CHAT_REPEAT_CNT = 160,
        RES_GLOBAL_CONF_TYPE_COINBUY_LIMIT = 0x16,
        RES_GLOBAL_CONF_TYPE_COINBUY_LVLINC = 0x1c,
        RES_GLOBAL_CONF_TYPE_COINDRAW_CD = 0x13,
        RES_GLOBAL_CONF_TYPE_COINDRAW_FREELIMIT = 0x12,
        RES_GLOBAL_CONF_TYPE_CONTIWIN_REFRESHTIME = 0x63,
        RES_GLOBAL_CONF_TYPE_COUPONS_FIRSTPAY = 60,
        RES_GLOBAL_CONF_TYPE_COUPONS_FIRSTPAY_LIMIT = 0x3e,
        RES_GLOBAL_CONF_TYPE_COUPONS_RENEWVAL = 0x3d,
        RES_GLOBAL_CONF_TYPE_COUPONS_RENEWVAL_LIMIT = 0x3f,
        RES_GLOBAL_CONF_TYPE_COUPONSCOSTCHKPCT = 0x55,
        RES_GLOBAL_CONF_TYPE_CUSTOM_EQUIP_GAME_NUM = 0xa9,
        RES_GLOBAL_CONF_TYPE_CUSTOM_EQUIP_HERO_NUM = 0xa7,
        RES_GLOBAL_CONF_TYPE_CUSTOM_EQUIP_HERO_WIN_RATE_TTH = 0xa6,
        RES_GLOBAL_CONF_TYPE_CUSTOM_EQUIP_WIN_RATE_TTH = 0xa8,
        RES_GLOBAL_CONF_TYPE_DAILY_PVP_COIN_LIMIT = 0x3b,
        RES_GLOBAL_CONF_TYPE_DEFENSE_LIMIT = 14,
        RES_GLOBAL_CONF_TYPE_DISCONNDTIME_TO_ASSIST = 110,
        RES_GLOBAL_CONF_TYPE_DISCONNDTIME_TO_HANGUP = 0xaf,
        RES_GLOBAL_CONF_TYPE_DONATE_FRIEND_COIN_NUM = 0x6c,
        RES_GLOBAL_CONF_TYPE_DRAWHERO_CD = 0x15,
        RES_GLOBAL_CONF_TYPE_DRAWHERO_FREE = 20,
        RES_GLOBAL_CONF_TYPE_DRAWSKIN_CD = 0x35,
        RES_GLOBAL_CONF_TYPE_DRAWSKIN_FREE = 0x34,
        RES_GLOBAL_CONF_TYPE_DRAWSYMBOL_COMMON_CD = 0x7a,
        RES_GLOBAL_CONF_TYPE_DRAWSYMBOL_SENIOR_CD = 0x7b,
        RES_GLOBAL_CONF_TYPE_ENTER_PVP_GUIDE = 0x36,
        RES_GLOBAL_CONF_TYPE_EXP_BAODIAN_ID = 0x43,
        RES_GLOBAL_CONF_TYPE_EXP_BINGFA_ID = 0x41,
        RES_GLOBAL_CONF_TYPE_EXP_JUANZHOU_ID = 0x40,
        RES_GLOBAL_CONF_TYPE_EXP_MIZHUAN_ID = 0x42,
        RES_GLOBAL_CONF_TYPE_EXP_QUANSHU_ID = 0x44,
        RES_GLOBAL_CONF_TYPE_EXPHERO_MAXDAYS = 0x83,
        RES_GLOBAL_CONF_TYPE_EXPSKIN_MAXDAYS = 0x84,
        RES_GLOBAL_CONF_TYPE_FIRSTWIN_CD = 150,
        RES_GLOBAL_CONF_TYPE_FIRSTWIN_PVPCOIN = 0x98,
        RES_GLOBAL_CONF_TYPE_FIRSTWIN_PVPEXP = 0x97,
        RES_GLOBAL_CONF_TYPE_FIRSTWIN_PVPLVL = 0x99,
        RES_GLOBAL_CONF_TYPE_FREC_LVL_GAP = 0x27,
        RES_GLOBAL_CONF_TYPE_GAIN_CHEST = 0xa2,
        RES_GLOBAL_CONF_TYPE_GET_TRANK_LIST_INTERVAL = 0x2f,
        RES_GLOBAL_CONF_TYPE_GUIDELEVEL2_FADE_GIFT_HERO = 0x66,
        RES_GLOBAL_CONF_TYPE_HANGUP_PUNISH_LIGHT = 0x81,
        RES_GLOBAL_CONF_TYPE_HANGUP_PUNISH_SERIOUS = 130,
        RES_GLOBAL_CONF_TYPE_HANGUPTIME_TO_AI = 0x70,
        RES_GLOBAL_CONF_TYPE_HANGUPTIME_TO_ASSIST = 0x6f,
        RES_GLOBAL_CONF_TYPE_HANGUPTIME_TO_WARN = 0x71,
        RES_GLOBAL_CONF_TYPE_HEADIMG_INITID = 170,
        RES_GLOBAL_CONF_TYPE_HERO_EXCHANGE_ID = 0x86,
        RES_GLOBAL_CONF_TYPE_HERO_HEAD_POINTS = 0x6d,
        RES_GLOBAL_CONF_TYPE_HEROPOOLEXP_LIMIT = 0x3a,
        RES_GLOBAL_CONF_TYPE_HEROSKIN_EXCHANGE_ID = 0x87,
        RES_GLOBAL_CONF_TYPE_HORIZON_RADIUS = 0x38,
        RES_GLOBAL_CONF_TYPE_HP_LIMIT = 11,
        RES_GLOBAL_CONF_TYPE_HURT_DOWN_RATE = 0x4c,
        RES_GLOBAL_CONF_TYPE_HURT_DOWN_RATE_LIMIT = 0x4d,
        RES_GLOBAL_CONF_TYPE_HURT_UP_RATE = 0x48,
        RES_GLOBAL_CONF_TYPE_HURT_UP_RATE_LIMIT = 0x49,
        RES_GLOBAL_CONF_TYPE_HURTED_DOWN_RATE = 0x4a,
        RES_GLOBAL_CONF_TYPE_HURTED_DOWN_RATE_LIMIT = 0x4b,
        RES_GLOBAL_CONF_TYPE_HURTED_UP_RATE = 0x4e,
        RES_GLOBAL_CONF_TYPE_HURTED_UP_RATE_LIMIT = 0x4f,
        RES_GLOBAL_CONF_TYPE_INACTIVE_KAVALUE = 0x85,
        RES_GLOBAL_CONF_TYPE_INIT_UNLOCK_SKILLID = 0x9a,
        RES_GLOBAL_CONF_TYPE_INITAP = 4,
        RES_GLOBAL_CONF_TYPE_INITCOIN = 5,
        RES_GLOBAL_CONF_TYPE_INITCOUPONS = 6,
        RES_GLOBAL_CONF_TYPE_INITHERO1 = 7,
        RES_GLOBAL_CONF_TYPE_INITHERO2 = 8,
        RES_GLOBAL_CONF_TYPE_INITHERO3 = 9,
        RES_GLOBAL_CONF_TYPE_INVITE_FRIEND_CD = 0x9c,
        RES_GLOBAL_CONF_TYPE_JUNGLE_GUIDE_HERO = 0x73,
        RES_GLOBAL_CONF_TYPE_JUNGLE_LEVEL = 0x79,
        RES_GLOBAL_CONF_TYPE_LOSE_MVP_SCORE_LIMIT = 0xb1,
        RES_GLOBAL_CONF_TYPE_LUCKYDRAW_REFRESH_TIME = 0x68,
        RES_GLOBAL_CONF_TYPE_LUCKYDRAW_REFRESH_WDAY = 0x67,
        RES_GLOBAL_CONF_TYPE_LVLCHALLENGE_LIMIT = 0x2a,
        RES_GLOBAL_CONF_TYPE_MAX = 0xb3,
        RES_GLOBAL_CONF_TYPE_MAX_DONATEAP_PERDAY = 0x10,
        RES_GLOBAL_CONF_TYPE_MAX_PVETIPS_COUNT = 0x5d,
        RES_GLOBAL_CONF_TYPE_MAX_PVPTIPS_COUNT = 0x5e,
        RES_GLOBAL_CONF_TYPE_MAX_RECVAP_PERDAY = 0x11,
        RES_GLOBAL_CONF_TYPE_MAX_REFRESH_USUALTASKCNT = 0x5c,
        RES_GLOBAL_CONF_TYPE_MAXAP = 3,
        RES_GLOBAL_CONF_TYPE_MONTH_CARD_RENEW_DAY_CNT = 0xa1,
        RES_GLOBAL_CONF_TYPE_MONTH_CARD_TASKID = 0xa3,
        RES_GLOBAL_CONF_TYPE_MOPUP_TICKET_ID = 0x21,
        RES_GLOBAL_CONF_TYPE_MOPUP_TICKET_NUM = 0x22,
        RES_GLOBAL_CONF_TYPE_NEWBIEGIFT_SHOWSEC = 0xae,
        RES_GLOBAL_CONF_TYPE_NULL = 0,
        RES_GLOBAL_CONF_TYPE_OPENBOXBYCOUPONS_LOOPCNT = 0x39,
        RES_GLOBAL_CONF_TYPE_PLAYER_INFO_COMMON_HERO_NUM = 0xb2,
        RES_GLOBAL_CONF_TYPE_PURE_BALANCE_LVL = 0x29,
        RES_GLOBAL_CONF_TYPE_PVE_SKILL1LVLMAX = 0x8e,
        RES_GLOBAL_CONF_TYPE_PVE_SKILL3LVLMAX = 0x8f,
        RES_GLOBAL_CONF_TYPE_PVP_GUIDE_LEVEL_ID = 0x37,
        RES_GLOBAL_CONF_TYPE_PVP_LEVEL_LIMIT = 0x2c,
        RES_GLOBAL_CONF_TYPE_PVPCOIN_DAILYLIMIT = 90,
        RES_GLOBAL_CONF_TYPE_PVPEXP_DAILYLIMIT = 0x5b,
        RES_GLOBAL_CONF_TYPE_QQSUPERVIP_GOLDADDRATIO = 0x54,
        RES_GLOBAL_CONF_TYPE_QQSUPERVIP_LOGINGIFT = 0x52,
        RES_GLOBAL_CONF_TYPE_QQSUPERVIP_REGISTERGIFT = 0x59,
        RES_GLOBAL_CONF_TYPE_QQVIP_GOLDADDRATIO = 0x53,
        RES_GLOBAL_CONF_TYPE_QQVIP_LOGINGIFT = 0x51,
        RES_GLOBAL_CONF_TYPE_QQVIP_REGISTERGIFT = 0x58,
        RES_GLOBAL_CONF_TYPE_RAND_SEL_HERO_GOLD = 0x9d,
        RES_GLOBAL_CONF_TYPE_RANK_FUNCOPEN_NEEDHERONUM = 0x65,
        RES_GLOBAL_CONF_TYPE_RANK_FUNCOPEN_NEEDLV = 100,
        RES_GLOBAL_CONF_TYPE_RECALL_FRIEND = 0x9e,
        RES_GLOBAL_CONF_TYPE_RECALL_ITEM_ID = 0xa5,
        RES_GLOBAL_CONF_TYPE_REFRESHDAY = 10,
        RES_GLOBAL_CONF_TYPE_RESIST_LIMIT = 15,
        RES_GLOBAL_CONF_TYPE_REWARD_LIMIT_REFRESH_TIME = 0x6b,
        RES_GLOBAL_CONF_TYPE_SHARE_TLOG_SPLIT_TIME = 0x88,
        RES_GLOBAL_CONF_TYPE_SKILLPOINTLIMIT = 0x1a,
        RES_GLOBAL_CONF_TYPE_SKILLPOINTREFRESH = 0x18,
        RES_GLOBAL_CONF_TYPE_SPBUY_LIMIT = 0x1b,
        RES_GLOBAL_CONF_TYPE_SPELL_LIMIT = 13,
        RES_GLOBAL_CONF_TYPE_SURRENDER_CD_TIME = 0x8b,
        RES_GLOBAL_CONF_TYPE_SURRENDER_START_TIME = 0x89,
        RES_GLOBAL_CONF_TYPE_SURRENDER_VALID_TIME = 0x8a,
        RES_GLOBAL_CONF_TYPE_TRAIN_LEVEL = 120,
        RES_GLOBAL_CONF_TYPE_TRANK_MAX_RANK_NO = 0x2e,
        RES_GLOBAL_CONF_TYPE_USERNAME_DOOR_LEVEL = 0x80,
        RES_GLOBAL_CONF_TYPE_VICTORYSHARE_PVPLVL = 0x9f,
        RES_GLOBAL_CONF_TYPE_WAIT_CONFIRM_TIME = 0x95,
        RES_GLOBAL_CONF_TYPE_WARM_1V1_SPECIAL_MAP_ID = 0x93,
        RES_GLOBAL_CONF_TYPE_WARM_5V5_SPECIAL_MAP_ID = 0x94,
        RES_GLOBAL_CONF_TYPE_WEEK_CARD_TASKID = 0xa4,
        RES_GLOBAL_CONF_TYPE_WXGAMECENTER_LOGIN_GOLDADDRATIO = 0x8d,
        RES_GLOBAL_CONF_TYPE_WXGAMECENTER_LOGINGIFT = 140
    }
}

