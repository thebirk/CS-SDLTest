using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum GameState
{
	LOADING,
	GAME,
}

public class Game
{
	Renderer renderer;
	Level level;
	Background bg;
	GameState state;
	Task loadingTask;

	public Game(Task loadingTask)
	{
		renderer = Renderer.Instance;
		state = GameState.LOADING;

		this.loadingTask = loadingTask;
		loadingTask.Wait();

		bg = new Background(2);

		level = Levels.CreateLevel1();
		level.Player = new Player(bg);
		level.AddEntity(level.Player);
	}

	public void Draw()
	{
		switch(state)
		{
		case GameState.LOADING:
		{
			Renderer.Instance.FillRectUntranslated(0, 0, Renderer.Instance.Width, Renderer.Instance.Height, 255, 0, 255, 255);
		} break;
		case GameState.GAME:
		{
			bg.Draw();
			level.Draw();
		} break;
		}
	}

	public void Update()
	{
		switch(state)
		{
		case GameState.LOADING:
		{
			if(loadingTask.IsCompleted)
			{
				state = GameState.GAME;
			}
		} break;
		case GameState.GAME:
		{
			bg.Update();
			level.Update();

			if(Input.wasPressed(Key.Q))
			{
				Console.WriteLine("Eyo");
				level = Levels.CreateLevel1();
				level.AddEntity(new Player(bg));
			}
		} break;
		}
	}

}

public class KeyState
{
	public bool down = false;
	public bool pdown = false;
}

public class Input
{

	public static Dictionary<Key, KeyState> keys = new Dictionary<Key, KeyState>();

	public static void RegisterKey(Key key)
	{
		keys[key] = new KeyState();	
	}

	public static void KeyPressed(Key key)
	{
		if(keys.ContainsKey(key))
		{
			keys[key].down = true;
		} else
		{
			var state = new KeyState();
			state.down = true;
			keys.Add(key, state);
		}
	}

	public static void KeyReleased(Key key)
	{
		if (keys.ContainsKey(key))
		{
			keys[key].down = false;
		}
		else
		{
			keys.Add(key, new KeyState());
		}
	}

	public static bool isDown(Key key)
	{
		bool found = keys.TryGetValue(key, out KeyState state);
		if(!found)
		{
			return false;
		}
		return state.down;
	}

	public static bool wasPressed(Key key)
	{
		bool found = keys.TryGetValue(key, out KeyState state);
		if (!found)
		{
			return false;
		}
		return state.down && !state.pdown;
	}

}

public class Background
{
	public float speed { get; set; }

	private int depth;

	class Level
	{
		public Texture texture;
		public float yoffset;
		public int xoffset;
		public float speed;
	}
	private List<Level> levels = new List<Level>();

	public Background(int depth)
	{
		this.speed = 4.0f;
		this.depth = depth;

		var renderer = Renderer.Instance;
		int maxOffset = 20;

		UInt32[] colors =
		{
			0xFFFFFFFF,
			0xFFFFFFFF,
			0xFFBBFFBB,
			0xFFFFBBBB,
		};

		Random rand = new Random();
		for (int i = 0; i < depth; i++)
		{
			UInt32[] pixels = new UInt32[1920*1080];
			for(int j = 0; j < 1920*1080; j++)
			{
				if(rand.Next(1024*2) == 0)
				{
					pixels[j] = colors[rand.Next(colors.Length)];
				} else
				{
					pixels[j] = 0;
				}
			}
			levels.Add(new Level
			{
				texture = renderer.CreateTextureFromUInt32(pixels, 1920, 1080),
				//yoffset = (float)Math.Pow(50, (i+1)),
				yoffset = (i+1)*50f,
				xoffset = rand.Next(-maxOffset, maxOffset),
				speed = (float)(i+1)*50f,
			});
		}
	}

	public void Draw()
	{
		Renderer renderer = Renderer.Instance;

		renderer.Clear(0, 0, 0);

		foreach(Level level in levels)
		{
			renderer.DrawTexture(level.texture, level.xoffset, (int)level.yoffset - renderer.Height, renderer.Width, renderer.Height);
			renderer.DrawTexture(level.texture, level.xoffset, (int)level.yoffset, renderer.Width, renderer.Height);
		}
	}

	public void Update()
	{
		foreach(Level level in levels)
		{
			level.yoffset += level.speed * speed * Time.DeltaTime;
			if(level.yoffset >= Renderer.Instance.Height)
			{
				level.yoffset = 0;
			}
		}
	}
}
