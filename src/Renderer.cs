using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_image;

public enum Key : UInt32
{
	UP = SDL_Keycode.SDLK_UP,
	DOWN = SDL_Keycode.SDLK_DOWN,
	LEFT = SDL_Keycode.SDLK_LEFT,
	RIGHT = SDL_Keycode.SDLK_RIGHT,
	FIRE = SDL_Keycode.SDLK_SPACE,
	W = SDL_Keycode.SDLK_w,
	E = SDL_Keycode.SDLK_e,
	A = SDL_Keycode.SDLK_a,
	S = SDL_Keycode.SDLK_s,
	Q = SDL_Keycode.SDLK_q,
}

public class Renderer
{
	public int Width { get; set; }
	public int Height { get; set; }
	public Camera Camera { get; set; }

	public IntPtr SDLWindow { get; }
	public IntPtr SDLRenderer { get; }

	private bool _Fullscreen = false;
	public bool Fullscreen { get { return _Fullscreen; } }

	public bool ShouldClose { get; set; }

	public bool up = false, down = false, left = false, right = false;

	private static Renderer _renderer;
	public static Renderer Instance { get { return _renderer; } }

	public Renderer(int Width, int Height)
	{
		this.Width = Width;
		this.Height = Height;

		ShouldClose = false;

		if(SDL_Init(SDL_INIT_EVERYTHING) != 0) {
			Console.WriteLine("Failed to init SDL");
			Environment.Exit(1);
		}

		SDL_GetVersion(out SDL_version version);
		if(version.major < SDL_MAJOR_VERSION || version.minor < SDL_MINOR_VERSION || version.patch < SDL_PATCHLEVEL)
		{
			Console.Error.WriteLine("Running with a SDL dll older than what we compiled against. You are on your own now...");
			Console.Error.WriteLine("We compiled against " + version.major + "." + version.minor + "." + version.patch + ", you have " + version.major + "." + version.minor + "." + version.patch);
		}


		if(IMG_Init(IMG_InitFlags.IMG_INIT_PNG) == 0)
		{
			Console.WriteLine("Failed to init SDL_image");
			Environment.Exit(1);
		}

		SDLWindow = SDL_CreateWindow("Game", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, Width, Height, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
		if(SDLWindow == null)
		{
			Console.WriteLine("Failed to create SDL window");
			Environment.Exit(1);
		}

		SDLRenderer = SDL_CreateRenderer(SDLWindow, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
		//SDLRenderer = SDL_CreateRenderer(SDLWindow, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED|SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
		if (SDLRenderer == null)
		{
			Console.WriteLine("Failed to create SDL renderer");
			Environment.Exit(1);
		}

		//this.Width = 1920;
		//this.Height = 1080;
		//SDL_RenderSetLogicalSize(SDLRenderer, 1920, 1080);

		SDL_SetRenderDrawBlendMode(SDLRenderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

		Camera = new Camera(new Vector2());

		_renderer = this;
	}

	private Vector2 Translate(Vector2 pos)
	{
		Vector2 result = new Vector2();

		result = pos + Camera.Offset;

		return result;
	}

	public void Clear(byte r, byte g, byte b)
	{
		SDL_SetRenderDrawColor(SDLRenderer, r, g, b, 255);
		SDL_RenderClear(SDLRenderer);
	}

	public void DrawTexture(Texture texture, Transform transform, float alpha)
	{
		SDL_SetTextureAlphaMod(texture.SDLTexture, (byte)(alpha * 255));
		DrawTexture(texture, transform);
		SDL_SetTextureAlphaMod(texture.SDLTexture, 255);
	}

	public void DrawTexture(Texture texture, Transform transform)
	{
		Vector2 pos = transform.DoTransform();
		Vector2 translated = Translate(pos);

		translated.x *= Width;
		translated.y *= Height;

		SDL_Rect dest = new SDL_Rect
		{
			x = (int)(translated.x - transform.center.x * transform.scale),
			y = (int)(translated.y - transform.center.y * transform.scale),
			w = (int)(texture.Width * transform.scale),
			h = (int)(texture.Height * transform.scale),

			//w = texture.Width,
			//h = texture.Height,
			//x = (int)translated.x,
			//y = (int)translated.y,
		};
		SDL_Point center = new SDL_Point
		{
			x = (int)transform.center.x,
			y = (int)transform.center.y,
			//x = 0, y = 0,
		};
		SDL_RenderCopyEx(SDLRenderer, texture.SDLTexture, IntPtr.Zero, ref dest, transform.rotation, IntPtr.Zero, SDL_RendererFlip.SDL_FLIP_NONE);
	}

	public void DrawTexture(Texture texture, int x, int y)
	{
		Vector2 translated = Translate(new Vector2(x, y));
		SDL_Rect dest = new SDL_Rect
		{
			x = (int)translated.x,
			y = (int)translated.y,
			w = texture.Width,
			h = texture.Height,
		};
		SDL_RenderCopy(SDLRenderer, texture.SDLTexture, IntPtr.Zero, ref dest);
	}

	public void DrawTexture(Texture texture, int x, int y, int w, int h)
	{
		Vector2 translated = Translate(new Vector2(x, y));
		SDL_Rect dest = new SDL_Rect
		{
			x = (int)translated.x,
			y = (int)translated.y,
			w = w,
			h = h,
		};
		SDL_RenderCopy(SDLRenderer, texture.SDLTexture, IntPtr.Zero, ref dest);
	}

	public void FillRectUntranslated(float x, float y, float w, float h, byte r, byte g, byte b, byte a)
	{
		SDL_SetRenderDrawColor(SDLRenderer, r, g, b, a);
		SDL_Rect rect = new SDL_Rect
		{
			x = (int)x,
			y = (int)y,
			w = (int)w,
			h = (int)h
		};
		SDL_RenderFillRect(SDLRenderer, ref rect);
	}

	public void FillRect(int x, int y, int w, int h, byte r, byte g, byte b, byte a)
	{
		SDL_SetRenderDrawColor(SDLRenderer, r, g, b, a);
		Vector2 translated = Translate(new Vector2(x, y));
		SDL_Rect rect = new SDL_Rect {
			x = (int)translated.x,
			y = (int)translated.y,
			w = w,
			h = h
		};
		SDL_RenderFillRect(SDLRenderer, ref rect);
	}

	public void Present()
	{
		SDL_RenderPresent(SDLRenderer);
	}

	public void Update()
	{
		SDL_Event e;
		while(SDL_PollEvent(out e) != 0)
		{
			switch(e.type)
			{
			case SDL_EventType.SDL_QUIT:
			{
				ShouldClose = true;
			} break;
			case SDL_EventType.SDL_KEYDOWN:
			{
				if (Enum.IsDefined(typeof(Key), (Key)e.key.keysym.sym))
				{
					Input.KeyPressed((Key)e.key.keysym.sym);
				}
			} break;
			case SDL_EventType.SDL_KEYUP:
			{
				if (e.key.keysym.sym == SDL_Keycode.SDLK_F11)
				{
					_Fullscreen = !_Fullscreen;
					if (_Fullscreen)
					{
						SDL_SetWindowFullscreen(SDLWindow, (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);
					}
					else
					{
						SDL_SetWindowFullscreen(SDLWindow, 0);
					}
				}
				if (Enum.IsDefined(typeof(Key), (Key)e.key.keysym.sym))
				{
					Input.KeyReleased((Key)e.key.keysym.sym);
				}
			} break;
			case SDL_EventType.SDL_WINDOWEVENT:
			{
				switch(e.window.windowEvent)
				{
				case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
				{
					Width = e.window.data1;
					Height = e.window.data2;
				} break;
				}
			} break;
			}
		}
	}

	public Texture CreateTextureFromUInt32(UInt32[] pixels, int width, int height)
	{
		return new Texture(this, pixels, width, height);
	}

	public Texture LoadTexture(String path)
	{
		return new Texture(this, path);
	}

	private static byte[] ReadToEnd(Stream stream)
	{
		long originalPosition = 0;

		if (stream.CanSeek)
		{
			originalPosition = stream.Position;
			stream.Position = 0;
		}

		try
		{
			byte[] readBuffer = new byte[4096];

			int totalBytesRead = 0;
			int bytesRead;

			while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
			{
				totalBytesRead += bytesRead;

				if (totalBytesRead == readBuffer.Length)
				{
					int nextByte = stream.ReadByte();
					if (nextByte != -1)
					{
						byte[] temp = new byte[readBuffer.Length * 2];
						Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
						Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
						readBuffer = temp;
						totalBytesRead++;
					}
				}
			}

			byte[] buffer = readBuffer;
			if (readBuffer.Length != totalBytesRead)
			{
				buffer = new byte[totalBytesRead];
				Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
			}
			return buffer;
		}
		finally
		{
			if (stream.CanSeek)
			{
				stream.Position = originalPosition;
			}
		}
	}

	public Texture LoadTextureFromStream(Stream stream, string name, long length = 0)
	{
		//TODO(thebirk): Read using the length
		byte[] data = ReadToEnd(stream);
		IntPtr rwops = SDL_RWFromMem(data, data.Length);
		Texture t = new Texture(this, rwops, name);
		return t;
	}
}

public class Texture
{
	public int Width { get; }
	public int Height { get; }
	public IntPtr SDLTexture { get; }

	public Texture(Renderer renderer, String path)
	{
		SDLTexture = IMG_LoadTexture(renderer.SDLRenderer, path);
		if (SDLTexture == IntPtr.Zero)
		{
			Console.Error.WriteLine("Failed to load texture: " + path);
			Debugger.Break();
			Environment.Exit(1);
		}

		SDL_QueryTexture(SDLTexture, out uint format, out int access, out int w, out int h);
		Width = w;
		Height = h;
	}

	public Texture(Renderer renderer, IntPtr rwops, String name)
	{
		SDLTexture = IMG_LoadTexture_RW(renderer.SDLRenderer, rwops, 0);
		if (SDLTexture == IntPtr.Zero)
		{
			Console.Error.WriteLine("Failed to load texture: " + name);
			Debugger.Break();
			Environment.Exit(1);
		}

		SDL_QueryTexture(SDLTexture, out uint format, out int access, out int w, out int h);
		Width = w;
		Height = h;
	}

	private static readonly UInt32 ALPHA_MASK = 0xFF000000U;
	private static readonly UInt32 RED_MASK   = 0x00FF0000U;
	private static readonly UInt32 GREEN_MASK = 0x0000FF00U;
	private static readonly UInt32 BLUE_MASK  = 0x000000FFU;

	public Texture(Renderer renderer, UInt32[] pixelsSafe, int width, int height)
	{
		GCHandle handle = GCHandle.Alloc(pixelsSafe, GCHandleType.Pinned);
		IntPtr pixels = handle.AddrOfPinnedObject();
		IntPtr surface = SDL_CreateRGBSurfaceFrom(pixels, width, height, 32, width*4, RED_MASK, GREEN_MASK, BLUE_MASK, ALPHA_MASK);
		SDLTexture = SDL_CreateTextureFromSurface(renderer.SDLRenderer, surface);
		SDL_FreeSurface(surface);
		handle.Free();
		Width = width;
		Height = height;
	}

}