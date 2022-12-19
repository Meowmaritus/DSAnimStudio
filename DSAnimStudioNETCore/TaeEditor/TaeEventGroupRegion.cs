using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEventGroupRegion : ITaeClonable
    {
        public TaeEditAnimEventGraph Graph;
        public TAE.Animation TaeAnim;
        public TAE.EventGroup Group = null;
        public int GetGroupIndex() => TaeAnim.EventGroups.IndexOf(Group);
        public bool Collapsed = false;

        public TaeEventGroupRegion(TaeEditAnimEventGraph graph, TAE.Animation anim, TAE.EventGroup group)
        {
            Graph = graph;
            TaeAnim = anim;
            Group = group;
        }


        public string GetGroupText()
        {
            var strBld = new StringBuilder();
            var evBoxGrp = Group;
            if (evBoxGrp != null)
            {
                int groupIndex = GetGroupIndex();
                if (evBoxGrp.GroupType != 128)
                {
                    strBld.Append($"Group {groupIndex} [Type {evBoxGrp.GroupType}] - ");
                }
                

                if (evBoxGrp.GroupData.DataType == TAE.EventGroup.EventGroupDataType.ApplyToSpecificCutsceneEntity)
                {
                    //sb.AppendLine();
                    //sb.Append("Affects Remo Entity ");
                    if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.Character)
                        strBld.Append($"c{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                    else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.Object)
                        strBld.Append($"o{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                    else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.DummyNode)
                        strBld.Append($"d{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                    else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.MapPiece)
                    {
                        if (evBoxGrp.GroupData.Block >= 0)
                            strBld.Append($"m{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}B{evBoxGrp.GroupData.Block}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                        else
                            strBld.Append($"m{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}B{RemoManager.BlockInt}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                    }

                    if (evBoxGrp.GroupData.CutsceneEntityType != TAE.EventGroup.EventGroupDataStruct.EntityTypes.MapPiece && 
                        (evBoxGrp.GroupData.Block >= 0 || evBoxGrp.GroupData.Area >= 0))
                    {
                        strBld.Append($" (from m{evBoxGrp.GroupData.Area:D2}_{evBoxGrp.GroupData.Block:D2})");
                    }
                }
            }
            return strBld.ToString();
        }

        /// <summary>
        /// Inclusive
        /// </summary>
        public int StartRow = 0;

        /// <summary>
        /// Inclusive
        /// </summary>
        public int RowCount = 2;

        /// <summary>
        /// Exclusive
        /// </summary>
        public int EndRow => StartRow + RowCount;

        public void ShiftRow(int shift)
        {
            StartRow += shift;

            foreach (var b in Boxes)
            {
                b.SetRowSilently(b.Row + shift);
            }

            //if (Boxes.Count > 0)
            //{
            //    int firstEventsRow = Boxes.OrderBy(b => b.Row).First().Row;

            //    int boxRowShift = StartRow - firstEventsRow;

            //    foreach (var b in Boxes)
            //    {
            //        b.Row += boxRowShift;
            //    }
            //}
        }

        private List<TaeEditAnimEventBox> Boxes = new List<TaeEditAnimEventBox>();

        public IReadOnlyList<TaeEditAnimEventBox> BoxesInGroup => Boxes;

        public void AddEvent(TaeEditAnimEventBox evBox)
        {
           
            evBox.CurrentGroupRegion?.RemoveEvent(evBox);

            evBox.CurrentGroupRegion = this;
            evBox.MyEvent.Group = Group;
            if (!Boxes.Contains(evBox))
                Boxes.Add(evBox);

            //if (Boxes.Count == 1)
            //{
            //    StartRow = evBox.Row;
            //    RowCount = 2;
            //}
            //else
            //{
            //    if (evBox.Row >= EndRow)
            //    {
            //        RowCount = evBox.Row - StartRow;
            //    }
            //}

                
            
        }

        public void RemoveEvent(TaeEditAnimEventBox evBox)
        {
            if (Boxes.Contains(evBox))
            {
                Boxes.Remove(evBox);
                evBox.MyEvent.Group = null;
            }
            //CropRegionToEvents();
        }

        public void CropRegionToEvents()
        {
            if (Boxes.Count > 0)
            {
                StartRow = Boxes.OrderBy(b => b.Row).First().Row - 1;
                RowCount = (Boxes.OrderBy(b => b.Row).Last().Row - Boxes.OrderBy(b => b.Row).First().Row) + 2;
            }
            else
            {
                RowCount = 1;
            }
        }

        public object ToClone()
        {
            var gr = new TaeEventGroupRegion(Graph, TaeAnim, Group);
            gr.Boxes = Boxes.ToList();
            gr.Collapsed = Collapsed;
            gr.StartRow = StartRow;
            gr.RowCount = RowCount;
            return gr;
        }

        public object FromClone(object cloneObj)
        {
            var clone = (TaeEventGroupRegion)cloneObj;
            Boxes = clone.Boxes.ToList();
            Collapsed = clone.Collapsed;
            StartRow = clone.StartRow;
            RowCount = clone.RowCount;
            return this;
        }
    }
}
