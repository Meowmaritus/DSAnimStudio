using DSAnimStudio.TaeEditor;
using Newtonsoft.Json;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using System.Threading;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public zzz_DocumentIns ParentDocument;
        public DSAProj(zzz_DocumentIns doc)
        {
            ParentDocument = doc;
        }

        public const string EXT = ".dsaproj";

        public enum Versions : int
        {
            v0 = 0,
            v1 = 1,
            v2 = 2,
            v3 = 3,
            v4 = 4,
            v5 = 5,
            v6 = 6,
            v7 = 7,
            v8 = 8,
            v9 = 9,
            v10 = 10,
            v11 = 11,
            v20_00_00 = 20_00_00,
            v20_00_01 = 20_00_01,
            v20_00_02 = 20_00_02,
            v20_00_03 = 20_00_03,
            v21_00_00 = 21_00_00,
            v21_01_00 = 21_01_00,
        }

        public const Versions LATEST_FILE_VERSION = Versions.v21_01_00;
        public Versions FILE_VERSION = LATEST_FILE_VERSION;

        public string DisplayName;
        public TaeContainerInfo ContainerInfo;

        public void INNER_UnloadStringsToSaveMemory()
        {
            foreach (var category in AnimCategories)
                category.INNER_UnloadStringsToSaveMemory();
        }

        public void SAFE_UnloadStringsToSaveMemory() => SAFE(INNER_UnloadStringsToSaveMemory);


        public void SafeAccessAnimCategoriesList(Action<List<AnimCategory>> doAction)
        {
            lock (_lock_DSAProj)
            {
                doAction?.Invoke(AnimCategories);
            }
        }

        public void SafeAccessAnimCategoriesDict(Action<Dictionary<int, List<AnimCategory>>> doAction)
        {
            lock (_lock_DSAProj)
            {
                doAction?.Invoke(NEW_AnimCategoriesLookupDict);
            }
        }

        private object _lock_DSAProj = new();
        private void SAFE(System.Action act)
        {
            lock (_lock_DSAProj)
            {
                act.Invoke();
            }
        }

        private T SAFE<T>(System.Func<T> func)
        {
            lock (_lock_DSAProj)
            {
                return func.Invoke();
            }
        }

        public void SafeAccess(Action<DSAProj> doAction)
        {
            lock (_lock_DSAProj)
            {
                doAction?.Invoke(this);
            }
        }

        public SoulsAssetPipeline.SoulsGames GameType;
        private List<AnimCategory> AnimCategories = new List<AnimCategory>();
        public List<BinderFile> TaeFileStubs = new List<BinderFile>();

        private List<string> SoundBanksToLoad = new List<string>();

        private List<string> INNER_GetSoundBanksToLoad() => SoundBanksToLoad.ToList();
        public List<string> SAFE_GetSoundBanksToLoad() => SAFE(INNER_GetSoundBanksToLoad);

        private void INNER_SetSoundBanksToLoad(List<string> banks)
        {
            bool different = !SoundBanksToLoad.SequenceEqual(banks);
            SoundBanksToLoad = banks.ToList();
            if (different)
            {
                INNER_MarkAllModified();
            }
        }
        public void SAFE_SetSoundBanksToLoad(List<string> banks) => SAFE(() => INNER_SetSoundBanksToLoad(banks));


        private string[] _GetDefaultSoundBanks(SoulsGames game, string chrID, bool player)
        {
            switch (game)
            {
                case SoulsGames.AC6:
                    if (!player)
                        return new string[] 
                        { 
                            "weapon_enemy", "weapon", "sfx", "impact", "default_work_unit", "bullet_glide", "bomb", 
                            $"cs_{chrID}", "cs_main", "vcmain" 
                        };
                    else
                        return new string[] 
                        {
                            "weapon_enemy", "weapon", "sfx", "impact", "default_work_unit", "bullet_glide", "bomb",
                            "cs_main", "vcmain" 
                        };
                case SoulsGames.BB:
                    if (!player)
                        return new string[] { $"sprj_{chrID}", "sprj_main" };
                    else
                        return new string[] { "sprj_main" };
                case SoulsGames.DES:
                    if (!player)
                        return new string[] { $"ds_se_{chrID}", "ds_se_main" };
                    else
                        return new string[] { "ds_se_main" };
                case SoulsGames.DS1:
                case SoulsGames.DS1R:
                    if (!player)
                        return new string[] { $"frpg_{chrID}", "frpg_main" };
                    else
                        return new string[] { "frpg_main" };
                case SoulsGames.DS3:
                    if (!player)
                        return new string[] { $"fdp_{chrID}", "fdp_main", "fdp_main_dlc1", "fdp_main_dlc2" };
                    else
                        return new string[] { "fdp_main", "fdp_main_dlc1", "fdp_main_dlc2" };
                case SoulsGames.ER:
                    if (!player)
                        return new string[] { $"cs_{chrID}", "cs_main", "vcmain" };
                    else
                        return new string[] { "cs_main", "vcmain" };
                case SoulsGames.ERNR:
                    if (!player)
                        return new string[] { $"cs_{chrID}", "cs_main", "vcmain" };
                    else
                        return new string[] { "cs_main", "vcmain" };
                case SoulsGames.SDT:
                    if (!player)
                        return new string[] { $"{chrID}", "main" };
                    else
                        return new string[] { "main" };
            }
            return new string[0];
        }
        public void INNER_InitDefaultSoundBanksToLoad()
        {
            SoundBanksToLoad = _GetDefaultSoundBanks(GameType, 
                Utils.GetShortIngameFileName(ContainerInfo.GetMainBinderName()), 
                RootTaeProperties.SaveEachCategoryToSeparateTae).ToList();
        }
        public void SAFE_InitDefaultSoundBanksToLoad()
            => SAFE(INNER_InitDefaultSoundBanksToLoad);


        private int INNER_GetAnimCategoriesCount() => AnimCategories.Count;
        public int SAFE_GetAnimCategoriesCount() => SAFE(INNER_GetAnimCategoriesCount);

        private bool INNER_CategoryExists(AnimCategory category) => AnimCategories.Contains(category);
        public bool SAFE_CategoryExists(AnimCategory category) => SAFE(() => INNER_CategoryExists(category));

        private bool INNER_CategoryExists(int categoryID) => NEW_AnimCategoriesLookupDict.ContainsKey(categoryID);
        public bool SAFE_CategoryExists(int categoryID) => SAFE(() => INNER_CategoryExists(categoryID));




        private void INNER_RemoveAllCategoriesWithID(int categoryID)
        {
            List<AnimCategory> existing = new();
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
            {
                foreach (var x in NEW_AnimCategoriesLookupDict[categoryID])
                    existing.Add(x);
            }
            foreach (var x in existing)
            {
                INNER_RemoveAnimCategory(x);
            }
        }
        public void SAFE_RemoveAllCategoriesWithID(int categoryID) => SAFE(() => INNER_RemoveAllCategoriesWithID(categoryID));



        private List<AnimCategory> INNER_GetAllAnimCategoriesFromCategoryID(int categoryID)
        {
            List<AnimCategory> result = new();
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
            {
                foreach (var x in NEW_AnimCategoriesLookupDict[categoryID])
                    result.Add(x);
            }
            return result;
        }

        public List<AnimCategory> SAFE_GetAllAnimCategoriesFromCategoryID(int categoryID) => SAFE(() => INNER_GetAllAnimCategoriesFromCategoryID(categoryID));


        private AnimCategory INNER_GetFirstAnimCategoryFromCategoryID(int categoryID)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
                return NEW_AnimCategoriesLookupDict[categoryID][0];
            return null;
        }
        public AnimCategory SAFE_GetFirstAnimCategoryFromCategoryID(int categoryID) => SAFE(() => INNER_GetFirstAnimCategoryFromCategoryID(categoryID));

        
        private Dictionary<int, List<AnimCategory>> NEW_AnimCategoriesLookupDict = new();


        private void INNER_ClearAllAnimCategories()
        {
            AnimCategories.Clear();
            NEW_AnimCategoriesLookupDict.Clear();
            _allAnimCategoryIDs.Clear();
        }

        public void SAFE_ClearAllAnimCategories() => SAFE(INNER_ClearAllAnimCategories);

        public List<Animation> INNER_GetAllAnimationsWithID(SplitAnimID id)
        {
            var result = new List<Animation>();
            lock (_lock_DSAProj)
            {
                if (NEW_AnimCategoriesLookupDict.ContainsKey(id.CategoryID))
                {
                    var categories = NEW_AnimCategoriesLookupDict[id.CategoryID];
                    foreach (var cate in categories)
                    {
                        if (cate.INNER_AnimExists_BySubID(id.SubID))
                        {
                            var anims = cate.INNER_GetAnimationsBySubID(id.SubID);
                            foreach (var anim in anims)
                            {
                                if (!result.Contains(anim))
                                    result.Add(anim);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public List<Animation> SAFE_GetAllAnimationsWithID(SplitAnimID id)
        {
            var result = new List<Animation>();
            lock (_lock_DSAProj)
            {
                result = INNER_GetAllAnimationsWithID(id);
            }
            return result;
        }

        private void INNER_ResortAnimCategoryIDs(bool force)
        {
            if (_animCategoryListsNeedResorting || force)
            {
                _allAnimCategoryIDs = _allAnimCategoryIDs.OrderBy(x => x).ToList();
                AnimCategories = AnimCategories.OrderBy(x => x.CategoryID).ToList();
            }
            _animCategoryListsNeedResorting = false;
        }

        public void SAFE_ResortAnimCategoryIDs(bool force = false)
        {
            lock (_lock_DSAProj)
            {
                INNER_ResortAnimCategoryIDs(force);
            }
        }

        private List<AnimCategory> INNER_GetAnimCategoriesByID(int categoryID)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
                return NEW_AnimCategoriesLookupDict[categoryID].ToList();
            else
                return new List<AnimCategory>();
        }

        public List<AnimCategory> SAFE_GetAnimCategoriesByID(int categoryID)
        {
            List<AnimCategory> result = new();
            lock (_lock_DSAProj)
            {
                result = INNER_GetAnimCategoriesByID(categoryID);
            }
            return result;
        }

        private List<AnimCategory> INNER_GetAllAnimCategories() => AnimCategories.ToList();
        public List<AnimCategory> SAFE_GetAllAnimCategories() => SAFE(INNER_GetAllAnimCategories);

        private void INNER_AddAnimCategory(AnimCategory category)
        {
            if (!NEW_AnimCategoriesLookupDict.ContainsKey(category.CategoryID))
                NEW_AnimCategoriesLookupDict.Add(category.CategoryID, new List<AnimCategory>());

            var list = NEW_AnimCategoriesLookupDict[category.CategoryID];
            if (!list.Contains(category))
                list.Add(category);

            if (!AnimCategories.Contains(category))
                AnimCategories.Add(category);

            category.ParentProj = this;

            if (!_allAnimCategoryIDs.Contains(category.CategoryID))
            {
                _allAnimCategoryIDs.Add(category.CategoryID);
                //AllAnimCategoryIDs = AllAnimCategoryIDs.OrderBy(x => x).ToList();
            }

            if (AnimCategories.Count >= 2)
            {
                if (AnimCategories[AnimCategories.Count - 2].CategoryID > category.CategoryID)
                {
                    _animCategoryListsNeedResorting = true;
                }
            }

        }

        public void SAFE_AddAnimCategory(AnimCategory category)
        {
            lock (_lock_DSAProj)
            {
                INNER_AddAnimCategory(category);
            }
        }
        
        private void INNER_RemoveAllInstancesOfAnimationByID(SplitAnimID animID)
        {
            var categories = INNER_GetAnimCategoriesByID(animID.CategoryID);
            foreach (var cate in categories)
            {
                cate.SAFE_RemoveAllAnimationsBySubID(animID.SubID);
            }
        }

        public void SAFE_RemoveAllInstancesOfAnimationByID(SplitAnimID animID)
        {
            lock (_lock_DSAProj)
            {
                INNER_RemoveAllInstancesOfAnimationByID(animID);
            }
        }

        private void INNER_RegistAnimationIDChange(Animation anim, SplitAnimID oldID, SplitAnimID newID)
        {
            if (newID != oldID)
            {
                anim.ParentCategory.SAFE_RemoveAnimation(anim);
                bool changedCategories = newID.CategoryID != oldID.CategoryID;
                anim.SplitID = newID;
                if (changedCategories)
                {
                    anim.ParentCategory = INNER_RegistCategory(newID.CategoryID);
                }
                anim.ParentCategory.SAFE_AddAnimation(anim);
                //TODO: See if this lags lol
                anim.ParentCategory.SAFE_ResortAnimIDs();
            }
        }

        public void SAFE_RegistAnimationIDChange(Animation anim, SplitAnimID oldID, SplitAnimID newID)
        {
            lock (_lock_DSAProj)
            {
                INNER_RegistAnimationIDChange(anim, oldID, newID);
            }
        }

        private void INNER_RegistCategoryIDChange(AnimCategory category, int oldID, int newID)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(oldID))
            {
                var list = NEW_AnimCategoriesLookupDict[oldID];
                if (list.Contains(category))
                    list.Remove(category);
                if (list.Count == 0)
                {
                    NEW_AnimCategoriesLookupDict.Remove(oldID);
                    if (_allAnimCategoryIDs.Contains(oldID))
                        _allAnimCategoryIDs.Remove(oldID);
                }
            }

            if (!NEW_AnimCategoriesLookupDict.ContainsKey(newID))
                NEW_AnimCategoriesLookupDict.Add(newID, new List<AnimCategory>());

            var listNew = NEW_AnimCategoriesLookupDict[newID];
            if (!listNew.Contains(category))
                listNew.Add(category);
            if (!_allAnimCategoryIDs.Contains(newID))
                _allAnimCategoryIDs.Add(newID);

            category.CategoryID = newID;
        }

        public void SAFE_RegistCategoryIDChange(AnimCategory category, int oldID, int newID)
        {
            lock (_lock_DSAProj)
            {
                INNER_RegistCategoryIDChange(category, oldID, newID);
            }
        }

        private void INNER_RegistAnimCategoryModifiedFlag(AnimCategory category, bool modified)
        {
            if (modified)
            {
                if (!_modifiedCategories.Contains(category))
                    _modifiedCategories.Add(category);
            }
            else
            {
                if (_modifiedCategories.Contains(category))
                    _modifiedCategories.Remove(category);
            }
        }

        public void SAFE_RegistAnimCategoryModifiedFlag(AnimCategory category, bool modified)
        {
            lock (_lock_DSAProj)
            {
                INNER_RegistAnimCategoryModifiedFlag(category, modified);
            }
        }

       

        private void INNER_RemoveAnimCategory(AnimCategory category)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(category.CategoryID))
            {
                var list = NEW_AnimCategoriesLookupDict[category.CategoryID];
                if (list.Contains(category))
                    list.Remove(category);

                if (list.Count == 0)
                {
                    NEW_AnimCategoriesLookupDict.Remove(category.CategoryID);
                    if (_allAnimCategoryIDs.Contains(category.CategoryID))
                        _allAnimCategoryIDs.Remove(category.CategoryID);
                }
            }

            if (AnimCategories.Contains(category))
                AnimCategories.Remove(category);

            // Actually, removing something from a sorted list shouldn't really cause any issues.
            //_animCategoryListsNeedResorting = true;
        }

        public void SAFE_RemoveAnimCategory(AnimCategory category)
        {
            lock (_lock_DSAProj)
            {
                INNER_RemoveAnimCategory(category);
            }
        }

        //public void RemoveAnimCategoriesByID(int categoryID)
        //{

        //}

        //public void NEW_RegistAnimCategoryIDChange(AnimCategory category, int oldID, int newID)
        //{
        //    if (NEW_AnimCategoriesLookupDict.ContainsKey(oldID))
        //    {
        //        var oldList = NEW_AnimCategoriesLookupDict[oldID];
        //        if (oldList.Contains(category))
        //            oldList.Remove(category);
        //    }

        //    if (!NEW_AnimCategoriesLookupDict.ContainsKey(newID))
        //        NEW_AnimCategoriesLookupDict.Add(newID, new List<AnimCategory>());

        //    var newList = NEW_AnimCategoriesLookupDict[newID];
        //    if (!newList.Contains(category))
        //        newList.Add(category);
        //}

        public TaeProperties RootTaeProperties = new TaeProperties();

        public object _lock_Tags = new object();
        public List<Tag> Tags = new List<Tag>();
        
        
        
        public ErrorContainerClass ErrorContainer = new ErrorContainerClass();


        public TAE.Template Template;

        public List<BinderFile> WriteTaeFilesForAnibnd(IProgress<double> prog, double progStart, double progWeight)
        {
            int TOTAL_FILE_COUNT = AnimCategories.Count;

            double currentProg = 0;
            double progPerCategory = (1.0 / (double)(TOTAL_FILE_COUNT));


            List<BinderFile> result = new List<BinderFile>();
            
            SoulsAssetPipeline.Animation.TAE NewTae()
            {
                SoulsAssetPipeline.Animation.TAE tae = new();
                tae.Format = RootTaeProperties.Format;
                tae.IsOldDemonsSoulsFormat_0x10000 = RootTaeProperties.IsOldDemonsSoulsFormat_0x10000;
                tae.IsOldDemonsSoulsFormat_0x1000A = RootTaeProperties.IsOldDemonsSoulsFormat_0x1000A;
                tae.AnimCount2Value = RootTaeProperties.AnimCount2Value;
                tae.BigEndian = RootTaeProperties.BigEndian;
                tae.Flags1 = RootTaeProperties.Flags1;
                tae.Flags2 = RootTaeProperties.Flags2;
                tae.Flags3 = RootTaeProperties.Flags3;
                tae.Flags4 = RootTaeProperties.Flags4;
                tae.Flags5 = RootTaeProperties.Flags5;
                tae.Flags6 = RootTaeProperties.Flags6;
                tae.Flags7 = RootTaeProperties.Flags7;
                tae.Flags8 = RootTaeProperties.Flags8;
                tae.SkeletonName = RootTaeProperties.SkeletonName;
                tae.SibName = RootTaeProperties.SibName;
                tae.ActionSetVersion = RootTaeProperties.ActionSetVersion_ForSingleTaeOutput;
                tae.SaveWithActionTracksStripped = RootTaeProperties.SaveWithActionTracksStripped;
                tae.Animations = new();
                return tae;
            }

            void registTaeToBinder(TAE tae, int bindIndex, string shortTaeName)
            {
                var binderFile = new BinderFile();
                binderFile.Flags = RootTaeProperties.BindFlags;
                binderFile.CompressionType = RootTaeProperties.BindDcxType;
                binderFile.ID = RootTaeProperties.TaeRootBindID + bindIndex;
                binderFile.Name = $"{RootTaeProperties.BindDirectory}\\{shortTaeName}.tae";
                binderFile.Bytes = tae.Write();
                result.Add(binderFile);
            }

            double progPerCategory_Capture = progPerCategory;
            int categoryIndex = 0;

            if (RootTaeProperties.SaveEachCategoryToSeparateTae)
            {
                
                foreach (var category in AnimCategories)
                {
                    double currentProgAtCategoryStart = (progPerCategory * categoryIndex);
                    var tae = NewTae();

                    // This overrides the default global one from RootTaeProperties, which is applied in NewTae()
                    tae.ActionSetVersion = category.ActionSetVersion_ForMultiTaeOutput;

                    tae.ID = 2000 + category.CategoryID;

                    double progPerAnimation = progPerCategory_Capture / (double)category.SAFE_GetAnimCount();

                    category.SafeAccessAnimations(anims =>
                    {
                        int animIndex = 0;
                        foreach (var anim in anims)
                        {
                            if (anim.IS_DUMMY_ANIM)
                                continue;

                            var newAnim = anim.LegacyToBinary(this);
                            newAnim.ID %= ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                            tae.Animations.Add(newAnim);

                            
                            currentProg = currentProgAtCategoryStart + (progPerAnimation * animIndex);
                            prog?.Report(progStart + (currentProg * progWeight));
                            animIndex++;
                        }
                    });
                    
                    
                    registTaeToBinder(tae, category.CategoryID, $"a{category.CategoryID:D2}");
                    categoryIndex++;
                    currentProg = (progPerCategory * categoryIndex);
                    prog?.Report(progStart + (currentProg * progWeight));
                    
                }
            }
            else
            {
                var mainTae = NewTae();

                mainTae.ActionSetVersion = RootTaeProperties.ActionSetVersion_ForSingleTaeOutput;

                mainTae.ID = RootTaeProperties.DefaultTaeProjectID;
                foreach (var category in AnimCategories)
                {
                    double currentProgAtCategoryStart = (progPerCategory * categoryIndex);

                    double progPerAnimation = progPerCategory_Capture / (double)category.SAFE_GetAnimCount();

                    category.SafeAccessAnimations(anims =>
                    {
                        int animIndex = 0;
                        foreach (var anim in anims)
                        {
                            if (anim.IS_DUMMY_ANIM)
                                continue;

                            mainTae.Animations.Add(anim.LegacyToBinary(this));

                            currentProg = currentProgAtCategoryStart + (progPerAnimation * animIndex);
                            prog?.Report(progStart + (currentProg * progWeight));
                            animIndex++;
                        }
                    });

                    categoryIndex++;
                    currentProg = (progPerCategory * categoryIndex);
                    prog?.Report(progStart + (currentProg * progWeight));
                }
                registTaeToBinder(mainTae, 0, RootTaeProperties.DefaultTaeShortName);

                
            }

            prog?.Report(progStart + (1.0 * progWeight));

            return result;
        }


        private List<AnimCategory> _modifiedCategories = new List<AnimCategory>();
        private List<AnimCategory> CategoriesWithErrors = new List<AnimCategory>();

        public void SafeAccessModifiedCategories(Action<List<AnimCategory>> doAction)
        {
            lock (_lock_DSAProj)
            {
                doAction.Invoke(_modifiedCategories);
            }
        }

        public void SafeAccessErroredCategories(Action<List<AnimCategory>> doAction)
        {
            lock (_lock_DSAProj)
            {
                doAction.Invoke(CategoriesWithErrors);
            }
        }


        private bool scanForErrors_Cancel = false;
        private bool scanForErrors_Finished = false;
        private bool scanForErrors_Queued = false;

        private Thread scanForErrors_Thread = null;

        public float TimeSinceLastErrorCheck = 0;

        public void CheckScanForErrorsQueue()
        {
            if (scanForErrors_Thread != null)
            {
                if (scanForErrors_Finished || !scanForErrors_Thread.IsAlive)
                    scanForErrors_Thread = null;
            }

            if (scanForErrors_Thread == null && scanForErrors_Queued)
            {
                scanForErrors_Queued = false;
                scanForErrors_Cancel = false;
                scanForErrors_Finished = false;

                scanForErrors_Thread = new Thread(new ThreadStart(() =>
                {
                    ScanForErrors_Inner();

                    scanForErrors_Finished = true;
                }));
                scanForErrors_Thread.IsBackground = true;
                scanForErrors_Thread.Start();
            }
        }

        public void ScanForErrors_Background()
        {
            if (scanForErrors_Thread != null)
            {
                scanForErrors_Cancel = true;
            }
            scanForErrors_Queued = true;
        }
        
        public void ScanForErrors()
        {
            ScanForErrors_Inner();
        }

        public bool IsAsyncErrorCheckRunning()
        {
            return scanForErrors_Thread != null && scanForErrors_Thread.IsAlive;
        }

        private void ScanForErrors_Inner()
        {
            //TODO
            //return;

            //List<SplitAnimID> duplicateAnimIDs = new List<SplitAnimID>();
            ErrorContainer.ClearErrors();


            List<Animation> animationsAlreadyCheckedForDuplicates = new List<Animation>();

            var categoriesToCheck = _modifiedCategories.ToList();
            var animsToCheck = new List<Animation>();
            foreach (var category in CategoriesWithErrors)
            {
                if (!categoriesToCheck.Contains(category))
                    categoriesToCheck.Add(category);
            }

            foreach (var modCategory in categoriesToCheck)
            {
                modCategory.SafeAccessModifiedAnimations(modAnims =>
                {
                    foreach (var anim in modAnims)
                    {
                        if (!animsToCheck.Contains(anim))
                            animsToCheck.Add(anim);
                    }
                });
                modCategory.SafeAccessErroredAnimations(errorAnims =>
                {
                    foreach (var anim in errorAnims)
                    {
                        if (!animsToCheck.Contains(anim))
                            animsToCheck.Add(anim);
                    }
                });
                
                
            }

            foreach (var modCategory in categoriesToCheck)
            {
                modCategory.ErrorContainer.ClearErrors();
            }

            foreach (var modAnim in animsToCheck)
            {
                modAnim.ErrorContainer.ClearErrors();
            }

            foreach (var modAnim in animsToCheck)
            {
                if (animationsAlreadyCheckedForDuplicates.Contains(modAnim))
                    continue;

                modAnim.ErrorContainer.ClearErrors();

                var allAnimsWithThisID = SAFE_GetAllAnimationsWithID(modAnim.SplitID);

                if (allAnimsWithThisID.Count > 1)
                {


                    foreach (var anim in allAnimsWithThisID)
                    {
                        if (!animationsAlreadyCheckedForDuplicates.Contains(anim))
                            animationsAlreadyCheckedForDuplicates.Add(anim);

                        var error = new ErrorState.DuplicateAnimID(anim.ParentCategory, anim, ParentDocument.EditorScreen, this, anim.SplitID);
                        anim.ErrorContainer.AddError(error);
                        anim.ParentCategory.ErrorContainer.AddError(error);
                        ErrorContainer.AddError(error);

                        anim.ParentCategory.SafeAccessErroredAnimations(errorAnims =>
                        {
                            if (!errorAnims.Contains(anim))
                                errorAnims.Add(anim);
                        });

                        

                        if (!anim.ParentCategory.ParentProj.CategoriesWithErrors.Contains(anim.ParentCategory))
                            anim.ParentCategory.ParentProj.CategoriesWithErrors.Add(anim.ParentCategory);

                        //if (!anim.ErrorContainer.AnyDuplicateIDErrors())
                        //{
                        //    var error = new ErrorState.DuplicateAnimID(anim.ParentCategory, anim, ParentDocument.EditorScreen, this, anim.NewID);
                        //    anim.ErrorContainer.AddError(error);
                        //    anim.ParentCategory.ErrorContainer.AddError(error);
                        //    ErrorContainer.AddError(error);
                        //}




                    }
                }
            }




            var errorFreeCategories = new List<AnimCategory>();
            foreach (var category in CategoriesWithErrors)
            {
                if (!category.ErrorContainer.AnyErrors())
                    errorFreeCategories.Add(category);

                

                category.SafeAccessErroredAnimations(erroredAnims =>
                {
                    var errorFreeAnims = new List<Animation>();
                    foreach (var anim in erroredAnims)
                    {
                        if (!anim.ErrorContainer.AnyErrors())
                            errorFreeAnims.Add(anim);
                    }
                    foreach (var anim in errorFreeAnims)
                    {
                        erroredAnims.Remove(anim);
                    }
                });

                

            }
            foreach (var category in errorFreeCategories)
            {
                CategoriesWithErrors.Remove(category);
            }


            //List<SplitAnimID> fullAnimIDsAlreadyScanned = new List<SplitAnimID>();
            //List<SplitAnimID> duplicateAnimIDs = new List<SplitAnimID>();    

            //ErrorContainer.ClearErrors();

            //// FIRST PASS

            //List<Animation> animationsInWrongCategory = new List<Animation>();

            //List<DSAProj.AnimCategory> allCategories = new();
            //Main.WinForm.Invoke(() =>
            //{
            //    allCategories = AnimCategories.ToList();
            //});

            //foreach (var tae in allCategories)
            //{
            //    if (!tae.IsModified && !tae.ErrorContainer.AnyErrors())
            //        continue;

            //    if (scanForErrors_Cancel)
            //        return;

            //    List<DSAProj.Animation> animsInCategory = new();

            //    Main.WinForm.Invoke(() =>
            //    {
            //        animsInCategory = tae.Animations.ToList();
            //    });

            //    tae.ErrorContainer.ClearErrors();
            //    foreach (var anim in animsInCategory)
            //    {
            //        if (!anim.IsModified && !anim.ErrorContainer.AnyErrors())
            //            continue;

            //        Main.WinForm.Invoke(() =>
            //        {
            //            anim.ErrorContainer.ClearErrors();

            //            if (fullAnimIDsAlreadyScanned.Contains(anim.NewID))
            //            {
            //                if (!duplicateAnimIDs.Contains(anim.NewID))
            //                    duplicateAnimIDs.Add(anim.NewID);
            //            }

            //            if (!fullAnimIDsAlreadyScanned.Contains(anim.NewID))
            //                fullAnimIDsAlreadyScanned.Add(anim.NewID);

            //            anim.ParentAnimCategory = tae;

            //            if (anim.NewID.CategoryID != tae.CategoryID)
            //            {
            //                animationsInWrongCategory.Add(anim);
            //            }
            //        });
            //    }

            //}

            //foreach (var anim in animationsInWrongCategory)
            //{
            //    if (!anim.IsModified && !anim.ErrorContainer.AnyErrors())
            //        continue;

            //    if (scanForErrors_Cancel)
            //        return;
            //    Main.WinForm.Invoke(() =>
            //    {
            //        var wrongCategory = anim.ParentAnimCategory;
            //        var properCategory = RegistCategory(anim.NewID.CategoryID);
            //        if (!properCategory.Animations.Contains(anim))
            //        {
            //            properCategory.Animations.Add(anim);
            //        }
            //        if (wrongCategory != null && wrongCategory.Animations.Contains(anim))
            //        {
            //            wrongCategory.Animations.Remove(anim);
            //        }
            //        anim.ParentAnimCategory = properCategory;
            //    });
            //}

            //// SECOND PASS
            //foreach (var tae in allCategories)
            //{
            //    if (!tae.IsModified && !tae.ErrorContainer.AnyErrors())
            //        continue;

            //    if (scanForErrors_Cancel)
            //        return;

            //    List<DSAProj.Animation> animsInCategory = new();

            //    Main.WinForm.Invoke(() =>
            //    {
            //        tae.Animations = tae.Animations.OrderBy(x => x.NewID.GetFullID(this)).ToList();
            //        animsInCategory = tae.Animations.ToList();
            //    });



            //    foreach (var anim in animsInCategory)
            //    {
            //        if (!anim.IsModified && !anim.ErrorContainer.AnyErrors())
            //            continue;
            //        if (scanForErrors_Cancel)
            //            return;
            //        Main.WinForm.Invoke(() =>
            //        {
            //            if (duplicateAnimIDs.Contains(anim.NewID))
            //            {
            //                var error = new ErrorState.DuplicateAnimID(tae, anim, ParentDocument.EditorScreen, this, anim.NewID);
            //                anim.ErrorContainer.AddError(error);
            //                tae.ErrorContainer.AddError(error);
            //                ErrorContainer.AddError(error);
            //            }
            //        });
            //    }
            //}
        }
        
        public List<Animation> GetAllAnimsWithID(SplitAnimID id)
        {
            var result = new List<Animation>();
            var matchingCategories = SAFE_GetAnimCategoriesByID(id.CategoryID);
            foreach (var cate in matchingCategories)
            {
                var matchingAnims = cate.SAFE_GetAnimationsBySubID(id.SubID);
                foreach (var anim in matchingAnims)
                {
                    result.Add(anim);
                }
            }

            //var result = new List<(AnimCategory Tae, Animation Anim)>();
            //foreach (var tae in AnimCategories)
            //{
            //    foreach (var anim in tae.Animations)
            //    {
            //        if (anim.NewID == id)
            //            result.Add((tae, anim));
            //    }
            //}

            return result;
        }
        
        public EditorFlags GetAllTAEsRuntimeFlags()
        {
            EditorFlags flags = EditorFlags.None;
            foreach (var tae in AnimCategories)
            {
                flags |= tae.RuntimeFlags;
            }
            return flags;
        }

        private void INNER_FixupAfterLoading()
        {
            foreach (var cate in AnimCategories)
            {
                if (cate.INNER_GetAnimCount() == 0)
                {
                    cate.INNER_AddAnimation(Animation.NewDummyAnim(this, cate));
                }
            }
        }

        public static DSAProj CreateFromContainer(TaeContainerInfo containerInfo, SoulsAssetPipeline.Animation.TAE.Template template, zzz_DocumentIns doc, bool checkValidity = true, bool readFromBackup = false)
        {
            if (checkValidity)
            {
                var valid = containerInfo.CheckValidity(out string containerErrorMsg, out _);
                if (!valid)
                {
                    throw new Exception(containerErrorMsg);
                }
            }

            DSAProj proj = new DSAProj(doc);
            proj.GameType = doc.GameRoot.GameType;
            

            if (containerInfo is TaeContainerInfo.ContainerAnibnd asAnibnd)
            {
                var anibnd = Utils.ReadBinder(asAnibnd.AnibndPath + (readFromBackup ? ".dsasbak" : ""));
                proj.DisplayName = Utils.GetShortIngameFileName(asAnibnd.AnibndPath);
                proj.ContainerInfo = asAnibnd.GetClone();
                proj.SAFE_GenerateFromAnibnd(anibnd, isPlayerAnibnd: doc.GameRoot.IsChrPlayerChr(Utils.GetShortIngameFileName(asAnibnd.AnibndPath)), template: template);
                return proj;
            }
            else if (containerInfo is TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
            {
                var binder = Utils.ReadBinder(asAnibndInBinder.BinderPath + (readFromBackup ? ".dsasbak" : ""));
                proj.DisplayName = Utils.GetShortIngameFileName(asAnibndInBinder.BinderPath) + $":{asAnibndInBinder.BindID}";
                var info = asAnibndInBinder.GetClone();
                proj.ContainerInfo = info;

                foreach (var f in binder.Files)
                {
                    if (f.ID == info.BindID)
                    {
                        var anibnd = Utils.ReadBinder(f.Bytes);

                        proj.SAFE_GenerateFromAnibnd(anibnd, isPlayerAnibnd: false, template: template);
                    }
                }
                return proj;
            }
            else
            {
                throw new NotImplementedException();
            }

            return proj;
        }


        private bool _animCategoryListsNeedResorting = false;

        private List<int> _allAnimCategoryIDs = new List<int>();

        //public Dictionary<int, AnimCategory> AllAnimCategoriesDict = new Dictionary<int, AnimCategory>();


        private bool INNER_AnyModified() => _modifiedCategories.Count > 0;
        public bool SAFE_AnyModified() => SAFE(INNER_AnyModified);


        private void INNER_ClearAllModified()
        {
            var modifiedCategoriesClone = _modifiedCategories.ToList();
            foreach (var tae in modifiedCategoriesClone)
                tae.SAFE_ClearAllModified();

            _modifiedCategories.Clear();
        }
        public void SAFE_ClearAllModified() => SAFE(INNER_ClearAllModified);

        

        private void INNER_ClearRuntimeFlagsOnAll(EditorFlags flags)
        {
            foreach (var tae in AnimCategories)
                tae.SAFE_ClearRuntimeFlagsOnAll(flags);
        }
        public void SAFE_ClearRuntimeFlagsOnAll(EditorFlags flags) => SAFE(() => INNER_ClearRuntimeFlagsOnAll(flags));
        




        private bool INNER_AnimExists(SplitAnimID id)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(id.CategoryID))
            {
                var categories = NEW_AnimCategoriesLookupDict[id.CategoryID];
                foreach (var category in categories)
                {
                    if (category.SAFE_AnimExists_ByFullID(id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool SAFE_AnimExists(SplitAnimID id) => SAFE(() => INNER_AnimExists(id));




        public Animation INNER_GetFirstAnimInCategory(int categoryID)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
            {
                var anims = NEW_AnimCategoriesLookupDict[categoryID][0].SAFE_GetAnimations();
                if (anims.Count > 0)
                    return anims[0];
            }
            return null;
        }
        public Animation SAFE_GetFirstAnimInCategory(int categoryID) => SAFE(() => INNER_GetFirstAnimInCategory(categoryID));
        



        public Animation INNER_GetLastAnimInCategory(int categoryID)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
            {
                var anims = NEW_AnimCategoriesLookupDict[categoryID][0].SAFE_GetAnimations();
                if (anims.Count > 0)
                    return anims.Last();
            }
            return null;
        }
        public Animation SAFE_GetLastAnimInCategory(int categoryID) => SAFE(() => INNER_GetLastAnimInCategory(categoryID));




        public AnimCategory INNER_GuiHelperSelectNextCategory(AnimCategory currentCategory)
        {
            int index = AnimCategories.IndexOf(currentCategory);
            index++;
            if (index >= AnimCategories.Count)
                index = 0;
            return AnimCategories[index];
        }
        public AnimCategory SAFE_GuiHelperSelectNextCategory(AnimCategory currentCategory) => SAFE(() => INNER_GuiHelperSelectNextCategory(currentCategory));




        public AnimCategory INNER_GuiHelperSelectPrevCategory(AnimCategory currentCategory)
        {
            int index = AnimCategories.IndexOf(currentCategory);
            index--;
            if (index < 0)
                index = AnimCategories.Count - 1;
            return AnimCategories[index];
        }
        public AnimCategory SAFE_GuiHelperSelectPrevCategory(AnimCategory currentCategory) => SAFE(() => INNER_GuiHelperSelectPrevCategory(currentCategory));




        public Animation INNER_GuiHelperSelectNextAnimation(Animation currentAnim)
        {
            var category = currentAnim.ParentCategory;
            int animIndex = category.SAFE_GetAnimIndexInList(currentAnim);
            animIndex++;
            if (animIndex >= category.SAFE_GetAnimCount())
                return INNER_GuiHelperSelectNextCategory(category).SAFE_GetFirstAnimInList();
            else
                return category.SAFE_GetAnimByIndex(animIndex);
        }
        public Animation SAFE_GuiHelperSelectNextAnimation(Animation currentAnim) => SAFE(() => INNER_GuiHelperSelectNextAnimation(currentAnim));




        public Animation INNER_GuiHelperSelectPrevAnimation(Animation currentAnim)
        {
            var category = currentAnim.ParentCategory;
            int animIndex = category.SAFE_GetAnimIndexInList(currentAnim);
            animIndex--;
            if (animIndex < 0)
                return SAFE_GuiHelperSelectPrevCategory(category).SAFE_GetLastAnimInList();
            else
                return category.SAFE_GetAnimByIndex(animIndex);
        }
        public Animation SAFE_GuiHelperSelectPrevAnimation(Animation currentAnim) => SAFE(() => INNER_GuiHelperSelectPrevAnimation(currentAnim));




        public AnimCategory INNER_RegistCategory(int categoryID)
        {
            //TESTING
            //if (categoryID == 999)
            //{
            //    Console.WriteLine("breakpoint");
            //}

            if (!NEW_AnimCategoriesLookupDict.ContainsKey(categoryID))
            {
                var category = new AnimCategory(this);
                category.CategoryID = categoryID;
                SAFE_AddAnimCategory(category);
            }
            return NEW_AnimCategoriesLookupDict[categoryID].First();
        }
        public AnimCategory SAFE_RegistCategory(int categoryID) => SAFE(() => INNER_RegistCategory(categoryID));


        //public void AddOrOverwriteCategory(int categoryID, AnimCategory category, bool overwriteOK)
        //{
        //    if (AllAnimCategoriesDict.ContainsKey(categoryID) && !overwriteOK)
        //        return;

        //    if (AllAnimCategoriesDict.ContainsKey(categoryID))
        //        AllAnimCategoriesDict.Remove(categoryID);
        //    var matches = AnimCategories.Where(x => x.CategoryID == categoryID).ToList();
        //    foreach (var m in matches)
        //        AnimCategories.Remove(m);

        //    AllAnimCategoriesDict[categoryID] = category;
        //    AnimCategories.Add(category);
        //    AnimCategories = AnimCategories.OrderBy(c => c.CategoryID).ToList();
        //    if (!AllAnimCategoryIDs.Contains(categoryID))
        //        AllAnimCategoryIDs.Add(categoryID);
        //}

        private void INNER_GenerateFromAnibnd(IBinder anibnd, bool isPlayerAnibnd, SoulsAssetPipeline.Animation.TAE.Template template)
        {
            INNER_ClearAllAnimCategories();

            RootTaeProperties = new TaeProperties();

            RootTaeProperties.SaveEachCategoryToSeparateTae = isPlayerAnibnd;

            var ver_0001 = anibnd.Files.Any(f => f.ID == 9999999) && ParentDocument.GameRoot.GameType != SoulsGames.DES;

            RootTaeProperties.TaeRootBindID = ver_0001 ? 5000000 : 3000000;
            var bindIDMax = ver_0001 ? 5099999 : 3099999;

            bool isRootTAE = true;
            foreach (var bf in anibnd.Files)
            {
                if (bf.ID >= RootTaeProperties.TaeRootBindID && bf.ID <= bindIDMax)
                {
                    if (bf.Bytes.Length < 4)
                        continue;
                    var tae = SoulsAssetPipeline.Animation.TAE.Read(bf.Bytes);

                    if (isRootTAE)
                    {
                        RootTaeProperties.BindFlags = bf.Flags;
                        RootTaeProperties.BindDirectory = System.IO.Path.GetDirectoryName(bf.Name);
                        RootTaeProperties.BindDcxType = bf.CompressionType;
                        RootTaeProperties.Format = tae.Format;
                        RootTaeProperties.IsOldDemonsSoulsFormat_0x1000A = tae.IsOldDemonsSoulsFormat_0x1000A;
                        RootTaeProperties.IsOldDemonsSoulsFormat_0x10000 = tae.IsOldDemonsSoulsFormat_0x10000;
                        RootTaeProperties.AnimCount2Value = tae.AnimCount2Value;
                        RootTaeProperties.BigEndian = tae.BigEndian;
                        RootTaeProperties.Flags1 = tae.Flags1;
                        RootTaeProperties.Flags2 = tae.Flags2;
                        RootTaeProperties.Flags3 = tae.Flags3;
                        RootTaeProperties.Flags4 = tae.Flags4;
                        RootTaeProperties.Flags5 = tae.Flags5;
                        RootTaeProperties.Flags6 = tae.Flags6;
                        RootTaeProperties.Flags7 = tae.Flags7;
                        RootTaeProperties.Flags8 = tae.Flags8;
                        RootTaeProperties.SkeletonName = tae.SkeletonName;
                        RootTaeProperties.SibName = tae.SibName;

                        if (isPlayerAnibnd)
                            RootTaeProperties.ActionSetVersion_ForSingleTaeOutput = (int)tae.ActionSetVersion;
                        else
                            RootTaeProperties.ActionSetVersion_ForSingleTaeOutput = -1;

                        RootTaeProperties.SaveWithActionTracksStripped = ParentDocument.GameRoot.GameTypeUsesLegacyEmptyEventGroups;
                        RootTaeProperties.DefaultTaeProjectID = tae.ID;
                        RootTaeProperties.DefaultTaeShortName = Utils.GetShortIngameFileName(bf.Name);
                    }

                    var taeBindIndex = bf.ID % RootTaeProperties.TaeRootBindID;

                    if (isPlayerAnibnd)
                    {
                        // Do not do this for non-player ones since those will be generating based on the anim entry IDs.
                        var category = INNER_RegistCategory(taeBindIndex);

                        category.ActionSetVersion_ForMultiTaeOutput = (int)tae.ActionSetVersion;
                    }

                    
                    foreach (var a in tae.Animations)
                    {


                        if (isPlayerAnibnd)
                        {

                            var category = INNER_RegistCategory(taeBindIndex);
                            var entry = new Animation(this, category);

                            //TESTING
                            //if (a.ID >= (ParentDocument.GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000))
                            //{
                            //    var idString = a.ID.ToString();
                            //    idString.Insert(idString.Length - (ParentDocument.GameRoot.GameTypeHasLongAnimIDs ? 6 : 4), "_");
                            //    Console.WriteLine($"PLAYER TAE ANIM ID OUT OF RANGE - CATEGORY {taeBindIndex} - LOCAL ANIM ID: {idString}");
                            //}

                            entry.SplitID = SplitAnimID.FromFullID(this, (ParentDocument.GameRoot.GameTypeUpperAnimIDModBy * taeBindIndex) + a.ID);
                            entry.INNER_SetHeader(a.Header.GetClone());
                            entry.INNER_SetActionTracks(a.ActionTracks.Select(g => ActionTrack.FromBinary(g)).ToList());
                            entry.INNER_SetActions(a.Actions.Select(e => Action.FromBinary(e)).ToList());

                            entry.INNER_GenerateTrackNames(template, true);
                            category.INNER_AddAnimation(entry);
                        }
                        else
                        {

                            int categoryID = (int)(a.ID / ParentDocument.GameRoot.GameTypeUpperAnimIDModBy);
                            var category = SAFE_RegistCategory(categoryID);
                            var entry = new Animation(this, category);
                            entry.INNER_SetHeader(a.Header.GetClone());
                            entry.INNER_SetActionTracks(a.ActionTracks.Select(g => ActionTrack.FromBinary(g)).ToList());
                            entry.INNER_SetActions(a.Actions.Select(e => Action.FromBinary(e)).ToList());
                            entry.INNER_GenerateTrackNames(template, true);
                            entry.SplitID = SplitAnimID.FromFullID(this, a.ID);
                            category.INNER_AddAnimation(entry);
                        }
                    }

                    isRootTAE = false;
                }
            }

            INNER_InitDefaultSoundBanksToLoad();

            INNER_FixupAfterLoading();

            //TESTING
            //Console.WriteLine("breakpoint lol");
        }

        public void SAFE_GenerateFromAnibnd(IBinder anibnd, bool isPlayerAnibnd, SoulsAssetPipeline.Animation.TAE.Template template)
            => SAFE(() => INNER_GenerateFromAnibnd(anibnd, isPlayerAnibnd, template));



        public (int Min, int Max) INNER_GetAnimCategoryMinMax()
        {
            int min = -1;
            int max = -1;
            foreach (var s in _allAnimCategoryIDs)
            {
                if (s < min)
                    min = s;
                if (s > max)
                    max = s;
            }
            return (min, max);
        }
        public (int Min, int Max) SAFE_GetAnimCategoryMinMax() => SAFE(INNER_GetAnimCategoryMinMax);



        public AnimCategory INNER_GetFirstAnimCategoryFromFullID(SplitAnimID id)
        {
            if (NEW_AnimCategoriesLookupDict.ContainsKey(id.CategoryID))
                return NEW_AnimCategoriesLookupDict[id.CategoryID][0];
            else
                return null;
        }
        public AnimCategory SAFE_GetFirstAnimCategoryFromFullID(SplitAnimID id) => SAFE(() => INNER_GetFirstAnimCategoryFromFullID(id));



        public Animation INNER_GetFirstAnimationFromFullID(SplitAnimID id)
        {
            if (id == ParentDocument.EditorScreen?.SelectedAnim?.SplitID)
                return ParentDocument.EditorScreen.SelectedAnim;


            if (NEW_AnimCategoriesLookupDict.ContainsKey(id.CategoryID))
            {
                var categories = NEW_AnimCategoriesLookupDict[id.CategoryID];
                foreach (var category in categories)
                {
                    return category.SAFE_FindFirstAnimByFullID(id);
                }
            }


            //int categoryID = id.CategoryID;
            //if (AllAnimCategoriesDict.ContainsKey(categoryID))
            //{
            //    return AllAnimCategoriesDict[categoryID].FindAnim(id);
            //}

            return null;
        }
        public Animation SAFE_GetFirstAnimationFromFullID(SplitAnimID id) => SAFE(() => INNER_GetFirstAnimationFromFullID(id));


        public Animation INNER_SolveAnimRefChain(SplitAnimID startID)
        {
            var anim = INNER_GetFirstAnimationFromFullID(startID);

            if (anim == null)
                return anim;

            var thisAnimFullID = anim.SplitID.GetFullID(ParentDocument.GameRoot);

            TAE.Animation.AnimFileHeader headerClone = anim.INNER_GetHeaderClone();
            if (headerClone is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOther)
            {
                if (asImportOther.ImportFromAnimID >= 0)
                {
                    if (asImportOther.ImportFromAnimID != thisAnimFullID)
                    {
                        var referencedAnim = INNER_GetFirstAnimationFromFullID(SplitAnimID.FromFullID(this, asImportOther.ImportFromAnimID));
                        if (referencedAnim == null)
                            return null;
                        return INNER_SolveAnimRefChain(referencedAnim.SplitID);
                    }
                    else
                    {
                        return anim;
                    }
                    
                }
            }

            return anim;
        }
        public Animation SAFE_SolveAnimRefChain(SplitAnimID startID) => SAFE(() => INNER_SolveAnimRefChain(startID));




        public void INNER_DeserializeFromFile(string fileName, SoulsAssetPipeline.Animation.TAE.Template template, string outputDir = null)
        {
            var fileBytes = System.IO.File.ReadAllBytes(fileName);
            var br = new BinaryReaderEx(false, fileBytes);
            INNER_Deserialize(br, template, outputDir ?? System.IO.Path.GetDirectoryName(fileName));
        }
        public void SAFE_DeserializeFromFile(string fileName, SoulsAssetPipeline.Animation.TAE.Template template, string outputDir = null) 
            => SAFE(() => INNER_DeserializeFromFile(fileName, template, outputDir));




        public void INNER_MarkAllModified()
        {
            foreach (var tae in AnimCategories)
            {
                tae.INNER_SetIsModified(true);
            }
        }
        public void SAFE_MarkAllModified() => SAFE(INNER_MarkAllModified);




        public void INNER_ApplyTemplate(SoulsAssetPipeline.Animation.TAE.Template template)
        {
            try
            {
                foreach (var cat in AnimCategories)
                {
                    cat.SafeAccessAnimations(animList =>
                    {
                        foreach (var anim in animList)
                        {
                            anim.UnSafeAccessActions(actions =>
                            {
                                for (int i = 0; i < actions.Count; i++)
                                {
                                    actions[i].ApplyTemplate(cat, template, anim.SplitID.GetFullID(this), i, actions[i].Type);
                                }
                            });
                           
                        }
                    });
                    
                }

                Template = template;
            }
            catch (Exception ex) when (Main.EnableErrorHandler.ApplyTaeTemplate)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to apply action template:\n\n{ex}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        public void SAFE_ApplyTemplate(SoulsAssetPipeline.Animation.TAE.Template template) => SAFE(() => INNER_ApplyTemplate(template));




        public void INNER_ScanAnimNamesFromOtherProj(DSAProj other)
        {
            if (other == this)
                return;

            foreach (var t in AnimCategories)
            {
                t.UnSafeAccessAnimations(animList =>
                {
                    foreach (var a in animList)
                    {

                        a.UnSafeAccessHeader(header =>
                        {
                            if (header.AnimFileName == null)
                            {
                                var fullID = a.SplitID;
                                var otherRef = other.INNER_GetFirstAnimationFromFullID(fullID);
                                if (otherRef != null)
                                {
                                    otherRef.UnSafeAccessHeader(otherHeader =>
                                    {
                                        header.AnimFileName = otherHeader.AnimFileName;
                                        header.IsNullHeader = otherHeader.IsNullHeader;
                                        a.RuntimeFlags |= EditorFlags.NeedsResave_NullAnimNameBug;
                                        t.RuntimeFlags |= EditorFlags.NeedsResave_NullAnimNameBug;
                                    });
                                  

                                }
                            }
                        });
                        

                    }
                });
                
            }
        }
        public void ScanAnimNamesFromOtherProj(DSAProj other) => SAFE(() => INNER_ScanAnimNamesFromOtherProj(other));


        public void INNER_DeserializeFromBytes(byte[] bytes, SoulsAssetPipeline.Animation.TAE.Template template, string relativeToDir)
        {
            var br = new BinaryReaderEx(false, bytes);
            INNER_Deserialize(br, template, relativeToDir);
        }
        public void SAFE_DeserializeFromBytes(byte[] bytes, SoulsAssetPipeline.Animation.TAE.Template template, string relativeToDir) 
            => SAFE(() => INNER_DeserializeFromBytes(bytes, template, relativeToDir));
        public void SAFE_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, string relativeToDir)
            => SAFE(() => INNER_Deserialize(br, template, relativeToDir));
        private void INNER_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, string relativeToDir)
        {
            br.AssertASCII("DPRJ");
            FILE_VERSION = (Versions)br.ReadInt32();

            if (FILE_VERSION > LATEST_FILE_VERSION)
                throw new DSAProjTooNewException(LATEST_FILE_VERSION, FILE_VERSION);

            DisplayName = br.ReadUTF16();

            if (FILE_VERSION >= Versions.v21_00_00)
                ContainerInfo = TaeContainerInfo.ReadFromBinary(br, relativeToDir);
            else
                ContainerInfo = TaeContainerInfo.ReadFromBinary(br, null);

            GameType = (SoulsAssetPipeline.SoulsGames)br.ReadInt32();
            ParentDocument.GameRoot.GameType = GameType;

            RootTaeProperties = TaeProperties.Deserialize(br);
            lock (_lock_Tags)
            {
                Tags = new List<Tag>();
                if (FILE_VERSION >= Versions.v20_00_00)
                {
                    int tagCount = br.ReadInt32();
                    for (int i = 0; i < tagCount; i++)
                    {
                        var t = new Tag();
                        t.Deserialize(br, this);
                        Tags.Add(t);
                    }
                }
            }

            int animCategoryCount = br.ReadInt32();
            INNER_ClearAllAnimCategories();
            for (int i = 0; i < animCategoryCount; i++)
            {
                var cat = new AnimCategory(this);
                cat.SAFE_Deserialize(br, template, this);

                if (FILE_VERSION < Versions.v21_00_00 && RootTaeProperties.SaveEachCategoryToSeparateTae)
                {
                    cat.ActionSetVersion_ForMultiTaeOutput = (int)RootTaeProperties.ActionSetVersion_ForSingleTaeOutput;
                }

                INNER_AddAnimCategory(cat);
            }

            if (FILE_VERSION < Versions.v21_00_00 && RootTaeProperties.SaveEachCategoryToSeparateTae)
            {
                RootTaeProperties.ActionSetVersion_ForSingleTaeOutput = -1;
            }

            INNER_ResortAnimCategoryIDs(force: true);

            TaeFileStubs.Clear();
            if (FILE_VERSION >= Versions.v3)
            {
                int taeFileStubCount = br.ReadInt32();
                for (int i = 0; i < taeFileStubCount; i++)
                {
                    BinderFile stub = new BinderFile();
                    stub.Flags = (Binder.FileFlags)br.ReadByte();
                    stub.ID = br.ReadInt32();
                    stub.Name = br.ReadNullPrefixUTF16();
                    stub.Bytes = new byte[0];
                    stub.CompressionType = (DCX.Type)br.ReadInt32();
                    TaeFileStubs.Add(stub);
                }
            }

            SoundBanksToLoad.Clear();
            if (FILE_VERSION >= Versions.v21_00_00)
            {
                int soundFileCount = br.ReadInt32();
                for (int i = 0; i < soundFileCount; i++)
                {
                    string s = br.ReadASCII();
                    s = Utils.RemoveInvalidFileNameChars(Utils.GetShortIngameFileName(s));
                    if (!SoundBanksToLoad.Contains(s))
                        SoundBanksToLoad.Add(s);
                }
                // Preserve order of strings since it's the audio load order.
            }

            INNER_FixupAfterLoading();



            // Convert to latest file version upon loading.
            if (FILE_VERSION != LATEST_FILE_VERSION)
            {
                foreach (var tae in AnimCategories)
                {
                    tae.RuntimeFlags |= EditorFlags.NeedsResave_DSAProjVersionOutdated;
                }
            }


            //FILE_VERSION = LATEST_FILE_VERSION;
        }









        public byte[] SAFE_SerializeToBytes(IProgress<double> prog, double progStart, double progWeight, string relativeToDir)
            => SAFE(() => INNER_SerializeToBytes(prog, progStart, progWeight, relativeToDir));
        public byte[] INNER_SerializeToBytes(IProgress<double> prog, double progStart, double progWeight, string relativeToDir)
        {
            var bw = new BinaryWriterEx(false);
            INNER_Serialize(bw, prog, progStart, progWeight, relativeToDir);
            return bw.FinishBytes();
        }

        public void SAFE_SerializeToFile(string fileName, IProgress<double> prog, double progStart, double progWeight, string relativeToDir = null)
            => SAFE(() => INNER_SerializeToFile(fileName, prog, progStart, progWeight, relativeToDir));
        public void INNER_SerializeToFile(string fileName, IProgress<double> prog, double progStart, double progWeight, string relativeToDir = null)
        {
            var bw = new BinaryWriterEx(false);
            INNER_Serialize(bw, prog, progStart, progWeight, relativeToDir ?? System.IO.Path.GetDirectoryName(fileName));
            System.IO.File.WriteAllBytes(fileName, bw.FinishBytes());
        }
        
        public void SAFE_Serialize(BinaryWriterEx bw, IProgress<double> prog, double progStart, double progWeight, string relativeToDir)
            => SAFE(() => INNER_Serialize(bw, prog, progStart, progWeight, relativeToDir));
        private void INNER_Serialize(BinaryWriterEx bw, IProgress<double> prog, double progStart, double progWeight, string relativeToDir)
        {
            bw.WriteASCII("DPRJ");
            FILE_VERSION = LATEST_FILE_VERSION;
            bw.WriteInt32((int)FILE_VERSION);

            bw.WriteUTF16(DisplayName, true);
            ContainerInfo.WriteToBinary(bw, relativeToDir);
            GameType = ParentDocument.GameRoot.GameType;
            bw.WriteInt32((int)GameType);

            RootTaeProperties.Serialize(bw);

            lock (_lock_Tags)
            {
                bw.WriteInt32(Tags.Count);
                foreach (var t in Tags)
                {
                    t.Serialize(bw);
                }
            }

            double weightPerCategory = 1.0 / (double)AnimCategories.Count;

            bw.WriteInt32(AnimCategories.Count);
            for (int i = 0; i < AnimCategories.Count; i++)
            {
                var cache = AnimCategories[i].INNER_GetSerializedCache();
                if (!AnimCategories[i].INNER_GetIsModified() && cache != null)
                    bw.WriteBytes(cache);
                else
                    AnimCategories[i].INNER_Serialize(bw, this);

                double p = weightPerCategory * (double)i;
                prog?.Report(progStart + (progWeight * p));
            }
            bw.WriteInt32(TaeFileStubs.Count);
            for (int i = 0; i < TaeFileStubs.Count; i++)
            {
                bw.WriteByte((byte)TaeFileStubs[i].Flags);
                bw.WriteInt32(TaeFileStubs[i].ID);
                bw.WriteNullPrefixUTF16(TaeFileStubs[i].Name);
                //TaeFileStubs[i].Bytes
                bw.WriteInt32((int)TaeFileStubs[i].CompressionType);
            }
            bw.WriteInt32(SoundBanksToLoad.Count);
            for (int i = 0; i < SoundBanksToLoad.Count; i++)
            {
                bw.WriteASCII(SoundBanksToLoad[i], true);
            }
            prog?.Report(progStart + progWeight);
        }
        



    }
}
