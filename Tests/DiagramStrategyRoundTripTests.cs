namespace StockSharp.Tests;

using StockSharp.Diagram;

/// <summary>
/// A diagram strategy carries its composition in a field that is a hand-off, not a store: settings
/// can arrive before there is a composition to put them in, and once one has taken them the field
/// holds nothing. Both halves are easy to get wrong in a way only a round-trip shows.
/// </summary>
[TestClass]
public class DiagramStrategyRoundTripTests : BaseTestClass
{
	private static CompositionDiagramElement NewComposition(string name)
		=> new(new CompositionModel<InMemoryCompositionModelNode, InMemoryCompositionModelLink>(new InMemoryCompositionModelBehavior()))
		{
			Name = name,
		};

	/// <summary>
	/// A clone is loaded before its diagram is copied in, so the settings have nowhere to go when
	/// they arrive. They have to wait rather than be dropped, or the composition that turns up
	/// afterwards is empty.
	/// </summary>
	[TestMethod]
	public void SettingsThatArriveBeforeACompositionAreGivenToItWhenItComes()
	{
		var saved = new SettingsStorage();

		var source = new DiagramStrategy { Composition = NewComposition("the one that was saved") };
		source.Save(saved);

		var loaded = new DiagramStrategy();
		loaded.Load(saved);

		loaded.Composition = NewComposition("the one that turned up later");

		loaded.Composition.Name.AssertEqual("the one that was saved",
			"settings loaded before a composition existed have to reach the composition that arrives");
	}

	/// <summary>
	/// Saving is a read of the strategy. A copy of the composition left behind by a save outlives
	/// what it described, and the next composition to arrive would be loaded from it.
	/// </summary>
	[TestMethod]
	public void SavingLeavesNothingBehindForTheNextCompositionToSwallow()
	{
		var source = new DiagramStrategy { Composition = NewComposition("first") };
		source.Save(new SettingsStorage());

		source.Composition = NewComposition("second");

		source.Composition.Name.AssertEqual("second",
			"a save must not leave settings that the next composition is then loaded from");
	}
}
