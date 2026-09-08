namespace ProductManagementTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            return Ok(new[]
            {
                new {Id=1,Name="Teacher 1", Subject="Maths"},
                new {Id=2,Name="Teacher 2", Subject="Science"},
                 new {Id=3,Name="Teacher 3", Subject="History"}
            });
        }
    }
}