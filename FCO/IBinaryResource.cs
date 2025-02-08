namespace SUFcoTool
{
    public interface IBinaryResource
    {
        public T Read<T>(BinaryReader reader, bool in_IsGens);
        public void Write<T>(BinaryReader reader, bool in_IsGens);
    }
}