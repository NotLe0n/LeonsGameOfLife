using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private int gameSpeed = 10;

        private bool MouseWithinBounds => Input.Mouse.X > 0 && Input.Mouse.Y > 0 && Input.Mouse.X < cells.GetLength(0) * cellWidth && Input.Mouse.Y < cells.GetLength(1) * cellWidth;
        private Point MouseCellPos => new Point(
            (int)((Input.Mouse.X - Input.Mouse.X % cellWidth) / cellWidth / scale),
            (int)((Input.Mouse.Y - Input.Mouse.Y % cellWidth) / cellWidth / scale));

        private int[,] cells;
        private const int cellWidth = 2;
        private bool playing = false;
        private bool drawCross = true;
        private bool drawGrid = false;

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

            if (playing && gameTimer % gameSpeed == 0)
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

            Input.lastKeyboard = Input.Keyboard;
            Input.lastMouse = Input.Mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.White);

            var gameMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(0, 0, 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameMatrix);

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    // Draw Grid y
                    if (drawGrid)
                    {
                        spriteBatch.Draw(solid, new Rectangle(x * cellWidth, y * cellWidth, 1, cellWidth), Color.Black);
                        spriteBatch.Draw(solid, new Rectangle(0, y * cellWidth, cellWidth * cells.GetLength(0), 1), Color.Black);
                    }

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
            if (drawCross)
            {
                spriteBatch.Draw(solid, new Rectangle(0, cellWidth * cells.GetLength(0) / 2, cellWidth * cells.GetLength(0), 1), Color.Red);
                spriteBatch.Draw(solid, new Rectangle(cellWidth * cells.GetLength(1) / 2, 0, 1, cellWidth * cells.GetLength(0)), Color.Red);
            }

            // Draw Mouse
            if (!playing && MouseWithinBounds)
            {
                spriteBatch.Draw(solid, new Rectangle(MouseCellPos.X * cellWidth, MouseCellPos.Y * cellWidth, cellWidth, cellWidth), Color.Gray);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// copied method
        /// </summary>
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
            Input.ToggleKeybind(Keys.Space, () => playing = !playing);
            Input.ToggleKeybind(Keys.R, () => cells = new int[cells.GetLength(0), cells.GetLength(1)]);
            Input.ToggleKeybind(Keys.S, () => scale = 1);
            Input.ToggleKeybind(Keys.C, () => drawCross = !drawCross);
            Input.ToggleKeybind(Keys.G, () => drawGrid = !drawGrid);

            if (!playing && MouseWithinBounds)
            {
                if (Input.Mouse.LeftButton == ButtonState.Pressed)
                {
                    cells[MouseCellPos.X, MouseCellPos.Y] = 1;
                }
                else if (Input.Mouse.RightButton == ButtonState.Pressed)
                {
                    cells[MouseCellPos.X, MouseCellPos.Y] = 0;
                }
            }

            if (Input.Mouse.ScrollWheelValue != 0)
            {
                scale -= (Input.lastMouse.ScrollWheelValue - Input.Mouse.ScrollWheelValue) / 8000f;
            }
        }

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_MaximizeWindow(System.IntPtr window);

        private void MaximizeWindow()
        {
            Window.Position = Point.Zero;

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 50;
            SDL_MaximizeWindow(Window.Handle);

            graphics.ApplyChanges();
        }
    }
}
