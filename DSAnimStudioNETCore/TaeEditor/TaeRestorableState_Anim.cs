using SoulsAssetPipeline.Animation;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeRestorableState_Anim : TaeRestorableState
    {
        public DSAProj Proj;
        public DSAProj.AnimCategory Category;
        public DSAProj.Animation Anim;
            
        public TaeRestorableState_Anim(TaeEditorScreen mainScreen, DSAProj.Animation anim, TAE.Template template, bool isCustomRegist, List<int> registeredAnimCategories, List<SplitAnimID> registeredAnims)
            : base(mainScreen, template, isCustomRegist, registeredAnimCategories, registeredAnims)
        {
            Proj = mainScreen.Proj;
            Category = anim.ParentCategory;
            Anim = anim;
            TaeTemplate = template;
            if (!isCustomRegist)
                SerializedBytes = Anim.SerializeToBytes(mainScreen.Proj);
        }
            
        public override void InnerRestoreState()
        {
            // File version latest because serializing to bytes always serializes as the latest version.
            // Only loading has backward compatibility with old versions.
            Anim.DeserializeFromBytes(SerializedBytes, TaeTemplate, Proj, Category);
            Anim.SafeAccessActions(actions =>
            {
                int i = 0;
                foreach (var act in actions)
                {
                    act.ApplyTemplate(Anim.ParentCategory, Proj.Template, Anim.SplitID.GetFullID(Proj), i, act.Type);
                    act.RequestUpdateText(true);
                    i++;
                }
            });

            Anim.SAFE_SetIsModified(true);
        }
    }
}