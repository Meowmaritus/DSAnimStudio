using Managed.X86;
using System;
using System.Collections.Generic;
using System.Threading;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class ExtraFuncs
    {
        public static string GetNgPlusText(int ngLevel)
        {
            if (ngLevel == 0)
            {
                return "NG";
            }
            else if (ngLevel == 1)
            {
                return "NG+";
            }
            else
            {
                return "NG+" + ngLevel.ToString();
            }
        }

        public static void MsgBoxOK(string text)
        {
            SetGenDialog(text, 1, "OK");
        }

        public static void MsgBoxBtn(string text, string btnText)
        {
            MsgBoxChoice(text, btnText, "");
        }

        public static int MsgBoxChoice(string text, string choice1, string choice2)
        {
            return SetGenDialog(text, 2, choice1, choice2).Response;
        }

        //public static void Warp_Coords(float x, float y, float z, float? rotx = null)
        //{
        //    var charptr1 = RInt32(0x137dc70);
        //    charptr1 = RInt32(charptr1 + 0x4);
        //    charptr1 = RInt32(charptr1);
        //    var charmapdataptr = RInt32(charptr1 + 0x28);

        //    WFloat(charmapdataptr + 0xd0, x);
        //    WFloat(charmapdataptr + 0xd4, y);
        //    WFloat(charmapdataptr + 0xd8, z);

        //    if (rotx.HasValue)
        //    {
        //        WFloat(charmapdataptr + 0xE4, (float)(((rotx / 360) * 2 * Math.PI) - Math.PI));
        //    }
        //    else
        //    {
        //        WFloat(charmapdataptr + 0xe4, Entity.Player.Location.Heading);
        //    }

        //    WBool(charmapdataptr + 0xc8, true);
        //}

        //public static void WarpEntity_Coords(int entityPtr, float x, float y, float z, float rotx)
        //{
        //    entityPtr = RInt32(entityPtr + 0x28);

        //    WFloat(entityPtr + 0xd0, x);
        //    WFloat(entityPtr + 0xd4, y);
        //    WFloat(entityPtr + 0xd8, z);

        //    float facing = (float)(((rotx / 360) * 2 * Math.PI) - Math.PI);

        //    WFloat(entityPtr + 0xe4, facing);
        //    WBool(entityPtr + 0xc8, true);
        //}

        public static void BlackScreen()
        {
            uint tmpptr = 0;
            tmpptr = RUInt32(0x1378520);
            tmpptr = RUInt32(tmpptr + 0x10);

            WBool(tmpptr + 0x26D, true);

            WFloat(tmpptr + 0x270, 0);
            WFloat(tmpptr + 0x274, 0);
            WFloat(tmpptr + 0x278, 0);
        }

        public static void ClearPlayTime()
        {
            int tmpPtr = RInt32(0x1378700);
            WInt32(tmpPtr + 0x68, 0);
        }

        
        /*
         *   Use Chr.View() instead
         */
        //public static void ControlEntity(int entityPtr, bool state)
        //{
        //    entityPtr = RInt32(entityPtr + 0x28);

        //    int ctrlptr = RInt32(0x137dc70);
        //    ctrlptr = RInt32(ctrlptr + 4);
        //    ctrlptr = RInt32(ctrlptr);
        //    ctrlptr = RInt32(ctrlptr + 0x28);
        //    ctrlptr = RInt32(ctrlptr + 0x54);

        //    WInt32(entityPtr + 0x244, ctrlptr * (state ? 0xff : 0 & 1));
        //}

        public static void DisableAI(bool state)
        {
            WBool(0x13784ee, state);
        }

        public static void PlayerExterminate(bool state)
        {
            WBool(0x13784d3, state);
        }

        public static void FadeIn()
        {
            int tmpptr = 0;
            tmpptr = RInt32(0x1378520);
            tmpptr = RInt32(tmpptr + 0x10);

            WBool(tmpptr + 0x26d, true);

            float val = 0.0F;

            for (int i = 0; i <= 33; i++)
            {
                val = val + 0.03F;
                WFloat(tmpptr + 0x270, val);
                WFloat(tmpptr + 0x274, val);
                WFloat(tmpptr + 0x278, val);
                Thread.Sleep(33);
            }

            WBool(tmpptr + 0x26d, false);
        }

        public static void FadeOut()
        {
            int tmpptr = 0;
            tmpptr = RInt32(0x1378520);
            tmpptr = RInt32(tmpptr + 0x10);

            WBool(tmpptr + 0x26D, true);

            float val = 1.0F;

            for (int i = 0; i <= 33; i++)
            {
                val = val - 0.03F;
                val = val - 0.03F;
                val = val - 0.03F;
                WFloat(tmpptr + 0x270, val);
                WFloat(tmpptr + 0x274, val);
                WFloat(tmpptr + 0x278, val);
                Thread.Sleep(33);
            }
        }

        public static void ForceEntityDrawGroup(int entityptr)
        {
            WInt32(entityptr + 0x264, -1);
            WInt32(entityptr + 0x268, -1);
            WInt32(entityptr + 0x26c, -1);
            WInt32(entityptr + 0x270, -1);
        }

        /*
         * These are just garbage tbh
         */

        //public static void SetCamPos(float xpos, float ypos, float zpos, float xrot, float yrot)
        //{
        //    int tmpPtr = 0;

        //    tmpPtr = RInt32(0x1378714);

        //    WFloat(tmpPtr + 0xb0, xpos);
        //    WFloat(tmpPtr + 0xb4, ypos);
        //    WFloat(tmpPtr + 0xb8, zpos);

        //    tmpPtr = RInt32(0x137d6dc);
        //    tmpPtr = RInt32(tmpPtr + 0x3c);
        //    tmpPtr = RInt32(tmpPtr + 0x60);

        //    WFloat(tmpPtr + 0x144, xrot);
        //    WFloat(tmpPtr + 0x150, yrot);
        //}

        //public static void SetFreeCam(bool state)
        //{
        //    if (state)
        //    {
        //        //WBytes(&HEFDBAF, {&H90, &H90, &H90, &H90, &H90})
        //        WBytes(0x404e59, new byte[] {
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90
        //    });
        //        WBytes(0x404e63, new byte[] {
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90
        //    });
        //        WBytes(0xf06c46, new byte[] {
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90,
        //        0x90
        //    });
        //    }
        //    else
        //    {
        //        //WBytes(&HEFDBAF, {&HE8, &H7c, &H72, &H50, &HFF})
        //        WBytes(0x404e59, new byte[] {
        //        0x66,
        //        0xf,
        //        0xd6,
        //        0x46,
        //        0x20
        //    });
        //        WBytes(0x404e63, new byte[] {
        //        0x66,
        //        0xf,
        //        0xd6,
        //        0x46,
        //        0x28
        //    });
        //        WBytes(0xf06c46, new byte[] {
        //        0xf3,
        //        0xf,
        //        0x11,
        //        0x83,
        //        0x44,
        //        0x1,
        //        0x0,
        //        0x0
        //    });
        //    }
        //}

        public static void SetClearCount(int clearCount)
        {
            int tmpPtr = 0;
            tmpPtr = RInt32(0x1378700);

            WInt32(tmpPtr + 0x3c, clearCount);
        }

        private static void SetCaption(string str)
        {
            int tmpptr = 0;
            byte alpha = 0;

            bool state = false;
            state = (str.Length > 0);

            if (state)
            {
                alpha = 254;
            }
            else
            {
                alpha = 0;
            }

            tmpptr = RInt32(0x13786d0);

            WInt32(tmpptr + 0x40, (state ? 1 : 0) & 4);
            WInt32(tmpptr + 0xb18, alpha);
            WInt32(tmpptr + 0xb14, 100);

            tmpptr = RInt32(0x13785dc);
            tmpptr = RInt32(tmpptr + 0x10);

            WUnicodeStr(tmpptr + 0x12c, str + (char)0);
        }

        public static void SetSaveEnable(bool state)
        {
            int tmpPtr = 0;
            tmpPtr = RInt32(0x13784a0);

            WBool(tmpPtr + 0xb40, state);
        }

        public static void SetSaveSlot(int slot)
        {
            WInt32(RInt32(0x13784a0) + 0xa70, slot);
        }

        public static void SetUnknownNpcName(string name)
        {
            if (name.Length > 21)
                name = name.Substring(0, 21);
            //Prevent runover into code
            WUnicodeStr(0x11a784c, name + (char)0);
        }

        public static int GetClosestEntityToEntity(int entityPtr)
        {
            var ptrList = GetEntityPtrList();

            var closestDist = float.PositiveInfinity;
            int closestPtr = -1;

            foreach (int p in ptrList)
            {
                if (p == entityPtr)
                    continue;
                var dist = GetDistanceBetweenEntities(entityPtr, p);
                if ((dist < closestDist))
                {
                    closestPtr = p;
                    closestDist = dist;
                }
            }

            return closestPtr;
        }

        public static int[] GetEntityPtrList()
        {
            var resultList = new List<int>();

            int tmpPtr = 0;

            int mapCount = 0;
            int mapPtrs = 0;

            int entitiesPtr = 0;
            int entitiesCnt = 0;

            int entityPtr = 0;

            tmpPtr = RInt32(0x137d644);

            mapPtrs = tmpPtr + 0x74;
            mapCount = RInt32(tmpPtr + 0x70);

            for (int mapNum = 0; mapNum <= mapCount - 1; mapNum++)
            {
                entitiesPtr = RInt32(mapPtrs + 4 * mapNum);
                entitiesCnt = RInt32(entitiesPtr + 0x3c);
                entitiesPtr = RInt32(entitiesPtr + 0x40);

                for (int entityNum = 0; entityNum <= entitiesCnt - 1; entityNum++)
                {
                    entityPtr = RInt32(entitiesPtr + entityNum * 0x20);

                    resultList.Add(entityPtr);
                }
            }

            return resultList.ToArray();
        }

        //public static LuaTable GetAllCurrentlyLoadedMsbData(Delegate getNewTable)
        //{
        //    int tmpPtr = 0;
        //    int tmpCnt = 0;

        //    int mapCount = 0;
        //    int mapPtrs = 0;

        //    int entitiesPtr = 0;
        //    int entitiesCnt = 0;

        //    int entityPtr = 0;

        //    var tableIndexMapName = "";
        //    var tableIndexEntityName = "";

        //    tmpPtr = RInt32(0x137d644);

        //    mapPtrs = tmpPtr + 0x74;
        //    mapCount = RInt32(tmpPtr + 0x70);

        //    const int maxMapNameLength = 12;
        //    //Length of "mXX_XX_XX_XX"
        //    const int maxEntityNameLength = 10;
        //    //NEEDS TESTING

        //    var result = (LuaTable)(getNewTable.DynamicInvoke());
        //    for (int mapNum = 0; mapNum <= mapCount - 1; mapNum++)
        //    {
        //        entitiesPtr = RInt32(mapPtrs + 4 * mapNum);

        //        tableIndexMapName = RUnicodeStr(RInt32(RInt32(entitiesPtr + 0x60) + 4), maxMapNameLength);

        //        //Console.WriteLine("MAP " & tableIndexMapName)

        //        entitiesCnt = RInt32(entitiesPtr + 0x3c);
        //        entitiesPtr = RInt32(entitiesPtr + 0x40);

        //        result[tableIndexMapName] = (LuaTable)(getNewTable.DynamicInvoke());

        //        for (int entityNum = 0; entityNum <= entitiesCnt - 1; entityNum++)
        //        {
        //            entityPtr = RInt32(entitiesPtr + entityNum * 0x20);

        //            tmpPtr = RInt32(entityPtr + 0x54);
        //            tmpCnt = RInt32(entityPtr + 0x58) - 1;

        //            tmpPtr = RInt32(tmpPtr + 0x28) + 0x10;
        //            tmpPtr = RInt32(RInt32(tmpPtr + 4 * tmpCnt));

        //            tableIndexEntityName = RAsciiStr(tmpPtr, maxEntityNameLength);

        //            //Console.WriteLine("ENTITY " & tableIndexEntityName)

        //            int thisEntityPtr = entityPtr;

        //            result[tableIndexMapName + "." + tableIndexEntityName] = new Entity(() => thisEntityPtr);
        //        }
        //    }

        //    return result;
        //}

        //public static Vec3 GetEntityVec3(int entityPtr)
        //{
        //    return new Vec3(GetEntityPosX(entityPtr), GetEntityPosY(entityPtr), GetEntityPosZ(entityPtr));
        //}

        public static void MoveEntityLaterallyTowardEntity(int entityFromPtr, int entityToPtr, float speed)
        {
            MoveEntityLaterally(entityFromPtr, GetAngleBetweenEntities(entityFromPtr, entityToPtr), speed);
        }

        public static float GetAngleBetweenEntities(int entityPtrA, int entityPtrB)
        {
            var x1 = GetEntityPosX(entityPtrA);
            var z1 = GetEntityPosZ(entityPtrA);

            var x2 = GetEntityPosX(entityPtrB);
            var z2 = GetEntityPosZ(entityPtrB);

            return (float)Math.Atan2(z2 - z1, x2 - x1);
            //TODO: Check my trig cuz I did this at 6:42 AM
        }

        public static float GetDistanceSqrdBetweenEntities(int entityPtrA, int entityPtrB)
        {
            var x1 = GetEntityPosX(entityPtrA);
            var y1 = GetEntityPosY(entityPtrA);
            var z1 = GetEntityPosZ(entityPtrA);

            var x2 = GetEntityPosX(entityPtrB);
            var y2 = GetEntityPosY(entityPtrB);
            var z2 = GetEntityPosZ(entityPtrB);

            return (float)Math.Pow(x1 - x2, 2) + (float)Math.Pow(y1 - y2, 2) + (float)Math.Pow(z1 - z2, 2);
        }

        public static float GetDistanceBetweenEntities(int entityPtrA, int entityPtrB)
        {
            return (float)Math.Sqrt(GetDistanceSqrdBetweenEntities(entityPtrA, entityPtrB));
        }

        public static void MoveEntityLaterally(int entityPtr, float angle, float speed)
        {
            MoveEntityAtSpeed(entityPtr, (float)Math.Cos(angle) * speed, 0, (float)Math.Sin(angle) * speed, 0);
        }

        public static void MoveEntityAtSpeed(int entityPtr, float speedX, float speedY, float speedZ, float speedRot = 0)
        {
            SetEntityPosX(entityPtr, GetEntityPosX(entityPtr) + speedX);
            SetEntityPosY(entityPtr, GetEntityPosY(entityPtr) + speedY);
            SetEntityPosZ(entityPtr, GetEntityPosZ(entityPtr) + speedZ);
            SetEntityRotation(entityPtr, GetEntityRotation(entityPtr) + speedRot);
        }

        public static float GetEntityPosX(int entityPtr)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            return RFloat(entityPosPtr + 0x10);
        }

        public static float GetEntityPosY(int entityPtr)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            return RFloat(entityPosPtr + 0x14);
        }

        public static float GetEntityPosZ(int entityPtr)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            return RFloat(entityPosPtr + 0x18);
        }

        public static float GetEntityRotation(int entityPtr)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            return Convert.ToSingle(RFloat(entityPosPtr + 0x4) / Math.PI * 180) + 180;
        }

        public static void SetEntityPosX(int entityPtr, float posX)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            WFloat(entityPosPtr + 0x10, posX);
        }

        public static void SetEntityPosY(int entityPtr, float posY)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            WFloat(entityPosPtr + 0x14, posY);
        }

        public static void SetEntityPosZ(int entityPtr, float posZ)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            WFloat(entityPosPtr + 0x18, posZ);
        }

        public static void SetEntityRotation(int entityPtr, float angle)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            WFloat(entityPosPtr + 0x4, Convert.ToSingle(angle * Math.PI / 180) - Convert.ToSingle(Math.PI));
        }

        public static void SetEntityCoordsDirectly(int entityPtr, float posX, float posY, float posZ, float? angle)
        {
            var entityPosPtr = RInt32(entityPtr + 0x28);
            entityPosPtr = RInt32(entityPosPtr + 0x1c);
            WFloat(entityPosPtr + 0x10, posX);
            WFloat(entityPosPtr + 0x14, posY);
            WFloat(entityPosPtr + 0x18, posZ);
            WFloat(entityPosPtr + 0x4, angle.HasValue ? Convert.ToSingle(angle * Math.PI / 180 - Math.PI) : 0);
        }

        public static int GetInGameTimeInMs()
        {
            return RInt32(RInt32(0x1378700) + 0x68);
        }

        //public static void SetEntityLoc(int entityPtr, Loc location)
        //{
        //    WarpEntity_Coords(entityPtr, location.Pos.X, location.Pos.Y, location.Pos.Z, location.Rot.HeadingValue);
        //}

        //public static void PlayerHide(bool state)
        //{
        //    WBool(0x13784e7, state);
        //}

        public static void ShowHUD(bool state)
        {
            uint tmpptr = 0;
            tmpptr = RUInt32(0x1378700);
            tmpptr = RUInt32(tmpptr + 0x2c);

            WBool(tmpptr + 0xD, state);
        }

        ////TODO: waitforload -> WaitForLoadEnd
        //public static void WaitForLoadEnd()
        //{
        //    int tmpptr = 0;
        //    tmpptr = RInt32(0x1378700);

        //    int msPlayed = 0;
        //    bool loading = true;

        //    msPlayed = RInt32(tmpptr + 0x68);

        //    while (loading)
        //    {
        //        loading = (msPlayed == RInt32(tmpptr + 0x68));
        //        Thread.Sleep(33);
        //    }
        //}

        ////TODO: waittillload -> WaitForLoadStart
        //public static void WaitForLoadStart()
        //{
        //    int tmpptr = 0;
        //    tmpptr = RInt32(0x1378700);

        //    int msPlayed = 0;
        //    bool loading = false;

        //    msPlayed = RInt32(tmpptr + 0x68);

        //    while (!loading)
        //    {
        //        loading = (msPlayed == RInt32(tmpptr + 0x68));
        //        Thread.Sleep(33);
        //    }
        //}

        //public static void WarpEntity_Player(int entityptr)
        //{
        //    int playerptr = DSLua.Expr("GetEntityPtr(10000)");
        //    WarpEntity_Entity(entityptr, playerptr);
        //}

        //public static void WarpPlayer_Entity(int entityptr)
        //{
        //    int playerptr = DSLua.Expr("GetEntityPtr(10000)");
        //    WarpEntity_Entity(playerptr, entityptr);
        //}

        //public static void WarpEntity_Entity(int entityptrSrc, int entityptrDest)
        //{
        //    //TODO: Check validity of entity pointers
        //    var destEntityPosPtr = RInt32(entityptrDest + 0x28);
        //    destEntityPosPtr = RInt32(destEntityPosPtr + 0x1c);
        //    var facing = RFloat(destEntityPosPtr + 0x4);
        //    var posX = RFloat(destEntityPosPtr + 0x10);
        //    var posY = RFloat(destEntityPosPtr + 0x14);
        //    var posZ = RFloat(destEntityPosPtr + 0x18);

        //    WarpEntity_Coords(entityptrSrc, posX, posY, posZ, facing);
        //}

        public static int GetEntityPtrByName(string mapName, string entName)
        {
            string tmpStr = "";

            int tmpPtr = 0;
            int tmpCnt = 0;

            int mapCount = 0;
            int mapPtrs = 0;

            int entitiesPtr = 0;
            int entitiesCnt = 0;

            int entityPtr = 0;

            tmpPtr = RInt32(0x137d644);

            mapPtrs = tmpPtr + 0x74;
            mapCount = RInt32(tmpPtr + 0x70);

            const int maxMapNameLength = 12;
            //Length of "mXX_XX_XX_XX"
            const int maxEntityNameLength = 10;
            //NEEDS TESTING

            for (int mapNum = 0; mapNum <= mapCount - 1; mapNum++)
            {
                entitiesPtr = RInt32(mapPtrs + 4 * mapNum);

                tmpStr = RUnicodeStr(RInt32(RInt32(entitiesPtr + 0x60) + 4), maxMapNameLength);

                if (tmpStr == mapName)
                {
                    entitiesCnt = RInt32(entitiesPtr + 0x3c);
                    entitiesPtr = RInt32(entitiesPtr + 0x40);

                    for (int entityNum = 0; entityNum <= entitiesCnt - 1; entityNum++)
                    {
                        entityPtr = RInt32(entitiesPtr + entityNum * 0x20);

                        tmpPtr = RInt32(entityPtr + 0x54);
                        tmpCnt = RInt32(entityPtr + 0x58) - 1;

                        tmpPtr = RInt32(tmpPtr + 0x28) + 0x10;
                        tmpPtr = RInt32(RInt32(tmpPtr + 4 * tmpCnt));

                        tmpStr = RAsciiStr(tmpPtr, maxEntityNameLength);

                        if (tmpStr == entName)
                        {
                            return entityPtr;
                        }
                    }
                }
            }
            return 0;
        }

        public static void SetBriefingMsg(string str)
        {
            int tmpptr = 0;
            tmpptr = RInt32(0x13785dc);
            tmpptr = RInt32(tmpptr + 0x7c);

            WUnicodeStr(tmpptr + 0x3b7a, str + (char)0);

            IngameFuncs.RequestOpenBriefingMsg(10010721, true);
        }

        public class GenDiagResult
        {
            public int Response = 0;
            public int Val = 0;

            public GenDiagResult(int response, int val)
            {
                this.Response = response;
                this.Val = val;
            }
        }

        //TODO: Make less bad

        public static GenDiagResult SetGenDialog(string str, int type, string btn0 = "", string btn1 = "")
        {
            //50002 = Overridden Maintext
            //65000 = Overridden Button 0
            //70000 = Overridden Button 1

            int tmpptr = 0;
            tmpptr = RInt32(0x13785dc);
            tmpptr = RInt32(tmpptr + 0x174);

            str = str.Replace('\n', (char)0xA);

            //Weird issues if exactly 6 characters
            if (str.Length == 6)
                str = str + "  ";
            WUnicodeStr(tmpptr + 0x1a5c, str + (char)0);

            //Set Default Ok/Cancel if not overridden
            WInt32(0x12e33e4, 1);
            WInt32(0x12e33e8, 2);

            //Clear previous values
            WInt32(0x12e33f8, -1);
            WInt32(0x12e33fc, -1);

            WInt32(0x12e33e0, 50002);
            if (btn0.Length > 0)
            {
                WInt32(0x12e33e4, 65000);
                WUnicodeStr(tmpptr + 0x2226, btn0 + (char)0);
            }
            if (btn1.Length > 0)
            {
                WInt32(0x12e33e8, 70000);
                WUnicodeStr(tmpptr + 0x350c, btn1 + (char)0);
            }

            tmpptr = RInt32(0x13786d0);
            WInt32(tmpptr + 0x60, type);

            //Wait for response
            var genDiagResponse = -1;
            var genDiagVal = -1;

            tmpptr = 0x12e33f8;

            while (genDiagResponse == -1)
            {
                genDiagResponse = RInt32(tmpptr);
                genDiagVal = RInt32(tmpptr + 0x4);
                Thread.Sleep(33);
            }
            Thread.Sleep(500);

            //EVERY TIME I TRIED RETURNING A LUA TABLE ALL THE VALUES IN IT WERE NIL BECAUSE FUCK ME
            return new GenDiagResult(genDiagResponse, genDiagVal);
        }

        public static void Wait(int val)
        {
            Thread.Sleep(val);
        }

        //Public Shared Function WaitForBossDeath(ByVal boost As Integer, match As Integer) As Boolean
        //    Dim eventPtr As Integer
        //    eventPtr = RInt32(&H137D7D4)
        //    eventPtr = RInt32(eventPtr)

        //    Dim hpPtr As Integer
        //    hpPtr = RInt32(&H137DC70)
        //    hpPtr = RInt32(hpPtr + 4)
        //    hpPtr = RInt32(hpPtr)
        //    hpPtr = hpPtr + &H2D4

        //    Dim bossdead As Boolean = False
        //    Dim selfdead As Boolean = False

        //    While Not (bossdead Or selfdead)
        //        bossdead = (RInt32(eventPtr + boost) And match)
        //        selfdead = (RInt32(hpPtr) = 0)
        //        Console.WriteLine(Hex(eventPtr) & " - " & Hex(RInt32(eventPtr)))
        //        Thread.Sleep(33)
        //    End While

        //    If bossdead Then
        //        Return True
        //    Else
        //        Return False
        //    End If
        //End Function

        //public static void DropItem(string cat, string item, int num)
        //{
        //    var TargetBufferSize = 1024;

        //    byte[] bytes = null;
        //    byte[] bytes2 = null;

        //    int bytcat = 0x1;
        //    int bytitem = 0x6;
        //    int bytcount = 0x10;
        //    int bytptr1 = 0x15;
        //    int bytptr2 = 0x32;
        //    int bytjmp = 0x38;

        //    bytes = new byte[] { 0xbd, 0, 0, 0, 0, 0xbb, 0xf0, 0x0, 0x0, 0x0, 0xb9, 0xff, 0xff, 0xff, 0xff, 0xba,
        //    0, 0, 0, 0, 0xa1, 0xd0, 0x86, 0x37, 0x1, 0x89, 0xa8, 0x28, 0x8, 0x0, 0x0, 0x89,
        //    0x98, 0x2c, 0x8, 0x0, 0x0, 0x89, 0x88, 0x30, 0x8, 0x0, 0x0, 0x89, 0x90, 0x34, 0x8, 0x0,
        //    0x0, 0xa1, 0xbc, 0xd6, 0x37, 0x1, 0x50, 0xe8, 0, 0, 0, 0, 0xc3 };

        //    //cllItemCatsIDs(clsItemCatsIDs("Weapons") / &H10000000)("Target Shield+15"))

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(ScriptLibResources.clsItemCatsIDs[cat]));
        //    Array.Copy(bytes2, 0, bytes, bytcat, bytes2.Length);

        //    int tmpCat = 0;
        //    tmpCat = Convert.ToInt32((int)ScriptLibResources.clsItemCatsIDs[cat] / 0x10000000);
        //    if (tmpCat == 4)
        //        tmpCat = 3;

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(ScriptLibResources.cllItemCatsIDs[tmpCat][item]));
        //    Array.Copy(bytes2, 0, bytes, bytitem, bytes2.Length);

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(num));
        //    Array.Copy(bytes2, 0, bytes, bytcount, bytes2.Length);

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(0x13786d0));
        //    Array.Copy(bytes2, 0, bytes, bytptr1, bytes2.Length);

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(0x137d6bc));
        //    Array.Copy(bytes2, 0, bytes, bytptr2, bytes2.Length);

        //    bytes2 = BitConverter.GetBytes(Convert.ToInt32(0 - (((uint)Injected.ItemDropPointers.GetHandle() + 0x3C) - (0xDC8C60))));
        //    Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);

        //    Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)Injected.ItemDropPointers.GetHandle(), bytes, TargetBufferSize, 0);
        //    //MsgBox(Hex(dropPtr))
        //    Kernel.CreateRemoteThread(DARKSOULS.GetHandle(), 0, 0, (uint)Injected.ItemDropPointers.GetHandle(), 0, 0, 0);

        //    Thread.Sleep(5);
        //}

        public static void SetKeyGuideText(string text)
        {
            WInt32(Pointers.MenuPtr + 0x158, RInt32(Pointers.MenuPtr + 0x1c));
            WUnicodeStr(0x11a7770, text.Replace('\n', (char)0xA));
        }

        public static void SetLineHelpText(string text)
        {
            WInt32(Pointers.MenuPtr + 0x154, RInt32(Pointers.MenuPtr + 0x1c));
            WUnicodeStr(0x11a7758, text.Replace('\n', (char)0xA));
        }

        public static void SetKeyGuideTextPos(float x, float y)
        {
            WFloat(Pointers.KeyPtr + 0x78, x);
            WFloat(Pointers.KeyPtr + 0x7c, y);
        }

        public static void SetLineHelpTextPos(float x, float y)
        {
            WFloat(Pointers.LinePtr + 0x78, x);
            WFloat(Pointers.LinePtr + 0x7C, y);
        }

        public static void SetKeyGuideTextClear()
        {
            WInt32(Pointers.MenuPtr + 0x158, -1);
        }

        public static void SetLineHelpTextClear()
        {
            WInt32(Pointers.MenuPtr + 0x154, -1);
        }

        public static void ForcePlayerStableFootPos()
        {
            WorldState.LastStandPosX = WorldChrMan.LocalPlayer.MovementCtrl.Transform.X;
            WorldState.LastStandPosY = WorldChrMan.LocalPlayer.MovementCtrl.Transform.Y;
            WorldState.LastStandPosZ = WorldChrMan.LocalPlayer.MovementCtrl.Transform.Z;
        }

        public static int GetChrPtr(int entityId)
        {
            //throw new NotImplementedException(); //TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 
            return CallReg<int>(0xD6C360, new dynamic[] { entityId }, eax: entityId);
        }



        /*
            char __usercall sub_D770C0@<al>(signed int a1@<eax>, int a2@<edi>)
            {
              int v2; // eax@1
              int v3; // esi@2
              int v4; // eax@3
              int v5; // esi@5
              char result; // al@7

              v2 = GET_POINTER_FROM_CHRID(a1);
              if ( v2 && (v3 = *(v2 + 0x28)) != 0 )
              {
                *(*(*(v3 + 0x10) + 0x10) + 0x2C) = a2;
                v4 = *(v3 + 0x20);
                if ( v4 )
                  sub_EC7440(v4, a2);
                *(*(*(v3 + 0x10) + 0x10) + 0x30) = a2 > 0;
                v5 = *(v3 + 0x20);
                if ( v5 )
                {
                  if ( a2 > 0 )
                  {
                    *(v5 + 0x61) |= 2u;
                    return 1;
                  }
                  *(v5 + 0x61) &= 0xFDu;
                }
                result = 1;
              }
              else
              {
                result = 0;
              }
              return result;
            } 
        */
        public static bool ChrUpdateHitMask(Enemy chr, int NewHitMask)
        {
            if (chr.Address == 0)
                return false;

            int addr = chr.MovementCtrl.Address;

            if (addr <= 0)
                return false;

            addr = RInt32(addr + 0x10);
            addr = RInt32(addr + 0x10);

            WInt32(addr + 0x2C, NewHitMask);

            var something = RInt32(chr.MovementCtrl.Address + 0x20);

            if (something > 0)
            {
                Call<int>(0xEC7440, something, NewHitMask);
            }

            WBool(addr + 0x30, NewHitMask > 0);

            something = RInt32(chr.MovementCtrl.Address + 0x20);

            if (something > 0)
            {
                var somethingElse = RUInt32(something + 0x61);

                if (NewHitMask > 0)
                {
                    somethingElse |= 2u;
                    WUInt32(something + 0x61, somethingElse);
                }
                else
                {
                    somethingElse &= 0xFDu;
                    WUInt32(something + 0x61, somethingElse);
                }
            }

            return true;
        }
    }
}
