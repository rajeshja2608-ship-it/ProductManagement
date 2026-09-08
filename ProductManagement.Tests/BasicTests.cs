namespace ProductManagement.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Addition_Should_ReturnCorrectResult()
        {
            // Arrange
            int a = 10;
            int b = 20;

            // Act
            int result = a + b;

            // Assert
            Assert.Equal(30, result);
        }
    }
}