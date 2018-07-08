using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.NuclexGui;
using MonoGame.Extended.NuclexGui.Controls;
using MonoGame.Extended.NuclexGui.Controls.Desktop;
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
        private float usery = 0;
        private float othery = 0;

        private int paddlew = 10;
        private int paddleh = 70;

        private int balld = 10;

        private int othersc = 0;
        private int usersc = 0;

        private float aiyvel = -1f;

        private List<Tuple<Vector2, Vector2>> ailines = new List<Tuple<Vector2, Vector2>>();

        private bool drawTraj = true;
        private bool bothAI = true;

        private bool allowDrag = true;

        private Vector2 ballpos;
        private Vector2 ballvel = new Vector2(5, 5);

        private SpriteFont font;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D plain;

        private readonly InputListenerComponent _inputManager;
        private readonly GuiManager _gui;
        private GuiWindowControl _window;

        private bool uiDismissed = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _inputManager = new InputListenerComponent(this);
            _gui = new GuiManager(Services, new GuiInputService(_inputManager));
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

            _gui.Screen = new GuiScreen(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _gui.Screen.Desktop.Bounds = new UniRectangle(UniScalar.Zero, UniScalar.Zero, new UniScalar(1f, 0), new UniScalar(1f, 0));
            _gui.Initialize();

            _window = new GuiWindowControl
            {
                Name = "window",
                Bounds = new UniRectangle(new UniVector(new UniScalar(0), new UniScalar(0)), new UniVector(new UniScalar(_gui.Screen.Width), new UniScalar(_gui.Screen.Height))),
                Title = "Options",
                EnableDragging = false,
            };

            float choiced = 25f;
            _window.Children.Add(new GuiOptionControl()
            {
                Name = "c_control",
                Text = "User can control",
                Selected = false,
                Bounds = new UniRectangle(new UniScalar(0, 50), new UniScalar(1 / 5f, 0), new UniScalar(choiced), new UniScalar(choiced)),
            });
            _window.Children.Add(new GuiOptionControl()
            {
                Name = "c_traj",
                Text = "Draw trajectories",
                Selected = true,
                Bounds = new UniRectangle(new UniScalar(0, 50), new UniScalar(1 / 5f, 50), new UniScalar(choiced), new UniScalar(choiced)),
            });
            _window.Children.Add(new GuiOptionControl()
            {
                Name = "c_drag",
                Text = "Drag to move ball",
                Selected = true,
                Bounds = new UniRectangle(new UniScalar(0, 50), new UniScalar(1 / 5f, 100), new UniScalar(choiced), new UniScalar(choiced)),
            });
            //window.Children.Add(new MonoGame.Extended.NuclkexGui.Controls.Desktop.)

            var okbutton = new GuiButtonControl
            {
                Name = "okbutton",
                Bounds = new UniRectangle(new UniScalar(0, 50), new UniScalar(1, -100), new UniScalar(100), new UniScalar(40)),
                Text = "OK",
            };
            okbutton.Pressed += (object sender, EventArgs e) =>
            {
                foreach(GuiControl control in _window.Children)
                {
                    if (control is GuiOptionControl)
                    {
                        bool val = ((GuiOptionControl)control).Selected;
                        switch (control.Name)
                        {
                            case "c_traj":
                                this.drawTraj = val;
                                break;
                            case "c_drag":
                                this.allowDrag = val;
                                break;
                            case "c_control":
                                this.bothAI = !val;
                                break;
                        }
                    }
                }
                uiDismissed = true;
                _gui.Screen.Desktop.Children.Remove(_window);
            };
            _window.Children.Add(okbutton);

            var exitbutton = new GuiButtonControl
            {
                Name = "exitbutton",
                Bounds = new UniRectangle(new UniScalar(1, -150), new UniScalar(1, -100), new UniScalar(100), new UniScalar(40)),
                Text = "Exit",
            };
            exitbutton.Pressed += (object sender, EventArgs e) =>
            {
                Exit();
            };
            _window.Children.Add(exitbutton);

            _gui.Screen.Desktop.Children.Add(_window);
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
            if (uiDismissed)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    uiDismissed = false;
                    _gui.Screen.Desktop.Children.Add(_window);
                }
            }

            //gui stuff
            _inputManager.Update(gameTime);
            _gui.Update(gameTime);

            if (!uiDismissed) return;

            //updt paddle
            if (!bothAI)
            {
                usery = (int)(Mouse.GetState().Position.Y - (paddleh / 2.0));
            }

            if (allowDrag)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
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
            DoAdvancedAI();

            //apply vel to ball
            ballpos.X += ballvel.X;
            ballpos.Y += ballvel.Y;

            base.Update(gameTime);
        }

        private void DoAdvancedAI(double smooth = 50,double iters = 1)
        {
            ailines.Clear();

            Vector2 simpos = new Vector2(ballpos.X, ballpos.Y);
            Vector2 simvel = new Vector2(ballvel.X, ballvel.Y);

            while (true)
            {
                double theta = MathHelper.ToPol(simvel.X, simvel.Y).Item1;

                bool headingSide = false;
                bool topbias = false;

                if (simvel.Y < 0)
                {
                    //heading towards top
                    double xtop = (Math.Tan(theta) * simpos.X - simpos.Y) / Math.Tan(theta);
                    if (xtop < GraphicsDevice.Viewport.Width && xtop > 0)
                    {
                        //will hit top
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

            float dist = Math.Abs(midy - target.Y);
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

            float dist = Math.Abs(midy - target.Y);
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
                    new RectSprite(plain,seg.Item1, seg.Item2).Draw(spriteBatch, Color.Red, 2);
                }
            }

            spriteBatch.End();

            //gui
            _gui.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
