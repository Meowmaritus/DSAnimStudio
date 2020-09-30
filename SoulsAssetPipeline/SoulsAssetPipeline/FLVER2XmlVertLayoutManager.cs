using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static SoulsAssetPipeline.FLVERImportHelpers;

namespace SoulsAssetPipeline
{
    public static class FLVER2XmlVertLayoutManager
    {
        internal static Dictionary<SoulsGames, FLVER2XmlVertLayoutMtdList> MTDListsPerGame = null;

        public static void LoadAllXMLs()
        {
            MTDListsPerGame = new Dictionary<SoulsGames, FLVER2XmlVertLayoutMtdList>();
            //TODO: Load from /SapResources/FLVER2XmlVertLayouts_{gameType}.xml
        }

        public static SoulsFormats.FLVER2.BufferLayout GetBufferLayout(SoulsGames game, string mtdName)
        {
            if (MTDListsPerGame == null)
                LoadAllXMLs();

            if (MTDListsPerGame.ContainsKey(game) &&
                MTDListsPerGame[game].BufferLayoutsPerMTD.ContainsKey(mtdName))
            {
                return MTDListsPerGame[game].BufferLayoutsPerMTD[mtdName];
            }
            else
            {
                //ErrorTODO: Make error better
                throw new NotImplementedException($"MTD '{mtdName}' is not mapped for game '{game}'.");
            }
        }
    }
}
