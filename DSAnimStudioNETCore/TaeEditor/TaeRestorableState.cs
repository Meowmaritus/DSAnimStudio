using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.TaeEditor
{
    public abstract class TaeRestorableState
    {
        public TaeEditorScreen MainScreen;
        public TAE.Template TaeTemplate;

        public bool IsCustomRegist;
        public List<int> RegisteredAnimCategories = new List<int>();
        public List<SplitAnimID> RegisteredAnimations = new List<SplitAnimID>();

        protected TaeRestorableState(TaeEditorScreen mainScreen, TAE.Template template, bool isCustomRegist, List<int> registeredAnimCategories, List<SplitAnimID> registeredAnims)
        {
            MainScreen = mainScreen;
            TaeTemplate = template;
            IsCustomRegist = isCustomRegist;
            RegisteredAnimCategories = registeredAnimCategories;
            RegisteredAnimations = registeredAnims;

            if (IsCustomRegist)
            {
                var proj = mainScreen.Proj;
                foreach (var categoryID in registeredAnimCategories)
                {
                    if (!CustomSerializedAnimCategories.ContainsKey(categoryID))
                        CustomSerializedAnimCategories.Add(categoryID, new List<byte[]>());
                    var categories = proj.SAFE_GetAnimCategoriesByID(categoryID);
                    foreach (var cate in categories)
                    {
                        var serialized = cate.SAFE_SerializeToBytes(proj);
                        CustomSerializedAnimCategories[categoryID].Add(serialized);
                    }
                }

                foreach (var animID in registeredAnims)
                {
                    // Anim will be included with category regist.
                    if (registeredAnimCategories.Contains(animID.CategoryID))
                        continue;

                    if (!CustomSerializedAnims.ContainsKey(animID))
                        CustomSerializedAnims.Add(animID, new List<byte[]>());
                    var anims = proj.GetAllAnimsWithID(animID);
                    foreach (var anim in anims)
                    {
                        var serialized = anim.SerializeToBytes(proj);
                        CustomSerializedAnims[animID].Add(serialized);
                    }
                }
            }
        }

        public Dictionary<int, List<byte[]>> CustomSerializedAnimCategories = new Dictionary<int, List<byte[]>>();
        public Dictionary<SplitAnimID, List<byte[]>> CustomSerializedAnims = new Dictionary<SplitAnimID, List<byte[]>>();

        public int GetByteCount()
        {
            int result = 0;
            if (SerializedBytes != null)
            {
                result += SerializedBytes.Length;
            }
            foreach (var kvp in CustomSerializedAnimCategories)
            {
                foreach (var item in kvp.Value)
                {
                    result += item.Length;
                }
            }
            foreach (var kvp in CustomSerializedAnims)
            {
                foreach (var item in kvp.Value)
                {
                    result += item.Length;
                }
            }
            return result;
        }

        public byte[] SerializedBytes;
        public abstract void InnerRestoreState();
        public void RestoreState()
        {
            var proj = MainScreen.Proj;
            if (IsCustomRegist)
            {
                
                foreach (var kvp in CustomSerializedAnimCategories)
                {
                    if (proj.SAFE_CategoryExists(kvp.Key))
                    {
                        proj.SAFE_RemoveAllCategoriesWithID(kvp.Key);
                        //var existingCategories = proj.GetAllAnimCategoriesFromCategoryID(kvp.Key);
                        //foreach (var existing in existingCategories)
                        //{
                        //    proj.NEW_RemoveAnimCategory(existing);
                        //}
                    }

                    foreach (var serializedBytes in kvp.Value)
                    {
                        var category = new DSAProj.AnimCategory(proj);
                        category.SAFE_DeserializeFromBytes(serializedBytes, TaeTemplate, proj);
                        proj.SAFE_AddAnimCategory(category);
                    }
                }

                foreach (var kvp in CustomSerializedAnims)
                {
                    if (proj.SAFE_AnimExists(kvp.Key))
                    {
                        proj.SAFE_RemoveAllInstancesOfAnimationByID(kvp.Key);
                    }

                    foreach (var serializedBytes in kvp.Value)
                    {
                        var category = proj.SAFE_RegistCategory(kvp.Key.CategoryID);
                        var anim = new DSAProj.Animation(proj, category);
                        anim.DeserializeFromBytes(serializedBytes, TaeTemplate, proj, category);
                        category.SAFE_AddAnimation(anim);
                    }
                }
            }
            else
            {
                InnerRestoreState();
            }

            proj.ScanForErrors_Background();
        }
    }
}
