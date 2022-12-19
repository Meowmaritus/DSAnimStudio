using DSAnimStudio.DbgMenus;
using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class Tests
    {
        //static void TestMCP_MGC(string mapName)
        //{
        //    if (InterrootLoader.Type != InterrootLoader.InterrootType.InterrootDS1)
        //        return;

        //    //GFX.ModelDrawer.AddMap(mapName, false);

        //    var box = new DbgPrimWireBox(Transform.Default, Vector3.One, Color.Red);
        //    var mcp = DataFile.LoadFromFile<MCP>(InterrootLoader.GetInterrootPath($@"map\{mapName}\{mapName}.mcp"));
        //    int bi = 0;
        //    foreach (var mcpBox in mcp.Boxes)
        //    {
        //        var min = new Vector3(mcpBox.MinX, mcpBox.MinY, mcpBox.MinZ);
        //        var max = new Vector3(mcpBox.MaxX, mcpBox.MaxY, mcpBox.MaxZ);
        //        var size = max - min;
        //        var center = min + (size / 2f);
        //        DBG.AddPrimitive(box.Instantiate($"Box [{(bi++)}]", new Transform(center.X, center.Y, center.Z, 0, 0, 0, size.X, size.Y, size.Z)));
        //    }
        //    var sphere = new DbgPrimWireSphere(Transform.Default, 0.35f, 32, 32, Color.Cyan);
        //    var mcg = DataFile.LoadFromFile<MCG>(InterrootLoader.GetInterrootPath($@"map\{mapName}\{mapName}.mcg"));
        //    var lines = new DbgPrimWire();
        //    List<Vector3> startPoints = new List<Vector3>();
        //    List<Vector3> endPoints = new List<Vector3>();
        //    for (int i = 0; i < mcg.Paths.Count; i++)
        //    {
        //        var pointsThatReferenceThisShit = mcg.Points.Where(x => x.NearbyPathIndices.Contains(i)).ToList();
        //        startPoints.Add(new Vector3(pointsThatReferenceThisShit[0].PosX, pointsThatReferenceThisShit[0].PosY, pointsThatReferenceThisShit[0].PosZ));
        //        endPoints.Add(new Vector3(pointsThatReferenceThisShit[1].PosX, pointsThatReferenceThisShit[1].PosY, pointsThatReferenceThisShit[1].PosZ));
        //    }
        //    void AddPath(int pathIndex, Color col) =>
        //        lines.AddLine(startPoints[pathIndex], endPoints[pathIndex], col);
        //    void AddPathString(int pathIndex, string str, Color col)
        //    {
        //        var position = (startPoints[pathIndex] + endPoints[pathIndex]) / 2.0f;
        //        lines.AddDbgLabel(position, 5f, str, col);
        //    }

        //    //for (int i = 0; i < mcg.Paths.Count; i++)
        //    //{
        //    //    AddPath(i, Color.Cyan);
        //    //    AddPathString(i, mcg.Paths[i].GetDebugReport(), Color.Cyan);

        //    //    //foreach (var edgeIndex in mcg.Edges[i].OtherEdgeIndicesA)
        //    //    //{
        //    //    //    AddEdge(edgeIndex, Color.Cyan);
        //    //    //}

        //    //    //foreach (var edgeIndex in mcg.Edges[i].OtherEdgeIndicesB)
        //    //    //{
        //    //    //    AddEdge(edgeIndex, Color.Yellow);
        //    //    //}

        //    //    //break;
        //    //}

        //    float getPathLength(int pathIndex)
        //    {
        //        var start = startPoints[pathIndex];
        //        var end = endPoints[pathIndex];
        //        return (start - end).Length();
        //    }

        //    void addPointDbg(int pointIndex)
        //    {
        //        DBG.AddPrimitive(sphere.Instantiate($"Point [{pointIndex}]", new Transform(new Vector3(mcg.Points[pointIndex].PosX, mcg.Points[pointIndex].PosY, mcg.Points[pointIndex].PosZ), Vector3.Zero)));

        //        int pidx = 0;
        //        foreach (var adjPointIndex in mcg.Points[pointIndex].NearbyPointIndices)
        //        {
        //            DBG.AddPrimitive(sphere.Instantiate($"INDEX IN POINT'S LIST:{pidx++}, Point Near [{adjPointIndex}]", new Transform(new Vector3(mcg.Points[adjPointIndex].PosX, mcg.Points[adjPointIndex].PosY, mcg.Points[adjPointIndex].PosZ), Vector3.Zero)));
        //        }

        //        pidx = 0;
        //        foreach (var path in mcg.Points[pointIndex].NearbyPathIndices)
        //        {
        //            AddPath(path, Color.Cyan);
        //            AddPathString(path, $"PATH INDEX IN POINT'S LIST:{pidx++}\n{mcg.Paths[path].GetDebugReport()}\nPATH LENGTH: {getPathLength(path)}", Color.Cyan);
        //        }
        //    }

        //    //addPointDbg(0);
        //    //addPointDbg(1);
        //    //addPointDbg(2);

        //    for (int i = 0; i < mcg.Points.Count; i++)
        //    {
        //        addPointDbg(i);
        //    }

        //    DBG.AddPrimitive(lines);

        //    Console.WriteLine("TEST");
        //}

        public static void SetupTests(List<DbgMenuItem> menu)
        {
            //menu.Add(new DbgMenuItem()
            //{
            //    Text = "Test MGC & MCP",
            //    Items = DbgMenuItemSpawnMap.IDList.Select(x => new DbgMenuItem()
            //    {
            //        Text = x,
            //        ClickAction = m => TestMCP_MGC(x)
            //    }).ToList()
            //});
        }


    }
}
