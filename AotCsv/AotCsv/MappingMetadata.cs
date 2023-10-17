using System.Collections.Immutable;
using System.Globalization;

namespace Oucc.AotCsv;

public sealed record class MappingMetadata(Type Type, ImmutableArray<MemberMetadata> Members) { }

public sealed record class MemberMetadata(int InternalId, Type Type, string Name, string HeaderName, uint? Index, string? Format, string? DateTimeFormat, DateTimeStyles DateTimeStyles, bool IsProperty, bool IsWritable, bool IsReadable) {
    internal int InternalId { get; } = InternalId;
}

