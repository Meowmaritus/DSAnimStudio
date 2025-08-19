using SoulsAssetPipeline.Animation;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeRestorableState_Proj : TaeRestorableState
    {
        public DSAProj Proj;

        
        
        public TaeRestorableState_Proj(TaeEditorScreen mainScreen, DSAProj proj, TAE.Template template, bool isCustomRegist, List<int> registeredAnimCategories, List<SplitAnimID> registeredAnims)
            : base(mainScreen, template, isCustomRegist, registeredAnimCategories, registeredAnims)
        {
            MainScreen = mainScreen;
            Proj = proj;
            TaeTemplate = template;

            byte[] serializedBytes = null;

            mainScreen.ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous(null, "Saving global undo state", prog =>
            {
                if (!isCustomRegist)
                    serializedBytes = Proj.SAFE_SerializeToBytes(prog, 0, 1, relativeToDir: null);
            });


            if (!isCustomRegist)
                SerializedBytes = serializedBytes;
        }
            
        public override void InnerRestoreState()
        {
            var currentAnimationID = MainScreen.SelectedAnim?.SplitID;
            Proj.SAFE_DeserializeFromBytes(SerializedBytes, template: TaeTemplate, relativeToDir: null);
            Proj.SAFE_ApplyTemplate(TaeTemplate);
            if (currentAnimationID.HasValue)
            {
                MainScreen.GotoAnimID(currentAnimationID.Value, false, false, out _);
            }

            Proj.SAFE_MarkAllModified();
        }
    }
}