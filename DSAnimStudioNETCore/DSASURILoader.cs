using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using System.Web;

namespace DSAnimStudio
{
    /**
     * URI format is:
     * dsas://<action>?param1=<value1>&param2=<value>
     * e.g. dsas://load
     */
    public class DSASURILoader
    {
        Uri ParsedURI;
        System.Collections.Specialized.NameValueCollection ParsedQuery;
        TaeEditor.TaeEditorScreen TAE_EDITOR;

        public DSASURILoader(string InputURI, TaeEditor.TaeEditorScreen TaeEditor)
        {
            ParsedURI = new Uri(InputURI);
            ParsedQuery = HttpUtility.ParseQueryString(ParsedURI.Query);
            TAE_EDITOR = TaeEditor;
        }

        public static bool IsValid(string UriToCheck)
        {
            return UriToCheck.StartsWith("dsas://");
        }

        public bool Process()
        {
            switch (ParsedURI.Host)
            {
                case "open":
                    {
                        if (ParsedQuery.Get("suppress_prompts") != null)
                        {
                            zzz_DocumentManager.CurrentDocument.GameRoot.SuppressNextInterrootBrowsePrompt = true;
                            TAE_EDITOR.SuppressNextModelOverridePrompt = true;
                        }

                        DSAProj.TaeContainerInfo containerInfo = null;
                        //var containerType = DSAProj.TaeContainerInfo.ContainerTypes.Anibnd;

                        if (ParsedQuery.Get("t") != null)
                        {
                            string t = ParsedQuery.Get("t");
                            if (t == "chr")
                            {
                                string anibndPath = ParsedQuery.Get("ani");
                                string chrPath = ParsedQuery.Get("chr");
                                containerInfo = new DSAProj.TaeContainerInfo.ContainerAnibnd(anibndPath, chrPath);
                            }
                            else if (t == "parts")
                            {
                                string partsbndPath = ParsedQuery.Get("part");
                                string bindIdStr = ParsedQuery.Get("ani_id");
                                if (bindIdStr != null)
                                {
                                    int bindID = int.Parse(bindIdStr);
                                    containerInfo = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(partsbndPath, bindID);
                                }

                            }
                            else if (t == "obj")
                            {
                                string objbndPath = ParsedQuery.Get("obj");
                                string bindIdStr = ParsedQuery.Get("ani_id");
                                if (bindIdStr != null)
                                {
                                    int bindID = int.Parse(bindIdStr);
                                    containerInfo = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(objbndPath, bindID);
                                }

                            }
                        }

                        if (containerInfo == null)
                        {
                            if (Main.Config.RecentFilesList.Count > 0)
                            {
                                containerInfo = Main.Config.RecentFilesList[0];
                            }
                        }
                        if (containerInfo != null)
                        {
                            //zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DoLoadingTask("DSASURILoader", "Loading ANIBND and associated model(s) from URI...", progress =>
                            //{



                            //}, disableProgressBarByDefault: true);

                            zzz_DocumentManager.RequestFileOpenRecent = true;
                            zzz_DocumentManager.RequestFileOpenRecent_SelectedFile = containerInfo;
                            zzz_DocumentManager.RequestFileOpenRecent_AfterOpenAction += (doc) =>
                            {
                                var GoToAnimationIdParam = ParsedQuery.Get("anim");
                                if (GoToAnimationIdParam != null)
                                {
                                    string GoToAnimationIdString = GoToAnimationIdParam.Replace("_", "").Replace("a", "");
                                    if (int.TryParse(GoToAnimationIdString, out int id))
                                    {
                                        doc.EditorScreen.GotoAnimID(SplitAnimID.FromFullID(doc.GameRoot, id), true, true, out _);
                                    }
                                }
                            };
                        }

                            

                        return true;
                    }
            }

            return false;
        }
    }
}
