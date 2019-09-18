using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class GameDataManager
    {
        public enum GameTypes
        {
            None,
            DES,
            DS1,
            DS1R,
            DS2,
            DS2SOTFS,
            DS3,
            BB,
            SDT,
        }

        public static GameTypes GameType { get; private set; } = GameTypes.None;

        public static HKX.HKXVariation GetCurrentLegacyHKXType()
        {
            if (GameType == GameTypes.DES)
                return HKX.HKXVariation.HKXDeS;
            else if (GameType == GameTypes.DS1)
                return HKX.HKXVariation.HKXDS1;
            else if (GameType == GameTypes.DS3)
                return HKX.HKXVariation.HKXDS3;
            else if (GameType == GameTypes.BB)
                return HKX.HKXVariation.HKXBloodBorne;

            // TODO MAKE LESS SHIT
            return HKX.HKXVariation.HKXDS3;
        }

        public static string InterrootPath { get; private set; } = null;

        public static void Init(GameTypes gameType, string interroot)
        {
            GameType = gameType;
            InterrootPath = interroot;
            ParamManager.LoadParamBND();
        }

        public static Model LoadCharacter(string id)
        {
            Model chr = null;

            LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_{id}", $"Loading character {id}...", progress =>
            {
                

                if (GameType == GameTypes.DS3)
                {
                    var chrbnd = BND4.Read($@"{InterrootPath}\chr\{id}.chrbnd.dcx");
                    var texbnd = BND4.Read($@"{InterrootPath}\chr\{id}.texbnd.dcx");
                    var anibnd = BND4.Read($@"{InterrootPath}\chr\{id}.anibnd.dcx");

                    chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd);
                }
                else if (GameType == GameTypes.DS1)
                {
                    var chrbnd = BND3.Read($@"{InterrootPath}\chr\{id}.chrbnd");
                    var anibnd = BND3.Read($@"{InterrootPath}\chr\{id}.anibnd");

                    chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd: null,
                        possibleLooseDdsFolder: $@"{InterrootPath}\chr\{id}\");
                }
                else if (GameType == GameTypes.BB)
                {
                    var chrbnd = BND4.Read($@"{InterrootPath}\chr\{id}.chrbnd.dcx");
                    var anibnd = BND4.Read($@"{InterrootPath}\chr\{id}.anibnd.dcx");

                    chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd: null,
                        additionalTpfNames: new List<string> { $@"{InterrootPath}\chr\{id}_2.tpf.dcx" },
                        possibleLooseDdsFolder: $@"{InterrootPath}\chr\{id}\");
                }

                Scene.AddModel(chr);
            });

            if (id == "c0000")
            {
                chr.IS_PLAYER = true;

                LoadingTaskMan.DoLoadingTask("c0000_ANIBNDs", 
                    "Loading additional player ANIBNDs...", progress =>
                {
                    string[] anibnds = System.IO.Directory.GetFiles($@"{InterrootPath}\chr", 
                        GameType == GameTypes.DS1 ? "c0000_*.anibnd" : "c0000_*.anibnd.dcx")
                    .OrderBy(fn =>
                    {
                        var fnCheck = fn.ToLower();
                        if (fnCheck.Contains("lo"))
                            return 0;
                        else if (fnCheck.Contains("md"))
                            return 1;
                        else if (fnCheck.Contains("hi"))
                            return 2;
                        else
                            return 3;
                    }).ToArray();

                    for (int i = 0; i < anibnds.Length; i++)
                    {
                        IBinder anibnd = null;
                        if (BND3.Is(anibnds[i]))
                            anibnd = BND3.Read(anibnds[i]);
                        else
                            anibnd = BND4.Read(anibnds[i]);

                        chr.AnimContainer.LoadAdditionalANIBND(anibnd, null);

                        progress.Report(1.0 * i / anibnds.Length);
                    }

                    progress.Report(1);
                });
            }

            return chr;
            
        }
    }
}
