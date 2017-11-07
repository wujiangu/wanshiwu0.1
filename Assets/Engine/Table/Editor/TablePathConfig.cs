using System;

using System.IO;
using System.Collections.Generic;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

using UnityEngine;
using UnityEditor;

using google.protobuf;
using ProtoBuf;
using System.Reflection;

using System.Xml;
using ProtoBuf.Meta;

using System.Security.Cryptography;
using System.Text;

namespace cs
{
    public class TablePathConfig
    {
        public static string XLS_PATH = "../WSWShare/Table/Xls";

        public static string PROTO_PATH = "../WSWShare/Table/Proto";

        public static string CLINET_CODE_PATH = "Assets/Programs/Table";
        public static string CLIENT_DATA_PATH = "Assets/Resources/Main/Data/Table/";

        public static string SERVER_CODE_PATH = "../WSWServer/Code/Table/";
        public static string SERVER_BIN_DATA_PATH = "../WSWServer/Resources/Table/";
        public static string SERVER_TEXT_DATA_PATH = "../WSWServer/Resources/Table/";

        public const string ProtoGenMacPath = "Engine/Table/Editor/ProtoGen/mac/protoc";
        public const string ProtoGenEXEPath = "Engine/Table/Editor/ProtoGen/protoc.exe";
        public const string CSCodeTemplatePath = "Assets/Engine/Table/Editor/ProtoGen/csharp.xslt";

        public static string CombinePath(string name, DataUnitType type)
        {
            string fullpath = "";
            switch (type)
            {
                case DataUnitType.CODE_CS:
                    fullpath = Path.Combine(CLINET_CODE_PATH, name + ".cs");
                    break;
                case DataUnitType.CODE_CPP:
                    fullpath = SERVER_CODE_PATH;
                    break;
                case DataUnitType.CODE_PROTO:
                    fullpath = Path.Combine(PROTO_PATH, name + ".proto");
                    break;
                case DataUnitType.DATA_BIN:
                    fullpath = Path.Combine(SERVER_BIN_DATA_PATH, name + ".bin");
                    break;
                case DataUnitType.DATA_XLS:
                    fullpath = Path.Combine(XLS_PATH, name + ".xls");
                    break;
                case DataUnitType.DATA_ASSET:
                    fullpath = Path.Combine(CLIENT_DATA_PATH, name + ".asset");
                    break;
                case DataUnitType.DATA_TEXT:
                    fullpath = Path.Combine(SERVER_TEXT_DATA_PATH, name + ".txt");
                    break;
                default:
                    break;
            }

            return fullpath;
        }

        public static string GetMd5Hash(string path)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = null;

            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                data = md5Hash.ComputeHash(fs);
            }

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString().ToLower();
        }
    }
}

