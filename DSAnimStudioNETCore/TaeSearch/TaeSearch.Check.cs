using SoulsAssetPipeline.Animation;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public class Check
        {
            public ConditionDepthType DeepestCondition;
            public DSAProj Proj;
            public DSAProj.AnimCategory Category;
            public DSAProj.Animation Anim;
            public DSAProj.Action Act;
            public DSAProj.ActionTrack Track;
            public string ParameterName;
            // public object ParameterValue;
            // public TAE.Template.ParamTypes ParameterType;
            
            public bool StringIsCaseSensitive;

            public Check Clone()
            {
                var c = new Check();
                c.DeepestCondition = DeepestCondition;
                c.Proj = Proj;
                c.Category = Category;
                c.Anim = Anim;
                c.Act = Act;
                c.Track = Track;
                c.ParameterName = ParameterName;
                // c.ParameterValue = ParameterValue;
                // c.ParameterType = ParameterType;
                c.StringIsCaseSensitive = StringIsCaseSensitive;

                return c;
            }
        }
    }
}