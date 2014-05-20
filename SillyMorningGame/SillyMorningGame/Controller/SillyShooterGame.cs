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
using SillyMorningGame.Model;
using SillyMorningGame.View;

namespace SillyMorningGame.Controller
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SillyShooterGame : Microsoft.Xna.Framework.Game
    {
        #region Declaration Section
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;
        Player playerTwo;

        //Number that holds the player score
        int playerOnescore;
        int playerTwoscore;

        TimeSpan startTime;

        // The font used to display UI elements
        SpriteFont font;

        bool playerFacingLeft;
        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        GamePadState currentSecondGamePadState;
        GamePadState previousSecondGamePadState;

        // A movement speed for the player
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        Texture2D projectileTexture;
        List<Projectile> projectiles;
        List<Projectile> projectilesTwo;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;
        TimeSpan previousFireTimeTwo;

        Texture2D explosionTexture;
        List<Animation> explosions;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        #endregion

        /// <summary>
        /// Constructs the Game object
        /// </summary>
        public SillyShooterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            // Initialize the player class
            player = new Player();
            playerTwo = new Player();

            playerOnescore = 0;
            playerTwoscore = 0;

            startTime = TimeSpan.Zero;

            playerFacingLeft = false;
            // Set a constant player move speed
            playerMoveSpeed = 8.0f;

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();

            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();

            projectiles = new List<Projectile>();
            projectilesTwo = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

            explosions = new List<Animation>();

            // Load the music
            gameplayMusic = Content.Load<Song>("sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");
            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");
            // Start the music right away
            PlayMusic(gameplayMusic);

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

            // TODO: use this.Content to load your game content here

            Animation playerTwoAnimation = new Animation();
            Texture2D playerTwoTexture = Content.Load<Texture2D>("Sprites/shipAnimation");
            playerTwoAnimation.Initialize(playerTwoTexture, Vector2.Zero, 115, 69, 8, 30, Color.Purple, 1f, true);

            Vector2 playerTwoPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.Width/2, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 4);
            playerTwo.Initialize(playerTwoAnimation, playerTwoPosition);


            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("Sprites/shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);

            // Load the parallaxing background
            bgLayer1.Initialize(Content, "Images/bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "Images/bgLayer2", GraphicsDevice.Viewport.Width, -2);

            mainBackground = Content.Load<Texture2D>("Images/mainbackground");

            enemyTexture = Content.Load<Texture2D>("Sprites/sampleEnemy");

            explosionTexture = Content.Load<Texture2D>("Sprites/explosion");

            projectileTexture = Content.Load<Texture2D>("Sprites/laser");
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
            if ( GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.T))
                this.Exit();

            

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            previousSecondGamePadState = currentSecondGamePadState;
            currentSecondGamePadState = GamePad.GetState(PlayerIndex.Two);

            // TODO: Add your update logic here
            UpdatePlayer(gameTime);
            UpdatePlayerTwo(gameTime);

            // Update the parallaxing background
            bgLayer1.Update();
            bgLayer2.Update();

            // Update the enemies
            UpdateEnemies(gameTime);

            // Update the collision
            UpdateCollision();

            // Update the projectiles
            UpdateProjectiles();

            // Update the explosions
            UpdateExplosions(gameTime);

            startTime += gameTime.ElapsedGameTime;

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the player based on the input from the user via gamepad or keyboard
        /// </summary>
        /// <param name="gameTime">The current time reference of the game</param>
        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Get Thumbstick Controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            // Use the Keyboard / Dpad
            if ( currentKeyboardState.IsKeyDown(Keys.Left) ||
            currentGamePadState.DPad.Left == ButtonState.Pressed )
            {
                player.Position.X -= playerMoveSpeed;
                player.PlayerAnimation.IsFlippedHorizontally = true;
                playerFacingLeft = true;
                
            }
            if ( currentKeyboardState.IsKeyDown(Keys.Right) ||
            currentGamePadState.DPad.Right == ButtonState.Pressed )
            {
                player.Position.X += playerMoveSpeed;
                player.PlayerAnimation.IsFlippedHorizontally = false;
                playerFacingLeft = false;
            }
            if ( currentKeyboardState.IsKeyDown(Keys.Up) ||
            currentGamePadState.DPad.Up == ButtonState.Pressed )
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if ( currentKeyboardState.IsKeyDown(Keys.Down) ||
            currentGamePadState.DPad.Down == ButtonState.Pressed )
            {
                player.Position.Y += playerMoveSpeed;
            }

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            // Fire only every interval we set as the fireTime
            if ( gameTime.TotalGameTime - previousFireTime > fireTime )
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                // Play the laser sound
                laserSound.Play();
            }
        }

        /// <summary>
        /// Updates all the information for player2 based on the GameTime
        /// </summary>
        /// <param name="gameTime">The current game information</param>
        private void UpdatePlayerTwo(GameTime gameTime)
        {
            playerTwo.Update(gameTime);

            playerTwo.Position.X += currentSecondGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            playerTwo.Position.Y -= currentSecondGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;
           
            // Use the Keyboard
            if ( currentKeyboardState.IsKeyDown(Keys.A) || currentSecondGamePadState.DPad.Left == ButtonState.Pressed )
            {
                playerTwo.Position.X -= playerMoveSpeed;   
            }
            if ( currentKeyboardState.IsKeyDown(Keys.D) || currentSecondGamePadState.DPad.Right == ButtonState.Pressed )
            {
                playerTwo.Position.X += playerMoveSpeed + playerMoveSpeed + playerMoveSpeed;
            }
            if ( currentKeyboardState.IsKeyDown(Keys.W) || currentSecondGamePadState.DPad.Up == ButtonState.Pressed )
            {
                playerTwo.Position.Y -= playerMoveSpeed;
            }
            if ( currentKeyboardState.IsKeyDown(Keys.S) || currentSecondGamePadState.DPad.Down == ButtonState.Pressed )
            {
                playerTwo.Position.Y += playerMoveSpeed;
            }

            // Make sure that the player does not go out of bounds
            playerTwo.Position.X = MathHelper.Clamp(playerTwo.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            playerTwo.Position.Y = MathHelper.Clamp(playerTwo.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            // Fire only every interval we set as the fireTime
            if ( currentSecondGamePadState.Buttons.B == ButtonState.Pressed || currentKeyboardState.IsKeyDown(Keys.Space) && gameTime.TotalGameTime - previousFireTimeTwo > fireTime )
            {
                // Reset our current time
                previousFireTimeTwo = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectileTwo(playerTwo.Position + new Vector2(playerTwo.Width / 2, 0));
                // Play the laser sound
                laserSound.Play();
            }
        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if ( gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime )
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for ( int i = enemies.Count - 1; i >= 0; i-- )
            {
                enemies [i].Update(gameTime);

                if ( enemies [i].Active == false )
                {
                    // If not active and health <= 0
                    if ( enemies [i].Health <= 0 )
                    {
                        // Add an explosion
                        AddExplosion(enemies [i].Position);
                        //Add to the player's score
                        playerOnescore += enemies [i].Value;
                        playerTwoscore -= enemies [i].Value;
                        // Play the explosion sound
                        explosionSound.Play();
                    }
                    enemies.RemoveAt(i);
                }
            }
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for ( int i = explosions.Count - 1; i >= 0; i-- )
            {
                explosions [i].Update(gameTime);
                if ( explosions [i].Active == false )
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;
            Rectangle rectangle3;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int) player.Position.X,
            (int) player.Position.Y,
            player.Width,
            player.Height);

            rectangle3 = new Rectangle((int) playerTwo.Position.X,
            (int) playerTwo.Position.Y,
            playerTwo.Width,
            playerTwo.Height);

            // Do the collision between the player and the enemies
            for ( int i = 0; i < enemies.Count; i++ )
            {
                rectangle2 = new Rectangle((int) enemies [i].Position.X,
                (int) enemies [i].Position.Y,
                enemies [i].Width,
                enemies [i].Height);

                // Determine if the two objects collided with each
                // other
                if ( rectangle1.Intersects(rectangle2) )
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies [i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies [i].Health = 0;

                    // If the player health is less than zero we died
                    if ( player.Health <= 0 )
                        player.Active = false;
                }

            }

            // Do the collision between the playerTwo and the enemies
            for ( int i = 0; i < enemies.Count; i++ )
            {
                rectangle2 = new Rectangle((int) enemies [i].Position.X,
                (int) enemies [i].Position.Y,
                enemies [i].Width,
                enemies [i].Height);

                // Determine if the two objects collided with each
                // other
                if ( rectangle1.Intersects(rectangle3) )
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    playerTwo.Health -= enemies [i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies [i].Health = 0;

                    // If the player health is less than zero we died
                    if ( playerTwo.Health <= 0 )
                        playerTwo.Active = false;
                }

            }

            // Projectile vs Enemy Collision
            for ( int i = 0; i < projectiles.Count; i++ )
            {
                for ( int j = 0; j < enemies.Count; j++ )
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int) projectiles [i].Position.X -
                    projectiles [i].Width / 2, (int) projectiles [i].Position.Y -
                    projectiles [i].Height / 2, projectiles [i].Width, projectiles [i].Height);

                    rectangle2 = new Rectangle((int) enemies [j].Position.X - enemies [j].Width / 2,
                    (int) enemies [j].Position.Y - enemies [j].Height / 2,
                    enemies [j].Width, enemies [j].Height);

                    // Determine if the two objects collided with each other
                    if ( rectangle1.Intersects(rectangle2) )
                    {
                        enemies [j].Health -= projectiles [i].Damage;
                        projectiles [i].Active = false;
                    }
                }
            }
            // Projectile2 vs Enemy Collision
            for ( int i = 0; i < projectilesTwo.Count; i++ )
            {
                for ( int j = 0; j < enemies.Count; j++ )
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int) projectilesTwo [i].Position.X -
                    projectilesTwo [i].Width / 2, (int) projectilesTwo [i].Position.Y -
                    projectilesTwo [i].Height / 2, projectilesTwo [i].Width, projectilesTwo [i].Height);

                    rectangle2 = new Rectangle((int) enemies [j].Position.X - enemies [j].Width / 2,
                    (int) enemies [j].Position.Y - enemies [j].Height / 2,
                    enemies [j].Width, enemies [j].Height);

                    // Determine if the two objects collided with each other
                    if ( rectangle1.Intersects(rectangle2) )
                    {
                        enemies [j].Health -= projectilesTwo [i].Damage;
                        projectilesTwo [i].Active = false;
                    }
                }
            }
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for ( int i = projectiles.Count - 1; i >= 0; i-- )
            {
                projectiles [i].Update();

                if ( projectiles [i].Active == false )
                {
                    projectiles.RemoveAt(i);
                }
            }

            // Update the ProjectilesTwo
            for ( int i = projectilesTwo.Count - 1; i >= 0; i-- )
            {
                projectilesTwo [i].Update();

                if ( projectilesTwo [i].Active == false )
                {
                    projectilesTwo.RemoveAt(i);
                }
            }
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 60, 55, 10, 5, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            if ( playerFacingLeft )
            {
                projectile.ProjectileMoveSpeed *= -1;
            }
            projectiles.Add(projectile);
        }

        private void AddProjectileTwo(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            if ( playerFacingLeft )
            {
                projectile.ProjectileMoveSpeed *= -1;
            }
            projectilesTwo.Add(projectile);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.OliveDrab);

            // TODO: Add your drawing code here
            string test = (gameTime.ElapsedGameTime.Minutes - gameTime.ElapsedGameTime.Seconds) + ":"; 
            // Start drawing
            spriteBatch.Begin();

            spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            // Draw the moving background
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);

            // Draw the Enemies
            for ( int i = 0; i < enemies.Count; i++ )
            {
                enemies [i].Draw(spriteBatch);
            }

            // Draw the Projectiles
            for ( int i = 0; i < projectiles.Count; i++ )
            {
                projectiles [i].Draw(spriteBatch);
            }

            for ( int i = 0; i < projectilesTwo.Count; i++ )
            {
                projectilesTwo [i].Draw(spriteBatch);
            }

            // Draw the explosions
            for ( int i = 0; i < explosions.Count; i++ )
            {
                explosions [i].Draw(spriteBatch);
            }

            // Draw the score
            spriteBatch.DrawString(font, "Player One score: " + playerOnescore, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            spriteBatch.DrawString(font, "Player Two score: " + playerTwoscore, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Height), Color.Purple);
            // Draw the player health
            spriteBatch.DrawString(font, "Player One health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
            spriteBatch.DrawString(font, "Player Two health: " + playerTwo.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Height - 30), Color.Purple);

            spriteBatch.DrawString(font, "Game time: " + startTime.Minutes.ToString() + ":" + startTime.Seconds.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.Width - 100, GraphicsDevice.Viewport.TitleSafeArea.Height), Color.Purple);

            // Draw the Player
            player.Draw(spriteBatch);
            playerTwo.Draw(spriteBatch);

            // Stop drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
