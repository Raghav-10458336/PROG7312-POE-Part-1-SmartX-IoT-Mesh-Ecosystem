
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.DataStructures;
using System;
using System.Linq;

namespace SmartX.Tests
{
    [TestClass]
    public class AvlTreeTests
    {
        // Verify all four AVL rotation patterns preserve sorted data.
        [DataTestMethod]
        [DataRow(30, 20, 10)] // LL
        [DataRow(10, 20, 30)] // RR
        [DataRow(30, 10, 20)] // LR
        [DataRow(10, 30, 20)] // RL
        public void Insert_RotationPatterns_PreserveSortedOrder(
            int first, int second, int third)
        {
            var tree = new AvlTree<int, string>();

            tree.Insert(first, first.ToString());
            tree.Insert(second, second.ToString());
            tree.Insert(third, third.ToString());

            CollectionAssert.AreEqual(
                new[] { "10", "20", "30" },
                tree.GetAll().ToArray());

            Assert.AreEqual(3, tree.Count);
        }

        [TestMethod]
        public void Insert_ManyAscendingKeys_PreservesAllValues()
        {
            var tree = new AvlTree<int, int>();

            for (int i = 1; i <= 1000; i++)
                tree.Insert(i, i);

            Assert.AreEqual(1000, tree.Count);

            CollectionAssert.AreEqual(
                Enumerable.Range(1, 1000).ToArray(),
                tree.GetAll().ToArray());
        }

        [TestMethod]
        public void Insert_DuplicateKeys_PreservesEveryValue()
        {
            var tree = new AvlTree<int, string>();

            tree.Insert(10, "First");
            tree.Insert(10, "Second");
            tree.Insert(10, "Third");

            Assert.AreEqual(3, tree.Count);

            CollectionAssert.AreEqual(
                new[] { "First", "Second", "Third" },
                tree.GetAll().ToArray());
        }

        [TestMethod]
        public void GetRange_IncludesBothBoundaries()
        {
            var tree = CreateNumberTree();

            CollectionAssert.AreEqual(
                new[] { 20, 30, 40 },
                tree.GetRange(20, 40).ToArray());
        }

        [TestMethod]
        public void GetRange_ExcludesValuesOutsideRange()
        {
            var tree = CreateNumberTree();

            CollectionAssert.AreEqual(
                new[] { 30 },
                tree.GetRange(25, 35).ToArray());
        }

        [TestMethod]
        public void GetRange_ReversedRange_ReturnsEmpty()
        {
            var tree = CreateNumberTree();

            Assert.AreEqual(0, tree.GetRange(40, 20).Count);
        }

        [TestMethod]
        public void GetRange_NoMatchingKeys_ReturnsEmpty()
        {
            var tree = CreateNumberTree();

            Assert.AreEqual(0, tree.GetRange(60, 80).Count);
        }

        [TestMethod]
        public void GetRecent_ReturnsNewestFirst()
        {
            var tree = CreateNumberTree();

            CollectionAssert.AreEqual(
                new[] { 50, 40, 30 },
                tree.GetRecent(10, 50, 3).ToArray());
        }

        [TestMethod]
        public void GetRecent_FiltersBeforeApplyingLimit()
        {
            var tree = CreateNumberTree();

            CollectionAssert.AreEqual(
                new[] { 40, 20 },
                tree.GetRecent(
                    10, 50, 2,
                    value => value % 20 == 0).ToArray());
        }

        [TestMethod]
        public void GetRecent_IncludesBothBoundaries()
        {
            var tree = CreateNumberTree();

            CollectionAssert.AreEqual(
                new[] { 40, 30, 20 },
                tree.GetRecent(20, 40, 10).ToArray());
        }

        [TestMethod]
        public void GetRecent_DuplicateKeys_ReturnsNewestInsertedFirst()
        {
            var tree = new AvlTree<int, string>();

            tree.Insert(10, "First");
            tree.Insert(10, "Second");
            tree.Insert(20, "Newest");

            CollectionAssert.AreEqual(
                new[] { "Newest", "Second", "First" },
                tree.GetRecent(10, 20, 10).ToArray());
        }

        [TestMethod]
        public void GetRecent_ZeroLimit_ReturnsEmpty()
        {
            var tree = CreateNumberTree();

            Assert.AreEqual(0, tree.GetRecent(10, 50, 0).Count);
        }

        [TestMethod]
        public void GetRecent_ReversedRange_ReturnsEmpty()
        {
            var tree = CreateNumberTree();

            Assert.AreEqual(0, tree.GetRecent(50, 10, 5).Count);
        }

        [TestMethod]
        public void GetRecent_NoMatchingPredicate_ReturnsEmpty()
        {
            var tree = CreateNumberTree();

            Assert.AreEqual(
                0,
                tree.GetRecent(
                    10, 50, 5,
                    value => value > 100).Count);
        }

        [TestMethod]
        public void TimestampRange_ReturnsCorrectHistoricalReadings()
        {
            var tree = new AvlTree<DateTime, string>();

            DateTime start = new(
                2026, 11, 11, 12, 0, 0,
                DateTimeKind.Utc);

            tree.Insert(start, "12:00");
            tree.Insert(start.AddMinutes(30), "12:30");
            tree.Insert(start.AddHours(1), "13:00");
            tree.Insert(start.AddHours(2), "14:00");

            CollectionAssert.AreEqual(
                new[] { "12:30", "13:00" },
                tree.GetRange(
                    start.AddMinutes(30),
                    start.AddHours(1)).ToArray());
        }

        private static AvlTree<int, int> CreateNumberTree()
        {
            var tree = new AvlTree<int, int>();

            foreach (int value in new[] { 30, 10, 50, 20, 40 })
                tree.Insert(value, value);

            return tree;
        }
    }
}
