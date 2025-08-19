using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public class zzz_DocumentIns : IDisposable
    {
        public string GUID = Guid.NewGuid().ToString();

        public zzz_GameDataIns GameData; // Class filled in.
        public zzz_GameRootIns GameRoot; // Class filled in.
        public zzz_LoadingTaskManIns LoadingTaskMan; // Class filled in.
        public zzz_ParamManagerIns ParamManager; // Class filled in.
        public zzz_RumbleCamManagerIns RumbleCamManager; // Class filled in.
        public zzz_SceneIns Scene; // Class filled in.
        public zzz_SoundManagerIns SoundManager; // Class filled in.
        public zzz_TexturePoolIns TexturePool; // Class filled in.
        public zzz_WorldViewManagerIns WorldViewManager;
        public zzz_FmgManagerIns FmgManager;
        public NewFmodIns Fmod;
        public ImguiOSD.Window.Animations SpWindowAnimations;

        public zzz_DocumentIns()
        {
            GameData = new(this);
            GameRoot = new(this);
            LoadingTaskMan = new(this);
            ParamManager = new(this);
            RumbleCamManager = new(this);
            Scene = new(this);
            SoundManager = new(this);
            TexturePool = new(this);
            WorldViewManager = new(this);
            FmgManager = new(this);

            Fmod = new NewFmodIns(this);

            SpWindowAnimations = new ImguiOSD.Window.Animations(this);

            EditorScreen = new TaeEditor.TaeEditorScreen(this, (Form)Form.FromHandle(Program.MainInstance.Window.Handle));
        }

        public zzz_DocumentIns(DSAProj fakeProj)
        {
            FakeProj = fakeProj;
        }

        //public List<string> FmodFevsLoaded = new List<string>();

        public void OnSwitchedToDoc()
        {
            if (Main.WinFormDisposed())
                return;

            var proj = Proj;

            // This stuff will happen only the first frame
            if (RequestInitAfterBeingSwitchedTo_IsFirstFrame)
            {
                SoundManager.StopAllSounds(immediate: true);
                GameRoot.LoadMTDBND();
                SpWindowAnimations.SetRequestLoadStuffStoredWhenChangingDocs();

                RequestInitAfterBeingSwitchedTo_IsFirstFrame = false;
            }

            // This stuff will happen every frame until the RequestInitAfterBeingSwitchedTo is cleared.

            // Nothing...yet

            if (proj == null)
                return;

            // This stuff will happen once at the end before clearing RequestInitAfterBeingSwitchedTo



            if (zzz_SoundManagerIns.CurrentDocInControl != this)
            {
                zzz_SoundManagerIns.CurrentDocInControl = this;

                if (SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.FMOD)
                {
                    if (Main.WinFormDisposed())
                        return;
                    NewFmodManager.SwitchToFmodIns(Fmod);
                }
                //else if (SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise)
                //{
                //    var soundBankNames = SoundManager.GetAdditionalSoundBankNames();
                //    Wwise.PurgeLoadedAssets(SoundManager);
                //    Wwise.AddLookupBanks(soundBankNames);
                //    SoundManager.NeedsWwiseRefresh = true;
                //}
                //else if (SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
                //{
                //    var soundBankNames = SoundManager.GetAdditionalSoundBankNames();
                //    MagicOrchestra.PurgeLoadedAssets(SoundManager);
                //    MagicOrchestra.AddLookupBanks(soundBankNames);
                //    SoundManager.NeedsWwiseRefresh = true;
                //}
            }

            RequestInitAfterBeingSwitchedTo = false;

            //Main.MainThreadLazyDispatch(() =>
            //{
            //    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false);
            //    GC.Collect();
            //});
        }

        //public void BeforeSwitchingFromDoc()
        //{
        //    FmodFevsLoaded = Fmod.LoadedFEVs_FullPaths.Values.ToList();
        //}

        public string ImguiTabDisplayName;

        //public bool ImguiTabOpen;

        public bool Hidden = false;

        public bool RequestClose = false;
        public bool RequestClose_ForceDelete = false;

        public bool WasSelectedPrevFrame = false;

        public bool RequestImguiTabSelect = false;

        public bool WasHiddenPrevFrame = false;


        //public bool IsJustSwitchingTo = false;

        public bool RequestInitAfterBeingSwitchedTo = false;
        public bool RequestInitAfterBeingSwitchedTo_IsFirstFrame = false;


        public TaeEditor.TaeEditorScreen EditorScreen;

        public DSAProj Proj => EditorScreen?.Proj ?? FakeProj;

        public DSAProj FakeProj = null;

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (this == zzz_DocumentManager._hiddenDefaultDocument)
                return;

            if (!IsDisposed)
            {

                if (SoundManager != null)
                {
                    SoundManager.PurgeLoadedAssets();
                    SoundManager.DisposeAll();

                    if (SoundManager?.WwiseManager != null)
                    {
                        SoundManager.WwiseManager.ParentDocument = null;
                        SoundManager.WwiseManager = null;
                    }

                    if (SoundManager?.MagicOrchestraManager != null)
                    {
                        SoundManager.MagicOrchestraManager.ParentDocument = null;
                        SoundManager.MagicOrchestraManager = null;
                    }


                    SoundManager.ParentDocument = null;
                    SoundManager = null;
                }



                if (GameData != null)
                {
                    GameData.ClearAll();
                    GameData.ParentDocument = null;
                    GameData = null;
                }

                if (Scene != null)
                {
                    Scene.ClearScene();
                    Scene.ParentDocument = null;
                    Scene = null;
                }

                if (TexturePool != null)
                {
                    TexturePool.Flush();
                    TexturePool.ParentDocument = null;
                    TexturePool = null;
                }

                if (Fmod != null)
                {
                    Fmod.Purge();
                    Fmod.Shutdown();
                    Fmod.ParentDocument = null;
                    Fmod = null;
                }

                if (RumbleCamManager != null)
                {
                    RumbleCamManager.ClearAll();
                    RumbleCamManager.ParentDocument = null;
                    RumbleCamManager = null;
                }

                if (EditorScreen != null)
                {
                    EditorScreen.Dispose();
                    if (EditorScreen.FileContainer != null)
                    {
                        EditorScreen.FileContainer.Proj = null;
                        EditorScreen.FileContainer = null;
                    }
                    EditorScreen.ParentDocument = null;
                    EditorScreen = null;
                }

                if (GameRoot != null)
                {
                    GameRoot.ClearInterroot();
                    GameRoot.ParentDocument = null;
                    GameRoot = null;
                }

                if (FakeProj != null)
                {
                    FakeProj.ParentDocument = null;
                    FakeProj = null;
                }

                if (FmgManager != null)
                {
                    FmgManager.Dispose();
                    FmgManager.ParentDocument = null;
                    FmgManager = null;
                }

                if (LoadingTaskMan != null)
                {
                    LoadingTaskMan?.KILL_ALL_TASKS();
                    LoadingTaskMan.ParentDocument = null;
                    LoadingTaskMan = null;
                }

                if (ParamManager != null)
                {
                    ParamManager.Dispose();
                    ParamManager.ParentDocument = null;
                    ParamManager = null;
                }

                if (WorldViewManager != null)
                {
                    WorldViewManager.Dispose();
                    WorldViewManager.ParentDocument = null;
                    WorldViewManager = null;
                }

                if (SpWindowAnimations != null)
                {
                    SpWindowAnimations.Document = null;
                    SpWindowAnimations = null;
                }

                // Hidden is used for placeholder doc, no point in forcing a whole GC collect for that.
                if (!Hidden)
                {
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                    Main.MainThreadLazyDispatch(() =>
                    {
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);
                        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false);
                        GC.Collect();
                    }, waitForAllLoadingTasks: true);
                }

                IsDisposed = true;
            }

        }
    }
}
