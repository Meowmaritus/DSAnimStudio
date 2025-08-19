using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public abstract class ErrorState
        {
            public AnimCategory SourceAnimCategory;
            public Animation SourceAnimation;
            public string GUID = Guid.NewGuid().ToString();
            public abstract string ShortDescription { get; }
            public abstract void AddImguiRightClickEntry();

            protected ErrorState(AnimCategory sourceAnimCategory, Animation sourceAnimation)
            {
                SourceAnimCategory = sourceAnimCategory;
                SourceAnimation = sourceAnimation;
            }
            
            
            public class DuplicateAnimID : ErrorState
            {
                public readonly TaeEditorScreen MainScreen;
                public readonly DSAProj Proj;
                public readonly SplitAnimID AnimID;

                public DuplicateAnimID(AnimCategory sourceAnimCategory, Animation sourceAnimation, TaeEditorScreen mainScreen, DSAProj proj, SplitAnimID animID)
                    : base(sourceAnimCategory, sourceAnimation)
                {
                    MainScreen = mainScreen;
                    Proj = proj;
                    AnimID = animID;
                }

                public override string ShortDescription => $"There are multiple animations with ID '{AnimID.GetFormattedIDString(Proj)}'.";
                public override void AddImguiRightClickEntry()
                {
                    return;
                    //ImGui.MenuItem($"Show all instances of animation ID '{AnimID.GetFormattedIDString(Proj)}' being used###{GUID}_MenuItem");
                    //if (ImGui.IsItemClicked())
                    //{
                    //    var animInstances = Proj.GetAllAnimsWithID(AnimID);
                    //    string[] entryNames = animInstances
                    //        .Select(ins => $"Category {ins.ParentCategory.CategoryID} -> Animation {ins.SplitID.GetFormattedIDString(Proj)}").ToArray();
                        
                    //    OSD.NewGenericWindow(initAction: window =>
                    //        {
                    //            window.Title = $"Instances of Animation {AnimID.GetFormattedIDString(Proj)}";
                    //        },
                    //    buildAction: window =>
                    //    {
                    //        if (window.IsFirstFrameOpen)
                    //        {
                    //            ImGui.SetWindowSize(new Vector2(300, 300));
                    //        }
                    //        int animIndex = 0;
                    //        bool noLongerRelevant = false;
                    //        foreach (var ins in animInstances)
                    //        {
                    //            if (!ins.ParentCategory.Animations.Contains(ins))
                    //                noLongerRelevant = true;
                                    
                    //            string insName = $"Category {ins.ParentCategory.CategoryID} -> Anim {ins.SplitID.GetFormattedIDString(Proj)}";
                    //            bool insSelected = MainScreen.SelectedAnimCategory == ins.ParentCategory &&
                    //                               MainScreen.SelectedAnim == ins;
                    //            bool prevSelected = insSelected;
                    //            ImGui.Selectable($"{insName}###ErrorState{GUID}_DuplicateAnimID_Anim[{animIndex}]", ref insSelected);
                    //            if (insSelected && !prevSelected)
                    //            {
                    //                MainScreen.SelectNewAnimRef(ins.ParentCategory, ins, isPushCurrentToHistBackwardStack: true);
                    //                ImGui.SetWindowFocus(null);
                    //            }

                    //            animIndex++;
                    //        }

                    //        if (noLongerRelevant)
                    //            window.IsOpen = false;
                    //    });
                    //}
                }
            }
            
            
            
            
            
        }
    }
}
