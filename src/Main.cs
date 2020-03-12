using System;
using static SDL2.SDL;

public class Entry
{
    public static void Main(String[] args)
    {
		Renderer renderer = new Renderer(1280, 720);
		Game game = new Game(Resources.Load());

		double t = 0.0;
		const double dt = 1 / 60.0;
		double accumulator = 0.0;

		ulong lastTime = Time.Now();
		ulong lastFPS = Time.Now();

		int frames = 0;
		int updates = 0;

		while(!renderer.ShouldClose)
		{
			ulong now = Time.Now();
			double frameTime = Time.Seconds(lastTime, now);
			lastTime = now;

			accumulator += frameTime;

			while(accumulator >= dt)
			{
				{ // Update
					
					Time.DeltaTime = (float)dt;
					Time.T = (float)t;
					game.Update();
					foreach (var entry in Input.keys)
					{
						entry.Value.pdown = entry.Value.down;
					}
				}
				updates++;
				accumulator -= dt;
				t += dt;
			}

			{ // Render
				renderer.Clear(0, 0, 0);
				game.Draw();
				renderer.Update();
				renderer.Present();
				frames++;
			}

			now = Time.Now();
			if(Time.Seconds(lastFPS, now) >= 1.0)
			{
				lastFPS = now;
				Console.WriteLine("fps " + frames + ", ups " + updates);
				frames = 0;
				updates = 0;
			}
		}
    }
}

