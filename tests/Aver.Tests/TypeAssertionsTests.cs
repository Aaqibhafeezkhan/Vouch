using Xunit;

namespace Aver.Tests;

public interface IEntity { }
public class Parent : IEntity { public string Name { get; set; } = ""; }
public class Child : Parent { }
public class Employee { }
public class Person { }

public class TypeAssertionsTests
{
    [Fact] public void BeOfType_passes_on_exact_type() { object p = new Child(); p.Should().BeOfType<Child>(); }

    [Fact] public void BeOfType_is_exact() { object p = new Child(); Assert.Throws<AverException>(() => p.Should().BeOfType<Parent>()); }

    [Fact] public void BeOfType_Which_is_typed() { object p = new Child { Name = "A" }; Assert.Equal("A", p.Should().BeOfType<Child>().Which.Name); }

    [Fact]
    public void BeOfType_fails_with_exact_message()
    {
        object p = new Employee();
        Assert.Equal("Expected p to be of type Person, but found Employee.", Assert.Throws<AverException>(() => p.Should().BeOfType<Person>()).Message);
    }

    [Fact]
    public void BeOfType_fails_on_null_with_exact_message()
    {
        object? p = null;
        Assert.Equal("Expected p to be of type Person, but found null.", Assert.Throws<AverException>(() => p.Should().BeOfType<Person>()).Message);
    }

    [Fact] public void BeAssignableTo_accepts_derived() { object p = new Child(); p.Should().BeAssignableTo<Parent>(); }

    [Fact] public void BeAssignableTo_accepts_interface() { object p = new Child(); p.Should().BeAssignableTo<IEntity>(); }

    [Fact] public void BeAssignableTo_Which_is_typed() { object p = new Child { Name = "A" }; Assert.Equal("A", p.Should().BeAssignableTo<Parent>().Which.Name); }

    [Fact]
    public void BeAssignableTo_fails_with_exact_message()
    {
        object p = new Employee();
        Assert.Equal("Expected p to be assignable to IEntity, but Employee is not.", Assert.Throws<AverException>(() => p.Should().BeAssignableTo<IEntity>()).Message);
    }

    [Fact]
    public void BeAssignableTo_fails_on_null_with_exact_message()
    {
        object? p = null;
        Assert.Equal("Expected p to be assignable to IEntity, but found null.", Assert.Throws<AverException>(() => p.Should().BeAssignableTo<IEntity>()).Message);
    }

    [Fact] public void NotBeOfType_passes_on_other_type() { object p = new Child(); p.Should().NotBeOfType<Parent>(); }

    [Fact]
    public void NotBeOfType_fails_on_null_with_exact_message()
    {
        object? p = null;
        Assert.Equal("Expected p not to be of type Person, but found null.", Assert.Throws<AverException>(() => p.Should().NotBeOfType<Person>()).Message);
    }

    [Fact]
    public void NotBeAssignableTo_fails_on_null_with_exact_message()
    {
        object? p = null;
        Assert.Equal("Expected p not to be assignable to IEntity, but found null.", Assert.Throws<AverException>(() => p.Should().NotBeAssignableTo<IEntity>()).Message);
    }

    [Fact]
    public void NotBeOfType_fails_with_exact_message()
    {
        object p = new Employee();
        Assert.Equal("Expected p not to be of type Employee, but it was.", Assert.Throws<AverException>(() => p.Should().NotBeOfType<Employee>()).Message);
    }

    [Fact] public void NotBeAssignableTo_passes() { object p = new Employee(); p.Should().NotBeAssignableTo<IEntity>(); }

    [Fact]
    public void NotBeAssignableTo_fails_with_exact_message()
    {
        object p = new Child();
        Assert.Equal("Expected p not to be assignable to IEntity, but Child is.", Assert.Throws<AverException>(() => p.Should().NotBeAssignableTo<IEntity>()).Message);
    }

    [Fact] public void BeSameAs_passes() { var a = new Employee(); object o = a; o.Should().BeSameAs(a); }

    [Fact]
    public void BeSameAs_fails_with_exact_message()
    {
        object o = new Order(1);
        Assert.Equal("Expected o to refer to Order { Total = 2 }, but found another object: Order { Total = 1 }.",
            Assert.Throws<AverException>(() => o.Should().BeSameAs(new Order(2))).Message);
    }

    [Fact] public void NotBeSameAs_passes() { object o = new Employee(); o.Should().NotBeSameAs(new Employee()); }

    [Fact]
    public void NotBeSameAs_fails_with_exact_message()
    {
        var a = new Order(1);
        object o = a;
        Assert.Equal("Expected o not to refer to Order { Total = 1 }, but it did.", Assert.Throws<AverException>(() => o.Should().NotBeSameAs(a)).Message);
    }

    [Fact] public void BeOneOf_passes() { object o = 2; o.Should().BeOneOf(1, 2); }

    [Fact]
    public void BeOneOf_fails_with_exact_message()
    {
        object o = "z";
        Assert.Equal("Expected o to be one of [\"a\", \"b\"], but found \"z\".", Assert.Throws<AverException>(() => o.Should().BeOneOf("a", "b")).Message);
    }
}
