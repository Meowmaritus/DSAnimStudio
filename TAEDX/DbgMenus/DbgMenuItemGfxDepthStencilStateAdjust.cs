//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DarkSoulsModelViewerDX.DbgMenus
//{
//    public class DbgMenuItemGfxDepthStencilStateAdjust : DbgMenuItem
//    {

//        public DbgMenuItemGfxDepthStencilStateAdjust()
//        {
//            Text = "[GFX.DepthStencilStateConfig]";

//            Items.Add(new DbgMenuItemEnum<StencilOperation>("StencilPass",
//                v => GFX.DepthStencilStateConfig.StencilPass = v,
//                () => GFX.DepthStencilStateConfig.StencilPass));
//            // StencilMask
//            Items.Add(new DbgMenuItemEnum<CompareFunction>("StencilFunction",
//               v => GFX.DepthStencilStateConfig.StencilFunction = v,
//               () => GFX.DepthStencilStateConfig.StencilFunction));
//            Items.Add(new DbgMenuItemEnum<StencilOperation>("StencilFail",
//                v => GFX.DepthStencilStateConfig.StencilFail = v,
//                () => GFX.DepthStencilStateConfig.StencilFail));
//            Items.Add(new DbgMenuItemBool("StencilEnable", "True", "False",
//                v => GFX.DepthStencilStateConfig.StencilEnable = v,
//                () => GFX.DepthStencilStateConfig.StencilEnable));
//            Items.Add(new DbgMenuItemEnum<StencilOperation>("StencilDepthBufferFail",
//                v => GFX.DepthStencilStateConfig.StencilDepthBufferFail = v,
//                () => GFX.DepthStencilStateConfig.StencilDepthBufferFail));
//            // ReferenceStencil
//            Items.Add(new DbgMenuItemEnum<CompareFunction>("DepthBufferFunction",
//                v => GFX.DepthStencilStateConfig.DepthBufferFunction = v,
//                () => GFX.DepthStencilStateConfig.DepthBufferFunction));
//            Items.Add(new DbgMenuItemEnum<CompareFunction>("CounterClockwiseStencilFunction",
//                v => GFX.DepthStencilStateConfig.CounterClockwiseStencilFunction = v,
//                () => GFX.DepthStencilStateConfig.CounterClockwiseStencilFunction));
//            // StencilWriteMask
//            Items.Add(new DbgMenuItemEnum<StencilOperation>("CounterClockwiseStencilFail",
//                v => GFX.DepthStencilStateConfig.CounterClockwiseStencilFail = v,
//                () => GFX.DepthStencilStateConfig.CounterClockwiseStencilFail));
//            Items.Add(new DbgMenuItemEnum<StencilOperation>("CounterClockwiseStencilDepthBufferFail",
//                v => GFX.DepthStencilStateConfig.CounterClockwiseStencilDepthBufferFail = v,
//                () => GFX.DepthStencilStateConfig.CounterClockwiseStencilDepthBufferFail));
//            Items.Add(new DbgMenuItemBool("DepthBufferWriteEnable", "True", "False",
//                v => GFX.DepthStencilStateConfig.DepthBufferWriteEnable = v,
//                () => GFX.DepthStencilStateConfig.DepthBufferWriteEnable));
//            Items.Add(new DbgMenuItemBool("DepthBufferEnable", "True", "False",
//                v => GFX.DepthStencilStateConfig.DepthBufferEnable = v,
//                () => GFX.DepthStencilStateConfig.DepthBufferEnable));
//            Items.Add(new DbgMenuItemEnum<StencilOperation>("CounterClockwiseStencilPass",
//                v => GFX.DepthStencilStateConfig.CounterClockwiseStencilPass = v,
//                () => GFX.DepthStencilStateConfig.CounterClockwiseStencilPass));
//            Items.Add(new DbgMenuItemBool("TwoSidedStencilMode", "True", "False",
//                v => GFX.DepthStencilStateConfig.TwoSidedStencilMode = v,
//                () => GFX.DepthStencilStateConfig.TwoSidedStencilMode));
//        }
//    }
//}
