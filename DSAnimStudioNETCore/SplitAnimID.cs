using System;
using System.Diagnostics.CodeAnalysis;
using SoulsAssetPipeline;

namespace DSAnimStudio
{
    public struct SplitAnimID
    {
        //public int CompareTo(SplitAnimID other)
        //{
        //    return (int)((long)other - (long)this);
        //}

        public static SplitAnimID Invalid = new SplitAnimID()
        {
            CategoryID = -1,
            SubID = -1,
        };

        public bool IsValid => CategoryID >= 0 && SubID >= 0;

        public override string ToString()
        {
            var currentIDType = zzz_DocumentManager.CurrentDocument?.GameRoot?.CurrentAnimIDFormatType;

            if (currentIDType != null)
            {
                switch (currentIDType.Value)
                {
                    case zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY: return $"a{CategoryID:D2}_{SubID:D4}";
                    case zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY: return $"a{CategoryID:D3}_{SubID:D6}";
                    case zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ:
                        var ds2Meme = $"a{CategoryID:D3}_{SubID:D6}";
                        ds2Meme.Insert(ds2Meme.Length - 4, "_");
                        return ds2Meme;
                    default: throw new NotImplementedException();
                }
            }

            return $"[Cate {CategoryID} Anim {SubID}]";
        }

        public int CategoryID;
        public int SubID;
        

        public static SplitAnimID FromFullID(DSAProj proj, long id, SoulsGames? forceGame = null)
        {
            return FromFullID(proj.ParentDocument.GameRoot, id, forceGame);
        }

        
        public static SplitAnimID FromFullID(zzz_GameRootIns gameRoot, long id, SoulsGames? forceGame = null)
        {
            var split = new SplitAnimID();

            if (id < 0)
            {
                split.CategoryID = -1;
                split.SubID = -1;
                return split;
            }
            
            var type = gameRoot.GetAnimIDFormattingType(forceGame ?? gameRoot.GameType);
            if (type == zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY)
            {
                split.CategoryID = (int)(id / 1_0000);
                split.SubID = (int)(id % 1_0000);
            }
            else // Includes aXX_YY_ZZZZ as that is still a split of 6 digits between XX and YYZZZZ
            {
                split.CategoryID = (int)(id / 1_000000);
                split.SubID = (int)(id % 1_000000);
            }
            return split;
        }

        public int GetFullID(DSAProj proj, SoulsGames? forceGame = null)
        {
            return GetFullID(proj.ParentDocument.GameRoot, forceGame);
        }

        public int GetFullID(zzz_GameRootIns gameRoot, SoulsGames? forceGame = null)
        {
            if (CategoryID < 0 || SubID < 0)
                return -1;
            
            var type = gameRoot.GetAnimIDFormattingType(forceGame ?? gameRoot.GameType);
            switch (type)
            {
                case zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY: 
                    return (CategoryID * 1_0000) + (SubID % 1_0000);
                case zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY:
                case zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ: 
                    return (CategoryID * 1_000000) + (SubID % 1_000000);
                default: throw new NotImplementedException();
            }
        }

        public string GetFormattedIDString(DSAProj proj, SoulsGames? forceGame = null)
        {
            return GetFormattedIDString(proj.ParentDocument.GameRoot, forceGame);
        }

        public string GetFormattedIDString(zzz_GameRootIns gameRoot, SoulsGames? forceGame = null)
        {
            var type = gameRoot.GetAnimIDFormattingType(forceGame ?? gameRoot.GameType);
            switch (type)
            {
                case zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY: return $"a{CategoryID:D2}_{SubID:D4}";
                case zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY: return $"a{CategoryID:D3}_{SubID:D6}";
                case zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ:
                    var ds2Meme = $"a{CategoryID:D3}_{SubID:D6}";
                    ds2Meme.Insert(ds2Meme.Length - 4, "_");
                    return ds2Meme;
                default: throw new NotImplementedException();
            }
        }

        public static SplitAnimID Parse(DSAProj proj, string input)
        {
            return Parse(proj.ParentDocument.GameRoot, input);
        }

        public static SplitAnimID Parse(zzz_GameRootIns gameRoot, string input)
        {
            if (TryParse(gameRoot, input, out SplitAnimID parsed, out string detailedError))
            {
                return parsed;
            }
            
            throw new FormatException(detailedError);
        }
        
        public static bool TryParse(DSAProj proj, string input, out SplitAnimID parsed, out string detailedError)
        {
            var result = TryParse(proj.ParentDocument.GameRoot, input, out SplitAnimID outParsed, out string outDetailedError);
            parsed = outParsed;
            detailedError = outDetailedError;
            return result;
        }

        public static bool TryParse(zzz_GameRootIns gameRoot, string input, out SplitAnimID parsed, out string detailedError)
        {
            parsed = new SplitAnimID();
            input = input.Replace("a", "");
            if (input.Contains("_"))
            {
                var split = input.Split('_');
                if (split.Length != 2)
                {
                    detailedError = "SplitAnimID string can only have a single underscore ('_') in it.";
                    return false;
                }

                if (int.TryParse(split[0], out int upperID))
                {
                    if (upperID < 0)
                    {
                        detailedError =
                            $"Animation category ID '{upperID}' is not valid. Animation category ID cannot be less than zero.";
                        return false;
                    }
                    else if (upperID > zzz_DocumentManager.CurrentDocument.GameRoot.CurrentGameAnimUpperIDMax)
                    {
                        detailedError =
                            $"Animation category ID '{upperID}' is not valid. Animation category ID cannot be greater than the max the current game allows ({zzz_DocumentManager.CurrentDocument.GameRoot.CurrentGameAnimUpperIDMax}).";
                        return false;
                    }
                }
                else
                {
                    detailedError = $"Animation category ID '{split[0]}' is not valid. Animation category ID must be a properly formatted integer value.";
                    return false;
                }

                if (int.TryParse(split[1], out int lowerID))
                {
                    if (lowerID < 0)
                    {
                        detailedError =
                            $"Animation sub ID '{lowerID}' is not valid. Animation sub ID cannot be less than zero.";
                        return false;
                    }
                    else if (lowerID > zzz_DocumentManager.CurrentDocument.GameRoot.CurrentGameAnimLowerIDMax)
                    {
                        detailedError =
                            $"Animation sub ID '{lowerID}' is not valid. Animation sub ID cannot be greater than the max the current game allows ({zzz_DocumentManager.CurrentDocument.GameRoot.CurrentGameAnimLowerIDMax}).";
                        return false;
                    }
                }
                else
                {
                    detailedError = $"Animation sub ID '{split[1]}' is not valid. Animation sub ID must be a properly formatted integer value.";
                    return false;
                }

                parsed = new SplitAnimID()
                {
                    CategoryID = upperID,
                    SubID = lowerID,
                };
                detailedError = null;
                return true;
            }
            else
            {
                if (long.TryParse(input, out long fullID))
                {
                    parsed = SplitAnimID.FromFullID(gameRoot, fullID);
                    detailedError = null;
                    return true;
                }
                else
                {
                    detailedError =  $"Animation ID '{input}' is not valid. Animation ID must either be a properly " +
                                     $"formatted integer value or a split formatted value in the format " +
                                     $"of '{zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType.ToString()}'.";
                    return false;
                }
            }
        }


        //public static implicit operator long(SplitAnimID val)
        //{
        //    return val.GetFullID();
        //}

        //public static implicit operator SplitAnimID(long val)
        //{
        //    return FromFullID(val);
        //}

        //public override bool Equals([NotNullWhen(true)] object obj)
        //{
        //    if (obj is SplitAnimID asSplitAnimID)
        //    {
        //        return this == asSplitAnimID;
        //    }
        //    return false;
        //}

        public static bool operator ==(SplitAnimID a, SplitAnimID b)
        {
            return a.CategoryID == b.CategoryID && a.SubID == b.SubID;
        }

        public static bool operator !=(SplitAnimID a, SplitAnimID b)
        {
            return a.CategoryID != b.CategoryID || a.SubID != b.SubID;
        }
    }
}