using System;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     Based on FOV algorithm by Adam Milazzo
    ///     http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#mycode
    /// </summary>
    public class BeveledFOV
    {
        private readonly Func<byte, byte, byte, int, bool> _blocksLight;
        private readonly Func<byte, byte, DirectionFlags> _getUnconnectedNeighbours;
        private const byte MinVisibility = 0;
        private const byte MaxVisibility = byte.MaxValue;

        /// <summary>
        ///     Creates a new instance of <see cref="BeveledFOV"/>
        /// </summary>
        /// <param name="blocksLight">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     It also marks the tile as visible.
        ///     The function must ignore coordinates that are out of bounds
        /// </param>
        /// <param name="getUnconnectedNeighbours">
        ///     A function that returns which neighbours the opaque tile at the given X and Y coordinates is not connected to.
        ///     The function must ignore coordinates that are out of bounds
        /// </param>
        /// <remarks>
        ///     Functions take coordinates instead of <see cref="Point"/> for perf
        /// </remarks>
        public BeveledFOV(Func<byte, byte, byte, int, bool> blocksLight,
            Func<byte, byte, DirectionFlags> getUnconnectedNeighbours)
        {
            _blocksLight = blocksLight;
            _getUnconnectedNeighbours = getUnconnectedNeighbours;
        }

        public void Compute(Point origin, byte rangeLimit, byte visibilityFalloff)
        {
            _blocksLight(origin.X, origin.Y, MaxVisibility, 0);
            for (byte octant = 0; octant < 8; octant++)
            {
                Compute(octant, origin, rangeLimit, visibilityFalloff, x: 1, top: new Slope(1, 1),
                    bottom: new Slope(0, 1));
            }
        }

        public void Compute(
            Point origin, Direction heading,
            byte primaryFOV, byte primaryRange,
            byte secondaryFOV, byte secondaryRange,
            bool noFalloff)
        {
            if (primaryFOV > secondaryFOV)
            {
                throw new ArgumentOutOfRangeException(nameof(primaryFOV));
            }

            _blocksLight(origin.X, origin.Y, MaxVisibility, 0);
            var octantShift = Direction.East.OctantsTo(heading);
            if (octantShift == -1)
            {
                return;
            }

            octantShift = (byte)(8 + octantShift - secondaryFOV);
            var visibilityFalloff = noFalloff ? (byte)0 : (byte)(byte.MaxValue / secondaryRange);
            for (var octant = 0; octant < secondaryFOV - primaryFOV; octant++)
            {
                Compute((byte)((octant + octantShift) % 8), origin, secondaryRange, visibilityFalloff, x: 1,
                    top: new Slope(1, 1), bottom: new Slope(0, 1));
            }
            for (var octant = secondaryFOV + primaryFOV; octant < secondaryFOV * 2; octant++)
            {
                Compute((byte)((octant + octantShift) % 8), origin, secondaryRange, visibilityFalloff, x: 1,
                    top: new Slope(1, 1), bottom: new Slope(0, 1));
            }

            visibilityFalloff = noFalloff ? (byte)0 : (byte)(byte.MaxValue / primaryRange);
            for (var octant = secondaryFOV - primaryFOV; octant < secondaryFOV + primaryFOV; octant++)
            {
                Compute((byte)((octant + octantShift) % 8), origin, primaryRange, visibilityFalloff, x: 1,
                    top: new Slope(1, 1), bottom: new Slope(0, 1));
            }
        }

        private struct Slope // represents the slope Y/X as a rational number
        {
            public Slope(int y, int x)
            {
                Y = y;
                X = x;
            }

            public bool Greater(int y, int x) => Y * x > X * y;

            public bool GreaterOrEqual(int y, int x) => Y * x >= X * y;

            public readonly int X, Y;
        }

        private void Compute(byte octant, Point origin, byte rangeLimit, byte visibilityFalloff, int x, Slope top,
            Slope bottom)
        {
            // Throughout this function there are references to various parts of tiles. A tile's coordinates refer to its center,
            // and the following diagram shows the parts of the tile and the vectors from the origin that pass through those parts.
            //  n g  o      center:                    y / x
            // a------b   a top left:            (y*2+1) / (x*2-1)   i inner top left:       (y*4+1) / (x*4-1)
            //p|  /\  |q  b top right:           (y*2+1) / (x*2+1)   j inner top right:      (y*4+1) / (x*4+1)
            // |i/__\j|   c bottom left:         (y*2-1) / (x*2-1)   k inner bottom left:    (y*4-1) / (x*4-1)
            //e|/|  |\|f  d bottom right:        (y*2-1) / (x*2+1)   m inner bottom right:   (y*4-1) / (x*4+1)
            // |\|__|/|   e middle left:           (y*2) / (x*2-1)   n top left midpoint:    (y*4+2) / (x*4-1)
            //r|k\  /m|s  f middle right:          (y*2) / (x*2+1)   o top right midpoint:   (y*4+2) / (x*4+1)
            // |  \/  |   g top center:          (y*2+1) / (x*2)     p left upper midpoint:  (y*4+1) / (x*4-2)
            // c------d   h bottom center:       (y*2-1) / (x*2)     q right upper midpoint: (y*4+1) / (x*4+2)
            //  t h  u    r left lower midpoint: (y*4-1) / (x*4-2)   t bottom left midpoint: (y*4-2) / (x*4-1)
            //            s right lower midpoint:(y*4-1) / (x*4+2)   u bottom right midpoint:(y*4-2) / (x*4+1)
            for (; x <= rangeLimit; x++)
            {
                var rangeFalloff = x * visibilityFalloff;
                // Compute the Y coordinates of the top and bottom tiles for the x column of the sector.
                int topY;
                if (top.X == 1)
                {
                    // If top == ?/1 then it must be 1/1 because 0/1 < top <= 1/1. This is special-cased because top
                    // starts at 1/1 and remains 1/1 as long as it doesn't hit anything, so it's a common case
                    topY = x;
                }
                else
                {
                    // Get the tile that the top vector enters from the left. Since our coordinates refer to the center of the
                    // tile, this is (x-0.5)*top+0.5, which can be computed as (x-0.5)*top+0.5 = (2(x+0.5)*top+1)/2 =
                    // ((2x+1)*top+1)/2. Since top == a/b, this is ((2x+1)*a+b)/2b.
                    topY = ((x * 2 + 1) * top.Y + top.X) / (top.X * 2);
                }

                int bottomY;
                if (bottom.Y == 0)
                {
                    // If bottom == 0/?, then it's hitting the tile at Y=0 dead center. This is special-cased because
                    // bottom.Y starts at zero and remains zero as long as it doesn't hit anything, so it's common
                    bottomY = 0;
                }
                else
                {
                    // The tile that the bottom vector enters from the left
                    bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);
                }

                // Go through the tiles in the column now that we know which ones could possibly be visible
                var wasOpaque = -1; // 0:false, 1:true, -1:not applicable
                for (var y = topY; y >= bottomY; y--)
                {
                    // Calculate how much of the tile is visible. It can only be partially visible for the first
                    // and the last row in the sector. We can assume the tile is fully visible if the top slope is 1
                    // or the bottom is 0
                    var visibility = MaxVisibility;
                    if (y == topY && y != x)
                    {
                        var max = new Slope(y * 2 + 1, x * 2 - 1);
                        var min = new Slope(y * 2 - 1, x * 2 + 1);
                        // visible portion = (top - min) / (max - min)
                        visibility =
                            (byte)((MaxVisibility - MinVisibility) * (top.Y * min.X - top.X * min.Y) * max.X /
                                   ((max.Y * min.X - max.X * min.Y) * top.X) + MinVisibility);
                    }

                    if (y == bottomY && y != 0)
                    {
                        var max = new Slope(y * 2 + 1, x * 2 - 1);
                        var min = new Slope(y * 2 - 1, x * 2 + 1);
                        // hidden portion = (bottom - min) / (max - min)
                        visibility = (byte)(visibility -
                                            (MaxVisibility - MinVisibility) * (bottom.Y * min.X - bottom.X * min.Y) *
                                            max.X / ((max.Y * min.X - max.X * min.Y) * bottom.X) + MinVisibility);
                    }

                    var blocksLight = BlocksLight(x, y, octant, origin, visibility, rangeFalloff);

                    if (x == rangeLimit || rangeFalloff >= MaxVisibility)
                    {
                        continue;
                    }

                    if (blocksLight)
                    {
                        if (wasOpaque == 0)
                        {
                            // If we found a transition from clear to opaque, this sector is done in this column,
                            // so adjust the bottom vector upward and continue processing it in the next column
                            // if the opaque tile has a beveled top-left corner, move the bottom vector up to the top center.
                            // otherwise, move it up to the top left. the corner is beveled if the tiles above and to the left are
                            // clear.
                            // Top center by default
                            var newBottomY = y * 4 + 2;
                            var newBottomX = x * 4;
                            if (!TopLeftBeveled(x, y, octant, origin))
                            {
                                // Top left midpoint since the corner is not beveled to allow peaking through corner doors
                                newBottomX--;
                            }
                            if (top.Greater(newBottomY, newBottomX))
                            {
                                // We have to maintain the invariant that top > bottom, so the new sector
                                // created by adjusting the bottom is only valid if that's the case
                                // if we're at the bottom of the column, then just adjust the current sector rather than recursing
                                // since there's no chance that this sector can be split in two by a later transition back to clear
                                if (y == bottomY)
                                {
                                    bottom = new Slope(newBottomY, newBottomX);
                                    break;
                                }
                                Compute(octant, origin, rangeLimit, visibilityFalloff, x + 1, top,
                                    new Slope(newBottomY, newBottomX));
                            }
                            else
                            {
                                // The new bottom is greater than or equal to the top, so the new sector is empty and we'll ignore
                                // it. If we're at the bottom of the column, we'd normally adjust the current sector rather than
                                // recursing, so that invalidates the current sector and we're done
                                if (y == bottomY)
                                {
                                    return;
                                }
                            }
                        }
                        wasOpaque = 1;
                    }
                    else
                    {
                        if (wasOpaque == 1)
                        {
                            // If we found a transition from opaque to clear, adjust the top vector downwards. If the opaque tile
                            // has a beveled bottom-right corner, move the top vector down to the bottom center. Otherwise, move it
                            // down to the bottom right. The corner is beveled if the tiles below and to the right are clear.
                            // Bottom center of the tile above by default
                            var newTopY = y * 4 + 2;
                            var newTopX = x * 4;
                            var bottomRightBeveled = BottomRightBeveled(x, y + 1, octant, origin);
                            if (!bottomRightBeveled)
                            {
                                newTopX += 2; // Bottom right since the corner is not beveled
                            }
                            // We have to maintain the invariant that top > bottom. If not, the sector is empty and we're done.
                            if (bottom.GreaterOrEqual(newTopY, newTopX))
                            {
                                return;
                            }
                            top = new Slope(newTopY, newTopX);
                            if (bottomRightBeveled)
                            {
                                // Ensure the slope is under the bottom right corner of the next diagonal tile
                                // to avoid peaking around columns
                                var diagonalTopY = y*2+3;
                                var diagonalTopX = x*2+4;
                                if (top.Greater(diagonalTopY, diagonalTopX))
                                {
                                    top = new Slope(diagonalTopY, diagonalTopX);
                                }
                            }
                        }
                        wasOpaque = 0;
                    }
                }

                // If the column didn't end in a clear tile, then there's no reason to continue processing the current sector
                // because that means either 1) wasOpaque == -1, implying that the sector is empty or at its range limit, or
                // 2) wasOpaque == 1, implying that we found a transition from clear to opaque and we recursed and we never found
                // a transition back to clear, so there's nothing else for us to do that the recursive method hasn't already.
                // (if we didn't recurse (because y == bottomY), it would have executed a break, leaving wasOpaque equal to 0.)
                if (wasOpaque != 0)
                {
                    break;
                }
            }
        }

        // Switch statements repeated below for perf
        private bool BlocksLight(int x, int y, byte octant, Point origin, byte visibility, int rangeFalloff)
        {
            int nx = origin.X, ny = origin.Y;
            switch (octant)
            {
                case 0:
                    nx += x;
                    ny -= y;
                    break;
                case 1:
                    nx += y;
                    ny -= x;
                    break;
                case 2:
                    nx -= y;
                    ny -= x;
                    break;
                case 3:
                    nx -= x;
                    ny -= y;
                    break;
                case 4:
                    nx -= x;
                    ny += y;
                    break;
                case 5:
                    nx -= y;
                    ny += x;
                    break;
                case 6:
                    nx += y;
                    ny += x;
                    break;
                case 7:
                    nx += x;
                    ny += y;
                    break;
            }

            return _blocksLight((byte)nx, (byte)ny, visibility, rangeFalloff);
        }

        private bool TopLeftBeveled(int x, int y, byte octant, Point origin)
        {
            int nx = origin.X, ny = origin.Y;
            var directionsToCheck = DirectionFlags.NorthAndWest;
            switch (octant)
            {
                case 0:
                    nx += x;
                    ny -= y;
                    break;
                case 1:
                    nx += y;
                    ny -= x;
                    directionsToCheck = DirectionFlags.SouthAndEast;
                    break;
                case 2:
                    nx -= y;
                    ny -= x;
                    directionsToCheck = DirectionFlags.SouthAndWest;
                    break;
                case 3:
                    nx -= x;
                    ny -= y;
                    directionsToCheck = DirectionFlags.NorthAndEast;
                    break;
                case 4:
                    nx -= x;
                    ny += y;
                    directionsToCheck = DirectionFlags.SouthAndEast;
                    break;
                case 5:
                    nx -= y;
                    ny += x;
                    break;
                case 6:
                    nx += y;
                    ny += x;
                    directionsToCheck = DirectionFlags.NorthAndEast;
                    break;
                case 7:
                    nx += x;
                    ny += y;
                    directionsToCheck = DirectionFlags.SouthAndWest;
                    break;
            }

            return (_getUnconnectedNeighbours((byte)nx, (byte)ny) & directionsToCheck) == directionsToCheck;
        }

        private bool BottomRightBeveled(int x, int y, byte octant, Point origin)
        {
            int nx = origin.X, ny = origin.Y;
            var directionsToCheck = DirectionFlags.SouthAndEast;
            switch (octant)
            {
                case 0:
                    nx += x;
                    ny -= y;
                    break;
                case 1:
                    nx += y;
                    ny -= x;
                    directionsToCheck = DirectionFlags.NorthAndWest;
                    break;
                case 2:
                    nx -= y;
                    ny -= x;
                    directionsToCheck = DirectionFlags.NorthAndEast;
                    break;
                case 3:
                    nx -= x;
                    ny -= y;
                    directionsToCheck = DirectionFlags.SouthAndWest;
                    break;
                case 4:
                    nx -= x;
                    ny += y;
                    directionsToCheck = DirectionFlags.NorthAndWest;
                    break;
                case 5:
                    nx -= y;
                    ny += x;
                    break;
                case 6:
                    nx += y;
                    ny += x;
                    directionsToCheck = DirectionFlags.SouthAndWest;
                    break;
                case 7:
                    nx += x;
                    ny += y;
                    directionsToCheck = DirectionFlags.NorthAndEast;
                    break;
            }

            return (_getUnconnectedNeighbours((byte)nx, (byte)ny) & directionsToCheck) == directionsToCheck;
        }
    }
}