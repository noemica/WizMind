using System.Diagnostics;
using WizMind.Definitions;
using WizMind.Instances;
using WizMind.Interaction;

namespace WizMind.LuigiAi
{
    public class LuigiAiData(CogmindProcess cogmindProcess, GameDefinitions definitions)
    {
        private readonly CogmindProcess cogmindProcess = cogmindProcess;
        private Entity? cogmind;
        private LuigiAiStruct? data;
        private MachineHacking? machineHacking;
        private int lastAction;
        private readonly GameDefinitions definitions = definitions;
        private MapTile[,]? tiles;

        // Should only be needed for debugging
        public int LastActionNoCache => this.cogmindProcess.FetchLuigiAiStruct().actionReady;

        public int LastAction
        {
            get
            {
                if (this.data == null)
                {
                    // If data has been invalidated, refresh data now before returning the number
                    this.GetData();
                }

                return this.lastAction;
            }
        }

        public Entity Cogmind
        {
            get
            {
                if (this.cogmind == null)
                {
                    var entityStruct = this.cogmindProcess.FetchStruct<LuigiEntityStruct>(
                        this.GetData().player
                    );
                    this.cogmind = new Entity(
                        this.cogmindProcess,
                        this.definitions,
                        this,
                        entityStruct
                    );
                }

                return this.cogmind;
            }
        }

        public int Depth => this.GetData().locationDepth;

        public MachineHacking? MachineHacking
        {
            get
            {
                if (this.machineHacking == null && this.GetData().machineHacking != IntPtr.Zero)
                {
                    var machineHackingStruct =
                        this.cogmindProcess.FetchStruct<LuigiMachineHackingStruct>(
                            this.GetData().machineHacking
                        );
                    this.machineHacking = new MachineHacking(this, machineHackingStruct);
                }

                return this.machineHacking;
            }
        }

        public MapType MapType => this.GetData().locationMap;

        public int MapCursorIndex => this.GetData().mapCursorIndex;

        public int MapHeight => this.GetData().mapHeight;

        public int MapWidth => this.GetData().mapWidth;

        public IEnumerable<MapTile> AllTiles
        {
            get
            {
                foreach (var tile in this.Tiles)
                {
                    yield return tile;
                }
            }
        }

        // Optimize: Could delay fetching individual tiles until needed
        // Currently takes ~25ms but would be much faster to only fetch
        // a handful of requested tiles at a time
        public MapTile[,] Tiles
        {
            get
            {
                if (this.tiles == null)
                {
                    // Get the tile structs and convert to map tiles
                    var tilesList = this.cogmindProcess.FetchList<LuigiTileStruct>(
                        this.GetData().mapData,
                        this.MapWidth * this.MapHeight
                    );
                    this.tiles = new MapTile[this.MapWidth, this.MapHeight];

                    for (var y = 0; y < this.MapHeight; y++)
                    {
                        for (var x = 0; x < this.MapWidth; x++)
                        {
                            this.tiles[x, y] = new MapTile(
                                this.cogmindProcess,
                                this.definitions,
                                this,
                                tilesList[y + x * this.MapHeight],
                                x,
                                y
                            );
                        }
                    }
                }

                return this.tiles;
            }
        }

        public void CheckLastAction(int lastAction)
        {
            if (lastAction != this.LastAction)
            {
                Console.WriteLine("Accessing data after no longer valid");
            }
        }

        public MapTile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.MapWidth || y >= this.MapHeight)
            {
                throw new ArgumentException("Value out of range");
            }

            return this.Tiles[y, x];
        }

        public void InvalidateData()
        {
            this.cogmind = null;
            this.data = null;
            this.lastAction = 0;
            this.machineHacking = null;
            this.tiles = null;
        }

        private LuigiAiStruct GetData()
        {
            if (this.data == null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (this.data == null)
                {
                    TryGetData(stopwatch);
                }
            }

            return this.data.Value;

            void TryGetData(Stopwatch stopwatch)
            {
                var data = this.cogmindProcess.FetchLuigiAiStruct();

                if (
                    this.lastAction == 0
                    || data.actionReady != this.lastAction
                    || stopwatch.ElapsedMilliseconds > 5000
                )
                {
                    this.lastAction = data.actionReady;
                    this.data = data;

                    if (stopwatch.ElapsedMilliseconds > 5000)
                    {
                        Console.WriteLine("Passed wait timeout");
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
