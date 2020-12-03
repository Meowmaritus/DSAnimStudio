using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEventGroupRegion
    {
        public TaeEditAnimEventGraph Graph;
        public TAE.Animation TaeAnim;
        public TAE.EventGroup Group = null;
        public int GetGroupIndex() => TaeAnim.EventGroups.IndexOf(Group);

        public TaeEventGroupRegion(TaeEditAnimEventGraph graph, TAE.Animation anim, TAE.EventGroup group)
        {
            Graph = graph;
            TaeAnim = anim;
            Group = group;
        }


        public string GetGroupText()
        {
            var sb = new StringBuilder();
            var group = Group;
            if (group != null)
            {
                int groupIndex = GetGroupIndex();
                sb.Append($"Group {groupIndex}[Type {group.GroupType}]");

                if (group.GroupData is TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity entitySpecifier)
                {
                    sb.AppendLine();
                    //sb.Append("Affects Remo Entity ");
                    if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.Character)
                        sb.Append($"c{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                    else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.Object)
                        sb.Append($"o{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                    else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.DummyNode)
                        sb.Append($"d{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                    else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.MapPiece)
                    {
                        if (entitySpecifier.Block >= 0)
                            sb.Append($"m{entitySpecifier.CutsceneEntityIDPart1:D4}B{entitySpecifier.Block}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                        else
                            sb.Append($"m{entitySpecifier.CutsceneEntityIDPart1:D4}B{RemoManager.BlockInt}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                    }

                    if (entitySpecifier.Block >= 0 || entitySpecifier.Area >= 0)
                    {
                        sb.Append($" (from m{entitySpecifier.Area:D2}_{entitySpecifier.Block:D2})");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Inclusive
        /// </summary>
        public int StartRow = 0;

        /// <summary>
        /// Inclusive
        /// </summary>
        public int RowCount = 0;

        /// <summary>
        /// Exclusive
        /// </summary>
        public int EndRow => StartRow + RowCount;

        public void ShiftRow(int shift)
        {
            StartRow += shift;
            
            Graph.DisableGroupRegionAssignOnRowMove = true;

            foreach (var b in Boxes)
            {
                b.Row += shift;
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


            Graph.DisableGroupRegionAssignOnRowMove = false;
        }

        private List<TaeEditAnimEventBox> Boxes = new List<TaeEditAnimEventBox>();

        public void AddEvent(TaeEditAnimEventBox evBox)
        {
            if (evBox.CurrentGroupRegion != this)
            {
                evBox.CurrentGroupRegion?.RemoveEvent(evBox);

                evBox.CurrentGroupRegion = this;
                evBox.MyEvent.Group = Group;
                Boxes.Add(evBox);

                if (Boxes.Count == 1)
                {
                    StartRow = evBox.Row;
                    RowCount = 1;
                }
                else
                {
                    if (evBox.Row >= EndRow)
                    {
                        RowCount = evBox.Row - StartRow;
                    }
                }

                
            }
        }

        public void RemoveEvent(TaeEditAnimEventBox evBox)
        {
            if (Boxes.Contains(evBox))
            {
                Boxes.Remove(evBox);
                evBox.MyEvent.Group = null;
            }
        }

        public void CropRegionToEvents()
        {
            if (Boxes.Count > 0)
            {
                StartRow = Boxes.OrderBy(b => b.Row).First().Row;
                RowCount = (Boxes.OrderBy(b => b.Row).Last().Row - StartRow) + 1;
            }
        }
    }
}
