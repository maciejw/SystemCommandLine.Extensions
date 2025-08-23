using System;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Tests;

public class ExpressionExtensionTests
{
    private class Holder
    {
        public string? Name { get; set; }
        public int Number { get; set; }
        public string ReadOnly => "ro";
        public string WithPrivateSetter { get; private set; } = string.Empty;
        public string Field = string.Empty;
    }

    [Fact]
    public void GetPropertyName_Returns_Property_Name()
    {
        // Act
        var name = ExpressionExtensions.GetPropertyName<Holder, string?>(h => h.Name);

        // Assert
        Assert.Equal("Name", name);
    }

    [Fact]
    public void CreateArgumentMapper_Sets_Reference_Type_Property_Value()
    {
        // Arrange
        var setter = ExpressionExtensions.CreateArgumentMapper<Holder, string?>(h => h.Name);
        var holder = new Holder();

        // Act
        setter(holder, "value");

        // Assert
        Assert.Equal("value", holder.Name);
    }

    [Fact]
    public void CreateArgumentMapper_Sets_Value_Type_Property_Value()
    {
        // Arrange
        var setter = ExpressionExtensions.CreateArgumentMapper<Holder, int>(h => h.Number);
        var holder = new Holder();

        // Act
        setter(holder, 42);

        // Assert
        Assert.Equal(42, holder.Number);
    }

    [Fact]
    public void CreateArgumentMapper_Throws_When_Setter_Is_Not_Public()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.CreateArgumentMapper<Holder, string>(h => h.WithPrivateSetter));

        // Assert
        Assert.Equal("Property Holder.WithPrivateSetter has no accessible setter.", ex.Message);
    }

    [Fact]
    public void CreateArgumentMapper_Throws_When_Property_Has_No_Setter()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.CreateArgumentMapper<Holder, string>(h => h.ReadOnly));

        // Assert
        Assert.Equal("Property Holder.ReadOnly has no accessible setter.", ex.Message);
    }

    [Fact]
    public void GetPropertyName_Throws_When_Expression_Is_Field_Access()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.GetPropertyName<Holder, string>(h => h.Field));

        // Assert
        Assert.Equal("Expression must target a property", ex.Message);
    }

    [Fact]
    public void GetPropertyName_Throws_When_Expression_Is_Method_Call()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.GetPropertyName<Holder, string?>(h => h.ToString()));

        // Assert
        Assert.Equal("Expression must be a property access like t => t.Property.", ex.Message);
    }
}
