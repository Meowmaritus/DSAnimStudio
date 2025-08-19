using System;
using System.Collections.Generic;
using System.Linq;
using DSAnimStudio.ImguiOSD;

namespace DSAnimStudio
{

    public class TimeActRecorder
    {
        public struct Entry
        {
            public SplitAnimID AnimID;
            public float StartTime;
            public float EndTime;
        }

        public static DSAProj.Animation RecordActions(
            TaeEditor.TaeEditorScreen mainScreen, SplitAnimID destID, string shortName, List<Entry> entries)
        {
            var anim = mainScreen.CreateNewAnimWithFullID(destID, "[REC]" + shortName, nukeExisting: true);

            anim.SAFE_SetIsModified(true);
    
            DSAProj.Action blendAction = null;
            //List<TaeComboEntry> validComboEntries = new List<TaeComboEntry>();
            List<List<DSAProj.Action>> actionLists = new List<List<DSAProj.Action>>();
            List<List<DSAProj.Action>> actionLists_BeforeStart = new List<List<DSAProj.Action>>();
            List<List<DSAProj.Action>> actionLists_AfterEnd = new List<List<DSAProj.Action>>();

            int toFrame(float t)
            {
                return (int)Math.Round(t / (1f / 30f));
            }

            float toTime(int f)
            {
                return (float)(f * (1f / 30f));
            }

            float currentOutputAnimTime = 0;

            int currentActionListIndex = 0;

            var cloneToOriginalMap = new Dictionary<DSAProj.Action, DSAProj.Action>();

            foreach (var entry in entries)
            {
                float comboClipLength = (entry.EndTime - entry.StartTime);
            }
            
            foreach (var entry in entries)
            {
                float comboClipLength = (entry.EndTime - entry.StartTime);

                var newActionList = new List<DSAProj.Action>();
                var newActionList_BeforeStart = new List<DSAProj.Action>();
                var newActionList_AfterEnd = new List<DSAProj.Action>();

                int timespanMinFrame = toFrame(entry.StartTime);
                int timespanMaxFrame = toFrame(entry.EndTime);
                var animRef = mainScreen.Proj.SAFE_GetFirstAnimationFromFullID(entry.AnimID);

                animRef.SafeAccessActions(actions =>
                {
                    foreach (var act in actions)
                    {
                        if (act.Type == 16)
                        {
                            if (blendAction == null)
                                blendAction = act.GetClone(readFromTemplate: true);

                            continue;
                        }
                        //int actStartFrame = toFrame(act.StartTime);
                        //int actEndFrame = toFrame(act.EndTime);
                        //bool startsInThis = actStartFrame >= timespanMinFrame;
                        //bool endsInThis = actEndFrame <= timespanMaxFrame;



                        if (act.EndTime >= entry.StartTime && act.StartTime <= entry.EndTime)
                        {
                            var actClone = act.GetClone(readFromTemplate: true);

                            cloneToOriginalMap[actClone] = act;

                            newActionList.Add(actClone);

                            //Slide action over so it's relative to time 0
                            actClone.StartTime -= entry.StartTime;
                            actClone.EndTime -= entry.StartTime;
                            // Trim actions that start before this clip
                            if (actClone.StartTime < 0)
                            {
                                actClone.StartTime = 0;
                                newActionList_BeforeStart.Add(actClone);
                            }
                            //Trim actions that end after this clip
                            if (actClone.EndTime > comboClipLength)
                            {
                                actClone.EndTime = comboClipLength;
                                newActionList_AfterEnd.Add(actClone);
                            }

                            // Shift action to final clip position in output anim
                            actClone.StartTime += currentOutputAnimTime;
                            actClone.EndTime += currentOutputAnimTime;
                        }

                    }

                    if (currentActionListIndex > 0)
                    {
                        var prevClipActionsContinuingIntoThisOne = actionLists_AfterEnd[currentActionListIndex - 1];
                        foreach (var act in prevClipActionsContinuingIntoThisOne)
                        {
                            var matchingContinuationEv = newActionList_BeforeStart.FirstOrDefault(e => e.IsIdenticalTo(act));
                            if (matchingContinuationEv != null)
                            {
                                act.EndTime = matchingContinuationEv.EndTime;
                                newActionList_BeforeStart.Remove(act);
                                newActionList.Remove(act);
                            }
                        }
                    }

                    actionLists.Add(newActionList);
                    actionLists_BeforeStart.Add(newActionList_BeforeStart);
                    actionLists_AfterEnd.Add(newActionList_AfterEnd);

                    currentActionListIndex++;

                    currentOutputAnimTime += comboClipLength;
                });

                
            }



            anim.SafeAccessActions(animActions =>
            {
                for (int i = 0; i < actionLists.Count; i++)
                {

                    foreach (var ev in actionLists[i])
                    {
                        if (!animActions.Contains(ev))
                            animActions.Add(ev);
                    }

                    //foreach (var ev in eventLists_BeforeStart[i])
                    //{
                    //    if (!anim.Events.Contains(ev))
                    //        anim.Events.Add(ev);
                    //}

                    //foreach (var ev in eventLists_AfterEnd[i])
                    //{
                    //    if (!anim.Events.Contains(ev))
                    //        anim.Events.Add(ev);
                    //}
                }




                //anim.Actions = anim.Actions.OrderBy(e => rowCacheEntries.ContainsKey(cloneToOriginalMap[e]) ? rowCacheEntries[cloneToOriginalMap[e]] : -1).ToList();



                if (blendAction != null)
                {
                    blendAction.StartTime = 0;
                    blendAction.EndTime = (float)(8.0 * (1.0 / 30.0));
                    animActions.Insert(0, blendAction);
                }
            });
            
            

            return anim;
        }
    }
}