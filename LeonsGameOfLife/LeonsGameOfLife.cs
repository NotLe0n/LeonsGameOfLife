using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace LeonsGameOfLife
{
    public class LeonsGameOfLife : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D solid;
        private int gameTimer = 0;
        private float scale = 1;

        private KeyboardState lastKeyboard;
        private MouseState Mouse => Microsoft.Xna.Framework.Input.Mouse.GetState();
        private MouseState lastMouse;
        private bool MouseWithinBounds => Mouse.X > 0 && Mouse.Y > 0 && Mouse.X < cells.GetLength(0) * cellWidth && Mouse.Y < cells.GetLength(1) * cellWidth;
        private Point MouseCellPos => new Point((int)((Mouse.X - Mouse.X % cellWidth) / cellWidth / scale), (int)((Mouse.Y - Mouse.Y % cellWidth) / cellWidth / scale));

        private int[,] cells;
        private const int cellWidth = 2;
        private bool playing = false;

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_MaximizeWindow(IntPtr window);

        private void MaximizeWindow()
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 50;
            Window.Position = Point.Zero;
            SDL_MaximizeWindow(Window.Handle);
            graphics.ApplyChanges();
        }

        public LeonsGameOfLife()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Leon's game of Life";
            Window.AllowUserResizing = true;
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            MaximizeWindow();

            cells = new int[511, 511];
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            solid = Content.Load<Texture2D>("solid");
        }

        protected override void Update(GameTime gameTime)
        {
            gameTimer++;

            UpdateInput();

            if (playing && gameTimer % 10 == 0)
            {
                var nextCells = new int[cells.GetLength(0), cells.GetLength(1)];

                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    for (int y = 0; y < cells.GetLength(1); y++)
                    {
                        if (nextCells[x, y] == 0 && CountNeighbors(x, y) == 3)
                        {
                            nextCells[x, y] = 1;
                        }
                        else if (nextCells[x, y] == 1 && (CountNeighbors(x, y) < 2 || CountNeighbors(x, y) > 3))
                        {
                            nextCells[x, y] = 0;
                        }
                        else
                        {
                            nextCells[x, y] = cells[x, y];
                        }
                    }
                }
                cells = nextCells;
            }

            lastKeyboard = Keyboard.GetState();
            lastMouse = Mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.White);

            var gameMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(0, 0, 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameMatrix);

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                // Draw Grid x
                //spriteBatch.Draw(magicPixel, new Rectangle(x * cellWidth, 0, 1, cellWidth * cells.GetLength(1)), Color.Black);
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    // Draw Grid y
                    //spriteBatch.Draw(magicPixel, new Rectangle(0, y * cellWidth, cellWidth * cells.GetLength(0), 1), Color.Black);

                    // Draw Cells
                    if (cells[x, y] == 1)
                    {
                        spriteBatch.Draw(solid, new Rectangle(x * cellWidth, y * cellWidth, cellWidth, cellWidth), Color.Black);
                    }
                }
            }


            // Draw Border
            spriteBatch.Draw(solid, new Rectangle(0, 0, cellWidth * cells.GetLength(0), 1), Color.Black);
            spriteBatch.Draw(solid, new Rectangle(0, 0, 1, cellWidth * cells.GetLength(0)), Color.Black);
            spriteBatch.Draw(solid, new Rectangle(0, cellWidth * cells.GetLength(0), cellWidth * cells.GetLength(0), 1), Color.Black);
            spriteBatch.Draw(solid, new Rectangle(cellWidth * cells.GetLength(0), 0, 1, cellWidth * cells.GetLength(0)), Color.Black);

            // Draw Cross
            spriteBatch.Draw(solid, new Rectangle(0, cellWidth * cells.GetLength(0) / 2, cellWidth * cells.GetLength(0), 1), Color.Red);
            spriteBatch.Draw(solid, new Rectangle(cellWidth * cells.GetLength(1) / 2, 0, 1, cellWidth * cells.GetLength(0)), Color.Red);

            // Draw Mouse
            if (!playing && MouseWithinBounds)
            {
                spriteBatch.Draw(solid, new Rectangle(MouseCellPos.X * cellWidth, MouseCellPos.Y * cellWidth, cellWidth, cellWidth), Color.Gray);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private int CountNeighbors(int i, int j)
        {
            var lives = 0;

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (i + x >= 0 && i + x < cells.GetLength(0) && j + y >= 0 && j + y < cells.GetLength(1))
                    {
                        if (!(x == 0 && y == 0))
                        {
                            lives += cells[i + x, j + y];
                        }
                    }
                }
            }

            return lives;
        }

        private void UpdateInput()
        {
            if (!lastKeyboard.IsKeyDown(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                playing = !playing;
            }
            if (!lastKeyboard.IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.R))
            {
                cells = new int[cells.GetLength(0), cells.GetLength(1)];
            }

            if (!playing && MouseWithinBounds)
            {
                if (Mouse.LeftButton == ButtonState.Pressed)
                {
                    cells[MouseCellPos.X, MouseCellPos.Y] = 1;
                }
                else if (Mouse.RightButton == ButtonState.Pressed)
                {
                    cells[MouseCellPos.X, MouseCellPos.Y] = 0;
                }
            }

            if (Mouse.ScrollWheelValue != 0)
            {
                scale -= (lastMouse.ScrollWheelValue - Mouse.ScrollWheelValue) / 8000f;
            }
        }
    }
}
