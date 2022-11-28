using CosmoteerModLoader;

namespace ExampleMod;

public class ExampleMod : Mod
{
	public override string Name => "Example Mod";

	public override void OnEnterTitleScreen()
	{
		base.OnEnterTitleScreen();
		
		Console.WriteLine($"{nameof(ExampleMod)} entered title screen!");
	}

	public override void OnEnterGameRoot()
	{
		base.OnEnterGameRoot();
		
		Console.WriteLine($"{nameof(ExampleMod)} entered game root!");
	}
}