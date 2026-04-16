namespace Shared;

    using System.Numerics;
    using System;
using System.Security.Cryptography.X509Certificates;

public class Enemy : GameObject
    {
        public int state = 0; // 0 = wandering, 1 = chasing, 2 = attacking
        public GameObject target;
        public Vector2 targetPos;
        public float wanderTime = 5f;
        float wanderTimer = 0f;

        float radius = 600f; // how far the enemy can see. also used for wandering distance.

        public int hp = 400;

        public float shotTime = 0.66f;
        DateTime lastShot = DateTime.Now;
        public Enemy( Transform transform) : base(transform)
        {
        
            TargetCenter();
        }
        public void TargetCenter()
        {
            Vector2 centerPos = new Vector2(0, 0);
            Vector2 direction = Vector2.Normalize(centerPos - this.transform.GetPosition());
            targetPos = this.transform.GetPosition() + direction * radius;
                        //Console.WriteLine("Targeting center!" + targetPos.X + "," + targetPos.Y);
            //lock state to prevent new wander positions.
            this.wanderTimer = wanderTime;
            //Console.WriteLine(uid + " targeting center: " + targetPos.X + "," + targetPos.Y);

        }

        public virtual void Fire()
        {
            Projectile proj = new Projectile(new Transform(this.transform.rect.X, this.transform.rect.Y, 10, 10,this.transform.rotation), this.transform.Forward() * 20f, lifetime: 60);
            proj.owner = this.uid;
            proj.damage = 45;
            gl.AddGameObject(proj);
            lastShot = DateTime.Now;
        }
        public override void Update()
        {
            this.transform.Update();
            if (hp < 0)
            {
                this.Kill();
            }
            if (state == 0){

                //use a distance threshold because 1.00000000000001 is not equal to 1.00000000000000
                //Console.WriteLine (uid+ "Distance to target: " + Vector2.DistanceSquared(this.transform.GetPosition(), targetPos));
                if (Vector2.DistanceSquared(this.transform.GetPosition(), this.targetPos) < 100f)
                {
                    // Target reached
                    //Console.WriteLine(uid+"reached target: " + targetPos.X + "," + targetPos.Y);
                    wanderTimer -= 1f / GameConstants.updateRate; // Assuming Update is called at 60 FPS
                    this.transform.velocity = Vector2.Zero; // Stop moving when target is reached         

                    //when we get to the position, this is agreat time to scan for a player and switch to chasing if we find one.
                    GameObject[] nearbyObjects = this.gl.collisionManager.GetNearby(this, radius);
                    foreach (GameObject obj in nearbyObjects)
                    {
                        if (obj is Player)
                        {
                            target = obj;
                            state = 1; // Switch to chasing state
                            //Console.WriteLine(uid+"Player spotted! Switching to chasing state.");
                            break;
                        }
                    }
                }
                else
                {
                    this.transform.RotateTo(targetPos);
                    this.transform.velocity = this.transform.Forward() * 5;  
                }

                if (wanderTimer <= 0)
                {
                    //Console.WriteLine(uid+"new pos!");
                    // Pick a new random target position within a certain radius
                    Random rand = new Random();
                    float angle = (float)(rand.NextDouble() * 2 * Math.PI);
                    float distance = (float)Math.Clamp( 50+ rand.NextDouble() * radius, 50, radius);
                    float newX = MathF.Cos(angle) * distance;
                    float newY = MathF.Sin(angle) * distance;
                    targetPos = this.transform.GetPosition() + new Vector2(newX, newY);
                    wanderTimer = wanderTime;
                }
                //rotate to our target and move towards it.
            
            }else if (state == 1 && target != null){
                this.transform.RotateTo(target.transform.GetPosition());

                float distanceToTarget = Vector2.DistanceSquared(this.transform.GetPosition(), target.transform.GetPosition());
                if (distanceToTarget < 350f * 350f){ // if we're within 350 units of the player, start attacking.
                    //Console.WriteLine(uid+"In range! Starting attack.");
                    if ((DateTime.Now - lastShot).TotalSeconds >= shotTime)
                    {
                        Fire();
                        //Console.WriteLine("BANG");
                    }
                }
                // if we're farther then 250 units of the player, move towards them.
                if (distanceToTarget > 250f * 250f){ 
                    this.transform.velocity = this.transform.Forward() * 5f;
                }else{ // otherwise, stop
                    this.transform.velocity = Vector2.Zero;
                }

                if (distanceToTarget > radius * radius){ // if the player is outside our view radius, count down and return to state 0.
                    Console.WriteLine(uid+"Lost player!");

                    //reusing the wander timer
                    wanderTimer -= 1f / GameConstants.updateRate;
                    if (wanderTimer <= 0){
                        TargetCenter();
                        state = 0;
                        target = null;
                    }

                }//otherwise, reset the wanderTimer.
                else
                {
                    wanderTimer = wanderTime;
                }
            
            }
        }
        public override void Kill()
        {

            Explosion e = new Explosion(new Transform() { rect = this.transform.rect }, this.transform.velocity / 10, 1f);
            gl.AddGameObject(e);
            base.Kill();
            
        }
    //generate random trasnform out outside bundaries of world
        public static Transform GenerateTransform()
        {
            Random r = new Random();
            int size = 50;
            int spawnX,spawnY;
            int edge = r.Next(0,4);
            switch (edge)
            {
                case 0: //top
                    spawnX = r.Next(0, GameConstants.worldSizeX);
                    spawnY = -size;
                    break;
                case 1: //right
                    spawnX = GameConstants.worldSizeX + size;
                    spawnY = r.Next(0, GameConstants.worldSizeY);
                    break;
                case 2: //bottom
                    spawnX = r.Next(0, GameConstants.worldSizeX);
                    spawnY = GameConstants.worldSizeY + size;
                    break;
                case 3: //left
                    spawnX = -size;
                    spawnY = r.Next(0, GameConstants.worldSizeY);
                    break;
                default:
                    spawnX = -size;
                    spawnY = r.Next(0, GameConstants.worldSizeY);
                    break;

            }
            Transform t = new Transform(spawnX, spawnY, size, size);
            //e.SetTarget(new Vector2(GameConstants.worldSizeX/2, GameConstants.worldSizeY/2));//toggle center of screen for now.
            return t;
            //gl.AddGameObject(e):
        }
    //Helper function to spawn an enemy on world bounds.
    public static Enemy GenerateEnemy()
    {

        Enemy e = new Enemy(GenerateTransform());
        //e.SetTarget(new Vector2(GameConstants.worldSizeX/2, GameConstants.worldSizeY/2));//toggle center of screen for now.
        return e;
        //gl.AddGameObject(e):
        }
    }
    

