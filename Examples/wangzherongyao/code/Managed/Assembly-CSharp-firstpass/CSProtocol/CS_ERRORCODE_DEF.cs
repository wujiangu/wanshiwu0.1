﻿namespace CSProtocol
{
    using System;

    public enum CS_ERRORCODE_DEF
    {
        CS_ERR_APOLLOPAY_FAST = 0x34,
        CS_ERR_BANACNT = 0x21,
        CS_ERR_CHANGE_GUILD_NAME_AUTHORITY = 0x81,
        CS_ERR_CHANGE_NAME_ITEM_NOT_ENOUGH = 0x7f,
        CS_ERR_CHANGE_NAME_TYPE_INVALID = 0x7e,
        CS_ERR_CHAT_CD = 0x89,
        CS_ERR_CHAT_CONTENT = 150,
        CS_ERR_CHAT_DENY = 0x86,
        CS_ERR_CHAT_FRIEND_OFFLINE = 0x8b,
        CS_ERR_CHAT_NOTINGUILD = 0x8d,
        CS_ERR_CHAT_NOTINROOM = 140,
        CS_ERR_CHAT_NOTINTEAM = 0x93,
        CS_ERR_CHAT_REPEAT = 0x8a,
        CS_ERR_CHAT_STATE = 0x94,
        CS_ERR_CHAT_SUBTYPE = 0x95,
        CS_ERR_CHAT_UNLOCK = 0x87,
        CS_ERR_CHATPAY = 0x88,
        CS_ERR_COMMIT_ERR = 8,
        CS_ERR_CONSUME_BAN = 0x33,
        CS_ERR_DB = 0x80,
        CS_ERR_DUPLICATELOGIN = 0x15,
        CS_ERR_ERRCLTVERSION = 0x19,
        CS_ERR_FINSINGLEGAME_FAIL = 4,
        CS_ERR_FRIEND_ADD_FRIEND_DENY = 0x6b,
        CS_ERR_FRIEND_ADD_FRIEND_EXSIST = 0x6d,
        CS_ERR_FRIEND_ADD_FRIEND_SELF = 0x6c,
        CS_ERR_FRIEND_ADD_FRIEND_ZONE = 0x74,
        CS_ERR_FRIEND_AP_FULL = 0x72,
        CS_ERR_FRIEND_DONATE_AP_EXCEED = 0x69,
        CS_ERR_FRIEND_DONATE_REPEATED = 0x71,
        CS_ERR_FRIEND_LOADING = 0x73,
        CS_ERR_FRIEND_NOT_EXSIST = 0x6f,
        CS_ERR_FRIEND_NUM_EXCEED = 0x67,
        CS_ERR_FRIEND_OTHER = 0x75,
        CS_ERR_FRIEND_RECALL_EXCEED = 0x91,
        CS_ERR_FRIEND_RECALL_REPEATED = 0x90,
        CS_ERR_FRIEND_RECALL_TIME_LIMIT = 0x92,
        CS_ERR_FRIEND_RECORD_NOT_EXSIST = 0x66,
        CS_ERR_FRIEND_RECV_AP_EXCEED = 0x6a,
        CS_ERR_FRIEND_REQ_REPEATED = 110,
        CS_ERR_FRIEND_SEND_MAIL = 0x70,
        CS_ERR_FRIEND_TCAPLUS_ERR = 0x65,
        CS_ERR_GAMESVRSHUTDOWN = 0x16,
        CS_ERR_GET_ACNT_DETAIL_INFO_ERR = 0x77,
        CS_ERR_GET_BURNING_PROGRESS_ERR = 11,
        CS_ERR_GET_BURNING_REWARD_ERR = 12,
        CS_ERR_GET_CHAPTER_REWARD_ERR = 9,
        CS_ERR_GET_RANKING_LIST_INVALID_NUMBER_TYPE = 0x76,
        CS_ERR_GETVIDEOFRAPERROR = 40,
        CS_ERR_HASHCHKINVALID = 0x29,
        CS_ERR_INBLACKLIST = 0x18,
        CS_ERR_JOINMULTGAME_FAIL = 3,
        CS_ERR_JOINMULTGAME_INBANTIME = 14,
        CS_ERR_LOGINLIMIT = 0x23,
        CS_ERR_MONTH_WEEK_CARD_EXPIRED = 0x8e,
        CS_ERR_MONTH_WEEK_CARD_REWARD_GOT = 0x8f,
        CS_ERR_NOTINWHITELIST = 0x17,
        CS_ERR_ONLINECHKERRRELOGIN = 0x22,
        CS_ERR_PEER_FRIEND_NUM_EXCEED = 0x68,
        CS_ERR_PROTOCOLERR = 20,
        CS_ERR_QUITMULTGAME_FAIL = 5,
        CS_ERR_QUITSINGLEGAME_FAIL = 10,
        CS_ERR_RECONNLOGICWORLDIDINVALID = 0x10,
        CS_ERR_REGISTER_NAME_DUP_FAIL = 6,
        CS_ERR_REGISTERLIMITOFPERDAY = 0x20,
        CS_ERR_REGISTERLIMITOFTOTAL = 0x24,
        CS_ERR_REGISTERNAME = 30,
        CS_ERR_REQBOOTSINGLEERROR_TOVIDEO = 50,
        CS_ERR_RESET_BURNING_PROGRESS_ERR = 13,
        CS_ERR_ROOMNAME = 0x1f,
        CS_ERR_SHARE_TLOG_BEYOND_TIMELIMIT = 130,
        CS_ERR_SHOULD_REFRESH_TASK = 7,
        CS_ERR_SPECSALE_BALANCE = 0x7c,
        CS_ERR_SPECSALE_EXCEED = 0x7a,
        CS_ERR_SPECSALE_OTHER = 120,
        CS_ERR_SPECSALE_OUTDATE = 0x79,
        CS_ERR_STARTSINGLEGAME_FAIL = 2,
        CS_ERR_SURRENDER_CD = 0x85,
        CS_ERR_SURRENDER_NOT_START = 0x83,
        CS_ERR_SURRENDER_UNVALID_PLAYER = 0x84,
        CS_ERR_SVROAM = 0x13,
        CS_ERR_SYMBOLPAGE_NAME_ILLGEAL = 0x7d,
        CS_ERR_TRANK_INBANTIME = 15,
        CS_ERR_UPDCLT = 0x12,
        CS_ERR_VERSIONUPDKICK = 0x11,
        CS_FAIL = 1,
        CS_SUCCESS = 0
    }
}

