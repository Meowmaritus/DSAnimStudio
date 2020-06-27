using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class HavokDowngrade
    {
        private static object _lock_DoingTask = new object();
        private static object _lock_animFinished = new object();

        static string TAGTOOLS_DIR => $@"{Main.Directory}\Res\TagTools";
        static string TAGTOOLS_EXE => $@"{TAGTOOLS_DIR}\TagTools.exe";

        static string TEMPDIR => $@"{Main.Directory}\HavokDowngradeTEMP";

        static string pathCompendium => $@"{TEMPDIR}\in.compendium";

        const int PARALLEL_CONVERSIONS = 10;

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

        private static void StartDowngradingHKX(string debug_hkxName, byte[] hkx, bool useCompendium, int i, Action<byte[], int> whatToDoWithResult)
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
                var hkxName = copyOfHkxName;
                var standardOutput = proc.StandardOutput.ReadToEnd();
                var standardError = proc.StandardError.ReadToEnd();
                byte[] result = File.ReadAllBytes(pathOut);
                //Patch "hk_2012.2.0-r1" to "hk_2010.2.0-r1"
                result[0x2E] = 0x30;
                whatToDoWithResult.Invoke(result, j);
            };
            
            proc.Start();
            
        }

        //private static byte[] WaitForDowngradeHKXFinishAndRead(Process proc, int i)
        //{
        //    string pathOut = $@"{TEMPDIR}\out{i}.hkx";

        //    proc.WaitForExit();

            
        //}

        public static void DowngradeAnibnd(string anibndPath, IProgress<double> prog = null)
        {
            lock (_lock_DoingTask)
            {
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

                    if (skeleton != null)
                    {
                        StartDowngradingHKX(skeleton.Name, skeleton.Bytes, compendium != null, -1, (result, i) =>
                        {
                            skeleton.Bytes = result;
                            converted_skeleton = true;
                        });
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

                    int processCount = 0;

                    int meme = animations.Count;

                    for (int i = 0; i < meme; i++)
                    {
                        processCount++;

                        StartDowngradingHKX(animations[i].Name, animations[i].Bytes, compendium != null, i, (result, j) =>
                        {
                            lock (_lock_animFinished)
                            {
                                animations[j].Bytes = result;
                                converted_anim[j] = true;
                                numAnimsConverted++;
                                prog.Report(((numAnimsConverted) + 2) / progTotalFileCount);
                                processCount--;
                            }
                            
                        });

                        while (processCount > PARALLEL_CONVERSIONS)
                            System.Threading.Thread.Sleep(1000);
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

                        System.Threading.Thread.Sleep(1000);
                    }
                    while (!everythingFinished);

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
