using SoulsFormats;
using System.Linq;

namespace DSAnimStudio
{
    public static class SoulsFormatsExtensions
    {
        public static bool UsesBoneIndices(this FLVER.Vertex vertex)
        {
            return vertex.BoneIndices[0] != 0;
        }

        public static bool UsesBoneWeights(this FLVER.Vertex vertex)
        {
            return vertex.BoneWeights[0] != 0;
        }
    }
}
