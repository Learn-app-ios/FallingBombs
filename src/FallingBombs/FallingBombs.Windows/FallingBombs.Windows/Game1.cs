using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.ConvexHull;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.TextureTools;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

namespace FallingBombs.Windows
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Song song;
        private SpriteFont spriteFont;
        private Texture2D stars;
        private Texture2D chargeMarker;

        internal static Random random;

        private List<Mine> mines;
        private List<Explosion> explosions;
        private List<IPowerup> weaponCharges;
        private List<Bomb> bombs;

        private Body ball;
        private Body floorBody;
        public Body highScoreLine;
        private Body botherBall;
        private World world;

        private Color ballColor = new Color((int)byte.MaxValue, 0, 0, (int)byte.MaxValue);
        private Color floorColor = new Color(0, (int)byte.MaxValue, 0, (int)byte.MaxValue);
        private bool touchedGround;
        private string versionNumber = "1.9";
        private float worldScreenWidth = 66.6666f;
        private float powerupFadeTime = 30f;
        private float powerupFadeLength = 5f;
        private float timeWaitedTillBotherBallComes = 45f;
        private int ballStartFriction = 10;
        private int speedTimerMax = 30;
        private double highScore;
        private double score;
        private bool shielded;
        private DateTime timeDead;
        private DateTime lastExploded;
        private DateTime lastJumped;
        private int lastUppedScore;
        private int timeSinceStart;
        private static int errorsCaught;
        private int? lastErasedPoint;
        private float speedTimer;
        private float currentFloorLength;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        static Game1()
        {
            random = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            mines = new List<Mine>();
            explosions = new List<Explosion>();
            weaponCharges = new List<IPowerup>();
            bombs = new List<Bomb>();

            world = new World(new Vector2(0.0f, -20f));

            PrimitiveDrawing.SetUp(this.world);
            //FarseerPhysics.Settings.EnableDiagnostics = false;
            FarseerPhysics.Settings.MaxPolygonVertices = 30;

            highScore = 20; //TODO

            world.Clear();
            shielded = false;
            botherBall = null;
            speedTimer = 0;
            timeSinceStart = 0;
            touchedGround = true;
            score = 0;
            ball = BodyFactory.CreateCircle(world, 1.5f, 1f, new Vector2(0.0f, 3f), ballColor);
            ball.BodyType = BodyType.Dynamic;
            ball.Friction = 10;
            floorBody = new Body(world)
            {
                BodyType = BodyType.Static,
                Position = Vector2.Zero,
                FixedRotation = true
            };
            currentFloorLength = 0;
            BodyFactory.CreateRectangle(world, 4f, (float)(1706665.0 / 512.0), 1f, new Vector2(-35.4333f, ball.Position.Y), Color.Black).Friction = 0.0f;
            BodyFactory.CreateRectangle(world, 4f, (float)(1706665.0 / 512.0), 1f, new Vector2(35.3333f, ball.Position.Y), Color.Black).Friction = 0.0f;
            highScoreLine = BodyFactory.CreateRectangle(
                world,
                66.6666f,
                (float)(66.6666030883789 / (double)this.GraphicsDevice.Viewport.AspectRatio / 40.0),
                1f,
                new Vector2(0.0f, (float)(-this.highScore - 66.6666030883789 / (double)this.GraphicsDevice.Viewport.AspectRatio / 80.0)),
                Color.Red
            );
            highScoreLine.IsStatic = true;
            highScoreLine.IsSensor = true;
            ball.OnCollision += Ball_OnCollision;
            world.Step(1000);

            base.Initialize();
        }

        private bool Ball_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body == this.floorBody || bombs.Any(bomb => bomb.Body == fixtureB.Body))
            {
                touchedGround = (double)contact.Manifold.LocalPoint.Y <= (double)this.ball.Position.Y;
            }
            else if (fixtureB.Body.UserData is ShieldPowerup)
            {
                if (world.BodyList.Contains(fixtureB.Body))
                {
                    ShieldPowerup.sound.Play();

                    world.RemoveBody(fixtureB.Body);

                    shielded = true;

                    return false;
                }
            }
            else if (fixtureB.Body.UserData is ExplosionPowerup)
            {
                if (world.BodyList.Contains(fixtureB.Body))
                {
                    ExplosionPowerup.sound.Play();

                    if (weaponCharges.Count < 5)
                        weaponCharges.Add((IPowerup)fixtureB.Body.UserData);

                    world.RemoveBody(fixtureB.Body);

                    return false;
                }
            }
            else if (fixtureB.Body.UserData is SpeedPowerup)
            {
                if (world.BodyList.Contains(fixtureB.Body))
                {
                    SpeedPowerup.sound.Play();

                    world.RemoveBody(fixtureB.Body);

                    if ((double)speedTimer <= 0.0)
                        speedTimer = 30f;

                    return false;
                }
            }
            else if (fixtureB.Body.UserData is TeleportPowerup)
            {
                if (world.BodyList.Contains(fixtureB.Body))
                {
                    TeleportPowerup.sound.Play();

                    world.RemoveBody(fixtureB.Body);

                    if (weaponCharges.Count < 5)
                        weaponCharges.Add((IPowerup)fixtureB.Body.UserData);
                }
            }
            else if (fixtureB.Body.UserData is LazerPowerup && world.BodyList.Contains(fixtureB.Body))
            {
                LazerPowerup.sound.Play();

                world.RemoveBody(fixtureB.Body);

                if (weaponCharges.Count < 5)
                    weaponCharges.Add((IPowerup)fixtureB.Body.UserData);
            }

            return true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //AboutForm.FeedbackTexture = Content.Load<Texture2D>("email");
            stars = Content.Load<Texture2D>("Stars");
            chargeMarker = Content.Load<Texture2D>("ChargeMarker");
            Explosion.spriteSheet = Content.Load<Texture2D>("Explosion");
            Texture2D texture2D = Content.Load<Texture2D>(string.Format("Capture{0}", random.Next(1, 8)));

            song = Content.Load<Song>("fallingbombsmusic");

            spriteFont = Content.Load<SpriteFont>("font");

            Explosion.explosion1 = Content.Load<SoundEffect>("Explosion1");
            Explosion.explosion2 = Content.Load<SoundEffect>("Explosion2");
            Explosion.explosion3 = Content.Load<SoundEffect>("Explosion3");
            Explosion.explosion4 = Content.Load<SoundEffect>("Explosion4");
            Explosion.explosion5 = Content.Load<SoundEffect>("Explosion5");
            Explosion.explosion6 = Content.Load<SoundEffect>("Explosion6");
            ExplosionPowerup.sound = Content.Load<SoundEffect>("GrenadeUp");
            ShieldPowerup.sound = Content.Load<SoundEffect>("ShieldUp");
            SpeedPowerup.sound = Content.Load<SoundEffect>("SpeedUp");
            TeleportPowerup.sound = Content.Load<SoundEffect>("GravityUp");
            LazerPowerup.sound = Content.Load<SoundEffect>("LazerUp");

            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
