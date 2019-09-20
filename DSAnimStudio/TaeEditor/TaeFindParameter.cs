using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeFindParameter
    {
        
        public static List<TaeFindResult> Find(TaeEditorScreen editor, string paramVal, bool requireWholeValue)
        {
            List<TaeFindResult> result = new List<TaeFindResult>();
            
            foreach (var taeKvp in editor.FileContainer.AllTAEDict)
            {
                var tae = taeKvp.Value;
                foreach (var anim in tae.Animations)
                {
                    foreach (var ev in anim.Events)
                    {
                        if (ev.Parameters == null)
                            continue;
                        foreach (var kvp in ev.Parameters.Template)
                        {
                            if (kvp.Value.ValueToAssert == null)
                            {
                                var thisVal = kvp.Value.ValueToString(ev.Parameters[kvp.Key]);
                                if (requireWholeValue ? thisVal.Equals(paramVal) : thisVal.Contains(paramVal))
                                {
                                    var animID_Lower = GameDataManager.GameTypeHasLongAnimIDs
                                        ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                                    var animID_Upper = taeKvp.Key.StartsWith("a") ?
                                        long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeKvp.Key).Substring(1)) 
                                        : ((GameDataManager.GameTypeHasLongAnimIDs
                                        ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));


                                    result.Add(new TaeFindResult()
                                    {
                                        TAEName = Utils.GetFileNameWithoutDirectoryOrExtension(taeKvp.Key),
                                        TAERef = tae,
                                        AnimRef = anim,
                                        AnimName = (GameDataManager.GameTypeHasLongAnimIDs
                                            ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}"),
                                        EventTypeString = ev.TypeName,
                                        EventRef = ev,
                                        ParameterName = kvp.Key,
                                        MatchedValue = thisVal,
                                    });
                                }

                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class TaeFindResult
    {
        public string TAEName;
        public TAE TAERef;
        public string AnimName;
        public TAE.Animation AnimRef;
        public string EventTypeString;
        public TAE.Event EventRef;
        public string ParameterName;
        public string MatchedValue;

        public string[] ToListViewData()
        {
            return new string[]
            {
                TAEName,
                AnimName,
                EventTypeString,
                ParameterName,
                MatchedValue,
            };
        }

        public void GoThere(TaeEditorScreen editor)
        {
            editor.SelectNewAnimRef(TAERef, AnimRef);
            editor.SelectEvent(EventRef);
        }
    }
}
