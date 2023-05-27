using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Optimeet
{   [DataContract]
    public class Trie<T>
    {
        [DataMember]
        private TrieNode<T> _root;
        public Trie()
        {
            _root = new TrieNode<T>();
        }
        [DataMember]
        const int ALPHABET_FIRST_INDEX = 97;
        public void Add(string name, T value)
        {
            name = FormatName(name);
            TrieNode<T> temp = _root;
            char key;
            for (int i = 0; i < name.Length-1; i++)
            {
                key = name.ElementAt(i);
                //check if exists already
                if (temp.children[cti(key)]==null)
                {
                TrieNode<T> NewNode = new TrieNode<T>();
                temp.children[cti(key)] = NewNode;
                temp.count++;
                temp = NewNode;
                }
                else
                {
                    temp = temp.children[cti(key)];
                }
            }
            key = name.ElementAt(name.Length - 1);
            if (temp.children[cti(key)] == null)
            {
                TrieNode<T> lastNode = new TrieNode<T>
                {
                    Value = value
                };
                temp.children[cti(key)] = lastNode;
                temp.count++;
            }
            else throw new Exception("Cannot override an existing object");
        }

        public void Edit(string name, T newValue)
        {
            FindNode(name, _root).Value = newValue;
        }

        private TrieNode<T> FindNode(string name, TrieNode<T> temp)
        {            
            return name.Length == 1 ? temp.children[cti(name[0])] : FindNode(name.Substring(1), temp.children[cti(name[0])]);
        }

        public T Find(string name)
        {
            return FindNode(name, _root).Value;
        }

        public void Delete(string name)
        {
            name = FormatName(name);
            TrieNode<T> node = FindNode(name, _root);
            if (node.count > 0)
                node.Value = default;
            else
            { 
                node = FindFirstSingularAncestor(name, _root);
                RemoveNodeChain(node, _root);
            }
        }

        private string FormatName(string name)
        {
            //All names are stored with lowercase characters without spaces
            return name.ToLower().Replace(" ", "");
        }

        private void RemoveNodeChain(TrieNode<T> node, TrieNode<T> temp)
        {
            if (temp != null)
            {
                int i = 0, j = temp.count;
                if (temp.count != 0)
                    while (i < temp.children.Length && j > 0)
                    {
                        j = temp.children[i] == null ? j : j - 1;
                        if (temp.children[i] == node)
                        {
                            temp.children[i] = null;
                            temp.count--;
                            return;
                        }
                        else RemoveNodeChain(node, temp.children[i]);
                        i++;
                    }
            }
                return;
        }

        public Queue<string> GetChildrenNames()
        {
            Queue<string> names = new Queue<string>();
            return GetChildrenNames(names, _root, "");
        }

        private Queue<string> GetChildrenNames(Queue<string> names, TrieNode<T> temp, string name)
        {
            int i = 0, j = temp.count;
            if (temp.Value != null)
                names.Enqueue(name);
            if(temp.count > 0)
                while (i < temp.children.Length && j > 0)
                {
                    if(temp.children[i] != null)
                    { 
                        GetChildrenNames(names, temp.children[i], name + itc(i));
                        j--;
                    }
                    i++;
                }
            return names;
        }

        public Queue<T> GetChildren()
        {
            Queue<T> children = new Queue<T>();
            return GetChildren(children, _root, "");
        }

        private Queue<T> GetChildren(Queue<T> children, TrieNode<T> temp, string name)
        {
            int i = 0, j = temp.count;
            if (temp.Value != null)
                children.Enqueue(temp.Value);
            if (temp.count > 0)
                while (i < temp.children.Length && j > 0)
                {
                    if (temp.children[i] != null)
                    {
                        GetChildren(children, temp.children[i], name + itc(i));
                        j--;
                    }
                    i++;
                }
            return children;
        }

        private TrieNode<T> FindFirstSingularAncestor(string name, TrieNode<T> temp)
        {
            if (name.Length == 0)
                return null;
            else if ((temp.Value != null && temp.count > 0) || temp.count > 1)
            {
                TrieNode<T> TN = FindFirstSingularAncestor(name.Substring(1), temp.children[cti(name[0])]);
                return TN ?? temp.children[cti(name[0])];
            }
            else return FindFirstSingularAncestor(name.Substring(1), temp.children[cti(name[0])]);
        }

        private int cti(char c)
        {
            //cti - char to index
            return c - ALPHABET_FIRST_INDEX;
        }

        private char itc(int i)
        {
            //cti - char to index
            return (char)(i + ALPHABET_FIRST_INDEX);
        }
    }
    [DataContract]
    [KnownType(typeof(Trie<Contact>))]
    public class TrieNode<T>
    {
        [DataMember]
        public TrieNode<T>[] children = new TrieNode<T>[26];
        [DataMember]
        public T Value;
        [DataMember]
        public int count = 0;
    }
}
