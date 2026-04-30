namespace Shared;

    using System.Numerics;
    using System;

    public class Projectile : GameObject
    {
        //public float X;
        //public float Y;

        [Network(0)]
        public int LifetimeFrames {get; set;} = 30; 
        public int deathAnimSpeed = 5;
        int cDeathAnim = 20;
        bool dead = false;
        [Network(1)]
        public int damage {get; set;} = 1;
        [Network(2)]
        public Guid owner;

        public bool enemy = false;

        public Projectile( Transform transform, Vector2 velocity, int lifetime =5) : base(transform)
        {
        
            this.transform.velocity = velocity;
            LifetimeFrames = lifetime;
        }

        public override void Update()
        {
            this.transform.Update();

            LifetimeFrames--;
            if (LifetimeFrames < 0)
            {
                this.Kill();
            }
            
        }

        public override void Kill()
        {

            Explosion e = new Explosion(new Transform() { rect = this.transform.rect }, this.transform.velocity / 10, 1f);
            gl.AddGameObject(e);
            base.Kill();
            
        }
    }
    

