using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;
using static System.Reflection.Metadata.BlobBuilder;

namespace DSAnimStudio.TaeEditor
{
    public class NewGraph : IDisposable
    {
        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                InputMan.Dispose();
                _disposed = true;
            }
        }

        //private List<NewGraph> ChildGraphs = new List<NewGraph>();
        //private NewGraph SelectedChildGraph = null;
        //private object _lock_ChildGraphs = new object();
        //public string GraphTabName = "Main Graph";

        //public NewGraph GetSelectedChildGraph()
        //{
        //    NewGraph result = null;
        //    lock (_lock_ChildGraphs)
        //    {
        //        result = SelectedChildGraph;
        //    }
        //    return result;
        //}

        //public void SetSelectedChildGraph(NewGraph graph)
        //{
        //    lock (_lock_ChildGraphs)
        //    {
        //        if (graph == null || ChildGraphs.Contains(graph))
        //            SelectedChildGraph = graph;
        //    }
        //}

        //public List<NewGraph> GetChildGraphs()
        //{
        //    var result = new List<NewGraph>();
        //    lock (_lock_ChildGraphs)
        //    {
        //        result = ChildGraphs.ToList();
        //    }
        //    return result;
        //}

        //public void ClearChildGraphs()
        //{
        //    lock (_lock_ChildGraphs)
        //    {
        //        ChildGraphs.Clear();
        //    }

        //}

        //public void RegistChildGraph(NewGraph newGraph)
        //{
        //    lock (_lock_ChildGraphs)
        //    {
        //        if (!ChildGraphs.Contains(newGraph))
        //            ChildGraphs.Add(newGraph);
        //    }

        //}

        //public void UnregistChildGraph(NewGraph graph)
        //{
        //    lock (_lock_ChildGraphs)
        //    {
        //        if (ChildGraphs.Contains(graph))
        //        {
        //            ChildGraphs.Remove(graph);
        //        }
        //        if (SelectedChildGraph == graph)
        //            SelectedChildGraph = null;
        //    }

        //}

        //public bool IsBackgroundGraph => zzz_DocumentManager.CurrentDocument.EditorScreen.Graph != this;

        public DSAProj.Animation GhostAnimRef = null;
        public bool IsGhostGraph => GhostAnimRef != null;

        public object _lock_ActionBoxManagement = new object();

        public readonly TaeEditorScreen MainScreen;
        public DSAProj.Animation AnimRef { get; private set; }

        public TaePlaybackCursor PlaybackCursor;

        public TaeViewportInteractor ViewportInteractor;

        public TaeActionSimulationEnvironment ActionSim;

        //TODO: Make this prevent selection changing while editing stuff (affects: Ctrl+A, maybe more)
        public bool CanChangeSelectionRightNow = true;

        public NewGraphLayoutManager LayoutManager;
        public NewGraphDrawer Drawer;
        public NewGraphInput InputMan;

        public Rectangle Rect
        {
            get
            {
                var rect = OSD.SpWindowGraph.GetRect(subtractTitleBar: false);
                var rect2 = OSD.SpWindowGraph_Tracks.GetRect(subtractTitleBar: false);
                return new RectF(rect.X, rect2.Y, rect.Width, rect2.Height).ToRectRounded();
            }
        }
        public float ActionTrackNamePaneEffectiveWidth => OSD.SpWindowGraph_Tracks.GetRect().Width + 16;
        public float ActionTrackNamePaneDesiredWidth = 256f;
        public int TimeLineHeight = 24;

        public const int SpacingMeme = 1;
        public const int SpacingMemeBottomRightOfGraph = 0;

        public Rectangle WholeScrollViewerRect => new Rectangle((int)(Rect.X + ActionTrackNamePaneEffectiveWidth + SpacingMeme), 
            Rect.Y + TimeLineHeight + SpacingMeme, 
            (int)(Rect.Width - ActionTrackNamePaneEffectiveWidth - (SpacingMemeBottomRightOfGraph * 2)) - 2, 
            Rect.Height - TimeLineHeight - (SpacingMemeBottomRightOfGraph * 2) - 2);
        
        public Rectangle ScrollViewerContentsRect => new Rectangle((int)(Rect.X + ActionTrackNamePaneEffectiveWidth + SpacingMeme), 
            Rect.Y + TimeLineHeight + SpacingMeme, 
            ScrollViewer.RelativeViewport.Width, 
            ScrollViewer.RelativeViewport.Height);
        
        public Rectangle ActionTrackHeaderRect => new Rectangle((int)(Rect.X + SpacingMeme), 
            Rect.Y + TimeLineHeight + SpacingMeme, 
            (int)(ActionTrackNamePaneEffectiveWidth - (SpacingMeme * 2)), 
            Rect.Height - TimeLineHeight - (SpacingMeme * 2));

        public int ActionTrackEditButtonWidth => 32;
        public int ActionTrackEditButtonHeight => 15;
        public int SpacingBetweenEditButtonAndEdge => 2;
        public int SpacingBetweenEditButtonAndText => 4;
        public float ActionTrackEditButtonFontSize => 16;
        
        public Rectangle ActionTrackHeaderTextRect => new Rectangle((int)(Rect.X + SpacingMeme + SpacingBetweenEditButtonAndText + SpacingBetweenEditButtonAndEdge + ActionTrackEditButtonWidth), 
            Rect.Y + TimeLineHeight + SpacingMeme, 
            (int)(ActionTrackNamePaneEffectiveWidth - (SpacingMeme * 2)) - ActionTrackEditButtonWidth - SpacingBetweenEditButtonAndText - SpacingBetweenEditButtonAndEdge, 
            ScrollViewer.RelativeViewport.Height);

        public Rectangle ActionTrackHeaderEditButtonRect
        {
            get
            {
                var curTextRect = ActionTrackHeaderTextRect;
                return new Rectangle(Rect.X + SpacingMeme + SpacingBetweenEditButtonAndEdge, curTextRect.Y,
                    ActionTrackEditButtonWidth, curTextRect.Height);
            }
        }
        public TaeScrollViewer ScrollViewer { get; private set; } = new TaeScrollViewer();

        public NewGraph(TaeEditorScreen mainScreen, DSAProj.Animation startingAnimRef, DSAProj.Animation ghostAnimRef)
        {
            MainScreen = mainScreen;
            AnimRef = startingAnimRef;
            PlaybackCursor = new TaePlaybackCursor(this);
            ViewportInteractor = new TaeViewportInteractor(this);
            InputMan = new NewGraphInput(this);
            LayoutManager = new NewGraphLayoutManager(this);
            Drawer = new NewGraphDrawer(this);
            GhostAnimRef = ghostAnimRef;
        }



        public void SafeAccessActionList(Action<List<DSAProj.Action>> doAction)
        {
            AnimRef.SafeAccessActions(actions =>
            {
                lock (_lock_ActionBoxManagement)
                {
                    doAction?.Invoke(actions);
                }
            });
        }
        public void UnSafeAccessActionList(Action<List<DSAProj.Action>> doAction)
        {
            AnimRef.UnSafeAccessActions(actions =>
            {
                lock (_lock_ActionBoxManagement)
                {
                    doAction?.Invoke(actions);
                }
            });
        }

        public List<DSAProj.Action> GetActionListCopy_UsesLock(bool useGhostIfAvailable = true)
        {
            List<DSAProj.Action> actionsCopy = null;
            lock (_lock_ActionBoxManagement)
            {
                if (useGhostIfAvailable && GhostAnimRef != null)
                    actionsCopy = GhostAnimRef.SAFE_GetActions();
                else
                    actionsCopy = AnimRef.SAFE_GetActions();
            }
            return actionsCopy;
        }

        public List<DSAProj.ActionTrack> GetActionTracksCopy_UsesLock(bool useGhostIfAvailable = true)
        {
            List<DSAProj.ActionTrack> actionTracksCopy = null;
            lock (_lock_ActionBoxManagement)
            {
                if (useGhostIfAvailable && GhostAnimRef != null)
                    actionTracksCopy = GhostAnimRef.SAFE_GetActionTracks();
                else
                    actionTracksCopy = AnimRef.SAFE_GetActionTracks();
            }
            return actionTracksCopy;
        }

        public void UpdatePlaybackCursor(bool allowPlayPauseInput)
        {
            List<DSAProj.Action> eventsCopy = GetActionListCopy_UsesLock();

            AnimRef.SafeAccessActionTracks(tracks =>
            {
                foreach (var track in tracks)
                    track.PlaybackHighlight = false;
            });

          

            //lock (_lock_ChildGraphs)
            //{
            //    if (SelectedChildGraph != null)
            //    {
            //        SelectedChildGraph.PlaybackCursor = PlaybackCursor;
            //    }
            //}

            PlaybackCursor.Update(MainScreen, eventsCopy, ignoreDeltaTime: MainScreen.AnimSwitchRenderCooldown > 0);

            foreach (var ev in eventsCopy)
            {
                if ((ev.NewSimulationActive) && ev.TrackIndex >= 0 && ev.TrackIndex < AnimRef.SAFE_GetActionTrackCount())
                {
                    AnimRef.SAFE_GetActionTrackByIndex(ev.TrackIndex).PlaybackHighlight = true;
                }
            }
            
            

            //foreach (var ev in AnimRef.Events)
            //{
            //    ev.PrevCyclePlaybackHighlight = ev.PlaybackHighlight;
            //    //ev.PrevCyclePlaybackHighlight = CheckHighlight(PlaybackCursor.OldCurrentTimeMod, MainScreen.NewHoverEvent_PrevFrame);
            //}

            //throw new NotImplementedException();
            
        }

        public void Update(float elapsedTime)
        {
            //lock (_lock_ChildGraphs)
            //{
            //    if (SelectedChildGraph != null && !ChildGraphs.Contains(SelectedChildGraph))
            //        SelectedChildGraph = null;


            //    if (SelectedChildGraph != null)
            //    {
            //        SelectedChildGraph.LayoutManager.Update();
            //        SelectedChildGraph.InputMan.UpdateLoop();
            //    }
            //    else
            //    {
            //        LayoutManager.Update();
            //        InputMan.UpdateLoop();
            //    }
            //}
            LayoutManager.Update(elapsedTime);
            InputMan.UpdateLoop(elapsedTime, isReadOnly: IsGhostGraph);
        }

        public void UpdateMouseOutsideRect(float elapsedTime)
        {
            //lock (_lock_ChildGraphs)
            //{
            //    if (SelectedChildGraph != null)
            //    {
            //        // Update with mouse outside of graph rect
            //        SelectedChildGraph.InputMan.UpdateOutsideRect(elapsedSeconds, allowMouseUpdate);
            //    }
            //    else
            //    {
            //        // Update with mouse outside of graph rect
            //        InputMan.UpdateOutsideRect(elapsedSeconds, allowMouseUpdate);
            //    }
            //}
            InputMan.UpdateOutsideRect(elapsedTime, isReadOnly: IsGhostGraph);
            LayoutManager.UpdateMouseOutsideRect(elapsedTime);
        }


        

        public NewGraph()
        {
            LayoutManager = new NewGraphLayoutManager(this);
            Drawer = new NewGraphDrawer(this);
        }

        public void ReadFromAnimRef(DSAProj.Animation animRef, DSAProj.Animation ghostAnimRef)
        {
            AnimRef = animRef;
            GhostAnimRef = ghostAnimRef;
            //ViewportInteractor.CurrentModel?.ChrAsm?.ForeachWeaponSlot(slot =>
            //{
            //    if (slot.TaeManager != null)
            //    {
            //        slot.TaeManager.SelectAnimation(anim)
            //    }
            //});
        }

        public void WriteToAnimRef(DSAProj.Animation animRef)
        {
            //throw new NotImplementedException();
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex)
        {
            //lock (_lock_ChildGraphs)
            //{
            //    if (SelectedChildGraph != null)
            //    {
            //        SelectedChildGraph.Drawer?.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex);
            //    }
            //    else
            //    {
            //        Drawer?.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex);
            //    }
            //}
            Drawer?.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex, MainScreen.Proj);
            if (Main.Debug.EnableGraphDebug)
            {
                Drawer?.DrawDebug();
            }
        }

        public void AllBoxesEveryFrameUpdate()
        {

        }
    }
}
