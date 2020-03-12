using System;
using System.Diagnostics;
using System.Collections.Generic;

[Flags]
public enum EntityFlag : UInt16
{
	None     = 0b0000_0000_0000_0000,
	IsPlayer = 0b0000_0000_0000_0001,
}

public abstract class Entity
{
	public static readonly int MISSILE_Z = 0;
	public static readonly int ENEMY_Z = 1;
	public static readonly int PLAYER_Z = 1000;

	public Transform transform { get; set; }
	public bool removed { get; set; }
	public Level level { get; set; }
	public int Z { get; set; }
	public EntityFlag flags { get; set; }

	public Entity()
	{
		transform = new Transform();
		removed = false;
		Z = 0;
	}

	public abstract void Draw();
	public abstract void Update();

	public void Destroy()
	{
		removed = true;
	}

	public Rectangle getHitbox()
	{
		return new Rectangle(transform.position, transform.size);
	}
}

public class Missile : Entity
{
	Texture texture;
	Vector2 dir;
	float speed;
	float age;

	public Missile(Texture texture, Vector2 dir, Vector2 size, float speed, float age) : base()
	{
		this.texture = texture;
		this.dir = dir;
		this.speed = speed;
		this.age = age;
		transform.size = size;
		transform.center = new Vector2(texture.Width / 2, texture.Height / 2);
		transform.actualSize = new Vector2(texture.Width, texture.Height);
		Z = MISSILE_Z;
	}

	public override void Draw()
	{
		Renderer.Instance.DrawTexture(texture, transform);
	}

	public override void Update()
	{
		age -= Time.DeltaTime;
		if(age <= 0.0f) { Destroy(); return; }

		foreach(Entity e in level)
		{
			//if(e.transform.GetHitbox(e.))
		}

		transform.Translate(dir * (speed * Time.DeltaTime));
	}
}

public class ParticleEmitter
{
	Texture particleTexture;
	float minDegree;
	float maxDegree;
	int maxParticles;
	float maxAge;
	float maxSpeed;

	class Particle
	{
		public float age;
		public Texture texture;
		public Transform transform;
		public Vector2 dir;
		public float speed;
	}
	List<Particle> particles = new List<Particle>();
	List<Particle> toRemove = new List<Particle>();
	Random rand = new Random();

	public ParticleEmitter(Texture particleTexture, float maxAge, float minDegree, float maxDegree, int maxParticles, float maxSpeed, Transform player)
	{
		this.particleTexture = particleTexture;
		this.minDegree = minDegree;
		this.maxDegree = maxDegree;
		this.maxParticles = maxParticles;
		this.maxAge = maxAge;
		this.maxSpeed = maxSpeed;
	}

	public void Draw()
	{
		Renderer renderer = Renderer.Instance;
		foreach(Particle p in particles)
		{
			float alpha = MathUtil.Normalize(0, maxAge, p.age);
			renderer.DrawTexture(particleTexture, p.transform, alpha);
		}
	}

	public void Update(Transform player)
	{
		foreach(Particle p in particles)
		{
			p.age -= Time.DeltaTime;
			if(p.age <= 0.0f)
			{
				toRemove.Add(p);
				continue;
			}

			p.transform.Translate(p.dir * p.speed * Time.DeltaTime);
		}

		foreach(Particle p in toRemove)
		{
			particles.Remove(p);
		}
		toRemove.Clear();

		int toAdd = maxParticles - particles.Count;
		for (int i = 0; i < toAdd; i++)
		{
			//Vector2 dir = new Vector2(0, 1);
			Vector2 dir = new Vector2();
			float diff = maxDegree - minDegree;
			float degree = (float)rand.NextDouble() * diff + minDegree;
			dir.x = (float)Math.Cos((Math.PI / 180.0f) * degree);
			dir.y = (float)Math.Sin((Math.PI / 180.0f) * degree);
			dir.Normalize();
			// Console.WriteLine("dir: " + dir);

			Transform t = new Transform();
			t.Translate(player.position + new Vector2(0, player.center.y));
			Particle p = new Particle
			{
				age = maxAge * (float)rand.NextDouble() + 0.2f,
				texture = particleTexture,
				transform = t,
				dir = dir,
				speed = maxSpeed * (float)rand.NextDouble() + 30.0f,
			};
			p.transform.center = new Vector2(particleTexture.Width / 2, particleTexture.Height / 2);
			particles.Add(p);
		}
	}
}

public class Player : Entity
{
	Texture texture;
	Texture missileTexture;
	float fireCooldown = 0.0f;
	float hitCooldown = 0.0f;
	Background bg;

	bool spinning = false;
	float spinTime = 0.0f;

	public Player(Background bg) : base()
	{
		this.bg = bg;
		texture = Resources.GetTexture("ship");
		missileTexture = Resources.GetTexture("missile");
		transform.Translate(0.5f, 0.8f);
		transform.center = new Vector2(texture.Width / 2, texture.Height / 2);
		transform.actualSize = new Vector2(texture.Width, texture.Height);
		transform.scale = 0.5f;
		Z = PLAYER_Z;
		flags = EntityFlag.IsPlayer;
	}

	public override void Draw()
	{
		Renderer.Instance.DrawTexture(texture, transform);

		if(hitCooldown > 0.0f)
		{
			Renderer.Instance.FillRectUntranslated(0, 0, Renderer.Instance.Width, Renderer.Instance.Height, 255, 0, 0, 128);
		}
	}

	public override void Update()
	{
		var speed = 0.25f;
		Vector2 delta = new Vector2();
		if(Input.isDown(Key.UP))
		{
			delta.Add(0, -1);	
		} else if(Input.isDown(Key.DOWN)) {
			delta.Add(0, 1);
		}

		if (Input.isDown(Key.LEFT))
		{
			delta.Add(-1, 0);
		}
		else if (Input.isDown(Key.RIGHT))
		{
			delta.Add(1, 0);
		}
		transform.Translate(delta * speed * Time.DeltaTime);
		bg.speed = -(delta * speed * 200f * Time.DeltaTime).y + 2f;

		if (Input.isDown(Key.A))
		{
			transform.scale -= Time.DeltaTime * 2;
		}
		else if(Input.isDown(Key.S))
		{
			transform.scale += Time.DeltaTime * 2;
		}

		if(Input.isDown(Key.W) && !spinning)
		{
			spinning = true;
			spinTime = 1.0f;
		}

		if(Input.isDown(Key.E) && hitCooldown <= 0.0f)
		{
			hitCooldown = 0.1f;
		}

		if(hitCooldown >= 0.0f)
		{
			hitCooldown -= Time.DeltaTime;
		}

		if(spinning)
		{
			spinTime -= Time.DeltaTime;
			if(spinTime <= 0.0f)
			{
				spinning = false;
				transform.rotation = 0;
			} else
			{
				transform.Rotate(360 * 2 * Time.DeltaTime);
			}
		}

		fireCooldown -= Time.DeltaTime;
		if (Input.isDown(Key.FIRE) && fireCooldown <= 0.0f && !spinning)
		{
			Missile missile = new Missile(missileTexture, new Vector2(0, -1), new Vector2(1, 1), 0.5f, 5.0f);
			missile.transform.position = transform.DoTransform(); // + new Vector2(0, -transform.center.y*transform.scale);
			missile.transform.scale = 0.8f;
			// missile.transform.scale = transform.scale;
			level.AddEntity(missile);
			fireCooldown = 0.1f;
		}
	}
}

public class IState
{
	public virtual void Start(Enemy e)  {}
	public virtual void Update(Enemy e) {}
	public virtual void Stop(Enemy e)   {}
}

public class DummyState : IState
{
	public static IState Instance = new DummyState();
}

public class SpinState : IState
{
	public override void Update(Enemy e) {
		e.transform.Rotate(45 * Time.DeltaTime);
	}
}

public class FlyInState : IState
{
	IState nextState;
	float speed;
	Vector2 start;
	Vector2 dest;
	float time;

	public FlyInState(IState nextState, Vector2 dest, float speed)
	{
		this.nextState = nextState;
		this.dest = dest;
		this.speed = speed;
		time = 0.0f;
	}

	public override void Start(Enemy e) {
		this.start = e.transform.position;
	}

	public override void Update(Enemy e)
	{
		time += Time.DeltaTime * speed;
		e.transform.position = Vector2.Lerp(start, dest, time);
		if(e.transform.position == dest)
		{
			e.SetState(nextState);
		}
	}
}

public class Enemy : Entity
{
	public IState currentState;
	public Texture texture;

	public Enemy() : base()
	{
		currentState = DummyState.Instance;
		Z = ENEMY_Z;
	}

	public void SetState(IState state)
	{
		currentState.Stop(this);
		currentState = state;
		currentState.Start(this);
	}

	public override void Draw()
	{
		Renderer.Instance.DrawTexture(texture, transform);
	}

	public override void Update()
	{
		currentState.Update(this);
	}
}

public class Enemies
{
	public static Enemy CreateEnemy(string name)
	{
		switch(name)
		{
		case "BasicBlaster":
		{
			Enemy blaster = new Enemy();
			blaster.texture = Resources.GetTexture("ship2");
			blaster.transform.center = new Vector2(blaster.texture.Width / 2, blaster.texture.Height / 2);
			blaster.transform.scale = 1.0f;
			return blaster;
		}
		default:
		{
			Debugger.Break();
			Console.Error.WriteLine("Invalid entity name: " + name);
			Environment.Exit(1);
			return null;
		};
		}
	}
}