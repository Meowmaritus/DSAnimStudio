using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public class zzz_DocumentManager
    {
        public static bool RequestOpenFromPackedGameDataArchives = false;
        public static bool RequestFileOpenBrowse = false;
        public static bool RequestFileOpenRecent = false;
        public static DSAProj.TaeContainerInfo RequestFileOpenRecent_SelectedFile = null;
        public static Action<zzz_DocumentIns> RequestFileOpenRecent_AfterOpenAction = (doc) => { };

        public static void ClearAllRequests()
        {
            RequestOpenFromPackedGameDataArchives = false;
            RequestFileOpenBrowse = false;
            RequestFileOpenRecent = false;
            RequestFileOpenRecent_SelectedFile = null;
            RequestFileOpenRecent_AfterOpenAction = (doc) => { };
        }

        public static bool AnyDocumentWithANYLoadingTasks()
        {
            bool result = false;
            lock (_lock_DocumentSwitcher)
            {
                result = Documents.Any(doc => !doc.IsDisposed && doc.LoadingTaskMan.AnyTasks());
            }
            return result;
        }

        public static bool AnyDocumentWithAnyInteractionBlockingLoadingTasks()
        {
            bool result = false;
            lock (_lock_DocumentSwitcher)
            {
                result = Documents.Any(doc => !doc.IsDisposed && doc.LoadingTaskMan.AnyInteractionBlockingTasks());
            }
            return result;
        }

        public static bool AnyDocumentWithCriticalLoadingTasks()
        {
            bool result = false;
            lock (_lock_DocumentSwitcher)
            {
                result = Documents.Any(doc => !doc.IsDisposed && doc.LoadingTaskMan.AnyCriticalTasks());
            }
            return result;
        }

        public static bool AnyDocumentWithUnsavedChanges()
        {
            bool result = false;
            lock (_lock_DocumentSwitcher)
            {
                result = Documents.Any(doc => !doc.IsDisposed && !doc.Hidden && doc.Proj.SAFE_AnyModified());
            }
            return result;
        }

        public static bool AnyDocumentsRequestingToClose()
        {
            bool result = false;
            lock (_lock_DocumentSwitcher)
            {
                result = Documents.Any(doc => !doc.IsDisposed && doc.RequestClose);
            }
            return result;
        }

        public static void RequestCloseAllDocuments()
        {
            lock (_lock_DocumentSwitcher)
            {
                foreach (var doc in Documents)
                {
                    if (doc.Hidden)
                        continue;
                    if (doc.EditorScreen.IsReadOnlyFileMode)
                        continue;
                    if (doc.IsDisposed)
                        continue;
                    doc.RequestClose = true;
                    //if (doc.Proj?.SAFE_AnyModified() == true)
                    //{
                    //    var dlgResult = AskUserAboutClosingDocument(doc);
                    //    if (dlgResult == DialogResult.Cancel)
                    //    {
                    //        userClickedCancel = true;
                    //        break;
                    //    }
                    //}
                }
            }
        }

        public static object _lock_CurrentDocument = new object();

        public static void Init()
        {
            lock (_lock_CurrentDocument)
            {
                lock (_lock_DocumentSwitcher)
                {
                    _hiddenDefaultDocument = new zzz_DocumentIns();
                    _hiddenDefaultDocument.ImguiTabDisplayName = "Empty";
                    _hiddenDefaultDocument.Hidden = true;

                    CurrentDocument = null;
                }
            }
        }

        internal static zzz_DocumentIns _hiddenDefaultDocument;
        private static zzz_DocumentIns _currentDocument;

        private static zzz_DocumentIns _imguiSelectedDocument = null;

        public static zzz_DocumentIns CurrentDocument
        {
            get
            {
                if (_currentDocument != null && Documents.Contains(_currentDocument))
                    return _currentDocument;
                else
                    return _hiddenDefaultDocument;
            }
            private set
            {
                if (value != _currentDocument)
                {
                    //if (_currentDocument != null)
                    //{
                    //    _currentDocument.BeforeSwitchingFromDoc();
                    //}
                    value.RequestInitAfterBeingSwitchedTo = true;
                    value.RequestInitAfterBeingSwitchedTo_IsFirstFrame = true;
                    if (value == _hiddenDefaultDocument)
                        _currentDocument = null;
                    else
                        _currentDocument = value;
                }
            }
        }

        public static zzz_DocumentIns PreviousDocument = null;

        private static object _lock_DocumentSwitcher = new object();
        private static List<zzz_DocumentIns> Documents = new();
        private static List<zzz_DocumentIns> DeadDocuments = new();

        private static zzz_DocumentIns RequestedDocumentToSwitchTo = null;

        //private static void RequestSwitchDocument(zzz_DocumentIns document)
        //{
        //    if (!Documents.Contains(document))
        //        Documents.Add(document);
        //    RequestedDocumentToSwitchTo = document;
        //}

        public static void DestroyAllDocs()
        {
            lock (_lock_DocumentSwitcher)
            {
                foreach (var doc in Documents)
                {
                    doc?.Dispose();
                }
                Documents.Clear();
            }
        }

        private static void GoToLastGoodDocument_NoLock(zzz_DocumentIns curDocument, bool immediate = false)
        {
            if (PreviousDocument != null && PreviousDocument != curDocument)
            {
                RequestedDocumentToSwitchTo = PreviousDocument;
            }
            else
            {
                var validDocs = Documents.Where(x => !x.IsDisposed && x != curDocument).ToList();
                if (validDocs.Count > 0)
                {
                    RequestedDocumentToSwitchTo = validDocs.Last();
                }
                else
                {
                    //AddDocument("Empty", hidden: true);
                    RequestedDocumentToSwitchTo = _hiddenDefaultDocument;
                }
            }


            PreviousDocument = null;

            if (immediate)
            {
                CurrentDocument = RequestedDocumentToSwitchTo;
                RequestedDocumentToSwitchTo = null;
            }
        }

        public static void KillCurrentDocForLoadFail()
        {
            lock (_lock_DocumentSwitcher)
            {
                var curDoc = CurrentDocument;

                if (Documents.Contains(CurrentDocument))
                    Documents.Remove(CurrentDocument);

                CurrentDocument?.Dispose();
                // Placeholder so stuff doesnt explode
                CurrentDocument = new zzz_DocumentIns();

                GoToLastGoodDocument_NoLock(curDoc);
            }
        }

        public static void AddDocument(string docName, bool hidden = false, bool immediateSwitch = false)
        {
            lock (_lock_DocumentSwitcher)
            {
                var doc = new zzz_DocumentIns();
                doc.ImguiTabDisplayName = docName;
                doc.Hidden = hidden;
                Documents.Add(doc);
                RequestedDocumentToSwitchTo = doc;

                if (immediateSwitch)
                {
                    lock (_lock_CurrentDocument)
                    {
                        PreviousDocument = CurrentDocument;
                        CurrentDocument = doc;
                        if (!hidden)
                            doc.RequestImguiTabSelect = true;
                        RequestedDocumentToSwitchTo = null;
                    }
                }
            }
            
        }

        private static DialogResult AskUserAboutClosingDocument(zzz_DocumentIns doc)
        {
            var confirmDlg = System.Windows.Forms.MessageBox.Show(
                                $"The file '{doc.ImguiTabDisplayName}' has " +
                                $"unsaved changes. Would you like to save these changes before " +
                                $"closing the file?", "Save Unsaved Changes?",
                                System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                                System.Windows.Forms.MessageBoxIcon.None);

            if (confirmDlg == System.Windows.Forms.DialogResult.Yes)
            {
                doc.EditorScreen.SaveCurrentFile(saveMessage: $"Saving '{doc.ImguiTabDisplayName}'...");
                //doc.Proj.SAFE_ClearAllModified();
                doc.RequestClose = false;
                doc.RequestClose_ForceDelete = false;
            }
            else if (confirmDlg == System.Windows.Forms.DialogResult.No)
            {
                doc.RequestClose = false;
                doc.RequestClose_ForceDelete = true;
            }
            else
            {
                doc.RequestClose = false;
                Main.REQUEST_EXIT_NEXT_IS_AUTOMATIC = false;
                Main.REQUEST_EXIT = false;
            }

            return confirmDlg;
        }

        public static void CloseDocument(zzz_DocumentIns doc)
        {

        }

        public static void UpdateDocuments()
        {
            lock (_lock_CurrentDocument)
            {
                lock (_lock_DocumentSwitcher)
                {
                    if (CurrentDocument != null && CurrentDocument.Proj != null)
                    {
                        foreach (var doc in Documents)
                        {
                            if (doc.IsDisposed)
                            {
                                DeadDocuments.Add(doc);
                                continue;
                            }
                            if (doc != CurrentDocument && (doc.Proj == null))
                            {
                                doc.RequestClose_ForceDelete = true;
                            }
                        }
                    }
                }
            }

            foreach (var doc in DeadDocuments)
            {
                
                Documents.Remove(doc);
                if (PreviousDocument == doc)
                    PreviousDocument = null;
                doc?.Dispose();
            }
            DeadDocuments.Clear();

            var userCanInteractWithTabs = !CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks() && !DialogManager.AnyDialogsShowing;

            if (RequestOpenFromPackedGameDataArchives && userCanInteractWithTabs)
            {
                RequestOpenFromPackedGameDataArchives = false;

                // Commented out because it actually calls RequestFileOpenRecent from within this.
                zzz_DocumentManager.AddDocument("New Document", hidden: true, immediateSwitch: true);

                var thing = new TaeLoadFromArchivesWizard();
                thing.StartInCenterOf(Main.WinForm);
                thing.ShowDialog();
                thing?.Dispose();

                //Task.Run(() => { }).ContinueWith((task) =>
                //{
                //    var thing = new TaeLoadFromArchivesWizard();
                //    thing.StartInCenterOf(Main.WinForm);
                //    thing.ShowDialog();
                //    thing?.Dispose();
                //}, TaskScheduler.FromCurrentSynchronizationContext());

            }

            if (RequestFileOpenBrowse && userCanInteractWithTabs)
            {
                RequestFileOpenBrowse = false;

                // Commented out because it actually calls RequestFileOpenRecent from within this.
                //zzz_DocumentManager.AddDocument("New Document", hidden: true, immediateSwitch: true);

                zzz_DocumentManager.AddDocument("New Document", hidden: true, immediateSwitch: true);

                CurrentDocument.EditorScreen.File_Open();

                return;
            }

            if (RequestFileOpenRecent && userCanInteractWithTabs)
            {
                var selectedFile = RequestFileOpenRecent_SelectedFile;
                var afterOpenAction = RequestFileOpenRecent_AfterOpenAction;

                RequestFileOpenRecent = false;
                RequestFileOpenRecent_SelectedFile = null;
                RequestFileOpenRecent_AfterOpenAction = (doc) => { };

                zzz_DocumentManager.AddDocument("New Document", hidden: true, immediateSwitch: true);

                if (CurrentDocument.EditorScreen.NewLoadFile_FromDocManager(selectedFile) == true)
                {

                    afterOpenAction?.Invoke(CurrentDocument);
                }

                return;
            }

            lock (_lock_DocumentSwitcher)
            {
                lock (_lock_CurrentDocument)
                {
                    if (_currentDocument != null && !Documents.Contains(_currentDocument))
                        Documents.Add(_currentDocument);


                    if (RequestedDocumentToSwitchTo != null && RequestedDocumentToSwitchTo != CurrentDocument)
                    {
                        lock (_lock_CurrentDocument)
                        {



                            if (userCanInteractWithTabs)
                            {
                                var switchTo = RequestedDocumentToSwitchTo;
                                RequestedDocumentToSwitchTo = null;
                                PreviousDocument = _currentDocument;
                                CurrentDocument = switchTo;

                            }

                        }
                    }

                    //List<zzz_DocumentIns> documentsToClose = new List<zzz_DocumentIns>();

                    bool earlierDocRequestedCloseCancel = false;

                    foreach (var doc in Documents)
                    {
                        if (doc.IsDisposed)
                            continue;

                        var proj = doc.Proj;
                        if (proj != null)
                        {
                            //doc.Proj?.CheckScanForErrorsQueue();
                            //proj.TimeSinceLastErrorCheck += Main.DELTA_UPDATE;
                            //if (proj.TimeSinceLastErrorCheck >= 2)
                            //{
                            //    proj.ScanForErrors_Background();
                            //    proj.TimeSinceLastErrorCheck = 0;
                            //}
                            proj.CheckScanForErrorsQueue();
                        }

                        bool docSelectedThisFrame = (doc == CurrentDocument && !doc.Hidden && !doc.LoadingTaskMan?.AnyInteractionBlockingTasks() == true);

                        if (doc.RequestInitAfterBeingSwitchedTo && !doc.LoadingTaskMan?.AnyInteractionBlockingTasks() == true)
                        {
                            doc.OnSwitchedToDoc();
                        }

                        if (earlierDocRequestedCloseCancel && doc.RequestClose && !doc.RequestClose_ForceDelete)
                        {
                            doc.RequestClose = false;
                        }

                        if (doc.RequestClose)
                        {
                            if (doc.Proj?.SAFE_AnyModified() != false)
                            {
                                var result = AskUserAboutClosingDocument(doc);
                                if (result == DialogResult.Cancel)
                                {
                                    earlierDocRequestedCloseCancel = true;
                                    doc.RequestClose = false;
                                    Main.REQUEST_EXIT_NEXT_IS_AUTOMATIC = false;
                                    Main.REQUEST_EXIT = false;
                                }
                            }
                            else
                            {
                                doc.RequestClose_ForceDelete = true;
                            }
                        }

                        if (doc.RequestClose_ForceDelete)
                        {
                            DeadDocuments.Add(doc);
                            if (CurrentDocument == doc)
                                GoToLastGoodDocument_NoLock(doc, immediate: true);
                            continue;
                        }

                        var docName = doc.EditorScreen?.NewFileContainerName;
                        if (doc.EditorScreen?.IsFileOpen == true && !string.IsNullOrWhiteSpace(docName))
                        {
                            doc.ImguiTabDisplayName = $"[{doc.GameRoot.GameType}]{Utils.GetShortIngameFileName(docName)}{(doc.Proj?.SAFE_AnyModified() == true ? "*" : "")}";
                            if (doc.WasHiddenPrevFrame)
                            {
                                doc.RequestImguiTabSelect = true;
                            }
                            doc.Hidden = false;
                        }

                        if (doc == CurrentDocument && _imguiSelectedDocument != doc)
                        {
                            doc.RequestImguiTabSelect = true;
                        }

                        doc.WasSelectedPrevFrame = docSelectedThisFrame;
                        doc.WasHiddenPrevFrame = doc.Hidden;
                    }

                }
            }
        }

        public static void DrawImgui(ref bool anyItemFocused)
        {
            lock (_lock_CurrentDocument)
            {
                lock (_lock_DocumentSwitcher)
                {
                    zzz_DocumentIns newImguiSelectedDocument = null;
                    // Iterate through documents, create tabs to click on
                    // When clicking on tab, call RequestSwitchDocument

                    if (ImGui.BeginTabBar("TabBar_DocumentManager", ImGuiTabBarFlags.Reorderable))
                    {
                        //_test = true;

                        int i = 0;
                        foreach (var doc in Documents)
                        {
                            if (doc.Hidden)
                                continue;

                            var flags = ImGuiTabItemFlags.None;

                            if (doc.Proj?.SAFE_AnyModified() == true)
                            {
                                flags |= ImGuiTabItemFlags.UnsavedDocument;
                            }

                            bool isOpen = true;

                            bool selectingThisFrame = false;

                            string tabLabel = $"{doc.ImguiTabDisplayName}###TabBar_DocumentManager__Tab_{doc.GUID}";

                            //if (CurrentDocument == doc)
                            //{
                            //    selectingThisFrame = true;
                            //    flags |= ImGuiTabItemFlags.SetSelected;
                            //}
                            //else
                            //{
                            //    ImGui.SetTabItemClosed(tabLabel);
                            //}

                            if (doc.RequestImguiTabSelect)
                            {
                                doc.RequestImguiTabSelect = false;
                                flags |= ImGuiTabItemFlags.SetSelected;
                            }

                            

                            var tabItemResult = ImGui.BeginTabItem(tabLabel, ref isOpen, flags);

                            //ImguiOSD.OSD.TooltipManager_Toolbox.DoTooltip($"test_DocmuentManager_Document_{doc.GUID}", "tooltip test?");
                            var docFullPath = doc.EditorScreen?.FileContainer?.Info?.GetMainBinderName();

                            if (ImGui.IsItemHovered() && !string.IsNullOrWhiteSpace(docFullPath))
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(docFullPath);
                                ImGui.EndTooltip();
                            }

                            if (!isOpen)
                            {
                                doc.RequestClose = true;
                            }

                            anyItemFocused |= ImGui.IsItemFocused();

                            if (ImGui.IsItemClicked() && CurrentDocument != doc)
                            {
                                RequestedDocumentToSwitchTo = doc;
                            }

                            if (tabItemResult)
                            {
                                newImguiSelectedDocument = doc;
                                ImGui.EndTabItem();
                            }

                            i++;
                        }


                        ImGui.EndTabBar();
                    }


                    _imguiSelectedDocument = newImguiSelectedDocument;
                }
            }
        }
    }
}
