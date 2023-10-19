using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets;

[CsvSerializable]
public partial class PropertyOnlyClass
{
    public bool PropertyBoolean { get; set; }
    public bool? PropertyBooleanNullable { get; set; }
    public byte PropertyByte { get; set; }
    public byte? PropertyByteNullable { get; set; }
    public sbyte PropertySByte { get; set; }
    public sbyte? PropertySByteNullable { get; set; }
    public decimal PropertyDecimal { get; set; }
    public decimal? PropertyDecimalNullable { get; set; }
    public double PropertyDouble { get; set; }
    public double? PropertyDoubleNullable { get; set; }
    public float PropertyFloat { get; set; }
    public float? PropertyFloatNullable { get; set; }
    public short PropertyInt16 { get; set; }
    public short? PropertyInt16Nullable { get; set; }
    public ushort PropertyUInt16 { get; set; }
    public ushort? PropertyUInt16Nullable { get; set; }
    public int PropertyInt32 { get; set; }
    public int? PropertyInt32Nullable { get; set; }
    public uint PropertyUInt32 { get; set; }
    public uint? PropertyUInt32Nullable { get; set; }
    public nint PropertyIntPtr { get; set; }
    public nint? PropertyIntPtrNullable { get; set; }
    public nuint PropertyUIntPtr { get; set; }
    public nuint? PropertyUIntPtrNullable { get; set; }
    public long PropertyInt64 { get; set; }
    public long? PropertyInt64Nullable { get; set; }
    public ulong PropertyUInt64 { get; set; }
    public ulong? PropertyUInt64Nullable { get; set; }
    public char PropertyChar { get; set; }
    public char? PropertyCharNullable { get; set; }
    public string PropertyString { get; set; }
    public string? PropertyStringNullable { get; set; }

    public PropertyOnlyClass(bool @bool = false, bool? boolNullable = null, byte @byte = 0, byte? byteNullable = null, sbyte @sbyte = 0, sbyte? sbyteNullable = null, decimal @decimal = 0, decimal? decimalNullable = null,
        double @double = 0, double? doubleNullable = null, float @float = 0, float? floatNullable = null, short @short = 0, short? shortNullable = null, ushort @ushort = 0, ushort? ushortNullable = null,
        int @int = 0, int? intNullable = null, uint @uint = 0, uint? uintNullable = null, nint @nint = 0, nint? nintNullable = null, nuint @nuint = 0, nuint? nuintNullable = null, long @long = 0, long? longNullable = null,
        ulong @ulong = 0, ulong? ulongNullable = null, char @char = (char)0, char? charNullable = null, string @string = "", string? stringNullable = null)
    {
        PropertyBoolean = @bool;
        PropertyBooleanNullable = boolNullable;
        PropertyByte = @byte;
        PropertyByteNullable = byteNullable;
        PropertySByte = @sbyte;
        PropertySByteNullable = sbyteNullable;
        PropertyDecimal = @decimal;
        PropertyDecimalNullable = decimalNullable;
        PropertyDouble = @double;
        PropertyDoubleNullable = doubleNullable;
        PropertyFloat = @float;
        PropertyFloatNullable = floatNullable;
        PropertyInt16 = @short;
        PropertyInt16Nullable = shortNullable;
        PropertyUInt16 = @ushort;
        PropertyUInt16Nullable = ushortNullable;
        PropertyInt32 = @int;
        PropertyInt32Nullable = intNullable;
        PropertyUInt32 = @uint;
        PropertyUInt32Nullable = uintNullable;
        PropertyIntPtr = @nint;
        PropertyIntPtrNullable = nintNullable;
        PropertyUIntPtr = @nuint;
        PropertyUIntPtrNullable = nuintNullable;
        PropertyInt64 = @long;
        PropertyInt64Nullable = longNullable;
        PropertyUInt64 = @ulong;
        PropertyUInt64Nullable = ulongNullable;
        PropertyChar = @char;
        PropertyCharNullable = charNullable;
        PropertyString = @string;
        PropertyStringNullable = stringNullable;
    }
}

