using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace HyperDB
{
    /// <summary>
    /// Tree Manager
    /// </summary>
    public class DBManager
    {
        /// <summary>
        /// Number of keys of a node
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Uplimit number of child nodes of a node
        /// </summary>
        public int DivisionCount { get { return 1 << Dimension; } }

        DBNode root;
        public DBNode Root
        {
            get
            {
                if (root == null)
                {
                    root = new DBNode(this, MaxLevel);
                    root.Keys = new int[Dimension];

                    for (int i = 0; i < Dimension; i++)
                        root.Keys[i] = 0;
                }
                return root;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MaxLevel { get; private set; }
        public DBManager(int maxLevel, int dimension, Type leafNodeType = null)
        {
            MaxLevel = maxLevel;
            Dimension = dimension;

            if (leafNodeType == typeof(DBNode))
                leafNodeType = null;

            if (leafNodeType != null && !leafNodeType.IsSubclassOf(typeof(DBNode)))
                throw new TypeLoadException();

            this.leafNodeType = leafNodeType;
        }

        Type leafNodeType = null;
        public DBNode CreateLeafNode()
        {
            if (leafNodeType == null)
                return new DBNode(this);
            return (DBNode)Activator.CreateInstance(leafNodeType, this, -1);
        }

        public DBNode CreateNode()
        {
            return new DBNode(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        public void CheckKeysSize(int[] keys)
        {
            int size = Dimension;
            if (keys.Length != size)
                throw new Exception(String.Format(
                    "Wrong array size, expected size is {0} but got {1}",
                    size, keys.Length));
        }

        /// <summary>
        /// Mix specific bits of keys to get sublevel index
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetLevelIndex(int[] keys, int level)
        {
            int size = Dimension;
            int result = 0;
            for (int i = 0; i < size; i++)
            {
                result |= ((keys[i] >> level) & 1) << i;
            }
            return result;
        }

        public int[] CalcChildKeys(int[] parentKeys, int parentLevel, int childIndex)
        {
            CheckKeysSize(parentKeys);
            int[] result = new int[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                result[i] = (((childIndex >> i) & 1) << (parentLevel - 1)) | parentKeys[i];
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="searchLevel"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public QueryResult Search(int[] keys, int searchLevel, DBNode root = null)
        {
            if (root == null)
                root = Root;

            if (this != root.Manager)
                return null;

            CheckKeysSize(keys);

            if (root.Level < searchLevel)
                return null;

            for (int i = 0; i < Dimension; i++)
                if ((keys[i] >> root.Level) << root.Level != root.Keys[i])
                    return null;

            return Search_r(keys, searchLevel, root);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="searchLevel"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        QueryResult Search_r(int[] keys, int searchLevel, DBNode root)
        {
            if (root.Level == searchLevel)
                return new QueryResult(root, true);

            int index = GetLevelIndex(keys, root.Level - 1);
            if (root.ChildNodes[index] == null)
                return new QueryResult(root, false);

            var result = Search_r(keys, searchLevel, root.ChildNodes[index]);
            if (result == null)
                result = new QueryResult(root, false);
            return result;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="insertLevel"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public QueryResult Insert(int[] keys, int insertLevel, object userData = null)
        {
            for (int i = 0; i < Dimension; i++)
                if ((keys[i] >> Root.Level) << Root.Level != Root.Keys[i])
                    return null;
            if (!Root.HasChild)
                return Insert(Root, keys, insertLevel, userData);

            var s = Search(keys, insertLevel);
            if (s != null && !s.Succeed && s.Result.HasChild)
                return Insert(s.Result, keys, insertLevel, userData);
            if (s == null)
                return null;
            if (s.Succeed)
                return new QueryResult(s.Result, false, "Space in use");
            return new QueryResult(s.Result, false, "Zone in use");

        }

        QueryResult Insert(DBNode root, int[] keys, int insertLevel, object userData)
        {
            if (root.Level < insertLevel)
            {
                return null;
            }
            if (root.Level == insertLevel) //Insertion fail
            {
                return new QueryResult(root, false);
                //throw new DBInsertNodeExistException();
            }

            int index = GetLevelIndex(keys, root.Level - 1);
            //Debug.Assert(rootLevel >= insertLevel);

            if (root.ChildNodes[index] == null)
            {
                //root.ChildNodes[index] = CreateNode();
                if (root.Level == insertLevel + 1)
                {
                    var node = CreateLeafNode();
                    node.SetParent(root, index);
                    node.OnInsert(userData);
                    return new QueryResult(node, true);
                }
                else
                {
                    var node = CreateNode();
                    node.SetParent(root, index);
                }
            }
            return Insert(root.ChildNodes[index], keys, insertLevel, userData);
        }
        //class DBTreeErrorException : Exception { }
        //class DBInsertNodeExistException : Exception { }
        public void Delete(DBNode node)
        {
            if (node.Manager != this || node.Parent == null || node.Parent.Manager != this)
                return;
            var endnode = node.Parent;
            DeleteTree(node);

            while (endnode != null && !endnode.HasChild)
            {
                var parent = endnode.Parent;
                endnode.UnsetParent();
                endnode = parent;
            }

        }


        public void DeleteTree(DBNode node/*, bool checkManager=true*/)
        {
            for (int i = 0; i < DivisionCount; i++)
                if (node.ChildNodes[i] != null)
                    Delete(node.ChildNodes[i]);

            node.OnDelete();
            node.UnsetParent();


        }

        bool SubDividable(DBNode root, int[] keys, int divLevel)
        {
            if (root.Level <= divLevel)
                return false;

            for (int i = 0; i < Dimension; i++)
                if ((keys[i] >> root.Level) << root.Level != root.Keys[i])
                    return false;

            if (root.HasChild)
                return false;

            return true;

        }

        public void SubDivide(DBNode root, int[] keys, int divLevel, object userData = null)
        {
            if (keys != null && SubDividable(root, keys, divLevel))
                SingleSubDivide_R(root, keys, divLevel, userData);
            if (keys == null && SubDividable(root, root.Keys, divLevel))
                FullSubDivide(root, divLevel, userData);
        }

        void SingleSubDivide_R(DBNode root, int[] keys, int divLevel, object userData)
        {
            int index = GetLevelIndex(keys, root.Level - 1);

            for (int i = 0; i < DivisionCount; i++)
            {
                if (index != i || root.Level == divLevel + 1)
                {
                    var node = CreateLeafNode();
                    node.SetParent(root, i);
                    node.OnInsert(userData);
                }
                else
                {
                    var node = CreateNode();
                    node.SetParent(root, i);
                    node.OnInsert(userData);
                    SingleSubDivide_R(node, keys, divLevel, userData);
                }

            }
        }

        void FullSubDivide(DBNode root, int divLevel, object userData)
        {
            if (root.Level > divLevel + 1)
                for (int i = 0; i < DivisionCount; i++)
                {
                    var node = CreateNode();
                    node.SetParent(root, i);
                    FullSubDivide(node, divLevel, userData);
                }
            else
                for (int i = 0; i < DivisionCount; i++)
                {
                    var node = CreateLeafNode();
                    node.SetParent(root, i);
                    node.OnInsert(userData);
                }
        }
        public void SubExclude(DBNode root, int[] keys, int divLevel, object userData=null)
        {
            if (SubDividable(root, keys, divLevel))
            {
                var parent = root.Parent;
                var index = root.Index;
                DeleteTree(root);
                var node = CreateNode();
                node.SetParent(parent, index);
                SubExclude_R(node, keys, divLevel, userData);
            }
        }
        void SubExclude_R(DBNode root, int[] keys, int divLevel, object userData)
        {
            int index = GetLevelIndex(keys, root.Level - 1);

            for (int i = 0; i < DivisionCount; i++)
            {
                if (index != i)
                {
                    var node = CreateLeafNode();
                    node.SetParent(root, i);
                    node.OnInsert(userData);
                }
                else if (root.Level > divLevel + 1)
                {
                    var node = CreateNode();
                    node.SetParent(root, i);
                    SubExclude_R(node, keys, divLevel, userData);
                }

            }
        }

        public string Dump(DBNode root, int indent = 0)
        {

            var result = new String(' ', indent);
            result += root.Level + "_" + root.Index + " ";

            result += "[ ";
            foreach (var k in root.Keys)
                result += k + ", ";
            result = result.Substring(0, result.Length - 2) + " ]";
            if (!root.HasChild)
                result += " => [ " + root.ToString() + " ]";
            result += "\n";

            foreach (var c in root.ChildNodes)
            {
                if (c != null)
                    result += Dump(c, indent + 1);
            }
            return result;
        }
    }
}
