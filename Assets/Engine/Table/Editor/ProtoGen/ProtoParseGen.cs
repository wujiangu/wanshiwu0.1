using UnityEngine;
using System;
using google.protobuf;
using System.Text;

public class ProtoParseGen
{
    public sealed class ProtoGenException : Exception
    {
        public ProtoGenException(string file) : base("[ProtoGenException]An error occurred  " + file) { }
    }

    static private google.protobuf.FileDescriptorSet currentSet;

    static EnumDescriptorProto FindEnum(string name, google.protobuf.DescriptorProto message)
    {
        foreach (var cuEnum in message.enum_type)
        {
            if (string.Equals(cuEnum.name, name))
            {
                return cuEnum;
            }
        }

        foreach (var proto in currentSet.file)
        {
            foreach (var cuEnum in proto.enum_type)
            {
                if (string.Equals(cuEnum.name, name))
                {
                    return cuEnum;
                }
            }

            foreach (var current in proto.message_type)
            {
                foreach (var cuEnum in current.enum_type)
                {
                    if (string.Equals(cuEnum.name, name))
                    {
                        return cuEnum;
                    }
                }
            }
        }

        throw new ProtoGenException("Enum " + name + " Can not Find!! PleaseCheck!");
        return null;
    }
    static public void Gen(google.protobuf.FileDescriptorSet set)
    {
        currentSet = set;
        foreach (var proto in set.file)
        {
            foreach (var message in proto.message_type)
            {
                //message.nested_type
                GenMessageParse(message);
            }
        }
        currentSet = null;
    }

    internal static ProtoBuf.DataFormat GetDataFromat(google.protobuf.FieldDescriptorProto.Type type)
    {
        switch (type)
        {
            /*
             TYPE_DOUBLE' or type='TYPE_FLOAT'
                   or type='TYPE_FIXED32' or type='TYPE_FIXED64'
                   or type='TYPE_SFIXED32' or type='TYPE_SFIXED64'">FixedSize</xsl:when>
             */
            case FieldDescriptorProto.Type.TYPE_DOUBLE:
            case FieldDescriptorProto.Type.TYPE_FLOAT:
            case FieldDescriptorProto.Type.TYPE_FIXED32:
            case FieldDescriptorProto.Type.TYPE_FIXED64:
            case FieldDescriptorProto.Type.TYPE_SFIXED32:
            case FieldDescriptorProto.Type.TYPE_SFIXED64:
                return ProtoBuf.DataFormat.FixedSize;

            // <xsl:when test="type='TYPE_GROUP'">Group</xsl:when>
            case FieldDescriptorProto.Type.TYPE_GROUP:
                return ProtoBuf.DataFormat.Group;

            /*
               sl:when test="not(type) or type='TYPE_INT32' or type='TYPE_INT64'
               or type='TYPE_UINT32' or type='TYPE_UINT64'
               or type='TYPE_ENUM'">TwosComplement</xsl:when>
             */
            case FieldDescriptorProto.Type.TYPE_INT32:
            case FieldDescriptorProto.Type.TYPE_INT64:
            case FieldDescriptorProto.Type.TYPE_UINT32:
            case FieldDescriptorProto.Type.TYPE_UINT64:
            case FieldDescriptorProto.Type.TYPE_ENUM:
            default:
                return ProtoBuf.DataFormat.TwosComplement;

            // <xsl:when test="type='TYPE_SINT32' or type='TYPE_SINT64'">ZigZag</xsl:when>
            case FieldDescriptorProto.Type.TYPE_SINT32:
            case FieldDescriptorProto.Type.TYPE_SINT64:
                return ProtoBuf.DataFormat.ZigZag;
        }
    }
    internal static ProtoBuf.WireType GetIntWireType(ProtoBuf.DataFormat format, int width)
    {
        switch (format)
        {
            case ProtoBuf.DataFormat.ZigZag: return ProtoBuf.WireType.SignedVariant;
            case ProtoBuf.DataFormat.FixedSize: return width == 32 ? ProtoBuf.WireType.Fixed32 : ProtoBuf.WireType.Fixed64;
            case ProtoBuf.DataFormat.TwosComplement:
            case ProtoBuf.DataFormat.Default: return ProtoBuf.WireType.Variant;
            default: throw new InvalidOperationException();
        }
    }
 

    internal static ProtoBuf.WireType TryGetWireType(google.protobuf.FieldDescriptorProto.Type type)
    {
        switch (type)
        {
            /*
             else
            { // enum is fine for adding as a meta-type
                defaultWireType = WireType.None;
                return null;
            }
            */
            case FieldDescriptorProto.Type.TYPE_ENUM:
                return ProtoBuf.WireType.Variant;
            case FieldDescriptorProto.Type.TYPE_INT32:
            case FieldDescriptorProto.Type.TYPE_UINT32:
            case FieldDescriptorProto.Type.TYPE_FIXED32:
                return GetIntWireType(GetDataFromat(type), 32);
            case FieldDescriptorProto.Type.TYPE_INT64:
            case FieldDescriptorProto.Type.TYPE_UINT64:
            case FieldDescriptorProto.Type.TYPE_FIXED64:
                return GetIntWireType(GetDataFromat(type), 64);
            case FieldDescriptorProto.Type.TYPE_STRING:
                return ProtoBuf.WireType.String;
            case FieldDescriptorProto.Type.TYPE_SINT32:
            case FieldDescriptorProto.Type.TYPE_SFIXED32:
                return GetIntWireType(GetDataFromat(type),32);
            case FieldDescriptorProto.Type.TYPE_SINT64:
            case FieldDescriptorProto.Type.TYPE_SFIXED64:
                return GetIntWireType(GetDataFromat(type),64);
            
            case FieldDescriptorProto.Type.TYPE_FLOAT:
                return ProtoBuf.WireType.Fixed32;
            case FieldDescriptorProto.Type.TYPE_DOUBLE:
                return ProtoBuf.WireType.Fixed64;
            case FieldDescriptorProto.Type.TYPE_BOOL:
                return ProtoBuf.WireType.Variant;
            //case ProtoTypeCode.DateTime:
            //    return "ProtoBuf.WireType.Variant";
            //case ProtoTypeCode.Decimal:
            //    defaultWireType = WireType.String;
            //    return new DecimalSerializer(model);
            //case FieldDescriptorProto.Type.TYPE_INT32:
            //    defaultWireType = GetIntWireType(dataFormat, 32);
            //    return new ByteSerializer(model);
            //case ProtoTypeCode.SByte:
            //    defaultWireType = GetIntWireType(dataFormat, 32);
            //    return new SByteSerializer(model);
            //case ProtoTypeCode.Char:
            //    defaultWireType = WireType.Variant;
            //    return new CharSerializer(model);
            //case FieldDescriptorProto.Type.TYPE_INT16:
            //    return GetIntWireType(GetDataFromat(type), 32);
            //    return new Int16Serializer(model);
            //case ProtoTypeCode.UInt16:
            //     defaultWireType = GetIntWireType(dataFormat, 32);
            //    return new UInt16Serializer(model);
            //case ProtoTypeCode.TimeSpan:
            //    defaultWireType = GetDateTimeWireType(dataFormat);
            //    return new TimeSpanSerializer(model);
            //case ProtoTypeCode.Guid:
            //   defaultWireType = WireType.String;
            //    return new GuidSerializer(model);
            //ase ProtoTypeCode.Uri:
            //    defaultWireType = WireType.String;
            //    return new StringSerializer(model);
            //case ProtoTypeCode.ByteArray:
            //    defaultWireType = WireType.String;
            //    return new BlobSerializer(model, overwriteList);
            case FieldDescriptorProto.Type.TYPE_MESSAGE:
                return ProtoBuf.WireType.String;
        }
        return ProtoBuf.WireType.Variant;
    }

    static string GetFieldParseByType(google.protobuf.FieldDescriptorProto.Type type, string typeName)
    {
        //ProtoBuf.ProtoReader source;
        switch (type)
        {
            case FieldDescriptorProto.Type.TYPE_DOUBLE:
                return "source.ReadDouble()";
                break;

            case FieldDescriptorProto.Type.TYPE_FLOAT:
                return "source.ReadSingle()";
                break;

            case FieldDescriptorProto.Type.TYPE_INT64:
                return "source.ReadInt64()";
                break;

            case FieldDescriptorProto.Type.TYPE_UINT64:
                return "source.ReadUInt64()";
                break;

            case FieldDescriptorProto.Type.TYPE_INT32:
                return "source.ReadInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_FIXED64:
                return "source.ReadUInt64()";
                break;

            case FieldDescriptorProto.Type.TYPE_FIXED32:
                return "source.ReadUInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_BOOL:
                return "source.ReadBoolean()";
                break;

            case FieldDescriptorProto.Type.TYPE_STRING:
                return "source.ReadString()";
                break;

            case FieldDescriptorProto.Type.TYPE_GROUP:
                throw new ProtoGenException("FieldDescriptorProto.Type.TYPE_GROUP do not Support Now! Please Check!");
                return "";
                break;

            case FieldDescriptorProto.Type.TYPE_BYTES:
                //ProtoReader.AppendBytes(overwriteList ? null : (byte[])value, source);
                return "ProtoReader.AppendBytes(null, source)";
                break;

            case FieldDescriptorProto.Type.TYPE_UINT32:
                return "source.ReadUInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_ENUM:
                //return "";
                /*builder.AppendFormat(@"
                  int wireValue = source.ReadInt32();
                  switch(wireValue){
                  ");
                google.protobuf.EnumDescriptorProto enumProto = FindEnum(typeName,message); 
                foreach(var item in enumProto.value)
                {
                    builder.AppendFormat("case {0} : {1} = {2} break;\n",item.number,fieldName,typeName + item.name);
                }
                builder.AppendFormat("}");
                return "";
                */
                string name = typeName.Substring(1);
                return "(" + name + ")" + "source.ReadInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_SFIXED32:
                return "source.ReadInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_SFIXED64:
                return "source.ReadInt64()";
                break;

            case FieldDescriptorProto.Type.TYPE_SINT32:
                return "source.ReadInt32()";
                break;

            case FieldDescriptorProto.Type.TYPE_SINT64:
                return "source.ReadInt64()";
                break;
        }

        throw new ProtoGenException("Do not Support Now! Please Check!");
        return "";
    }

    static bool NeedsHint(ProtoBuf.WireType wireType)
    {
         return ((int)wireType & ~7) != 0;  
    }

    private static StringBuilder ms_builder = new StringBuilder(4096);

    static void GenMessageParse(google.protobuf.DescriptorProto message)
    {
        ms_builder.Length = 0;
        StringBuilder builder = ms_builder;
        builder.Append(
            @"
    public void Parse(ProtoBuf.ProtoReader source){
        int fieldNumber = 0;
        while ((fieldNumber = source.ReadFieldHeader()) > 0)
        {
            switch (fieldNumber)
            {
                default:
                    source.SkipField();
                    break;
            
    ");
        foreach (var field in message.field)
        {
            /*builder.AppendFormat(
                @"
                        case {0}:",field.number);
                        */
            builder.AppendFormat(@"
            case {0}:   //{1} {2} {3} {4} {5}
                ", field.number, field.name, field.label, field.type, field.type_name, GetDataFromat(field.type));
            if (field.label == google.protobuf.FieldDescriptorProto.Label.LABEL_REPEATED)
            {
                /*if (field.type == google.protobuf.FieldDescriptorProto.Type.TYPE_STRING)
                {
                    builder.AppendFormat(@"
                    SubItemToken {0}token = ProtoReader.StartSubItem(source);
                    while (ProtoReader.HasSubValue(WireType.String, source))
                    {{
                        {0}.Add({1});
                    }}
                    ProtoReader.EndSubItem({0}token, source);
                    break;
                    ",field.name,
                    GetFieldParseByType(field.type, field.type_name));
                }
                else */
                {
                    if (field.type == google.protobuf.FieldDescriptorProto.Type.TYPE_MESSAGE)
                    {
                        builder.AppendFormat(
                  @"    int {1}field = source.FieldNumber;
                    do
                    {{
                        {0} {1}temp = new {0}();
                        ProtoBuf.SubItemToken {1}token = ProtoBuf.ProtoReader.StartSubItem(source); 
                        {1}temp.Parse(source);
                        ProtoBuf.ProtoReader.EndSubItem({1}token, source);
                        {1}.Add({1}temp);
                    }} while (source.TryReadFieldHeader({1}field));
                    break;
                    ", field.type_name.Substring(1), field.name);
                    }
                    else
                    {
                        string temp = GetFieldParseByType(field.type, field.type_name);
                        var wiretype = TryGetWireType(field.type);
                        bool bNeedsHint  = NeedsHint(wiretype);
                        if(bNeedsHint)
                        builder.AppendFormat(
                  @"    int {0}field = source.FieldNumber;
                    do{{
                        source.Hint(ProtoBuf.WireType.{2}); 
                        {0}.Add({1});
                    }} while(source.TryReadFieldHeader({0}field));
                    break;
                    ", field.name,
                        temp,wiretype);
                        else
                          builder.AppendFormat(
                  @"    int {0}field = source.FieldNumber;
                    do{{
                        {0}.Add({1});
                    }} while(source.TryReadFieldHeader({0}field));
                    break;
                    ", field.name,temp);   

                    }

                }
            }
            else
            {
                if (field.type == google.protobuf.FieldDescriptorProto.Type.TYPE_MESSAGE)
                {
                    builder.AppendFormat(@"    {0} = new {1}();", field.name, field.type_name.Substring(1));
                    builder.AppendFormat(@"
                    ProtoBuf.SubItemToken {0}token = ProtoBuf.ProtoReader.StartSubItem(source); 
                    {0}.Parse(source);
                    ProtoBuf.ProtoReader.EndSubItem({0}token, source);
                    break;
                    ", field.name);
                }
                else
                {
                    var wiretype = TryGetWireType(field.type);
                    bool bNeedsHint  = NeedsHint(wiretype);
                    if(bNeedsHint)
                    builder.AppendFormat(
                  @"    source.Hint(ProtoBuf.WireType.{2}); 
                    {0} = {1};
                    break;
                    ", field.name,
                    GetFieldParseByType(field.type, field.type_name),wiretype);
                    else
                    builder.AppendFormat(
                  @"    {0} = {1};
                    break;
                    ", field.name,
                    GetFieldParseByType(field.type, field.type_name));
                }
            }

        }

        builder.Append(@"
            }
        }
    }");
        message.ParseCode = builder.ToString();
        foreach (var nestMsg in message.nested_type)
        {
            GenMessageParse(nestMsg);
        }
    }

    static void GenFieldParse(google.protobuf.FieldDescriptorProto field, StringBuilder builder)
    {

    }
}