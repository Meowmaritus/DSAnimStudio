using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class LoadingTaskMan
    {
        public class LoadingTask
        {
            public string DisplayString;
            public double ProgressRatio { get; private set; }
            public bool IsComplete { get; private set; }
            public bool IsBeingKilledManually = false;
            Thread taskThread;

            public LoadingTask(string displayString, Action<IProgress<double>> doLoad)
            {
                DisplayString = displayString;
                IProgress<double> prog = new Progress<double>(x =>
                {
                    ProgressRatio = x;
                    if (IsBeingKilledManually)
                    {
                        Kill();
                        IsComplete = true;
                    }
                });

                taskThread = new Thread(() =>
                {
                    doLoad.Invoke(prog);
                    // We don't check ProgressRatio to see if it's done, since
                    // the thread is INSTANTLY KILLED when complete, which would
                    // cause slight progress rounding errors to destroy the
                    // entire universe. Instead, it's only considered done 
                    // after the entire doLoad is complete.
                    IsComplete = true;
                });

                taskThread.IsBackground = true;
                taskThread.Start();
            }

            public void Kill()
            {
                if (taskThread != null && taskThread.IsAlive)
                    taskThread.Abort();
            }
        }

        internal static object _lock_TaskDictEdit = new object();

        public static Dictionary<string, LoadingTask> TaskDict = new Dictionary<string, LoadingTask>();

        public static bool AnyTasksRunning()
        {
            bool result = false;
            lock (_lock_TaskDictEdit)
            {
                result = TaskDict.Count > 0;
            }
            return result;
        }

        /// <summary>
        /// Starts a loading task if that task wasn't already running.
        /// </summary>
        /// <param name="taskKey">String key to reference task by. This is what determines if it's running already.</param>
        /// <param name="displayString">String to actually show onscreen next to the progress bar.</param>
        /// <param name="taskDelegate">The actual task to perform. Be sure to report a progress of 1.0 in the IProgress when done.</param>
        /// <returns>True if the task was just started. False if it was already running.</returns>
        public static bool DoLoadingTask(string taskKey, string displayString, Action<IProgress<double>> taskDelegate, int addFluffMilliseconds = 100)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                    return false;
                // As soon as the LoadingTask is created it starts.
                TaskDict.Add(taskKey, new LoadingTask(displayString, progress =>
                {
                    taskDelegate.Invoke(progress);
                    if (addFluffMilliseconds > 0)
                        Thread.Sleep(addFluffMilliseconds);
                }));
            }

            return true;
        }

        /// <summary>
        /// Starts a loading task if that task wasn't already running.
        /// </summary>
        /// <param name="taskKey">String key to reference task by. This is what determines if it's running already.</param>
        /// <param name="displayString">String to actually show onscreen next to the progress bar.</param>
        /// <param name="taskDelegate">The actual task to perform. Be sure to report a progress of 1.0 in the IProgress when done.</param>
        /// <returns>True if the task was just started. False if it was already running.</returns>
        public static bool DoLoadingTaskSynchronous(string taskKey, string displayString, Action<IProgress<double>> taskDelegate, int addFluffMilliseconds = 100)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                    return false;
                // As soon as the LoadingTask is created it starts.
                TaskDict.Add(taskKey, new LoadingTask(displayString, progress =>
                {
                    taskDelegate.Invoke(progress);
                    if (addFluffMilliseconds > 0)
                        Thread.Sleep(addFluffMilliseconds);
                }));
            }

            bool isTaskDone = false;
            while (!isTaskDone)
            {
                // Lock here when checking to prevent it from freaking out trying to enumerate
                // a dictionary which is being modified :fatcat:
                lock (_lock_TaskDictEdit)
                {
                    isTaskDone = !TaskDict.ContainsKey(taskKey);
                }

                Thread.Sleep(100);
            }

            return true;
        }

        public static bool KillTask(string taskKey)
        {
            if (IsTaskRunning(taskKey))
            {
                lock (_lock_TaskDictEdit)
                {
                    TaskDict[taskKey].IsBeingKilledManually = true;
                }
                return true;
            }

            return false;
        }

        public static bool IsTaskRunning(string taskKey)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                {
                    // While we're here, might as well double check if that task is done.
                    if (TaskDict[taskKey].IsComplete)
                    {
                        TaskDict[taskKey].Kill();
                        TaskDict.Remove(taskKey);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public static void Update(float elapsedTime)
        {
            lock (_lock_TaskDictEdit)
            {
                // Cleanup finished tasks
                var keyList = TaskDict.Keys;
                var keysOfTasksThatAreFinished = new List<string>();
                foreach (var key in keyList)
                {
                    if (TaskDict[key].IsComplete)
                    {
                        TaskDict[key].Kill();
                        keysOfTasksThatAreFinished.Add(key);
                    }
                }
                foreach (var key in keysOfTasksThatAreFinished)
                {
                    TaskDict.Remove(key);
                }
            }
        }

        public const int GuiDistFromEdgesOfScreenX = 8;
        public const int GuiDistFromEdgesOfScreenY = 20;
        public const int GuiDistBetweenProgressRects = 8;
        public const int GuiTaskRectWidth = 360;
        public const int GuiTaskRectHeight = 64;
        public const int GuiProgBarHeight = 20;
        public const int GuiProgBarDistFromRectEdge = 8;
        public const int GuiProgBarEdgeThickness = 2;
        public const float GuiProgNameDistFromEdge = 8;

        public static void DrawAllTasks()
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.Count > 0)
                {
                    GFX.SpriteBatch.Begin();

                    int i = 0;
                    foreach (var kvp in TaskDict)
                    {
                        // Draw Task Rect
                        Rectangle thisTaskRect = new Rectangle(
                            GFX.Device.Viewport.Width - GuiTaskRectWidth - GuiDistFromEdgesOfScreenX,
                            GuiDistFromEdgesOfScreenY + ((GuiDistBetweenProgressRects + GuiTaskRectHeight) * i),
                            GuiTaskRectWidth, GuiTaskRectHeight);

                        GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, thisTaskRect, Color.Black * 0.85f);

                        // Draw Progress Background Rect

                        Rectangle progBackgroundRect = new Rectangle(thisTaskRect.X + GuiProgBarDistFromRectEdge,
                            thisTaskRect.Y + GuiTaskRectHeight - GuiProgBarDistFromRectEdge - GuiProgBarHeight,
                            thisTaskRect.Width - (GuiProgBarDistFromRectEdge * 2), GuiProgBarHeight);

                        GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, progBackgroundRect, new Color(0.25f, 0.25f, 0.25f) * 0.95f);

                        // Draw Progress Foreground Rect

                        Rectangle progForegroundRect = new Rectangle(
                            progBackgroundRect.X + GuiProgBarEdgeThickness,
                            progBackgroundRect.Y + GuiProgBarEdgeThickness,
                            (int)((progBackgroundRect.Width - (GuiProgBarEdgeThickness * 2)) * kvp.Value.ProgressRatio),
                            progBackgroundRect.Height - (GuiProgBarEdgeThickness * 2));

                        GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, progForegroundRect, 
                            kvp.Value.IsBeingKilledManually ? Color.Red : Color.White);

                        // Draw Task Name

                        Vector2 taskNamePos = new Vector2(thisTaskRect.X + GuiProgNameDistFromEdge, thisTaskRect.Y + GuiProgNameDistFromEdge);

                        DBG.DrawOutlinedText(kvp.Value.DisplayString, taskNamePos,
                            Color.White, DBG.DEBUG_FONT_SMALL, startAndEndSpriteBatchForMe: false);

                        i++;
                    }

                    GFX.SpriteBatch.End();
                }
            }
        }
    }
}
