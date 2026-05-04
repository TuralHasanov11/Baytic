namespace Baytic.Api.Tests;

/// <summary>
/// Base class for unit tests using XUnit.
/// Use for testing business logic in isolation without external dependencies.
/// 
/// Usage:
/// <code>
/// public class CalculatorTests : BaseUnitTest
/// {
///     [Fact]
///     public void Add_WithPositiveNumbers_ReturnsSumCorrectly()
///     {
///         // Arrange
///         var calculator = new Calculator();
///
///         // Act
///         var result = calculator.Add(2, 3);
///
///         // Assert
///         Assert.Equal(5, result);
///     }
///
///     [Theory]
///     [InlineData(1, 2, 3)]
///     [InlineData(0, 0, 0)]
///     [InlineData(-1, 1, 0)]
///     public void Add_WithVariousNumbers_ReturnsSumCorrectly(int a, int b, int expected)
///     {
///         var calculator = new Calculator();
///         var result = calculator.Add(a, b);
///         Assert.Equal(expected, result);
///     }
/// }
/// </code>
/// </summary>
[Trait("Category", "Unit")]
public abstract class BaseUnitTest
{
}
