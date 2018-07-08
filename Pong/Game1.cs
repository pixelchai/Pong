using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static Random r = new Random();
        float usery = 0;
        float othery = 0;

        int paddlew = 10;
        int paddleh = 70;

        int balld = 10;

        int othersc = 0;
        int usersc = 0;

        float aiyvel = -1f;

        List<Tuple<Vector2, Vector2>> ailines = new List<Tuple<Vector2, Vector2>>();

        private bool drawTraj = true;
        private bool bothAI = true;

        private bool allowDrag = true;

        Vector2 ballpos;
        Vector2 ballvel = new Vector2(5, 5);

        SpriteFont font;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Texture2D plain;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.PreferredBackBufferHeight = 1000;
            //graphics.PreferredBackBufferWidth = 1920;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            ResetBallPos();
        }

        private void ResetBallPos()
        {
            ballpos = new Vector2((GraphicsDevice.Viewport.Width / 2) - (balld / 2), (GraphicsDevice.Viewport.Height / 2) - (balld / 2));

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            plain = new Texture2D(GraphicsDevice, 1, 1);
            plain.SetData(new[] { Color.White });

            font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
            spriteBatch.Dispose();
            plain.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //updt paddle
            if (!bothAI)
            {
                usery = (int)(Mouse.GetState().Position.Y - (paddleh / 2.0));
            }

            if (allowDrag)
            {
                if(Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    ballpos = Mouse.GetState().Position.ToVector2();
                }
            }

            //ball hit bounds
            if (ballpos.Y <= 0 || ballpos.Y + balld >= GraphicsDevice.Viewport.Height)
            {
                ballvel.Y *= -1;
            }
            if (ballpos.X <= 0 || ballpos.X + balld >= GraphicsDevice.Viewport.Width)
            {
                ballvel.X *= -1;
            }

            //collide with paddle
            if(ballpos.X+balld>=(GraphicsDevice.Viewport.Width - paddlew))
            {
                //ball our side
                if((ballpos.Y > usery) && (ballpos.Y < (usery + paddleh)))
                {
                    ballvel.X *= -1;
                }
                else
                {
                    //they score
                    othersc += 1;
                    ballvel = new Vector2(5, 5);
                    ResetBallPos();
                    return;
                }
            }
            else if (ballpos.X <= paddlew)
            {
                //ball their side
                if ((ballpos.Y > othery) && (ballpos.Y < (othery + paddleh)))
                {
                    ballvel.X *= -1;
                }
                else
                {
                    //we score
                    usersc += 1;
                    ballvel = new Vector2(-5, 5);
                    ResetBallPos();
                    return;
                }
            }

            //AI
            //DoAI();
            DoAdvancedAI();

            //apply vel to ball
            ballpos.X += ballvel.X;
            ballpos.Y += ballvel.Y;

            base.Update(gameTime);
        }

        private void DoAI()
        {
            //float amount = (ballpos.X/(GraphicsDevice.Viewport.Height))/3;
            //float amount = 1;
            //float amount = (float)Math.Pow((ballpos.X / 1), 2);
            //float amount = 1f;
            float dist = (float)Math.Sqrt(Math.Pow(ballpos.Y - othery + (paddleh / 2.0), 2) + Math.Pow(ballpos.X - paddlew, 2)); //actual distance
            float amount = (float)(Math.Log10(Math.Pow(dist,1)) / 5.0f);
            //float amount = (float)(Math.Log10(ballpos.X) / 3.0f);
            //ballpos = new Vector2(200, 200);
            //othery = (int)(ballpos.Y - paddlew);
            if (ballpos.X < GraphicsDevice.Viewport.Width)
            {
                //time to act
                if(ballpos.Y> (othery + paddlew/2))
                {
                    //aiyvel += (float)(r.Next((int)(amount/2.0*100),(int)(amount*100))/100.0);
                    aiyvel += (float)Math.Min((ballpos.Y - (othery + paddleh / 2.0))*amount/25,2);
                }
                else if(ballpos.Y<(othery+paddlew/2))
                {
                    //aiyvel -= (float)(r.Next((int)(amount / 2.0*100), (int)(amount*100))/100.0);
                    aiyvel-= (float)Math.Min(((othery + paddleh / 2.0)- ballpos.Y)*amount/25,2);
                }
            }

            //apply vel
            othery += aiyvel;

            if (othery < 0)
            {
                othery = 0;
                aiyvel = 1;
            }
            if (othery + paddleh > GraphicsDevice.Viewport.Height)
            {
                othery = GraphicsDevice.Viewport.Height - paddleh;
                aiyvel = -1;
            }
        }

        //private void DoAdvancedAI()
        //{
        //    ballpos = new Vector2(200, 200);

        //    float midy = othery + paddleh / 2f;
        //    float midx = paddlew;

        //    float dist = (float)Math.Sqrt(Math.Pow(ballpos.Y - midy, 2) + Math.Pow(ballpos.X - midx, 2)); //actual distance

        //    //float amount = 0.000005f* (float)Math.Pow(dist,2);
        //    //float amount = (float)(Math.Tanh(0.03 * dist - 5) / 2 + 0.5);
        //    float amount = dist/20;
        //    if (midy > ballpos.Y)
        //    {
        //        //too low
        //        float velnorm = (float)(0.1 * Math.Pow(Math.Abs(aiyvel), 2));
        //        aiyvel += velnorm * Math.Sign(aiyvel) * -1;

        //        aiyvel = 0;
        //        aiyvel -= amount;
        //    }else if(midy< ballpos.Y)
        //    {
        //        //too high
        //        float velnorm = (float)(0.1 * Math.Pow(Math.Abs(aiyvel), 2));
        //        aiyvel += velnorm * Math.Sign(aiyvel) * -1;

        //        //aiyvel = 0;
        //        aiyvel += amount;
        //    }

        //    //apply vel
        //    othery += aiyvel;
        //}

        private void DoAdvancedAI(double smooth = 20,double iters = 1)
        {
            ailines.Clear();

            Vector2 simpos = new Vector2(ballpos.X, ballpos.Y);
            Vector2 simvel = new Vector2(ballvel.X, ballvel.Y);

            while (true)
            {
                double theta = MathHelper.ToPol(simvel.X, simvel.Y).Item1;

                //ailines.Add(new Tuple<Vector2, Vector2>(simpos, MathHelper.ToRec(simpos, theta, 1000))); //debug line

                bool headingSide = false;
                bool topbias = false;

                if (simvel.Y < 0)
                {
                    //heading towards top
                    double xtop = (Math.Tan(theta) * simpos.X - simpos.Y) / Math.Tan(theta);
                    if (xtop < GraphicsDevice.Viewport.Width && xtop > 0)
                    {
                        //will hit top

                        //ailines.Add(new Tuple<Vector2, Vector2>(new Vector2((float)xtop, GraphicsDevice.Viewport.Height), new Vector2((float)xtop, 0)));
                        ailines.Add(new Tuple<Vector2, Vector2>(simpos, new Vector2((float)xtop, 0)));

                        //updt sim
                        simvel.Y *= -1;
                        simpos = new Vector2((float)xtop, 0);
                    }
                    else
                    {
                        //will hit side before gets to top (but is heading towards top)
                        headingSide = true;
                        topbias = true;
                    }
                }
                else
                {
                    //heading towards bottom
                    double xbot = (GraphicsDevice.Viewport.Height -balld - simpos.Y + Math.Tan(theta) * simpos.X) / Math.Tan(theta);
                    if (xbot < GraphicsDevice.Viewport.Width && xbot > 0)
                    {
                        //will hit bot

                        ailines.Add(new Tuple<Vector2, Vector2>(simpos, new Vector2((float)xbot, GraphicsDevice.Viewport.Height-balld)));

                        //updt sim
                        simvel.Y *= -1;
                        simpos = new Vector2((float)xbot, GraphicsDevice.Viewport.Height-balld);
                    }
                    else
                    {
                        headingSide = true;
                        topbias = false;
                    }
                }

                if (headingSide)
                {
                    if (simvel.X > 0)
                    {
                        //heading to right
                        double yright = Math.Tan(theta) * (GraphicsDevice.Viewport.Width - paddlew - (simpos.X+balld)) + simpos.Y;
                        if(yright<GraphicsDevice.Viewport.Height && yright > 0)
                        {
                            ailines.Add(new Tuple<Vector2, Vector2>(simpos, new Vector2(GraphicsDevice.Viewport.Width-paddlew-balld,(float)yright)));

                            simpos = new Vector2(GraphicsDevice.Viewport.Width-paddlew-balld, (float)yright);
                            //break;

                            simvel.X *= -1;

                            if (bothAI)
                            {
                                UserStepTo(simpos, smooth);
                            }
                        }
                        else
                        {
                            Vector2 simsimpos = new Vector2();
                            //should technically be impossible but not in corner case
                            if (!topbias)
                            {
                                //top right
                                simsimpos = new Vector2(GraphicsDevice.Viewport.Width - paddlew - balld, 0);
                            }
                            else
                            {
                                simsimpos = new Vector2(GraphicsDevice.Viewport.Width - paddlew - balld, GraphicsDevice.Viewport.Height);

                            }
                            if (bothAI)
                            {
                                UserStepTo(simsimpos, smooth);
                            }
                            break;
                        }
                    }
                    else
                    {
                        //heading to left
                        double yleft = Math.Tan(theta) * (paddlew - simpos.X) + simpos.Y;
                        if (yleft < GraphicsDevice.Viewport.Height && yleft > 0)
                        {
                            ailines.Add(new Tuple<Vector2, Vector2>(simpos, new Vector2(paddlew, (float)yleft)));

                            simpos = new Vector2(paddlew, (float)yleft);
                            break;
                        }
                        else
                        {
                            //should technically be impossible but not in corner case
                            if (!topbias)
                            {
                                //top left
                                simpos = new Vector2(paddlew, 0);
                            }
                            else
                            {
                                simpos = new Vector2(paddlew, GraphicsDevice.Viewport.Height);

                            }
                            break;
                        }
                    }
                }

            }

            AIStepTo(simpos,smooth);
        }

        private void AIStepTo(Vector2 target,double smooth=100)
        {
            float midy = othery + paddleh / 2f;
            float midx = paddlew;

            //float dist = (float)Math.Sqrt(Math.Pow(ballpos.Y - midy, 2) + Math.Pow(ballpos.X - midx, 2)); //actual distance
            float dist = Math.Abs(midy - target.Y);

            //float amount = 0.000005f* (float)Math.Pow(dist,2);
            //float amount = (float)(Math.Tanh(0.03 * dist - 5) / 2 + 0.5);
            float amount = (float)(smooth * (1 / (1 + Math.Pow(Math.E, -0.01 * (dist - 300)))));
            if (midy > target.Y)
            {
                //too low
                othery -= Math.Min(amount, dist);
            }
            else if (midy < target.Y)
            {
                //too high
                othery += Math.Min(amount, dist);
            }
        }

        private void UserStepTo(Vector2 target, double smooth = 100)
        {
            float midy = usery + paddleh / 2f;
            float midx = GraphicsDevice.Viewport.Width - paddlew;

            //float dist = (float)Math.Sqrt(Math.Pow(ballpos.Y - midy, 2) + Math.Pow(ballpos.X - midx, 2)); //actual distance
            float dist = Math.Abs(midy - target.Y);

            //float amount = 0.000005f* (float)Math.Pow(dist,2);
            //float amount = (float)(Math.Tanh(0.03 * dist - 5) / 2 + 0.5);
            float amount = (float)(smooth * (1 / (1 + Math.Pow(Math.E, -0.01 * (dist - 300)))));
            if (midy > target.Y)
            {
                //too low
                usery -= Math.Min(amount, dist);
            }
            else if (midy < target.Y)
            {
                //too high
                usery += Math.Min(amount, dist);
            }
        }

        private int tickno = 0;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(plain, new Rectangle(0, (int)othery, paddlew, paddleh), Color.White);
            spriteBatch.Draw(plain, new Rectangle(GraphicsDevice.Viewport.Width-paddlew, (int)usery, paddlew, paddleh), Color.White);
            spriteBatch.Draw(plain, new Rectangle((int)ballpos.X, (int)ballpos.Y, balld, balld),Color.White);
            string scoretext = "" + othersc + " : " + usersc;
            Vector2 scoretextd = font.MeasureString(scoretext);
            spriteBatch.DrawString(font, scoretext, new Vector2((GraphicsDevice.Viewport.Width/2)-(scoretextd.X/2),10), Color.White);

            //ai stuff
            if (drawTraj)
            {
                foreach (Tuple<Vector2, Vector2> seg in ailines)
                {
                    new RectSprite(GraphicsDevice, seg.Item1, seg.Item2).Draw(spriteBatch, Color.Red, 2);
                }
            }

            //test rectsprite
            //Vector2 centre = new Vector2(300, 300);
            //float radius = 50f;
            //float speed = 0.1f;
            //new RectSprite(GraphicsDevice, centre, new Vector2((float)(Math.Cos(tickno*speed) * radius + centre.X), (float)(Math.Sin(tickno*speed) * radius + centre.Y)))
            //    .Draw(spriteBatch,Color.White,2);


            spriteBatch.End();

            tickno += 1;
            base.Draw(gameTime);
        }
    }
}
