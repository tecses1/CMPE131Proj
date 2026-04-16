namespace Shared;

    using System.Numerics;
    using System;

    public class SpaceRacerGun : GameObject
    {
        //public float X;
        //public float Y;

        [Network(0)]
        public int LifetimeFrames {get; set;} = 5; 
        public int deathAnimSpeed = 5;
        int cDeathAnim = 20;
        bool dead = false;
        [Network(1)]
        public int damage {get; set;} = 9999; // OP gun that does infinite damage for 1 frame
        [Network(2)]
        public Guid owner;
        public SpaceRacerGun( Transform transform, Vector2 velocity, int lifetime =5) : base(transform)
        {
        
            this.transform.velocity = velocity;
            LifetimeFrames = lifetime;
        }

        public override void Update()
        {
            this.transform.Update();
            LifetimeFrames--;
            if (LifetimeFrames < 5)
            {
                this.Kill();
            }
            if(LifetimeFrames != 5)
            {
                damage = 0; // prevent damage after the first frame
            }  
        }

        public override void Kill()
        {

            Explosion e = new Explosion(new Transform() { rect = this.transform.rect }, this.transform.velocity / 10, 1f);
            gl.AddGameObject(e);
            base.Kill();
            
        }
    }
    

