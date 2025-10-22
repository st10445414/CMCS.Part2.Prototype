using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class SanityTests
{
    [Fact]
    public void It_Runs_Xunit() => Assert.True(2 + 2 == 4);
}
