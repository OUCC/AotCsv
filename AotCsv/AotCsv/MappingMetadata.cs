namespace Oucc.AotCsv;

public sealed record class MappingMetadata(Type Type, MemberMetadata[] Members) { }

public sealed record class MemberMetadata(Type Type, string HeaderName, uint? Index, string? Format, string? DateTimeFormat, bool IsProperty, bool IsWriteOnly, bool IsReadOnly) { }
