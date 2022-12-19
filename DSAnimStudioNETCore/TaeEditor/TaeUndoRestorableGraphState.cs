using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeUndoRestorableGraphState
    {
        public class TaeCloneStateItem
            
        {
            ITaeClonable orig;
            object cloned;
            public TaeCloneStateItem(ITaeClonable item)
            {
                orig = item;
                cloned = item.ToClone();
            }
            public static TaeCloneStateItem NewOrDefault(ITaeClonable item)
            {
                if (item == null)
                    return null;
                return new TaeCloneStateItem(item);
            }
            public object RestoreState()
            {
                return orig.FromClone(cloned);
            }
            public TaeCloneStateItem GetNewCloneStateOfCurrentState()
            {
                return new TaeCloneStateItem(orig);
            }
        }

        public class TaeCloneStateEnumerable<T>
            where T : ITaeClonable
        {
            private Dictionary<T, T> clones = new Dictionary<T, T>();
            public TaeCloneStateEnumerable(IEnumerable<T> thingsToClone)
            {
                foreach (var thing in thingsToClone)
                {
                    clones.Add(thing, (T)thing.ToClone());
                }
            }
            public IEnumerable<T> GetRestoredEnumerable()
            {
                return clones.Select(kvp => (T)kvp.Key.FromClone(kvp.Value));
            }
        }

        public TaeEditAnimEventGraph Graph;

        private IEnumerable<TaeCloneStateItem> VariousItemClones;
        private TaeCloneStateEnumerable<TaeEditAnimEventBox> BoxClones;
        private TaeCloneStateEnumerable<TaeEventGroupRegion> GroupRegionClones;

        private Dictionary<int, List<TaeEditAnimEventBox>> SortedByRowClone = new Dictionary<int, List<TaeEditAnimEventBox>>();


        public TaeUndoRestorableGraphState(TaeEditAnimEventGraph graph, IEnumerable<ITaeClonable> stuffToClone)
        {
            Graph = graph;
            VariousItemClones = stuffToClone.Select(thing => TaeCloneStateItem.NewOrDefault(thing));
            BoxClones = new TaeCloneStateEnumerable<TaeEditAnimEventBox>(graph.EventBoxes);
            GroupRegionClones = new TaeCloneStateEnumerable<TaeEventGroupRegion>(graph.GroupRegions);
            lock (graph._lock_EventBoxManagement)
                SortedByRowClone = graph.sortedByRow.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
        }

        public void RestoreState()
        {
            foreach (var item in VariousItemClones)
            {
                item.RestoreState();
            }
            Graph.EventBoxes = BoxClones.GetRestoredEnumerable().ToList();
            Graph.GroupRegions = GroupRegionClones.GetRestoredEnumerable().ToList();
            lock (Graph._lock_EventBoxManagement)
                Graph.sortedByRow = SortedByRowClone;
        }
    }
}
