namespace Shared;

    using System.Numerics;
    using System;

    public class SpaceRacerGun : GameObject
    {
        //public float X;
        //public float Y;

        [Network(0)]
        public int LifetimeFrames {get; set;} = 5; 

        [Network(1)]
        public int damage {get; set;} = 9999; // OP gun that does infinite damage for 1 frame
        
        [Network(2)]
        public Guid owner;
        public float beamLength = 1000f;
        public bool isLethalFrame = true;

        public SpaceRacerGun( Transform transform, Vector2 velocity, int lifetime =5) : base(transform)
        {
            this.transform.velocity = velocity;
            this.LifetimeFrames = lifetime;
            this.disableCollision = true; // disable collision for the gun itself, handled manually
        }

        public override void Update()
        {
            this.transform.Update();
            LifetimeFrames--;
            if (LifetimeFrames < 4) {
                isLethalFrame = false; // only lethal for the first frame
            }
            if (LifetimeFrames <= 0) {
                this.Kill();
            }
        }
    }
    

