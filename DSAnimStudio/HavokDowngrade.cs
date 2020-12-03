using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class HavokDowngrade
    {
        private static object _lock_DoingTask = new object();
        private static object _lock_animFinished = new object();
        private static object _lock_processQueue = new object();
        private static object _lock_messageBoxSpamPrevent = new object();
        private static bool _messageBoxSpamCancel = false;

        static string TAGTOOLS_DIR => $@"{Main.Directory}\Res\TagTools";
        static string TAGTOOLS_EXE => $@"{TAGTOOLS_DIR}\TagTools.exe";

        static string TEMPDIR => $@"{Main.Directory}\HavokDowngradeTEMP";

        static string pathCompendium => $@"{TEMPDIR}\in.compendium";

        const int PARALLEL_CONVERSIONS = 10;

        public static bool SimpleCheckIfHkxBytesAre2015(byte[] bytes)
        {
            //"TAG0", container name in 2015+
            return bytes[4] == 0x54 && bytes[5] == 0x41 && bytes[6] == 0x47 && bytes[7] == 0x30;
        }

        static void CreateTempIfNeeded()
        {
            if (!Directory.Exists(TEMPDIR))
                Directory.CreateDirectory(TEMPDIR);
        }

        static void DeleteTempIfNeeded()
        {
            if (Directory.Exists(TEMPDIR))
                Directory.Delete(TEMPDIR, true);
        }

        private static void SaveCompendiumToTemp(byte[] compendium)
        {
            File.WriteAllBytes(pathCompendium, compendium);
        }

        private static Process GetDowngradeHkxProcess(string debug_hkxName, byte[] hkx, 
            bool useCompendium, int i, Action<Process, byte[], int> whatToDoWithResult, bool isUpgrade = false)
        {
            // Change from 20150100 to 20160000 fatcat
            //hkx[0x13] = 0x36;
            //hkx[0x15] = 0x30;

            string workingDirectory = $@"{TEMPDIR}\{i}";

            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            string pathHkx = $@"{workingDirectory}\in.hkx";
            string pathOut = $@"{workingDirectory}\out.hkx";

            File.WriteAllBytes(pathHkx, hkx);

            var procStart = new ProcessStartInfo(TAGTOOLS_EXE,
                useCompendium ? $"\"{pathHkx}\" \"{pathCompendium}\" \"{pathOut}\""
                : $"\"{pathHkx}\" \"{pathOut}\"");

            procStart.CreateNoWindow = true;
            procStart.WindowStyle = ProcessWindowStyle.Hidden;
            procStart.UseShellExecute = false;
            procStart.RedirectStandardOutput = true;
            procStart.RedirectStandardError = true;
            procStart.WorkingDirectory = workingDirectory;

            var proc = new Process();
            proc.StartInfo = procStart;
            proc.EnableRaisingEvents = true;

            

            proc.OutputDataReceived += (o, e) =>
            {
                Console.WriteLine("OutputDataReceived => " + e.Data);
            };

            proc.ErrorDataReceived += (o, e) =>
            {
                Console.WriteLine("ErrorDataReceived => " + e.Data);
            };

            string copyOfHkxName = debug_hkxName;
            int j = i;
            proc.Exited += (o, e) =>
            {
                try
                {
                    var hkxName = copyOfHkxName;
                    var standardOutput = proc.StandardOutput.ReadToEnd();
                    var standardError = proc.StandardError.ReadToEnd();
                    byte[] result = File.ReadAllBytes(pathOut);
                    //Patch "hk_2012.2.0-r1" to "hk_2010.2.0-r1"
                    if (!isUpgrade)
                        result[0x2E] = 0x30;
                    whatToDoWithResult.Invoke(proc, result, j);
                }
                catch (FileNotFoundException)
                {
                    bool wasSpamCancelled = false;
                    lock (_lock_messageBoxSpamPrevent)
                    {
                        if (_messageBoxSpamCancel)
                        {
                            wasSpamCancelled = true;
                        }
                        else
                        {
                            _messageBoxSpamCancel = true;

                            System.Windows.Forms.MessageBox.Show($"TagTools failed to downgrade '{debug_hkxName}'.\n" +
                                "If this is DS1R, try copying the file from PTDE and saving it as '<filename>.2010'.\n" +
                                "If this is an SDT file, I guess you're just out of luck.", "Failed to Downgrade",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }

                    if (!wasSpamCancelled)
                    {
                        whatToDoWithResult.Invoke(proc, null, j);
                    }
                }
            };
            
            //proc.Start();

            return proc;
        }

        //private static byte[] WaitForDowngradeHKXFinishAndRead(Process proc, int i)
        //{
        //    string pathOut = $@"{TEMPDIR}\out{i}.hkx";

        //    proc.WaitForExit();

            
        //}

        public static byte[] UpgradeHkx2010to2015(byte[] hkx2010)
        {
            byte[] resultBytes = null;
            var newGuid = Guid.NewGuid().ToString();
            var proc = GetDowngradeHkxProcess(newGuid + ".2010.hkx", hkx2010, false, 0, (convertProc, result, j) =>
            {
                resultBytes = result;
            }, isUpgrade: true);

            proc.Start();
            proc.WaitForExit();

            return resultBytes;
        }

        public static byte[] DowngradeSingleFileInANIBND(IBinder anibnd, BinderFile file, bool isUpgrade)
        {
            byte[] resultBytes = null;

            lock (_lock_DoingTask)
            {
                BinderFile compendium = null;

                foreach (var f in anibnd.Files)
                {
                    var nameCheck = f.Name.ToUpper();

                    if (nameCheck.EndsWith(".COMPENDIUM"))
                    {
                        compendium = f;
                    }
                }

                CreateTempIfNeeded();

                if (compendium != null)
                {
                    SaveCompendiumToTemp(compendium.Bytes);
                }

                

                var proc = GetDowngradeHkxProcess(file.Name, file.Bytes, compendium != null, 0, (convertProc, result, j) => 
                {
                    resultBytes = result;
                }, isUpgrade);

                proc.Start();
                proc.WaitForExit();

                
            }

            return resultBytes;
        }


        public static void DowngradeAnibnd(string anibndPath, IProgress<double> prog = null)
        {
            lock (_lock_DoingTask)
            {
                lock (_lock_messageBoxSpamPrevent)
                    _messageBoxSpamCancel = false;

                IBinder anibnd = null;

                if (BND3.Is(anibndPath))
                    anibnd = BND3.Read(anibndPath);
                else if (BND4.Is(anibndPath))
                    anibnd = BND4.Read(anibndPath);
                else
                    throw new Exception("Invalid ANIBND");

                List<BinderFile> animations = new List<BinderFile>();
                BinderFile skeleton = null;
                BinderFile compendium = null;

                foreach (var f in anibnd.Files)
                {
                    var nameCheck = f.Name.ToUpper();

                    if (nameCheck.EndsWith(".COMPENDIUM"))
                    {
                        compendium = f;
                    }
                    else if (nameCheck.EndsWith("SKELETON.HKX"))
                    {
                        skeleton = f;
                    }
                    else if (nameCheck.EndsWith(".HKX"))
                    {
                        animations.Add(f);
                    }
                }

                double progTotalFileCount = animations.Count + 1;

                if (skeleton != null)
                    progTotalFileCount++;

                CreateTempIfNeeded();

                bool converted_skeleton = false;
                bool[] converted_anim = new bool[animations.Count];

                try
                {
                    if (compendium != null)
                    {
                        SaveCompendiumToTemp(compendium.Bytes);
                    }

                    bool wasCanceled = false;
                    List<Process> processQueue = new List<Process>();
                    List<Process> processDeleteQueue = new List<Process>();

                    if (skeleton != null)
                    {
                        var skellingtonProc = GetDowngradeHkxProcess(skeleton.Name, skeleton.Bytes, compendium != null, -1, (convertProc, result, i) =>
                        {
                            lock (_lock_animFinished)
                            {
                                if (result == null)
                                {

                                    wasCanceled = true;
                                    return;
                                }
                                else
                                {
                                    skeleton.Bytes = result;
                                    converted_skeleton = true;
                                }
                            }
                        });

                        lock (_lock_processQueue)
                        {
                            processQueue.Add(skellingtonProc);
                        }

                        skellingtonProc.Start();
                    }

                    prog.Report(1 / progTotalFileCount);

                    //for (int i = 0; i < animations.Count; i += PARALLEL_CONVERSIONS)
                    //{
                    //    //Process[] conversionProcs = new Process[PARALLEL_CONVERSIONS];

                    //    for (int j = 0; j < PARALLEL_CONVERSIONS; j++)
                    //    {
                    //        if ((i + j) >= animations.Count)
                    //            break;


                    //        StartDowngradingHKX(animations[i + j].Bytes, compendium != null, i + j, result =>
                    //        {
                    //            animations[i + j].Bytes = result;
                    //            converted_anim[i + j] = true;
                    //        });
                    //    }

                    //    //for (int j = 0; j < PARALLEL_CONVERSIONS; j++)
                    //    //{
                    //    //    if (j >= animations.Count)
                    //    //        break;

                    //    //    prog.Report(((i + j) + 2) / progTotalFileCount);

                    //    //    animations[i + j].Bytes = WaitForDowngradeHKXFinishAndRead(conversionProcs[j], i + j);
                    //    //}
                    //}

                    int numAnimsConverted = 0;

                    int meme = animations.Count;

                    for (int i = 0; i < meme; i++)
                    {
                        bool checkCancelled = false;
                        lock (_lock_processQueue)
                            checkCancelled = wasCanceled;
                        if (checkCancelled)
                            return;

                        var nextProc = GetDowngradeHkxProcess(animations[i].Name, animations[i].Bytes, compendium != null, i, (convertProc, result, j) =>
                        {
                          
                                if (result == null)
                                {
                                    lock (_lock_processQueue)
                                    {
                                        wasCanceled = true;

                                        foreach (var proc in processDeleteQueue)
                                        {
                                            if (!proc.HasExited)
                                                proc.Kill();

                                            if (processQueue.Contains(proc))
                                                processQueue.Remove(proc);

                                            proc.Dispose();
                                        }
                                        processDeleteQueue.Clear();

                                        foreach (var proc in processQueue)
                                        {
                                            if (!proc.HasExited)
                                                proc.Kill();

                                            proc.Dispose();
                                        }

                                        

                                        processQueue.Clear();

                                        for (int k = 0; k < meme; k++)
                                        {
                                            animations[k].Bytes = null;
                                            converted_anim[k] = false;
                                        }
                                        numAnimsConverted = meme;
                                    }
                                }
                                else
                                {
                                    animations[j].Bytes = result;
                                    converted_anim[j] = true;
                                    numAnimsConverted++;
                                    prog.Report(((numAnimsConverted) + 2) / progTotalFileCount);

                                    lock (_lock_processQueue)
                                    {
                                        processDeleteQueue.Add(convertProc);
                                    }
                                }

                                
                            



                        });

                        lock(_lock_processQueue)
                        {
                            processQueue.Add(nextProc);
                        }

                        nextProc.Start();

                        int processCount = PARALLEL_CONVERSIONS + 1;

                        do
                        {
                            Thread.Sleep(100);
                            lock (_lock_processQueue)
                            {
                                foreach (var proc in processDeleteQueue)
                                {
                                    try
                                    {
                                        if (!proc.HasExited)
                                            proc.Kill();
                                    }
                                    catch (InvalidOperationException)
                                    {

                                    }

                                    if (processQueue.Contains(proc))
                                        processQueue.Remove(proc);

                                    proc.Dispose();
                                }
                                processDeleteQueue.Clear();

                                foreach (var proc in processQueue)
                                {
                                    try
                                    {
                                        if (proc.HasExited)
                                            processDeleteQueue.Add(proc);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        processDeleteQueue.Add(proc);
                                    }

                                    
                                }

                                processCount = processQueue.Count;
                            }
                        }
                        while (processCount > PARALLEL_CONVERSIONS);

                    }

                    bool everythingFinished = false;
                    do
                    {
                        everythingFinished = true;

                        if (skeleton != null && !converted_skeleton)
                            everythingFinished = false;

                        if (numAnimsConverted < meme)
                            everythingFinished = false;

                        //for (int i = 0; i < converted_anim.Length; i++)
                        //{
                        //    if (!converted_anim[i])
                        //    {
                        //        everythingFinished = false;
                        //        break;
                        //    }
                        //}

                        System.Threading.Thread.Sleep(20);
                    }
                    while (!(everythingFinished || wasCanceled));

                    if (File.Exists(anibndPath + ".2010"))
                        File.Delete(anibndPath + ".2010");

                    if (anibnd is BND3 asBnd3)
                        asBnd3.Write(anibndPath + ".2010");
                    else if (anibnd is BND4 asBnd4)
                        asBnd4.Write(anibndPath + ".2010");

                    prog.Report(1);
                }
                finally
                {
                    DeleteTempIfNeeded();
                }

            }
        }
    }
}
