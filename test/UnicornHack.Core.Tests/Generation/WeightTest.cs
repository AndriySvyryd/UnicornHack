using Xunit;

namespace UnicornHack.Generation
{
    public class WeightTest
    {
        [Fact]
        public void DefaultWeight()
        {
            var weightFunction = new DefaultWeight {Multiplier = 0.5f}.CreateFragmentWeightFunction();

            Assert.Equal(0.5, weightFunction("foo", 2, 0, 0));
        }

        [Fact]
        public void InfiniteWeight()
        {
            var weightFunction = new InfiniteWeight().CreateFragmentWeightFunction();

            Assert.True(float.IsPositiveInfinity(weightFunction("foo", 2, 0, 0)));
        }

        [Fact]
        public void BranchWeight()
        {
            var weightFunction =
                new BranchWeight
                    {
                        Name = "foo",
                        MinDepth = 2,
                        MaxDepth = 3,
                        Matched = new DefaultWeight {Multiplier = 1}
                    }
                    .CreateFragmentWeightFunction();

            Assert.Equal(0, weightFunction("bar", 2, 0, 0));
            Assert.Equal(0, weightFunction("foo", 1, 0, 0));
            Assert.Equal(0, weightFunction("foo", 4, 0, 0));
            Assert.Equal(1, weightFunction("foo", 2, 0, 0));
            Assert.Equal(1, weightFunction("foo", 3, 0, 0));

            weightFunction =
                new BranchWeight
                    {
                        Name = "foo",
                        MinDepth = 2,
                        MaxDepth = 3,
                        NotMatched = new DefaultWeight {Multiplier = 1}
                    }
                    .CreateFragmentWeightFunction();

            Assert.Equal(1, weightFunction("bar", 2, 0, 0));
            Assert.Equal(1, weightFunction("foo", 1, 0, 0));
            Assert.Equal(1, weightFunction("foo", 4, 0, 0));
            Assert.Equal(0, weightFunction("foo", 2, 0, 0));
            Assert.Equal(0, weightFunction("foo", 3, 0, 0));

            weightFunction =
                new BranchWeight
                {
                    Name = "foo",
                    MinDepth = 2,
                    Matched = new DefaultWeight {Multiplier = 1},
                    NotMatched = new DefaultWeight {Multiplier = 2}
                }.CreateFragmentWeightFunction();

            Assert.Equal(2, weightFunction("foo", 1, 0, 0));
            Assert.Equal(1, weightFunction("foo", 2, 0, 0));

            weightFunction =
                new BranchWeight
                {
                    Name = "foo",
                    MaxDepth = 3,
                    Matched = new DefaultWeight {Multiplier = 1},
                    NotMatched = new DefaultWeight {Multiplier = 2}
                }.CreateFragmentWeightFunction();

            Assert.Equal(2, weightFunction("foo", 4, 0, 0));
            Assert.Equal(1, weightFunction("foo", 3, 0, 0));

            weightFunction =
                new BranchWeight
                {
                    Name = "foo",
                    Matched = new DefaultWeight {Multiplier = 1},
                    NotMatched = new DefaultWeight {Multiplier = 2}
                }.CreateFragmentWeightFunction();

            Assert.Equal(2, weightFunction("bar", 1, 0, 0));
            Assert.Equal(1, weightFunction("foo", 1, 0, 0));
        }

        [Fact]
        public void InstancesWeight()
        {
            var weightFunction =
                new InstancesWeight
                {
                    Max = 1,
                    W = new DefaultWeight {Multiplier = 1}
                }.CreateFragmentWeightFunction();

            Assert.Equal(1, weightFunction("foo", 2, 0, 0));
            Assert.Equal(0, weightFunction("foo", 2, 1, 0));
        }

        [Fact]
        public void TagInstancesWeight()
        {
            var weightFunction =
                new TagInstancesWeight
                {
                    Max = 1,
                    W = new DefaultWeight {Multiplier = 1}
                }.CreateFragmentWeightFunction();

            Assert.Equal(1, weightFunction("foo", 2, 0, 0));
            Assert.Equal(0, weightFunction("foo", 2, 0, 1));
        }

        [Fact]
        public void MaxWeight()
        {
            var weightFunction = new MaxWeight
                {
                    new DefaultWeight {Multiplier = 1},
                    new DefaultWeight {Multiplier = 2}
                }
                .CreateFragmentWeightFunction();

            Assert.Equal(2, weightFunction("foo", 1, 0, 0));

            weightFunction = new MaxWeight
                {
                    new DefaultWeight {Multiplier = 1},
                    new InfiniteWeight()
                }
                .CreateFragmentWeightFunction();

            Assert.True(float.IsPositiveInfinity(weightFunction("foo", 1, 0, 0)));
        }

        [Fact]
        public void MinWeight()
        {
            var weightFunction = new MinWeight
                {
                    new DefaultWeight {Multiplier = 1},
                    new DefaultWeight {Multiplier = 2}
                }
                .CreateFragmentWeightFunction();

            Assert.Equal(1, weightFunction("foo", 1, 0, 0));

            weightFunction = new MinWeight
                {
                    new DefaultWeight {Multiplier = 1},
                    new InfiniteWeight()
                }
                .CreateFragmentWeightFunction();

            Assert.Equal(1, weightFunction("foo", 1, 0, 0));
        }
    }
}
