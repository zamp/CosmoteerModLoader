using System.Reflection;
using Halfling;
using Halfling.Application.Bases;
using Halfling.IO;

namespace CosmoteerModLoader;

using Log = Logger<ModLoader>;

public enum GameState
{
	Invalid,
	TitleScreen,
	GameRoot,
	Assets
}

public class ModLoader
{
	private static bool _run;
	private static GameState _gameState;
	private static Mutex? _mutex;
	private static Assembly _cosmoteerAssembly;
	private static readonly List<Mod> _mods = new();

	public static Assembly CosmoteerAssembly => _cosmoteerAssembly;

	static void Main(string[] args)
	{
		#if DEBUG
		Logger.LogLevel = LogLevel.Debug;
		#endif
		
		Log.Info($"Starting Cosmoteer Mod Loader...");
		
		var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cosmoteer.dll");
		_cosmoteerAssembly = Assembly.LoadFrom(path);
		
		var gameAppType = _cosmoteerAssembly.GetType("Cosmoteer.GameApp");
		if (gameAppType == null)
			throw new Exception("GameApp not found");

		var gameAppMain = gameAppType.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
		if (gameAppMain == null)
			throw new Exception("Main method not found.");
		
		var instanceProperty = gameAppType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
		if (instanceProperty == null)
			throw new Exception("Instance property not found.");
		
		var cosmoteerThread = new Thread(() => gameAppMain.Invoke(null, new object[] { args }));
		cosmoteerThread.SetApartmentState(ApartmentState.STA);
		cosmoteerThread.Start();

		// wait for game to start up
		object? instance;
		do
		{
			instance = instanceProperty.GetValue(null);
			Thread.Sleep(1);
		} while (instance == null);
		
		// get mutex
		var mutexField = gameAppType.GetField("s_mutex", BindingFlags.Static | BindingFlags.NonPublic);
		if (mutexField == null)
			throw new Exception("Mutex field not found.");
		_mutex = (Mutex?)mutexField.GetValue(null);
		if (_mutex == null)
			throw new Exception("Mutex not found.");
		
		var genApp = (GenericAppWithSettings)instance;
		genApp.Exiting += Exit;
		
		Log.Info("Mod loader started. Waiting for game loop to start.");

		_run = true;
		while (_run)
		{
			TryEnterState(GameState.TitleScreen, OnEnterTitleScreen);
			TryEnterState(GameState.Assets, OnEnterAssetLoading);
			TryEnterState(GameState.GameRoot, OnEnterGameRoot);
		}
	}
	
	private static void TryEnterState(GameState state, Action onEntered)
	{
		if (App.Director.StateCount == 0)
			return;
		
		if (_gameState != state && App.Director.PeekState().GetType().Name == state.ToString())
		{
			Log.Info($"Entering state {state.ToString()}");
			_gameState = state;
			onEntered();
		}
	}
	
	private static void OnEnterTitleScreen()
	{
		Log.Debug("OnEnterTitleScreen");
		foreach (var mod in _mods)
			mod.OnEnterTitleScreen();
	}

	private static void OnEnterAssetLoading()
	{
		Log.Debug("OnEnterAssetLoading");
		
		// get mods
		CosmoteerMutex(() =>
		{
			var settings = _cosmoteerAssembly.GetType("Cosmoteer.Settings");
			if (settings == null)
				throw new Exception("Settings not found.");
			var enabledModsProperty = settings.GetProperty("EnabledMods", BindingFlags.Public | BindingFlags.Static);
			if (enabledModsProperty == null)
				throw new Exception("Enabled mods not found.");
			var enabledMods = (HashSet<AbsolutePath>)(enabledModsProperty.GetValue(null) ?? new HashSet<AbsolutePath>());
			Log.Info("Loading mods...");
			foreach (var absolutePath in enabledMods)
			{
				if (TryLoadMod(Path.Combine(absolutePath.Directory, absolutePath.Filename), out var mod))
					_mods.Add(mod!);
			}
		});
		
		Log.Info("Loaded mods:");
		foreach (var mod in _mods)
		{
			Log.Info(mod.Name);
		}
	}

	private static bool TryLoadMod(string path, out Mod? mod)
	{
		var files = Directory.GetFiles(path);
		foreach (var file in files)
		{
			if (file.EndsWith(".dll"))
			{
				var modAssembly = Assembly.LoadFrom(file);
				var modType = modAssembly.GetTypes().FirstOrDefault(x => typeof(Mod).IsAssignableFrom(x));
				if (modType == null)
				{
					Log.Error($"No class found that inherits {nameof(Mod)} type in DLL: {file}");
					continue;
				}
				mod = Activator.CreateInstance(modType) as Mod;
				if (mod == null)
					throw new Exception($"Could not create instance of mod.");
				return true;
			}
		}

		mod = default;
		return false;
	}

	private static void OnEnterGameRoot()
	{
		Log.Debug("OnEnterGameRoot");
		foreach (var mod in _mods)
		{
			mod.OnEnterGameRoot();
		}
	}

	private static void Exit(object? sender, EventArgs e)
	{
		_run = false;
	}

	public static void CosmoteerMutex(Action method)
	{
		try
		{
			_mutex?.WaitOne();
			method();
		}
		finally
		{
			_mutex?.ReleaseMutex();
		}
	}
}