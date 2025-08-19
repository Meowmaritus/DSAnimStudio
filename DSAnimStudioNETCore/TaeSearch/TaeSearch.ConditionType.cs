using System;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public enum ConditionType
        {
            AnimationCategoryName,
            AnimationCategoryID,
            AnimationSubID,
            AnimationFullID,
            AnimationName,
            TrackID,
            TrackName,
            ActionID,
            ActionName,
            Debug_ActionOverlap,
            Debug_ActionOverlapFull,
            ParameterName,
            ParameterValue,
            TagName,
        }

        public static string ConditionTypeToString(ConditionType c)
        {
            switch (c)
            {
                case ConditionType.AnimationCategoryName: return "Animation Category Name";
                case ConditionType.AnimationCategoryID: return "Animation Category ID";
                case ConditionType.AnimationSubID: return "Animation Sub ID";
                case ConditionType.AnimationFullID: return "Full Animation ID";
                case ConditionType.AnimationName: return "Animation Name";
                case ConditionType.TrackID: return "Track ID";
                case ConditionType.TrackName: return "Track Name";
                case ConditionType.ActionID: return "Action ID";
                case ConditionType.ActionName: return "Action Name";
                case ConditionType.ParameterName: return "Parameter Name";
                case ConditionType.ParameterValue: return "Parameter Value";
                case ConditionType.Debug_ActionOverlap: return "Debug - Action Overlaps Others";
                case ConditionType.Debug_ActionOverlapFull: return "Debug - Action Overlaps Others Exactly";
                case ConditionType.TagName: return "Tag Name";
                default: throw new NotImplementedException();
            }
        }
    }
}