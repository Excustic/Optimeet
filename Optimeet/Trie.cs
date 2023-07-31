using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Optimeet
{
    /// <summary>
    /// Represents a Trie data structure to store values of generic type T, associated with string keys.
    /// For more information about Trie data structures, read <see href="https://www.javatpoint.com/trie-data-structure">here</see>
    /// </summary>
    /// <typeparam name="T">The type of values to store in the Trie.</typeparam>
    [DataContract]
    public class Trie<T>
    {
        /// <summary>
        /// The root node of the Trie.
        /// </summary>
        [DataMember]
        private TrieNode<T> _root;
        /// <summary>
        /// Initializes a new instance of the Trie class.
        /// </summary>
        public Trie()
        {
            _root = new TrieNode<T>();
        }

        [DataMember]
        const int ALPHABET_FIRST_INDEX = 97;
        /// <summary>
        /// Adds a new <see cref="TrieNode{T}"/> with a key-value pair to the Trie.
        /// </summary>
        /// <param name="name">The string key to add to the new node.</param>
        /// <param name="value">The value associated with the key of the new node.</param>
        public void Add(string name, T value)
        {
            name = FormatName(name);
            TrieNode<T> temp = _root;
            char key;
            for (int i = 0; i < name.Length - 1; i++)
            {
                key = name.ElementAt(i);
                //check if exists already
                if (temp.children[cti(key)] == null)
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
        /// <summary>
        /// Finds and returns the value associated with Trie's node of the key that is equivalent to the last character in the given string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Find(string name)
        {
            return FindNode(name, _root).Value;
        }
        /// <summary>
        /// A recursive method that finds the <see cref="TrieNode{T}"/> associated with the given string in the Trie.
        /// <para></para>
        /// <example>
        /// For example: Given the word 'safe', the method will iterate over the trie in the following way.
        /// <para></para>
        /// root -> s -> a -> f -> e.
        /// This will return the node with the key 'e'.
        /// </example>
        /// </summary>
        /// <param name="name">The string key to find the value for.</param>
        /// <returns>The <see cref="TrieNode{T}"/> of the key that is equivalent to the last character in the given string.</returns>
        private TrieNode<T> FindNode(string name, TrieNode<T> temp)
        {
            return name == "" ? _root : name.Length == 1 ? temp.children[cti(name[0])] : FindNode(name.Substring(1), temp.children[cti(name[0])]);
        }
        /// <summary>
        /// Edits the value associated with the Trie's node of the key that is equivalent to the last character in the given string.
        /// </summary>
        /// <param name="name">The string key to edit the value for.</param>
        /// <param name="newValue">The new value to set for the key.</param>
        public void Edit(string name, T newValue)
        {
            FindNode(FormatName(name), _root).Value = newValue;
        }
        /// <summary>
        /// Deletes the <see cref="TrieNode{T}"/> associated with the given string in the Trie or the value of the node if it contains children
        /// </summary>
        /// <param name="name">The string key to delete from the Trie.</param>
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
        /// <summary>
        /// Formats a string to lowercase characters and removes whitespaces, allowing to store new nodes properly.
        /// </summary>
        /// <param name="name">The string key to format</param>
        /// <returns></returns>
        private string FormatName(string name)
        {
            //All names are stored with lowercase characters without spaces
            return name.ToLower().Replace(" ", "");
        }
        /// <summary>
        /// Removes the chain of nodes starting from the specified node in the Trie.
        /// </summary>
        /// <param name="node">The node to start removing the chain from.</param>
        /// <param name="temp">The current node being examined during the removal process.</param>
        /// <remarks>
        /// This method is used internally to remove a chain of nodes from the Trie starting from the specified node.
        /// It traverses the children of the current node, and if it finds the target node, it removes it and decrements
        /// the count of children. The method continues to traverse down the Trie recursively until the chain is completely removed.
        /// </remarks>
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
        /// <summary>
        /// Returns a queue of strings representing the nodes in the Trie with an assigned <see cref="TrieNode{T}.Value"/>.
        /// </summary>
        /// <returns>A queue of strings containing keys with children nodes.</returns>
        public Queue<string> GetChildrenNames()
        {
            Queue<string> names = new Queue<string>();
            return GetChildrenNames(names, _root, "");
        }
        /// <summary>
        /// Retrieves the names of all children nodes from the Trie starting from the given node.
        /// </summary>
        /// <param name="names">The queue to store the names of the children nodes.</param>
        /// <param name="temp">The current node being examined during the traversal.</param>
        /// <param name="name">The name constructed so far during the traversal.</param>
        /// <returns>A queue containing the names of all children nodes in the Trie.</returns>
        /// <remarks>
        /// This method is used internally to traverse the Trie starting from the given node and collect the names
        /// of all children nodes that have associated values. The traversal continues until all children nodes
        /// with values are explored, and their names are added to the queue.
        /// </remarks>
        private Queue<string> GetChildrenNames(Queue<string> names, TrieNode<T> temp, string name)
        {
            int i = 0, j = temp.count;
            if (temp.Value != null)
                names.Enqueue(name);
            if (temp.count > 0)
                while (i < temp.children.Length && j > 0)
                {
                    if (temp.children[i] != null)
                    {
                        GetChildrenNames(names, temp.children[i], name + itc(i));
                        j--;
                    }
                    i++;
                }
            return names;
        }
        /// <summary>
        /// Retrieves the values of all children nodes in the Trie, starting from the root node.
        /// </summary>
        /// <returns>A queue containing the values of all children nodes in the Trie.</returns>
        /// <remarks>
        /// This method is used to traverse the Trie starting from the root node and collect the values of all children nodes
        /// that have assigned values. It internally calls the private method 'GetChildren' with the root node as the starting point.
        /// The traversal continues until all children nodes with values are explored, and their values are added to the queue.
        /// </remarks>
        public Queue<T> GetChildren()
        {
            Queue<T> children = new Queue<T>();
            return GetChildren(children, _root);
        }
        /// <summary>
        /// Retrieves the values of all children nodes in the Trie associated with the given key, starting from the corresponding node.
        /// </summary>
        /// <param name="name">The string key to find values and their children for.</param>
        /// <returns>A queue containing the values of all children nodes in the Trie that start with the given key.</returns>
        /// <remarks>
        /// This method is used to traverse the Trie starting from the node corresponding to the given key and collect the values of all children nodes
        /// that have assigned values. It internally calls the private method <see cref="GetChildren(Queue{T}, TrieNode{T})"/> with the found node as the starting point.
        /// The traversal continues until all children nodes with values are explored, and their values are added to the queue.
        /// </remarks>
        public Queue<T> GetChildren(string name)
        {
            Queue<T> children = new Queue<T>();
            return GetChildren(children, FindNode(FormatName(name), _root));
        }
        /// <summary>
        /// Retrieves the values of all children nodes in the Trie starting from the given node.
        /// </summary>
        /// <param name="children">The queue to store the values of the children nodes.</param>
        /// <param name="temp">The current node being examined during the traversal.</param>
        /// <returns>A queue containing the values of all children nodes in the Trie starting from the given node.</returns>
        /// <remarks>
        /// This method is used internally to traverse the Trie starting from the given node and collect the values
        /// of all children nodes that have assigned values. It iterates through the children of the current node,
        /// and if a non-null child node is found, it enqueues the corresponding value and recursively calls itself
        /// with the child node as the new starting point. The traversal continues until all children nodes with values
        /// are explored, and their values are added to the queue.
        /// </remarks>
        private Queue<T> GetChildren(Queue<T> children, TrieNode<T> temp)
        {
            int i = 0, j = temp.count;
            if (temp.Value != null)
                children.Enqueue(temp.Value);
            if (temp.count > 0)
                while (i < temp.children.Length && j > 0)
                {
                    if (temp.children[i] != null)
                    {
                        GetChildren(children, temp.children[i]);
                        j--;
                    }
                    i++;
                }
            return children;
        }
        /// <summary>
        /// Finds the first ancestor node in the Trie that has a single child or a value associated with it.
        /// </summary>
        /// <param name="name">The string key used to trace the ancestor node.</param>
        /// <param name="temp">The current node being examined during the search.</param>
        /// <returns>
        /// The first ancestor node in the Trie that has only one child or a value associated with it,
        /// or null if no such ancestor is found.
        /// </returns>
        /// <remarks>
        /// This method is used internally to find the first ancestor node in the Trie that has a single child node
        /// or a value associated with it. It traces the Trie based on the provided key 'name', starting from the
        /// 'temp' node. The search continues until it finds an ancestor node with only one child or a value,
        /// or until the end of the key is reached. If such an ancestor node is found, it is returned; otherwise,
        /// the method returns null.
        /// </remarks>
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
        /// <summary>
        /// Converts a character to its alphabetical index
        /// </summary>
        /// <param name="c">The char to convert</param>
        /// <returns></returns>
        private int cti(char c)
        {
            //cti - char to index
            return c - ALPHABET_FIRST_INDEX;
        }
        /// <summary>
        /// Converts an alphabetical index to a character
        /// </summary>
        /// <param name="i">The index to convert</param>
        /// <returns></returns>
        private char itc(int i)
        {
            //cti - char to index
            return (char)(i + ALPHABET_FIRST_INDEX);
        }
    }
    /// <summary>
    /// Represents a node in the Trie data structure.
    /// </summary>
    /// <typeparam name="T">The type of values stored in the Trie.</typeparam>
    [DataContract]
    [KnownType(typeof(Trie<Contact>))]
    public class TrieNode<T>
    {
        /// <summary>
        /// Array of child nodes representing the next characters in the Trie.
        /// </summary>
        [DataMember]
        public TrieNode<T>[] children = new TrieNode<T>[26];
        /// <summary>
        /// The value associated with the Trie node.
        /// </summary>
        [DataMember]
        public T Value;
        /// <summary>
        /// The count of child nodes in the Trie.
        /// </summary>
        [DataMember]
        public int count = 0;
    }
}
