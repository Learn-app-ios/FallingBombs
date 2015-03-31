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
            // TODO: Add your initialization logic here

            base.Initialize();
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
