using FMOD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewFmodManager
    {
        public static NewFmodIns CurrentFmodIns = null;

        public static void SwitchToFmodIns(NewFmodIns fmod)
        {
            if (fmod != CurrentFmodIns)
            {
                if (CurrentFmodIns != null)
                {
                    //CurrentFmodIns.LastLoadedFEVs = CurrentFmodIns.LoadedFEVs_FullPaths.Values.ToList();
                    CurrentFmodIns.Purge();
                    CurrentFmodIns.Shutdown();
                }

                CurrentFmodIns = fmod;
                if (CurrentFmodIns != null)
                {
                    CurrentFmodIns.InitTest();
                }
            }
            
            
            
        }





        public static void PreProcessBloodborneFEV(zzz_DocumentIns doc, string shortFevName, string langSuffix)
        {
            var inputFEV = $"{doc.GameRoot.InterrootPath}sound\\{shortFevName}.fev";
            var outputFEV = $"{doc.GameRoot.InterrootPath}_DSAS_CONVERTED_SOUNDS\\{shortFevName}.fev";

            var inputFSB = $"{doc.GameRoot.InterrootPath}sound\\{shortFevName}.fsb";
            var outputFSB = $"{doc.GameRoot.InterrootPath}_DSAS_CONVERTED_SOUNDS\\{shortFevName}.fsb";

            if (!Directory.Exists($"{doc.GameRoot.InterrootPath}\\_DSAS_CONVERTED_SOUNDS"))
            {
                Directory.CreateDirectory($"{doc.GameRoot.InterrootPath}_DSAS_CONVERTED_SOUNDS");
            }

            if (File.Exists(inputFEV))
                File.Copy(inputFEV, outputFEV, overwrite: true);

            if (langSuffix != null)
            {
                var inputLangFEV = $"{doc.GameRoot.InterrootPath}sound\\{shortFevName}_{langSuffix}.fev";
                var outputLangFEV = $"{doc.GameRoot.InterrootPath}_DSAS_CONVERTED_SOUNDS\\{shortFevName}_{langSuffix}.fev";

                if (File.Exists(inputLangFEV))
                    File.Copy(inputLangFEV, outputLangFEV);

                var inputLangFSB = $"{doc.GameRoot.InterrootPath}sound\\{shortFevName}_{langSuffix}.fsb";
                var outputLangFSB = $"{doc.GameRoot.InterrootPath}_DSAS_CONVERTED_SOUNDS\\{shortFevName}_{langSuffix}.fsb";

                if (File.Exists(inputLangFSB) && !File.Exists(outputLangFSB))
                {
                    var inputLangFsbBytes = File.ReadAllBytes(inputLangFSB);
                    var outputLangFsbBytes = ConvertBloodborneFSB(inputLangFsbBytes);
                    if (outputLangFsbBytes != null)
                    {
                        File.WriteAllBytes(outputLangFSB, outputLangFsbBytes);
                    }
                }
            }

            if (File.Exists(inputFSB) && !File.Exists(outputFSB))
            {
                var inputFsbBytes = File.ReadAllBytes(inputFSB);
                var outputFsbBytes = ConvertBloodborneFSB(inputFsbBytes);
                if (outputFsbBytes != null)
                {
                    File.WriteAllBytes(outputFSB, outputFsbBytes);
                }
            }
        }

        public static byte[] ConvertBloodborneFSB(byte[] inputFile)
        {
            var guid = Guid.NewGuid().ToString();
            var exePath = $@"{Main.Directory}\Res\vgmstream\vgmstream_cmd.exe";
            var temp = $@"{Main.Directory}\Temp\{guid}";

            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);
            
            byte[] result = null;

            try
            {
                //var tid = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                var pathIn = $@"{temp}\in.fsb";
                var pathOut = $@"{temp}\out.fsb";

                File.WriteAllBytes(pathIn, inputFile);

                var procStart = new ProcessStartInfo(exePath, $"-o \"{temp}\\?s.wav\" -S 0 \"{pathIn}\"");

                procStart.CreateNoWindow = true;
                procStart.WindowStyle = ProcessWindowStyle.Hidden;
                procStart.UseShellExecute = false;
                procStart.WorkingDirectory = temp;

                

                try
                {
                    using (var proc = Process.Start(procStart))
                    {
                        proc.WaitForExit();

                        var fsbFileList = $"{temp}\\FsbFileList.lst";

                        var looseWavs = Directory.GetFiles($"{temp}", "*.wav");
                        looseWavs = looseWavs.OrderBy(x => int.Parse(Utils.GetShortIngameFileName(x).Split("__")[0])).ToArray();
                        File.WriteAllLines(fsbFileList, looseWavs);

                        var bankProcStart = new ProcessStartInfo($@"{Main.Directory}\Res\fsbankcl\fsbankcl.exe",
                            $"-o \"{pathOut}\" -format vorbis -quality 100 \"{fsbFileList}\"");

                        bankProcStart.CreateNoWindow = true;
                        bankProcStart.WindowStyle = ProcessWindowStyle.Hidden;
                        bankProcStart.UseShellExecute = false;
                        bankProcStart.WorkingDirectory = temp;



                        try
                        {
                            using (var bankProc = Process.Start(bankProcStart))
                            {
                                bankProc.WaitForExit();
                                result = File.ReadAllBytes(pathOut);

                                for (int i = 0; i < 0x18; i++)
                                {
                                    result[0x24 + i] = inputFile[0x24 + i];
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {
                    //Main.REQUEST_DISABLE_SOUND = true;
                    //System.Windows.Forms.MessageBox.Show("Unable to decode audio with vgmstream. Make sure you or your antivirus have" +
                    //    " not deleted the '/Res/vgmstream/vgmstream_cmd.exe' file (re-extract the .zip to get another copy if needed)." +
                    //    "\n\nDisabling sound simulation now.\n" +
                    //    "After fixing the issue, you may re-enable it from the 'Simulation' tab.", "",
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                }
            }
            finally
            {

            }

            if (Directory.Exists(temp))
                Directory.Delete(temp, recursive: true);

            return result;
        }



    }
}
