using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeFindParameter
    {

        public static List<TaeFindResult> FindEventTypeNum(TaeEditorScreen editor, int eventType)
        {
            List<TaeFindResult> result = new List<TaeFindResult>();

            foreach (var taeKvp in editor.FileContainer.AllTAEDict)
            {
                var tae = taeKvp.Value;
                foreach (var anim in tae.Animations)
                {
                    foreach (var ev in anim.Events)
                    {
                        if (ev.Type == eventType)
                        {
                            var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
                                ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                            var animID_Upper = taeKvp.Key.StartsWith("a") ?
                                long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeKvp.Key).Substring(1))
                                : ((GameRoot.GameTypeHasLongAnimIDs
                                ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));

                            var sb = new StringBuilder();

                            if (ev.Template != null)
                            {
                                sb.Append($"{ev.Template.Name}[{ev.Type}](");

                                bool first = true;
                                foreach (var kvp in ev.Parameters.Template)
                                {
                                    if (kvp.Value.ValueToAssert == null)
                                    {
                                        if (first)
                                            first = false;
                                        else
                                            sb.Append(", ");

                                        sb.Append(ev.Parameters[kvp.Key].ToString());
                                    }
                                }
                                sb.Append(")");
                            }
                            else
                            {
                                sb.Append($"[{ev.Type}]({string.Join(" ", ev.GetParameterBytes(false).Select(b => b.ToString("X2")))})");
                            }

                            

                            result.Add(new TaeFindResult()
                            {
                                TAEName = Utils.GetFileNameWithoutDirectoryOrExtension(taeKvp.Key),
                                TAERef = tae,
                                AnimRef = anim,
                                AnimName = (GameRoot.GameTypeHasLongAnimIDs
                                    ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}"),
                                EventTypeString = ev.TypeName,
                                EventRef = ev,
                                ParameterName = "Event Type Num",
                                MatchedValue = sb.ToString(),
                            });
                        }
                    }
                }
            }
            return result;
        }

        public static List<TaeFindResult> FindEventTypeName(TaeEditorScreen editor, string paramVal, bool requireWholeValue)
        {
            List<TaeFindResult> result = new List<TaeFindResult>();

            foreach (var taeKvp in editor.FileContainer.AllTAEDict)
            {
                var tae = taeKvp.Value;
                foreach (var anim in tae.Animations)
                {
                    foreach (var ev in anim.Events.Where(evt => evt.Template != null))
                    {
                        if (requireWholeValue ? ev.TypeName.Equals(paramVal) : ev.TypeName.Contains(paramVal))
                        {
                            var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
                                ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                            var animID_Upper = taeKvp.Key.StartsWith("a") ?
                                long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeKvp.Key).Substring(1))
                                : ((GameRoot.GameTypeHasLongAnimIDs
                                ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));

                            var sb = new StringBuilder($"{ev.Template.Name}[{ev.Type}](");
                            bool first = true;
                            foreach (var kvp in ev.Parameters.Template)
                            {
                                if (kvp.Value.ValueToAssert == null)
                                {
                                    if (first)
                                        first = false;
                                    else
                                        sb.Append(", ");

                                    sb.Append(ev.Parameters[kvp.Key].ToString());
                                }
                            }
                            sb.Append(")");

                            result.Add(new TaeFindResult()
                            {
                                TAEName = Utils.GetFileNameWithoutDirectoryOrExtension(taeKvp.Key),
                                TAERef = tae,
                                AnimRef = anim,
                                AnimName = (GameRoot.GameTypeHasLongAnimIDs
                                    ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}"),
                                EventTypeString = ev.TypeName,
                                EventRef = ev,
                                ParameterName = "Event Name",
                                MatchedValue = sb.ToString(),
                            });
                        }
                    }
                }
            }
            return result;
        }

        public static List<TaeFindResult> FindParameterName(TaeEditorScreen editor, string paramName, bool requireWholeValue)
        {
            List<TaeFindResult> result = new List<TaeFindResult>();

            string eventTypeNameSearch = null;

            if (paramName.Contains("."))
            {
                string[] split = paramName.Split('.');
                if (split.Length >= 2)
                {
                    eventTypeNameSearch = split[0];
                    paramName = split[1];
                }
            }

            foreach (var taeKvp in editor.FileContainer.AllTAEDict)
            {
                var tae = taeKvp.Value;
                foreach (var anim in tae.Animations)
                {
                    foreach (var ev in anim.Events)
                    {
                        if (ev.Parameters == null)
                            continue;

                        if (eventTypeNameSearch != null)
                        {
                            if (ev.TypeName == null)
                                continue;

                            if (!(requireWholeValue ? ev.TypeName.Equals(eventTypeNameSearch) : ev.TypeName.Contains(eventTypeNameSearch)))
                            {
                                continue;
                            }
                        }

                        foreach (var kvp in ev.Parameters.Template)
                        {
                            if (kvp.Value.ValueToAssert == null)
                            {
                                var thisVal = kvp.Value.ValueToString(ev.Parameters[kvp.Key]);
                                if (requireWholeValue ? kvp.Key.Equals(paramName) : kvp.Key.Contains(paramName))
                                {
                                    var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
                                        ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                                    var animID_Upper = taeKvp.Key.StartsWith("a") ?
                                        long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeKvp.Key).Substring(1))
                                        : ((GameRoot.GameTypeHasLongAnimIDs
                                        ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));


                                    result.Add(new TaeFindResult()
                                    {
                                        TAEName = Utils.GetFileNameWithoutDirectoryOrExtension(taeKvp.Key),
                                        TAERef = tae,
                                        AnimRef = anim,
                                        AnimName = (GameRoot.GameTypeHasLongAnimIDs
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

        public static List<TaeFindResult> FindParameterValue(TaeEditorScreen editor, string paramVal, bool requireWholeValue)
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
                                    var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
                                        ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                                    var animID_Upper = taeKvp.Key.StartsWith("a") ?
                                        long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeKvp.Key).Substring(1)) 
                                        : ((GameRoot.GameTypeHasLongAnimIDs
                                        ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));


                                    result.Add(new TaeFindResult()
                                    {
                                        TAEName = Utils.GetFileNameWithoutDirectoryOrExtension(taeKvp.Key),
                                        TAERef = tae,
                                        AnimRef = anim,
                                        AnimName = (GameRoot.GameTypeHasLongAnimIDs
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
