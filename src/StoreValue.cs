public record StoreValue(byte[] Value, DateTime? Expiry = null)
{
    public enum ValueType : short
    {
        String,
        List,
        Set,
        SortedSet,
        Hash,
        Zipmap,
        Ziplist,
        Intset,
        SortedSetinZiplist,
        HashmapinZiplist,
        ListinQuicklist
    }

    public ValueType Encoding;
    public byte[] Value = Value;
    public DateTime? Expiry = Expiry;
}