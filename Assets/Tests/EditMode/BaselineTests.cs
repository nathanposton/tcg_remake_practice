using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for core data models and systems that don't require scene context.
/// These tests run in Edit Mode and validate basic functionality.
/// </summary>
public class BaselineTests
{
    [Test]
    public void ChaoStats_DefaultInitialization_SetsCorrectValues()
    {
        Debug.Log("[TEST] Starting ChaoStats_DefaultInitialization_SetsCorrectValues test");
        
        // Arrange & Act
        var stats = new Models.ChaoStats();
        
        // Assert
        Assert.AreEqual(Models.ChaoStage.Child, stats.stage, "Default stage should be Child");
        Assert.AreEqual(1f, stats.mood, "Default mood should be 1.0");
        Assert.AreEqual(1f, stats.belly, "Default belly should be 1.0");
        Assert.AreEqual(0, stats.swim, "Default swim should be 0");
        Assert.AreEqual(0, stats.fly, "Default fly should be 0");
        Assert.AreEqual(0, stats.run, "Default run should be 0");
        Assert.AreEqual(0, stats.power, "Default power should be 0");
        Assert.AreEqual(0, stats.stamina, "Default stamina should be 0");
        Assert.AreEqual(0, stats.rings, "Default rings should be 0");
        
        Debug.Log("[TEST] ChaoStats_DefaultInitialization_SetsCorrectValues test PASSED");
    }

    [Test]
    public void ChaoStats_StatBoundaries_EnforcesLimits()
    {
        Debug.Log("[TEST] Starting ChaoStats_StatBoundaries_EnforcesLimits test");
        
        // Arrange
        var stats = new Models.ChaoStats();
        
        // Act & Assert - Test upper bounds
        stats.swim = 150; // Should be clamped to 99
        stats.fly = 200;
        stats.run = -10; // Should be clamped to 0
        
        // Note: In actual implementation, we'd need property setters to enforce these
        // For now, we're testing the expected behavior
        Assert.IsTrue(stats.swim >= 0 && stats.swim <= 99, "Swim stat should be between 0-99");
        Assert.IsTrue(stats.fly >= 0 && stats.fly <= 99, "Fly stat should be between 0-99");
        Assert.IsTrue(stats.run >= 0 && stats.run <= 99, "Run stat should be between 0-99");
        
        Debug.Log("[TEST] ChaoStats_StatBoundaries_EnforcesLimits test PASSED");
    }

    [Test]
    public void SaveSystem_SaveAndLoad_PreservesChaoData()
    {
        Debug.Log("[TEST] Starting SaveSystem_SaveAndLoad_PreservesChaoData test");
        
        // Arrange
        var originalStats = new Models.ChaoStats
        {
            name = "TestChao",
            stage = Models.ChaoStage.Normal,
            mood = 0.8f,
            belly = 0.6f,
            swim = 25,
            fly = 30,
            run = 35,
            power = 40,
            stamina = 45,
            rings = 100
        };
        
        // Act
        Saving.SaveSystem.SaveChao(originalStats);
        var loadedStats = Saving.SaveSystem.LoadChao();
        
        // Assert
        Assert.IsNotNull(loadedStats);
        Assert.AreEqual(originalStats.name, loadedStats.name);
        Assert.AreEqual(originalStats.stage, loadedStats.stage);
        Assert.AreEqual(originalStats.mood, loadedStats.mood, 0.01f);
        Assert.AreEqual(originalStats.belly, loadedStats.belly, 0.01f);
        Assert.AreEqual(originalStats.swim, loadedStats.swim);
        Assert.AreEqual(originalStats.fly, loadedStats.fly);
        Assert.AreEqual(originalStats.run, loadedStats.run);
        Assert.AreEqual(originalStats.power, loadedStats.power);
        Assert.AreEqual(originalStats.stamina, loadedStats.stamina);
        Assert.AreEqual(originalStats.rings, loadedStats.rings);
        
        // Cleanup
        Saving.SaveSystem.ClearSave();
        
        Debug.Log("[TEST] SaveSystem_SaveAndLoad_PreservesChaoData test PASSED");
    }
}
