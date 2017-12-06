using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HyperDB
{
    /// <summary>
    /// Tree Node
    /// </summary>
    public class DBNode
    {
        public DBManager Manager { get; private set; }
        public DBNode[] ChildNodes { get; private set; }
        public int ChildCount
        {
            get
            {
                int c = 0;
                foreach (var ch in ChildNodes)
                    if (ch != null)
                        c++;
                return c;
            }
        }

        public bool HasChild { get { return ChildCount > 0; } }
        public bool HasParent { get { return Parent != null; } }

        public int Index { get; private set; }
        public int Level { get; private set; }
        public DBNode Parent { get; private set; }

        public void SetParent(DBNode parent, int id)
        {
            if (parent != null && id >= 0 && id < Manager.DivisionCount)
            {
                Parent = parent;
                Parent.ChildNodes[id] = this;
                Index = id;
                Level = Parent.Level - 1;
                Keys = Manager.CalcChildKeys(parent.Keys, parent.Level, id);
            }
        }

        public void UnsetParent()
        {
            if (Parent != null && Index > 0 && Index < Manager.DivisionCount && Parent.ChildNodes[Index] == this)
            {
                Parent.ChildNodes[Index] = null;
            }
            Parent = null;
            Index = -1;
        }

        public virtual void OnInsert(object userData) { }
        public virtual void OnDelete() { }
        //public virtual void OnSubDivision(int[][] childKeys, int childLevel, object userData) { }

        public DBNode(DBManager manager, int level = -1)
        {
            this.Manager = manager;
            this.ChildNodes = new DBNode[manager.DivisionCount];
            Index = -1;
            Parent = null;
            Level = level;
        }

        public void Delete()
        {
            Manager.Delete(this);
        }

        public int[] Keys { get; set; }
        public void UpdateChildKeys()
        {
            for (int i = 0; i < ChildCount; i++)
                if (ChildNodes[i] != null)
                    ChildNodes[i].Keys = Manager.CalcChildKeys(Keys, Level, i);
        }

        public void UpdateChildKeysRecursively()
        {
            UpdateChildKeys();
            for (int i = 0; i < ChildCount; i++)
                if (ChildNodes[i] != null)
                    ChildNodes[i].UpdateChildKeysRecursively();
        }

    }

}
