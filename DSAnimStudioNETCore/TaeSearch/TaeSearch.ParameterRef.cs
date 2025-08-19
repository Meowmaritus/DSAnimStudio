namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public struct ParameterRef
        {
            public enum ParameterSubtype
            {
                None,
                f32gradStart,
                f32gradEnd,
            }
            public string ParameterName;
            public ParameterSubtype Subtype;
        }
    }
}