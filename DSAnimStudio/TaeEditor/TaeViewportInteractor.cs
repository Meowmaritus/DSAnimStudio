using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeViewportInteractor
    {
        public readonly TaeEditAnimEventGraph Graph;

        TaeEventSimulationEnvironment EventSim;

        public List<ParamData.NpcParam> PossibleNpcParams = new List<ParamData.NpcParam>();
        private int _selectedNpcParamIndex = -1;
        public int SelectedNpcParamIndex
        {
            get => _selectedNpcParamIndex;
            set
            {
                if (CurrentModel != null)
                {
                    CurrentModel.NpcParam = (value >= 0 && value < PossibleNpcParams.Count)
                        ? PossibleNpcParams[value] : null;
                    _selectedNpcParamIndex = value;
                    if (CurrentModel.NpcParam != null)
                    {
                        CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                    }
                }
            }
        }

        Model CurrentModel => Scene.Models.Count > 0 ? Scene.Models[0] : null;

        public TaeViewportInteractor(TaeEditAnimEventGraph graph)
        {
            Graph = graph;
            Graph.PlaybackCursor.PlaybackStarted += PlaybackCursor_PlaybackStarted;
            Graph.PlaybackCursor.PlaybackFrameChange += PlaybackCursor_PlaybackFrameChange;
            Graph.PlaybackCursor.ScrubFrameChange += PlaybackCursor_ScrubFrameChange;
            Graph.PlaybackCursor.PlaybackEnded += PlaybackCursor_PlaybackEnded;
            Graph.PlaybackCursor.EventBoxEnter += PlaybackCursor_EventBoxEnter;
            Graph.PlaybackCursor.EventBoxMidst += PlaybackCursor_EventBoxMidst;
            Graph.PlaybackCursor.EventBoxExit += PlaybackCursor_EventBoxExit;
            Graph.PlaybackCursor.PlaybackLooped += PlaybackCursor_PlaybackLooped;

            NewAnimationContainer.AutoPlayAnimContainersUponLoading = false;

            Scene.ClearScene();
            TexturePool.Flush();

            var shortFileName = Utils.GetFileNameWithoutAnyExtensions(
                Utils.GetFileNameWithoutDirectoryOrExtension(Graph.MainScreen.FileContainerName)).ToLower();

            var fileName = Graph.MainScreen.FileContainerName.ToLower();

            if (shortFileName.StartsWith("c"))
            {
                GameDataManager.LoadCharacter(shortFileName);

                if (CurrentModel.IS_PLAYER)
                {
                    PossibleNpcParams.Clear();
                    SelectedNpcParamIndex = -1;

                    CurrentModel.CreateChrAsm();

                    if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(GameDataManager.GameType))
                    {
                        Graph.MainScreen.Config.ChrAsmConfigurations.Add
                            (GameDataManager.GameType, new TaeConfigFile.ChrAsmConfig());
                    }

                    Graph.MainScreen.Config.ChrAsmConfigurations[GameDataManager.GameType]
                        .WriteToChrAsm(CurrentModel.ChrAsm);
                }
                else
                {
                    PossibleNpcParams = ParamManager.FindNpcParams(CurrentModel.Name);
                    if (PossibleNpcParams.Count > 0)
                        SelectedNpcParamIndex = 0;
                    else
                        SelectedNpcParamIndex = -1;

                }
            }
            else if (shortFileName.StartsWith("o"))
            {
                throw new NotImplementedException("OBJECTS NOT SUPPORTED YET");
            }
            else if (fileName.EndsWith(".partsbnd") || fileName.EndsWith(".partsbnd.dcx"))
            {
                throw new NotImplementedException("PARTS NOT SUPPORTED YET");
            }
        }

        public void SaveChrAsm()
        {
            if (CurrentModel.ChrAsm != null)
            {
                if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(GameDataManager.GameType))
                {
                    Graph.MainScreen.Config.ChrAsmConfigurations.Add
                        (GameDataManager.GameType, new TaeConfigFile.ChrAsmConfig());
                }

                Graph.MainScreen.Config.ChrAsmConfigurations[GameDataManager.GameType].CopyFromChrAsm(CurrentModel.ChrAsm);
            }
        }

        private void PlaybackCursor_PlaybackFrameChange(object sender, EventArgs e)
        {
            CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubCurrentAnimation((float)Graph.PlaybackCursor.GUICurrentTime);
        }

        private void CheckSimEnvironment()
        {
            if (EventSim == null || EventSim.MODEL != CurrentModel)
            {
                EventSim = new TaeEventSimulationEnvironment(CurrentModel);
            }
        }

        private void PlaybackCursor_EventBoxExit(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventExit(e);
        }

        private void PlaybackCursor_EventBoxMidst(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventMidFrame(e);
        }

        private void PlaybackCursor_EventBoxEnter(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventEnter(e);
        }

        private void PlaybackCursor_PlaybackEnded(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationEnd();
        }

        private void PlaybackCursor_ScrubFrameChange(object sender, EventArgs e)
        {
            CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubCurrentAnimation((float)Graph.PlaybackCursor.GUICurrentTime);
            CheckSimEnvironment();
            EventSim.OnSimulationScrub();
        }

        private void PlaybackCursor_PlaybackStarted(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart();
        }

        private void PlaybackCursor_PlaybackLooped(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart();
        }

        Model lastRightWeaponModelTAEWasReadFrom = null;
        TAE lastRightWeaponTAE = null;

        Model lastLeftWeaponModelTAEWasReadFrom = null;
        TAE lastLeftWeaponTAE = null;



        private void CheckChrAsmWeapons()
        {
            if (CurrentModel.ChrAsm != null)
            {
                if (CurrentModel.ChrAsm.RightWeaponModel != null)
                {
                    if (CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.Count > 0)
                    {
                        if (CurrentModel.ChrAsm.RightWeaponModel != lastRightWeaponModelTAEWasReadFrom)
                        {
                            lastRightWeaponTAE = TAE.Read(CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.First().Value);
                            lastRightWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.RightWeaponModel;
                        }
                    }
                    else
                    {
                        lastRightWeaponModelTAEWasReadFrom = null;
                        lastRightWeaponTAE = null;
                    }
                }

                if (CurrentModel.ChrAsm.LeftWeaponModel != null)
                {
                    if (CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.Count > 0)
                    {
                        if (CurrentModel.ChrAsm.LeftWeaponModel != lastLeftWeaponModelTAEWasReadFrom)
                        {
                            lastLeftWeaponTAE = TAE.Read(CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.First().Value);
                            lastLeftWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.LeftWeaponModel;
                        }
                    }
                    else
                    {
                        lastLeftWeaponModelTAEWasReadFrom = null;
                        lastLeftWeaponTAE = null;
                    }
                }
                    

               
            }
            else
            {
                lastRightWeaponModelTAEWasReadFrom = null;
                lastLeftWeaponModelTAEWasReadFrom = null;
                lastRightWeaponTAE = null;
                lastLeftWeaponTAE = null;
            }
        }

        private void FindWeaponAnim(TaeAnimRefChainSolver solver, Model weaponModel, TAE weaponTae)
        {
            if (weaponModel != null && weaponModel.AnimContainer.Animations.Count > 0)
            {
                // If weapon has TAE
                if (weaponTae != null)
                {
                    var compositeAnimID = solver.GetCompositeAnimIDOfAnimInTAE(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);
                    var matchingAnim = weaponTae.Animations.Where(a => a.ID == compositeAnimID).FirstOrDefault();
                    if (matchingAnim != null)
                    {
                        

                        int animID = (int)matchingAnim.ID;

                        bool AnimExists(int id)
                        {
                            return (weaponModel.AnimContainer.Animations.ContainsKey(solver.HKXNameFromCompositeID(id)));
                        }

                        if (matchingAnim.Unknown1 == 256)
                        {
                            if (matchingAnim.Unknown2 > 0 && AnimExists(matchingAnim.Unknown2))
                            {
                                animID = matchingAnim.Unknown2;
                            }
                        }
                        else if (matchingAnim.Unknown1 > 256 && AnimExists(matchingAnim.Unknown1))
                        {
                            animID = matchingAnim.Unknown1;
                        }

                        var weaponHkxName = solver.HKXNameFromCompositeID(animID);
                        weaponModel.AnimContainer.CurrentAnimationName = weaponHkxName;
                    }
                    else
                    {
                        weaponModel.AnimContainer.CurrentAnimationName = null;
                    }
                }
                else
                {
                    // If weapon has no TAE, it's usually just the player's TAE anim entry ID as an anim name.
                    var simpleAnimID = solver.GetHKXNameIgnoreReferences(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);
                    weaponModel.AnimContainer.CurrentAnimationName = simpleAnimID;
                }
            }
        }

        public void OnNewAnimSelected()
        {
            if (CurrentModel != null)
            {
                var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
                var mainChrAnimName = mainChrSolver.GetHKXName(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);

                CurrentModel.AnimContainer.CurrentAnimationName = mainChrAnimName;

                Graph.PlaybackCursor.HkxAnimationLength = CurrentModel.AnimContainer.CurrentAnimDuration;

                CheckChrAsmWeapons();

                FindWeaponAnim(mainChrSolver, lastRightWeaponModelTAEWasReadFrom, lastRightWeaponTAE);
                FindWeaponAnim(mainChrSolver, lastLeftWeaponModelTAEWasReadFrom, lastLeftWeaponTAE);

                CheckSimEnvironment();

                EventSim.OnNewAnimSelected();
            }
            
        }

        public void DrawDebug()
        {
            var printer = new StatusPrinter(Vector2.Zero, Color.Yellow);

            printer.AppendLine($"CHAR ANIM: {(CurrentModel?.AnimContainer?.CurrentAnimationName ?? "NONE")}");
            printer.AppendLine($"RWPN ANIM: {(CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.CurrentAnimationName ?? "NONE")}");
            printer.AppendLine($"LWPN ANIM: {(CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.CurrentAnimationName ?? "NONE")}");

            printer.AppendLine($"RWPN ANIM LIST:");
            var anims = CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.Animations;
            if (anims != null)
            {
                foreach (var a in anims.Keys)
                {
                    printer.AppendLine("  " + a);
                }
            }
            printer.AppendLine($"LWPN ANIM LIST:");
            var animsL = CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.Animations;
            if (animsL != null)
            {
                foreach (var a in animsL.Keys)
                {
                    printer.AppendLine("  " + a);
                }
            }


            printer.Draw();
        }
    }
}
