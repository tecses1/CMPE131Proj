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
        private int intialLifetime;
        public float beamLength = 1000f;

        public SpaceRacerGun( Transform transform, Vector2 velocity, int lifetime =5) : base(transform)
        {
        
            this.transform.velocity = velocity;
            this.LifetimeFrames = lifetime;
            this.intialLifetime = lifetime;
            this.disableCollision = true; // disable collision for the gun itself, handled manually
        }

        public override void Update()
        {
            this.transform.Update();
            if(LifetimeFrames < intialLifetime)
            {
                Vector2 startPos = this.transform.GetPosition();
                Vector2 forwardDir = this.transform.Forward();
                Vector2 endPos = startPos + forwardDir * beamLength;

                GameObject[] nearbyTargets = gl.collisionManager.GetNearby(this, beamLength);

                foreach(var target in nearbyTargets)
                {
                    if(target.uid == this.owner) continue; // skip self

                    if(target.transform.rect.IntersectsLine(startPos, endPos)) {
                        target.Kill(); // Instantly kill any target hit by the beam
                    }
                }
                damage = 0; // Only deal damage on the first frame of the beam

            }
            LifetimeFrames--;
            if (LifetimeFrames < 5)
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
    

