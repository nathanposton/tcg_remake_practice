using NUnit.Framework;
using UnityEngine;

public class MinimalTest
{
    [Test]
    public void SimpleTest_ShouldPass()
    {
        Debug.Log("[TEST] MinimalTest_ShouldPass - Starting");
        Assert.IsTrue(true, "This test should always pass");
        Debug.Log("[TEST] MinimalTest_ShouldPass - PASSED");
    }
    
    [Test]
    public void SimpleTest_ShouldFail()
    {
        Debug.Log("[TEST] MinimalTest_ShouldFail - Starting");
        Assert.IsTrue(false, "This test should always fail");
        Debug.Log("[TEST] MinimalTest_ShouldFail - This should not appear");
    }
}
