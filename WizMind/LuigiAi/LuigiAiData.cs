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

        // Should only be needed for debugging/sanity checking
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

        /// <summary>
        /// Verifies that the specified last action matches the overall game
        /// data's last action taken. If this doesn't match then something is
        /// using old data that should be refetched.
        /// </summary>
        /// <param name="lastAction"></param>
        public void CheckLastAction(int lastAction)
        {
            if (lastAction != this.LastAction)
            {
                Console.WriteLine("Accessing data after no longer valid");
            }
        }

        /// <summary>
        /// Invalidates all data immediately and triggers a refresh of data
        /// the next time it is requested.
        /// </summary>
        /// <param name="preserveLastAction">Whether to preserve the last action number</param>
        /// <remarks>
        /// If <c>preserveLastAction</c> is true, the last action is preserved
        /// and the game's last action is expected to be incremented before
        /// being updated again. This is expected for anything that would count
        /// as a game action like moving, shooting, or attaching/detaching
        /// parts. For certain wizard mode state changes like
        /// </remarks>
        public void InvalidateData(bool preserveLastAction)
        {
            this.cogmind = null;
            this.data = null;
            this.machineHacking = null;
            this.tiles = null;

            if (!preserveLastAction)
            {
                this.lastAction = 0;
            }
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
                if (this.lastAction != this.LastActionNoCache)
                {
                    // If this gets hit then there is something that should be
                    // invalidating data that is not
                    Console.WriteLine("Cached last action didn't match real last action");
                    this.lastAction = this.LastActionNoCache;
                }

                // Get the data from memory
                var data = this.cogmindProcess.FetchLuigiAiStruct();

                if (
                    this.lastAction == 0
                    || data.actionReady != this.lastAction
                    || stopwatch.ElapsedMilliseconds > TimeDuration.DataRefreshTimeout
                )
                {
                    this.lastAction = data.actionReady;
                    this.data = data;

                    if (stopwatch.ElapsedMilliseconds > TimeDuration.DataRefreshTimeout)
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
