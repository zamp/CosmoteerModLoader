namespace CosmoteerModLoader;

public abstract class Mod
{
	public abstract string Name { get; }

	public virtual void OnEnterTitleScreen()
	{
	}
	
	public virtual void OnEnterGameRoot()
	{
	}
}