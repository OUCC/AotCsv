using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets;

[CsvSerializable]
public partial class FieldOnlyClass
{
    public bool FieldBoolean;
    public bool? FieldBooleanNullable;
    public byte FieldByte;
    public byte? FieldByteNullable;
    public sbyte FieldSByte;
    public sbyte? FieldSByteNullable;
    public decimal FieldDecimal;
    public decimal? FieldDecimalNullable;
    public double FieldDouble;
    public double? FieldDoubleNullable;
    public float FieldFloat;
    public float? FieldFloatNullable;
    public short FieldInt16;
    public short? FieldInt16Nullable;
    public ushort FieldUInt16;
    public ushort? FieldUInt16Nullable;
    public int FieldInt32;
    public int? FieldInt32Nullable;
    public uint FieldUInt32;
    public uint? FieldUInt32Nullable;
    public nint FieldIntPtr;
    public nint? FieldIntPtrNullable;
    public nuint FieldUIntPtr;
    public nuint? FieldUIntPtrNullable;
    public long FieldInt64;
    public long? FieldInt64Nullable;
    public ulong FieldUInt64;
    public ulong? FieldUInt64Nullable;
    public char FieldChar;
    public char? FieldCharNullable;
    public string FieldString;
    public string? FieldStringNullable;

    public FieldOnlyClass(bool @bool = false, bool? boolNullable = null, byte @byte = 0, byte? byteNullable = null, sbyte @sbyte = 0, sbyte? sbyteNullable = null, decimal @decimal =0,decimal? decimalNullable = null,
        double @double = 0, double? doubleNullable = null,float @float =0,float? floatNullable = null,short @short =0,short? shortNullable = null, ushort @ushort = 0, ushort? ushortNullable = null,
        int @int = 0, int? intNullable = null, uint @uint = 0, uint? uintNullable = null, nint @nint = 0, nint? nintNullable = null, nuint @nuint =0,nuint? nuintNullable = null,long @long =0,long? longNullable = null,
        ulong @ulong = 0, ulong? ulongNullable = null, char @char = '\0', char? charNullable = null, string @string = "", string? stringNullable = null)
    {
        FieldBoolean = @bool;
        FieldBooleanNullable = boolNullable;
        FieldByte = @byte;
        FieldByteNullable = byteNullable;
        FieldSByte = @sbyte;
        FieldSByteNullable = sbyteNullable;
        FieldDecimal = @decimal;
        FieldDecimalNullable = decimalNullable;
        FieldDouble = @double;
        FieldDoubleNullable = doubleNullable;
        FieldFloat = @float;
        FieldFloatNullable = floatNullable;
        FieldInt16 = @short;
        FieldInt16Nullable = shortNullable;
        FieldUInt16 = @ushort;
        FieldUInt16Nullable = ushortNullable;
        FieldInt32 = @int;
        FieldInt32Nullable = intNullable;
        FieldUInt32 = @uint;
        FieldUInt32Nullable = uintNullable;
        FieldIntPtr = @nint;
        FieldIntPtrNullable = nintNullable;
        FieldUIntPtr = @nuint;
        FieldUIntPtrNullable = nuintNullable;
        FieldInt64 = @long;
        FieldInt64Nullable = longNullable;
        FieldUInt64 = @ulong;
        FieldUInt64Nullable = ulongNullable;
        FieldChar = @char;
        FieldCharNullable = charNullable;
        FieldString = @string;
        FieldStringNullable = stringNullable;
    }
}

