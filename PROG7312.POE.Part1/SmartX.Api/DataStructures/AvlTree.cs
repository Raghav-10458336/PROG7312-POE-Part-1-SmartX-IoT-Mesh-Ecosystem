
namespace SmartX.Api.DataStructures
{
    // Self-balancing binary search tree with logarithmic lookup.
    public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private sealed class Node
        {
            public TKey Key { get; }
            public List<TValue> Values { get; } = new();
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public int Height { get; set; } = 1;

            public Node(TKey key, TValue value)
            {
                Key = key;
                Values.Add(value);
            }
        }

        private Node? _root;

        public int Count { get; private set; }

        // Insert values while retaining duplicate timestamp keys.
        public void Insert(TKey key, TValue value)
        {
            _root = Insert(_root, key, value);
            Count++;
        }

        private static Node Insert(Node? node, TKey key, TValue value)
        {
            if (node is null)
                return new Node(key, value);

            int comparison = key.CompareTo(node.Key);

            if (comparison < 0)
                node.Left = Insert(node.Left, key, value);
            else if (comparison > 0)
                node.Right = Insert(node.Right, key, value);
            else
            {
                node.Values.Add(value);
                return node;
            }

            UpdateHeight(node);
            return Rebalance(node);
        }

        public IReadOnlyList<TValue> GetAll()
        {
            var results = new List<TValue>(Count);
            TraverseAscending(_root, results);
            return results.AsReadOnly();
        }

        // Inclusive range query that prunes irrelevant subtrees.
        public IReadOnlyList<TValue> GetRange(TKey start, TKey end)
        {
            var results = new List<TValue>();

            if (start.CompareTo(end) > 0)
                return results.AsReadOnly();

            TraverseRange(_root, start, end, results);
            return results.AsReadOnly();
        }

        // Retrieve newest matching values using bounded AVL traversal.
        public IReadOnlyList<TValue> GetRecent(
            TKey start,
            TKey end,
            int limit,
            Func<TValue, bool>? predicate = null)
        {
            var results = new List<TValue>();

            if (limit <= 0 || start.CompareTo(end) > 0)
                return results.AsReadOnly();

            TraverseRecent(
                _root,
                start,
                end,
                limit,
                predicate,
                results);

            return results.AsReadOnly();
        }

        private static void TraverseAscending(
            Node? node,
            List<TValue> results)
        {
            if (node is null)
                return;

            TraverseAscending(node.Left, results);
            results.AddRange(node.Values);
            TraverseAscending(node.Right, results);
        }

        private static void TraverseRange(
            Node? node,
            TKey start,
            TKey end,
            List<TValue> results)
        {
            if (node is null)
                return;

            int lower = node.Key.CompareTo(start);
            int upper = node.Key.CompareTo(end);

            if (lower > 0)
                TraverseRange(node.Left, start, end, results);

            if (lower >= 0 && upper <= 0)
                results.AddRange(node.Values);

            if (upper < 0)
                TraverseRange(node.Right, start, end, results);
        }

        private static void TraverseRecent(
            Node? node,
            TKey start,
            TKey end,
            int limit,
            Func<TValue, bool>? predicate,
            List<TValue> results)
        {
            if (node is null || results.Count >= limit)
                return;

            int lower = node.Key.CompareTo(start);
            int upper = node.Key.CompareTo(end);

            // Skip timestamps older than the requested range.
            if (lower < 0)
            {
                TraverseRecent(
                    node.Right, start, end, limit, predicate, results);
                return;
            }

            // Skip timestamps newer than the requested range.
            if (upper > 0)
            {
                TraverseRecent(
                    node.Left, start, end, limit, predicate, results);
                return;
            }

            // Visit newer timestamps first.
            TraverseRecent(
                node.Right, start, end, limit, predicate, results);

            for (int i = node.Values.Count - 1;
                 i >= 0 && results.Count < limit;
                 i--)
            {
                TValue value = node.Values[i];

                if (predicate is null || predicate(value))
                    results.Add(value);
            }

            // Visit older timestamps only if more results are needed.
            TraverseRecent(
                node.Left, start, end, limit, predicate, results);
        }

        private static int Height(Node? node) => node?.Height ?? 0;

        private static void UpdateHeight(Node node)
        {
            node.Height = 1 + Math.Max(
                Height(node.Left),
                Height(node.Right));
        }

        private static int BalanceFactor(Node node) =>
            Height(node.Left) - Height(node.Right);

        private static Node RotateRight(Node root)
        {
            Node pivot = root.Left!;
            root.Left = pivot.Right;
            pivot.Right = root;

            UpdateHeight(root);
            UpdateHeight(pivot);

            return pivot;
        }

        private static Node RotateLeft(Node root)
        {
            Node pivot = root.Right!;
            root.Right = pivot.Left;
            pivot.Left = root;

            UpdateHeight(root);
            UpdateHeight(pivot);

            return pivot;
        }

        private static Node Rebalance(Node node)
        {
            int balance = BalanceFactor(node);

            // Left-heavy subtree.
            if (balance > 1)
            {
                if (BalanceFactor(node.Left!) < 0)
                    node.Left = RotateLeft(node.Left!);

                return RotateRight(node);
            }

            // Right-heavy subtree.
            if (balance < -1)
            {
                if (BalanceFactor(node.Right!) > 0)
                    node.Right = RotateRight(node.Right!);

                return RotateLeft(node);
            }

            return node;
        }
    }
}
