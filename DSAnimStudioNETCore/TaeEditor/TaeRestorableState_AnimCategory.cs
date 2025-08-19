using SoulsAssetPipeline.Animation;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeRestorableState_AnimCategory : TaeRestorableState
    {
        public DSAProj Proj;
        public DSAProj.AnimCategory AnimCategory;
            
        public TaeRestorableState_AnimCategory(TaeEditorScreen mainScreen, DSAProj.AnimCategory animCategory, TAE.Template template, bool isCustomRegist, List<int> registeredAnimCategories, List<SplitAnimID> registeredAnims)
            : base(mainScreen, template, isCustomRegist, registeredAnimCategories, registeredAnims)
        {
            Proj = mainScreen.Proj;
            AnimCategory = animCategory;
            TaeTemplate = template;

            if (!isCustomRegist)
                SerializedBytes = AnimCategory.SAFE_SerializeToBytes(mainScreen.Proj);
        }
            
        public override void InnerRestoreState()
        {
            // File version latest because serializing to bytes always serializes as the latest version.
            // Only loading has backward compatibility with old versions.
            AnimCategory.SAFE_DeserializeFromBytes(SerializedBytes, template: TaeTemplate, proj: Proj);

            AnimCategory.SafeAccessAnimations(anims =>
            {
                foreach (var anim in anims)
                {
                    anim.UnSafeAccessActions(actions =>
                    {
                        int i = 0;
                        foreach (var act in actions)
                        {
                            act.ApplyTemplate(anim.ParentCategory, Proj.Template, anim.SplitID.GetFullID(Proj), i, act.Type);
                            act.RequestUpdateText(true);
                            i++;
                        }
                    });
                }
            });

            AnimCategory.SAFE_SetIsModified(true);
        }
    }
}