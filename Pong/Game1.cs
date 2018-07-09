using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.ViewportAdapters;
using Pong.Controls;
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
        private List<Vector2> trail = new List<Vector2>();
        private bool jointrails = true;

        private bool drawTraj = true;
        private bool bothAI = true;

        private bool allowDrag = true;
        private bool angleRandomisation = false;
        private int trailno = 0;
        private Vector2 ballpos;
        private Vector2 ballvel = Vector2.Zero;
        private float ballspeed = 1f;

        private SpriteFont font;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D plain;

        private bool uiDismissed = false;

        private GuiSystem _gui;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.ClientSizeChanged += Window_ClientSizeChanged;


            //change both to false to go fast
            this.IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _gui.ClientSizeChanged();
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

            Skin.CreateDefault(Content.Load<BitmapFont>("arial"));
            _gui = new GuiSystem(new DefaultViewportAdapter(GraphicsDevice), new GuiSpriteBatchRenderer(GraphicsDevice, () => Matrix.Identity));

            SDockPanel sDockPanel;
            CheckBox c_usercontrol;
            CheckBox c_traj;
            CheckBox c_drag;
            CheckBox c_resize;
            CheckBox c_cursor;
            CheckBox c_anglerand;
            Button okaybutton;
            Button exitbutton;
            NumericalBar speedbar;
            NumericalBar gspeedbar;
            NumericalBar trailbar;
            Label warninglabel;
            _gui.ActiveScreen = new Screen
            {
                Content = sDockPanel = new SDockPanel
                {
                    Items =
                    {
                        new DockPanel
                        {
                            AttachedProperties = {{DockPanel.DockProperty,Dock.Bottom}},
                            Items =
                            {
                                new DockPanel
                                {
                                    Items =
                                    {
                                        (exitbutton=new Button{
                                            Content ="Exit",
                                            Width=100,
                                            AttachedProperties ={{DockPanel.DockProperty, Dock.Right}}
                                        }),
                                        (okaybutton=new Button{
                                            Content ="OK",
                                        }),
                                    }
                                }
                            }
                        },
                        new StackPanel
                        {
                            Spacing = 0,
                            BackgroundColor = Color.DimGray,
                            Items =
                            {
                                new Label("Options")
                                {
                                        Height=70,
                                        Margin = new Thickness(10,10,10,3),
                                },
                                new Canvas
                                {
                                    Height =2,
                                    BackgroundColor = Color.White
                                },
                                (c_usercontrol=new CheckBox{
                                        Content="User can control",
                                        Margin = new Thickness(1),
                                        IsChecked=false,
                                }),
                                (c_traj=new CheckBox{
                                        Content="Draw trajectories",
                                        Margin = new Thickness(1),
                                        IsChecked=true,
                                }),
                                (c_drag=new CheckBox{
                                        Content="Move ball",
                                        Margin = new Thickness(1),
                                        IsChecked=true,
                                }),
                                (c_resize=new CheckBox{
                                        Content="Resize window",
                                        Margin = new Thickness(1),
                                        IsChecked=false,
                                }),
                                (c_cursor=new CheckBox{
                                        Content="Show cursor",
                                        Margin = new Thickness(1),
                                        IsChecked=true,
                                }),
                                (c_anglerand=new CheckBox{
                                        Content="Angle randomisation",
                                        Margin = new Thickness(1),
                                        IsChecked=false,
                                }),
                                new DockPanel
                                {
                                    Items =
                                    {
                                        new Label("Ball speed: ")
                                        {
                                            AttachedProperties ={{DockPanel.DockProperty, Dock.Left}}
                                        },
                                        (speedbar=new NumericalBar()
                                        {
                                            Suffix="x",
                                            LBound=0.1f,
                                            UBound=8f,
                                            DecimalPlaces=1,
                                            Value=1f
                                        })
                                    }
                                },
                                //new DockPanel
                                //{
                                //    Items =
                                //    {
                                //        new Label("Game speed: ")
                                //        {
                                //            AttachedProperties ={{DockPanel.DockProperty, Dock.Left}}
                                //        },
                                //        (gspeedbar=new NumericalBar()
                                //        {
                                //            Suffix="fps",
                                //            LBound=10,
                                //            UBound=500f,
                                //            DecimalPlaces=0,
                                //            Value=60f
                                //        })
                                //    }
                                //},
                                new DockPanel
                                {
                                    Items =
                                    {
                                        new Label("Ball trail (experimental): ")
                                        {
                                            AttachedProperties ={{DockPanel.DockProperty, Dock.Left}}
                                        },
                                        (trailbar=new NumericalBar()
                                        {
                                            LBound=0f,
                                            UBound=100f,
                                            DecimalPlaces=0,
                                            Value=0f
                                        })
                                    }
                                },
                                (warninglabel=new Label("NB: AI performance may be reduced with the current options")
                                {
                                    TextColor = Color.Transparent,
                                }),
                            },
                        },
                    }
                }
            };

            sDockPanel.OnPointerMoveEvent += (object sender, EventArgs e) =>
            {
                if (c_anglerand.IsChecked || speedbar.Value > 3.09)
                {
                    warninglabel.TextColor = Color.PaleVioletRed;
                }
                else
                {
                    warninglabel.TextColor = Color.Transparent;
                }
            };
            okaybutton.Clicked += (object sender, EventArgs e) =>
            {
                trail.Clear();

                this.bothAI = !c_usercontrol.IsChecked;
                this.drawTraj = c_traj.IsChecked;
                this.allowDrag = c_drag.IsChecked;
                base.Window.AllowUserResizing = c_resize.IsChecked;
                base.IsMouseVisible = c_cursor.IsChecked;
                this.angleRandomisation = c_anglerand.IsChecked;
                this.trailno = (int)trailbar.Value;

                this.ballspeed = speedbar.Value;
                if (ballvel == Vector2.Zero)
                {
                    ballvel = new Vector2(5 * ballspeed, 5 * ballspeed);
                }
                else
                {
                    ballvel = new Vector2(Math.Sign(ballvel.X) * 5*ballspeed, Math.Sign(ballvel.Y) * 5*ballspeed);
                }

                _gui.ActiveScreen.Hide();
                uiDismissed = true;
            };
            exitbutton.Clicked += (object sender, EventArgs e) =>
            {
                Exit();
            };
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
            {
                if (uiDismissed)
                {
                    uiDismissed = false;
                    _gui.ActiveScreen.Show();
                }
            }

            //gui stuff
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
            if(ballpos.X+balld+ballvel.X>=(GraphicsDevice.Viewport.Width - paddlew))
            {
                //ball our side
                if ((ballpos.Y > usery) && (ballpos.Y < (usery + paddleh)))
                {
                    if (angleRandomisation)
                    {
                        ballvel.X *= r.Next(-130, -80) / 100f;
                    }
                    else
                    {
                        ballvel.X *= -1;
                    }
                }
                else
                {
                    //they score
                    othersc += 1;
                    ballvel = new Vector2(5*ballspeed, 5*ballspeed);
                    ResetBallPos();
                    return;
                }
            }
            else if (ballpos.X+ballvel.X <= paddlew)
            {
                //ball their side
                if ((ballpos.Y > othery) && (ballpos.Y < (othery + paddleh)))
                {
                    if (angleRandomisation)
                    {
                        ballvel.X *= r.Next(-130, -80) / 100f;
                    }
                    else
                    {
                        ballvel.X *= -1;
                    }
                }
                else
                {
                    //we score
                    usersc += 1;
                    ballvel = new Vector2(-5*ballspeed, 5*ballspeed);
                    ResetBallPos();
                    return;
                }
            }

            //AI
            DoAdvancedAI(Math.Max(15.0*MathHelper.ToPol(ballvel).Item2,50));

            //apply vel to ball
            ballpos.X += ballvel.X;
            ballpos.Y += ballvel.Y;

            base.Update(gameTime);
        }

        private void DoAdvancedAI(double smooth = 60)
        {
            ailines.Clear();

            Vector2 simpos = new Vector2(ballpos.X+ballvel.X, ballpos.Y+ballvel.Y);
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
                                simsimpos = new Vector2(GraphicsDevice.Viewport.Width - paddlew - balld, paddleh/2);
                            }
                            else
                            {
                                simsimpos = new Vector2(GraphicsDevice.Viewport.Width - paddlew - balld, GraphicsDevice.Viewport.Height-paddleh/2);

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
                                simpos = new Vector2(paddlew, paddleh/2);
                            }
                            else
                            {
                                simpos = new Vector2(paddlew, GraphicsDevice.Viewport.Height-paddleh/2);

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

            if (trailno > 0)
            {
                
                if (ballspeed < 2)
                {
                    foreach (Vector2 pos in trail)
                    {
                        spriteBatch.Draw(plain, new Rectangle((int)pos.X, (int)pos.Y, balld, balld), Color.DarkGray);
                    }
                }
                else
                {
                    for (int i = 0; i + 1 < trail.Count; i += 1)
                    {
                        new RectSprite(plain, trail[i].X+balld/2f, trail[i].Y+balld/2f, trail[i + 1].X + balld / 2f, trail[i+1].Y + balld / 2f).Draw(spriteBatch, Color.DarkGray, balld, 1.2f);
                    }
                }
                trail.Add(ballpos);
                if (trail.Count > trailno)
                {
                    trail.RemoveAt(0);
                }
            }

            spriteBatch.Draw(plain, new Rectangle(0, (int)othery, paddlew, paddleh), Color.White);
            spriteBatch.Draw(plain, new Rectangle(GraphicsDevice.Viewport.Width-paddlew, (int)usery, paddlew, paddleh), Color.White);
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

            spriteBatch.Draw(plain, new Rectangle((int)ballpos.X, (int)ballpos.Y, balld, balld), Color.White);
            spriteBatch.End();

            //gui
            _gui.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
