namespace SystemCommandLine.Extensions.Tests;

public class ExpressionExtensionTests
{
    private class Holder
    {
        public string Name { get; set; } = "default-value";
        public int Number { get; set; }
        public string ReadOnly { get; private set; } = "ro";
        public string WithPrivateSetter { get; private set; } = string.Empty;
        public string Field = string.Empty;
    }

    [Fact]
    public void GetPropertyName_returns_property_name()
    {
        // Act
        string name = ExpressionExtensions.GetPropertyName<Holder, string?>(h => h.Name);

        // Assert
        Assert.Equal("Name", name);
    }

    [Fact]
    public void CreateArgumentMapper_sets_reference_type_property_value()
    {
        // Arrange
        Action<Holder, string?> mapper = ExpressionExtensions.CreateArgumentValueMapper<Holder, string?>(h => h.Name);
        Holder holder = new();

        // Act
        mapper(holder, "value");

        // Assert
        Assert.Equal("value", holder.Name);
    }

    [Fact]
    public void CreateArgumentMapper_does_not_set_null_value()
    {
        // Arrange
        Action<Holder, string?> mapper = ExpressionExtensions.CreateArgumentValueMapper<Holder, string?>(h => h.Name);
        Holder holder = new();

        // Act
        mapper(holder, null);

        // Assert
        Assert.Equal("default-value", holder.Name);
    }

    [Fact]
    public void CreateArgumentMapper_sets_value_type_property_value()
    {
        // Arrange
        Action<Holder, int> mapper = ExpressionExtensions.CreateArgumentValueMapper<Holder, int>(h => h.Number);
        Holder holder = new();

        // Act
        mapper(holder, 42);

        // Assert
        Assert.Equal(42, holder.Number);
    }

    [Fact]
    public void CreateArgumentMapper_throws_when_setter_is_not_public()
    {
        // Act
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.CreateArgumentValueMapper<Holder, string>(h => h.WithPrivateSetter));

        // Assert
        Assert.Equal("Property Holder.WithPrivateSetter has no accessible setter.", ex.Message);
    }

    [Fact]
    public void CreateArgumentMapper_throws_when_property_has_no_setter()
    {
        // Act
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.CreateArgumentValueMapper<Holder, string>(h => h.ReadOnly));

        // Assert
        Assert.Equal("Property Holder.ReadOnly has no accessible setter.", ex.Message);
    }

    [Fact]
    public void GetPropertyName_throws_when_expression_is_field_access()
    {
        // Act
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.GetPropertyName<Holder, string>(h => h.Field));

        // Assert
        Assert.Equal("Expression must target a property", ex.Message);
    }

    [Fact]
    public void GetPropertyName_throws_when_expression_is_method_call()
    {
        // Act
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            ExpressionExtensions.GetPropertyName<Holder, string?>(h => h.ToString()));

        // Assert
        Assert.Equal("Expression must be a property access like t => t.Property.", ex.Message);
    }
}
