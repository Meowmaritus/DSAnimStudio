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
                            GameRoot.SuppressNextInterrootBrowsePrompt = true;
                            TAE_EDITOR.SuppressNextModelOverridePrompt = true;
                        }

                        if (ParsedQuery.Get("c") != null)
                        {
                            TAE_EDITOR.FileContainerName = ParsedQuery.Get("c");
                        }

                        if (ParsedQuery.Get("chr") != null)
                        {
                            TAE_EDITOR.FileContainerName_Model = ParsedQuery.Get("chr");
                        }

                        var MostRecentContainer = Main.Config.RecentFilesList[0];

                        if (MostRecentContainer != null && TAE_EDITOR.FileContainerName == "")
                        {
                            TAE_EDITOR.FileContainerName = MostRecentContainer.TaeFile;
                        }

                        if (TAE_EDITOR.FileContainerName_Model == "")
                        {
                            if (MostRecentContainer.ModelFile != null)
                            {
                                TAE_EDITOR.FileContainerName_Model = MostRecentContainer.ModelFile;
                            }
                            else
                            {
                                TAE_EDITOR.FileContainerName_Model = "/chr/c0000.chrbnd.dcx";
                            }
                        }

                        LoadingTaskMan.DoLoadingTask("DSASURILoader", "Loading ANIBND and associated model(s) from URI...", progress =>
                        {
                            TAE_EDITOR.LoadCurrentFile();

                            var GoToAnimationIdParam = ParsedQuery.Get("id");
                            if (GoToAnimationIdParam != null)
                            {
                                string GoToAnimationIdString = GoToAnimationIdParam.Replace("_", "").Replace("a", "");
                                if (int.TryParse(GoToAnimationIdString, out int id))
                                {
                                    TAE_EDITOR.GotoAnimID(id, true, true, out _);
                                }
                            }
                        }, disableProgressBarByDefault: true);

                        return true;
                    }
            }

            return false;
        }
    }
}
