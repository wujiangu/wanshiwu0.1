//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: CuisineTable.proto
namespace ProtoTable
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CuisineTable")]
  public partial class CuisineTable : global::ProtoBuf.IExtensible,global::ProtoBuf.IParseable
  {
    public CuisineTable() {}
    
    private int _ID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ID", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
    public int ID
    {
      get { return _ID; }
      set { _ID = value; }
    }
    private int _Type;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Type", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
    public int Type
    {
      get { return _Type; }
      set { _Type = value; }
    }
    public void Parse(ProtoBuf.ProtoReader source){
        int fieldNumber = 0;
        while ((fieldNumber = source.ReadFieldHeader()) > 0)
        {
            switch (fieldNumber)
            {
                default:
                    source.SkipField();
                    break;
            
    
            case 1:   //ID LABEL_REQUIRED TYPE_SINT32  ZigZag
                    source.Hint(ProtoBuf.WireType.SignedVariant); 
                    ID = source.ReadInt32();
                    break;
                    
            case 2:   //Type LABEL_REQUIRED TYPE_SINT32  ZigZag
                    source.Hint(ProtoBuf.WireType.SignedVariant); 
                    Type = source.ReadInt32();
                    break;
                    
            }
        }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}