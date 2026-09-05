using FluentAssertions;
using Shopizy.SharedKernel.Domain;
using Xunit;

namespace Shopizy.SharedKernel.UnitTests.Domain;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public string Name { get; }

        public TestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    private sealed class AnotherEntity : Entity<Guid>
    {
        public AnotherEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Equals_WhenEntitiesHaveSameIdAndType_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Item 1");
        var entity2 = new TestEntity(id, "Item 2");

        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Equals_WhenEntitiesHaveDifferentIds_ReturnsFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid(), "Item 1");
        var entity2 = new TestEntity(Guid.NewGuid(), "Item 1");

        (entity1 != entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenComparingDifferentTypesWithSameId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Item 1");
        var entity2 = new AnotherEntity(id);

        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenComparingWithNull_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid(), "Item 1");

        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (null == entity).Should().BeFalse();
    }
}
