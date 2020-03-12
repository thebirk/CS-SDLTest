using System;
using System.Collections;
using System.Collections.Generic;

public abstract class LevelStage
{
	public float scroll { get; set; }

	public LevelStage(float scroll)
	{
		this.scroll = scroll;
	}

	public abstract bool DoStage(Level evel);
}

public class DummyStage : LevelStage
{
	public DummyStage() : base(0) { }

	public override bool DoStage(Level evel) { return false; }
}

public class BasicSpawnStage : LevelStage
{
	int count;
	string name;
	float cooldown;
	bool left;
	Vector2 dest;

	public BasicSpawnStage(float scroll, string name, int count) : base(scroll)
	{
		this.count = count;
		this.name = name;
		cooldown = 0.2f;
		dest = new Vector2(0.5f, 0.6f);
	}

	public override bool DoStage(Level level)
	{
		if(count > 0)
		{
			cooldown -= Time.DeltaTime;
			if(cooldown <= 0.0f)
			{
				Enemy e = Enemies.CreateEnemy(name);

				left = !left;
				if(left)
				{
					e.transform.position = new Vector2(0, 0);
					dest = new Vector2(dest);
				} else
				{
					e.transform.position = new Vector2(1, 0);
					dest = new Vector2(dest);
					cooldown = 0.5f;
				}
				e.transform.Rotate(180);

				dest = new Vector2(dest);
				dest.Sub(0, 0.1f);

				e.SetState(new FlyInState(new SpinState(), dest, 1.0f));
				level.AddEntity(e);
				count--;
			}

			return false;
		} else
		{
			return true;
		}
	}
}

public class Levels
{
	public static Level CreateLevel1()
	{
		Level level = new Level();

		level.stages.Enqueue(new BasicSpawnStage(2, "BasicBlaster", 5));
		level.stages.Enqueue(new DummyStage());

		return level;
	}
}

public class Level : IEnumerator<Entity>
{
	public float scroll;
	public List<Entity> entities = new List<Entity>();
	private List<Entity> entititesToAdd = new List<Entity>();

	public Queue<LevelStage> stages = new Queue<LevelStage>();
	public Player Player { get; set; }

	public Level()
	{
		Player = null;
	}

	public void AddEntity(Entity e)
	{
		e.level = this;
		entititesToAdd.Add(e);
	}

	public void Update()
	{
		scroll += Time.DeltaTime;

		LevelStage stage = stages.Peek();
		if(scroll >= stages.Peek().scroll)
		{
			if(stage.DoStage(this))
			{
				stages.Dequeue();
			}
		}

		foreach (Entity e in entities)
		{
			if (!e.removed)
			{
				e.Update();
			}
		}

		foreach (Entity e in entititesToAdd)
		{
			entities.Add(e);
		}
		entititesToAdd.Clear();
	}

	class Sorter : IComparer<Entity>
	{
		public int Compare(Entity a, Entity b)
		{
			return a.Z - b.Z;
		}
	}

	public void Draw()
	{
		entities.Sort(new Sorter());
		foreach (Entity e in entities)
		{
			if (!e.removed)
			{
				e.Draw();
			}
		}
	}

	//// Enumerator stuff
	private int offset = 0;
	public Entity Current { get { return entities[offset]; } }
	object IEnumerator.Current { get { return entities[offset]; } }

	public bool MoveNext()
	{
		offset++;
		if(offset < entities.Count)
		{
			var c = Current;
			if (c.removed)
			{
				return MoveNext();
			}
			else
			{
				return true;
			}
		} else
		{
			return false;
		}
		
	}

	public void Reset()
	{
		offset = 0;
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}

	public void Dispose() {}
}