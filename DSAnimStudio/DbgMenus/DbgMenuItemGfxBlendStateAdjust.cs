//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DarkSoulsModelViewerDX.DbgMenus
//{
//    public class DbgMenuItemGfxBlendStateAdjust : DbgMenuItem
//    {

//        public DbgMenuItemGfxBlendStateAdjust()
//        {
//            Text = "[GFX.BlendStateConfig]";

//            Items.Add(new DbgMenuItemEnum<BlendFunction>("AlphaBlendFunction", 
//                v => GFX.BlendStateConfig.AlphaBlendFunction = v, 
//                () => GFX.BlendStateConfig.AlphaBlendFunction));
//            Items.Add(new DbgMenuItemEnum<Blend>("AlphaDestinationBlend", 
//                v => GFX.BlendStateConfig.AlphaDestinationBlend = v, 
//                () => GFX.BlendStateConfig.AlphaDestinationBlend));
//            Items.Add(new DbgMenuItemEnum<Blend>("AlphaSourceBlend", 
//                v => GFX.BlendStateConfig.AlphaSourceBlend = v, 
//                () => GFX.BlendStateConfig.AlphaSourceBlend));

//            Items.Add(new DbgMenuItemNumber("BlendFactor.R", 0, 255, 1, v =>
//            {
//                GFX.BlendStateConfig.BlendFactor = new Color(
//                    (byte)(int)v, 
//                    GFX.BlendStateConfig.BlendFactor.G, 
//                    GFX.BlendStateConfig.BlendFactor.B, 
//                    GFX.BlendStateConfig.BlendFactor.A);
//            }, () => GFX.BlendStateConfig.BlendFactor.R));

//            Items.Add(new DbgMenuItemNumber("BlendFactor.G", 0, 255, 1, v =>
//            {
//                GFX.BlendStateConfig.BlendFactor = new Color(
//                    GFX.BlendStateConfig.BlendFactor.R, 
//                    (byte)(int)v, 
//                    GFX.BlendStateConfig.BlendFactor.B, 
//                    GFX.BlendStateConfig.BlendFactor.A);
//            }, () => GFX.BlendStateConfig.BlendFactor.G));

//            Items.Add(new DbgMenuItemNumber("BlendFactor.B", 0, 255, 1, v =>
//            {
//                GFX.BlendStateConfig.BlendFactor = new Color(
//                    GFX.BlendStateConfig.BlendFactor.R, 
//                    GFX.BlendStateConfig.BlendFactor.G, 
//                    (byte)(int)v, 
//                    GFX.BlendStateConfig.BlendFactor.A);
//            }, () => GFX.BlendStateConfig.BlendFactor.B));

//            Items.Add(new DbgMenuItemNumber("BlendFactor.A", 0, 255, 1, v =>
//            {
//                GFX.BlendStateConfig.BlendFactor = new Color(
//                    GFX.BlendStateConfig.BlendFactor.R, 
//                    GFX.BlendStateConfig.BlendFactor.G, 
//                    GFX.BlendStateConfig.BlendFactor.B, 
//                    (byte)(int)v);
//            }, () => GFX.BlendStateConfig.BlendFactor.A));

//            Items.Add(new DbgMenuItemEnum<BlendFunction>("ColorBlendFunction", 
//                v => GFX.BlendStateConfig.ColorBlendFunction = v, 
//                () => GFX.BlendStateConfig.ColorBlendFunction));
//            Items.Add(new DbgMenuItemEnum<Blend>("ColorDestinationBlend", 
//                v => GFX.BlendStateConfig.ColorDestinationBlend = v, 
//                () => GFX.BlendStateConfig.ColorDestinationBlend));
//            Items.Add(new DbgMenuItemEnum<Blend>("ColorSourceBlend", 
//                v => GFX.BlendStateConfig.ColorSourceBlend = v, 
//                () => GFX.BlendStateConfig.ColorSourceBlend));

//            Items.Add(new DbgMenuItemBool("IndependentBlendEnable", "True", "False",
//                v => GFX.BlendStateConfig.IndependentBlendEnable = v,
//                () => GFX.BlendStateConfig.IndependentBlendEnable));
//        }
//    }
//}
