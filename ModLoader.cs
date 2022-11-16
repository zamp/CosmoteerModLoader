using System.Reflection;

namespace CosmoteerModLoader
{
	public class ModLoader
	{
		[STAThread]
		static void Main(string[] args)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cosmoteer.dll");
			var cosmoteerAssembly = Assembly.LoadFrom(path);
			
			var gameApp = cosmoteerAssembly.GetType("Cosmoteer.GameApp");
			if (gameApp == null)
				throw new Exception("GameApp not found");

			var gameAppMain = gameApp.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
			if (gameAppMain == null)
				throw new Exception("Main method not found.");
			
			var cosmoteerThread = new Thread(() => gameAppMain.Invoke(null, new object[] { args }));
			cosmoteerThread.SetApartmentState(ApartmentState.STA);
			cosmoteerThread.Start();

			var instanceProperty = gameApp.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
			if (instanceProperty == null)
				throw new Exception("Instance property not found.");

			object? instance;
			do
			{
				instance = instanceProperty.GetValue(null);
				Thread.Sleep(1);
			} while (instance == null);

			while (true)
			{
				// TODO: initialize mods here
			}
		}
	}
}