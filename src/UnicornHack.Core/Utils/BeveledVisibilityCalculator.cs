using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     Based on the algorithm by Adam Milazzo
    ///     http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#mycode
    ///     The approach used is octant-based precise recursive shadowcasting with beveled corners
    /// </summary>
    public class BeveledVisibilityCalculator
    {
        private readonly Func<byte, byte, DirectionFlags> _getUnconnectedNeighbours;
        private readonly LevelComponent _level;
        private const byte MinVisibility = 0;
        public const byte MaxVisibility = 254;
        private const byte NormalizedMaxVisibility = MaxVisibility - MinVisibility;

        /// <summary>
        ///     Creates a new instance of <see cref="BeveledVisibilityCalculator" />
        /// </summary>
        /// <param name="getUnconnectedNeighbours">
        ///     A function that returns which neighbours the opaque tile at the given X and Y coordinates is not connected to.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <remarks>
        ///     Functions take coordinates instead of <see cref="Point" /> for perf
        /// </remarks>
        public BeveledVisibilityCalculator(Func<byte, byte, DirectionFlags> getUnconnectedNeighbours,
            LevelComponent level)
        {
            _getUnconnectedNeighbours = getUnconnectedNeighbours;
            _level = level;
        }

        /// <summary>
        ///     Calculates the cells visible from the given point in the given range
        /// </summary>
        /// <param name="origin"> The location of the observer </param>
        /// <param name="range"> The maximum visibility range </param>
        /// <param name="noFalloff"> Disables visibility falloff if set to <c>true</c> </param>
        /// <param name="blocksVisibility">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <param name="visibleTerrain"> Array that will contain the terrain visibility </param>
        public void ComputeOmnidirectional(
            Point origin,
            byte range,
            bool noFalloff,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility,
            byte[] visibleTerrain)
        {
            var (_, index) = blocksVisibility(origin.X, origin.Y, _level);
            var linearFalloff = noFalloff
                ? (byte)0
                : (byte)(MaxVisibility / range);
            visibleTerrain[index] = MaxVisibility;
            for (var octant = 0; octant < 8; octant++)
            {
                Compute(octant, origin, range, linearFalloff,
                    marginalCellsToDouble: DirectionFlags.None,
                    x: 1, top: new Slope(1, 1), bottom: new Slope(0, 1),
                    blocksVisibility, visibleTerrain,
                    visibleTerrainList: null);
            }
        }

        /// <summary>
        ///     Calculates the tiles visible from the given point in the given FOV
        /// </summary>
        /// <param name="origin"> The location of the observer </param>
        /// <param name="heading"> Direction of the observer, the bisector of the FOV </param>
        /// <param name="primaryFOVQuadrants"> The number of quadrants for the primary FOV </param>
        /// <param name="primaryRange"> The maximum visibility range for the primary FOV </param>
        /// <param name="totalFOVQuadrants"> The number of quadrants for the secondary FOV and the primary FOV </param>
        /// <param name="secondaryRange"> The maximum visibility range for the secondary FOV  </param>
        /// <param name="noFalloff"> Disables visibility falloff if set to <c>true</c> </param>
        /// <param name="blocksVisibility">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <param name="visibleTerrain"> Array that will contain the terrain visibility. </param>
        public void ComputeDirected(
            Point origin, Direction heading,
            int primaryFOVQuadrants, int primaryRange,
            int totalFOVQuadrants, int secondaryRange,
            bool noFalloff,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility,
            byte[] visibleTerrain)
        {
            if (primaryFOVQuadrants > totalFOVQuadrants)
            {
                throw new ArgumentOutOfRangeException(nameof(primaryFOVQuadrants));
            }

            if (totalFOVQuadrants > primaryFOVQuadrants
                && secondaryRange <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(secondaryRange));
            }

            if (totalFOVQuadrants > 0
                && primaryRange <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(primaryRange));
            }

            var (_, index) = blocksVisibility(origin.X, origin.Y, _level);
            visibleTerrain[index] = MaxVisibility;
            var octantShift = Direction.East.OctantsTo(heading);
            if (octantShift == -1)
            {
                return;
            }

            octantShift = 8 + octantShift - totalFOVQuadrants;
            var linearFalloff = noFalloff || secondaryRange == 0
                ? (byte)0
                : (byte)(MaxVisibility / secondaryRange);
            for (var octant = 0; octant < totalFOVQuadrants - primaryFOVQuadrants; octant++)
            {
                Compute((octant + octantShift) % 8, origin, secondaryRange, linearFalloff,
                    marginalCellsToDouble: DirectionFlags.None,
                    x: 1,
                    top: new Slope(1, 1),
                    bottom: new Slope(0, 1),
                    blocksVisibility,
                    visibleTerrain,
                    visibleTerrainList: null);
            }

            for (var octant = totalFOVQuadrants + primaryFOVQuadrants; octant < totalFOVQuadrants * 2; octant++)
            {
                Compute((octant + octantShift) % 8, origin, secondaryRange, linearFalloff,
                    marginalCellsToDouble: DirectionFlags.None,
                    x: 1,
                    top: new Slope(1, 1),
                    bottom: new Slope(0, 1),
                    blocksVisibility,
                    visibleTerrain,
                    visibleTerrainList: null);
            }

            linearFalloff = noFalloff || primaryRange == 0
                ? (byte)0
                : (byte)(MaxVisibility / primaryRange);
            for (var octant = totalFOVQuadrants - primaryFOVQuadrants;
                octant < totalFOVQuadrants + primaryFOVQuadrants;
                octant++)
            {
                Compute((octant + octantShift) % 8, origin, primaryRange, linearFalloff,
                    marginalCellsToDouble: DirectionFlags.None,
                    x: 1,
                    top: new Slope(1, 1),
                    bottom: new Slope(0, 1),
                    blocksVisibility,
                    visibleTerrain,
                    visibleTerrainList: null);
            }
        }

        /// <summary>
        ///     Calculates the visibility of tiles between the given origin and target points
        /// </summary>
        /// <param name="origin"> The location of the observer </param>
        /// <param name="target"> The location of the target </param>
        /// <param name="blocksVisibility">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <param name="manager"> The game manager </param>
        /// <returns> Array that will contain the terrain visibility. </returns>
        public List<(Point, byte)> ComputeLOS(
            Point origin, Point target,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility,
            GameManager manager)
        {
            var targetVector = origin.DifferenceTo(target);
            return ComputeLOS(origin, targetVector, targetVector.Length(), blocksVisibility, manager);
        }

        /// <summary>
        ///     Calculates the visibility of tiles between the given origin and target points
        /// </summary>
        /// <param name="origin"> The location of the observer </param>
        /// <param name="targetVector"> The vector towards the target </param>
        /// <param name="range"> The range </param>
        /// <param name="blocksVisibility">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <param name="manager"> The game manager </param>
        /// <returns> Array that will contain the terrain visibility. </returns>
        public List<(Point, byte)> ComputeLOS(
            Point origin,
            Vector targetVector,
            int range,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility,
            GameManager manager)
        {
            var targetOctant = targetVector.GetOctant();
            var octantTarget = targetVector.ToOctant(targetOctant);
            var visibleTerrainList = manager.IntByteListArrayPool.Rent();

            if (targetVector.IsCardinalAligned())
            {
                Compute(targetOctant, origin,
                    range,
                    linearFalloff: 0,
                    marginalCellsToDouble: DirectionFlags.DiagonalCross,
                    x: 1,
                    top: new Slope((octantTarget.Y << 1) + 1, (octantTarget.X << 1) - 1), // Top left corner
                    bottom: new Slope(0, 1),
                    blocksVisibility,
                    visibleTerrain: null,
                    visibleTerrainList);

                var adjacentOctant = GetCardinallyAdjacentOctant(targetOctant);
                var adjacentTarget = targetVector.ToOctant(adjacentOctant);
                var adjacentVisibleTerrainList = manager.IntByteListArrayPool.Rent();

                Compute(adjacentOctant, origin,
                    range,
                    linearFalloff: 0,
                    marginalCellsToDouble: DirectionFlags.DiagonalCross,
                    x: 1,
                    top: new Slope((adjacentTarget.Y << 1) + 1, (adjacentTarget.X << 1) - 1), // Top left corner,
                    bottom: new Slope(0, 1),
                    blocksVisibility,
                    visibleTerrain: null,
                    adjacentVisibleTerrainList);

                return CombineAdjacentOctants(visibleTerrainList, adjacentVisibleTerrainList, origin, onDiagonal: false);
            }
            else if (targetVector.IsIntercardinalAligned())
            {
                Compute(targetOctant, origin,
                    range,
                    linearFalloff: 0,
                    marginalCellsToDouble: DirectionFlags.Cross,
                    x: 1,
                    top: new Slope(1, 1),
                    bottom: new Slope((octantTarget.Y << 1) - 1, (octantTarget.X << 1) + 1), // Bottom right corner
                    blocksVisibility,
                    visibleTerrain: null,
                    visibleTerrainList);

                var adjacentOctant = GetDiagonallyAdjacentOctant(targetOctant);
                var adjacentTarget = targetVector.ToOctant(adjacentOctant);
                var adjacentVisibleTerrainList = manager.IntByteListArrayPool.Rent();

                Compute(adjacentOctant, origin,
                    range,
                    linearFalloff: 0,
                    marginalCellsToDouble: DirectionFlags.Cross,
                    x: 1,
                    top: new Slope(1, 1),
                    bottom: new Slope((adjacentTarget.Y << 1) - 1, (adjacentTarget.X << 1) + 1), // Bottom right corner
                    blocksVisibility,
                    visibleTerrain: null,
                    adjacentVisibleTerrainList);

                return CombineAdjacentOctants(visibleTerrainList, adjacentVisibleTerrainList, origin, onDiagonal: true);
            }

            Compute(targetOctant, origin,
                range,
                linearFalloff: 0,
                marginalCellsToDouble: DirectionFlags.All,
                x: 1,
                top: new Slope((octantTarget.Y << 1) + 1, (octantTarget.X << 1) - 1), // Top left corner
                bottom: new Slope((octantTarget.Y << 1) - 1, (octantTarget.X << 1) + 1), // Bottom right corner
                blocksVisibility,
                visibleTerrain: null,
                visibleTerrainList);

            var visibleCells = manager.PointByteListArrayPool.Rent();
            if (visibleCells.Capacity < visibleTerrainList.Count)
            {
                visibleCells.Capacity = visibleTerrainList.Count;
            }
            for (var i = 0; i < visibleTerrainList.Count; i++)
            {
                var (index, visibility) = visibleTerrainList[i];
                visibleCells.Add((_level.IndexToPoint[index], visibility));
            }

            visibleTerrainList.Clear();
            manager.IntByteListArrayPool.Return(visibleTerrainList);

            return visibleCells;
        }

        private List<(Point, byte)> CombineAdjacentOctants(
            List<(int, byte)> visibleTerrainList,
            List<(int, byte)> adjacentVisibleTerrainList,
            Point origin,
            bool onDiagonal)
        {
            var minLength = visibleTerrainList.Count;
            var maxLength = visibleTerrainList.Count;
            if (minLength > adjacentVisibleTerrainList.Count)
            {
                minLength = adjacentVisibleTerrainList.Count;
            }
            else
            {
                maxLength = adjacentVisibleTerrainList.Count;
            }

            var newLength = maxLength + minLength - (minLength >> 1);
            var manager = _level.Entity.Manager;
            var visibleTerrainCells = manager.PointByteListArrayPool.Rent();
            if (visibleTerrainCells.Capacity < newLength)
            {
                visibleTerrainCells.Capacity = newLength;
            }

            var m = 0;
            var n = 0;
            var firstDistance = 0;
            var secondDistance = 0;
            while (true)
            {
                var (firstIndex, firstVisibility) = m == visibleTerrainList.Count ? (-1, (byte)0) : visibleTerrainList[m];
                var (secondIndex, secondVisibility) = n == adjacentVisibleTerrainList.Count ? (-1, (byte)0) : adjacentVisibleTerrainList[n];
                if (firstIndex == secondIndex)
                {
                    if (firstIndex == -1
                        && secondIndex == -1)
                    {
                        break;
                    }

                    // Combine diagonal tiles
                    visibleTerrainCells.Add((_level.IndexToPoint[firstIndex], (byte)(firstVisibility + secondVisibility)));
                    m++;
                    n++;
                    firstDistance++;
                    secondDistance++;
                    continue;
                }

                Point firstPoint = default;
                Vector firstVector = default;
                Point secondPoint = default;
                Vector secondVector = default;

                if (firstIndex != -1)
                {
                    firstPoint = _level.IndexToPoint[firstIndex];
                    firstVector = origin.DifferenceTo(firstPoint);
                    firstDistance = firstVector.Length();
                }

                if (secondIndex != -1)
                {
                    secondPoint = _level.IndexToPoint[secondIndex];
                    secondVector = origin.DifferenceTo(secondPoint);
                    secondDistance = secondVector.Length();
                }

                if (firstIndex != -1)
                {
                    if ((!((onDiagonal && firstVector.IsIntercardinalAligned()) || (!onDiagonal && firstVector.IsCardinalAligned()))
                            && firstDistance <= secondDistance)
                        || secondIndex == -1)
                    {
                        visibleTerrainCells.Add((firstPoint, firstVisibility));
                        m++;
                    }
                }

                if (secondIndex != -1)
                {
                    if ((!(onDiagonal && secondVector.IsIntercardinalAligned() || (!onDiagonal && secondVector.IsCardinalAligned()))
                            && secondDistance <= firstDistance)
                        || firstIndex == -1)
                    {
                        visibleTerrainCells.Add((secondPoint, secondVisibility));
                        n++;
                    }
                }
            }

            visibleTerrainList.Clear();
            manager.IntByteListArrayPool.Return(visibleTerrainList);
            adjacentVisibleTerrainList.Clear();
            manager.IntByteListArrayPool.Return(adjacentVisibleTerrainList);

            return visibleTerrainCells;
        }

        /// <summary>
        ///     Calculate each cell visibility within the given sector in a given octant.
        /// </summary>
        /// <param name="octant"> The octant. 0-7 </param>
        /// <param name="origin"> The location of the observer. Absolute coordinates. </param>
        /// <param name="range"> The maximum visibility range. </param>
        /// <param name="linearFalloff"> How much the visibility drops off for each unblocked cell. 0-254 </param>
        /// <param name="marginalCellsToDouble">
        ///     If adjacent octants won't be computed this value makes the specified marginal cells to have the visibility
        ///     doubled to compensate when a wall is enountered.
        /// </param>
        /// <param name="x"> The relative column to start (or continue) the computation. </param>
        /// <param name="top"> The top limit of the visibility sector. 0/1 - 1/1 </param>
        /// <param name="bottom"> The bottom limit of the visibility sector. 0/1 - 1/1 </param>
        /// <param name="blocksVisibility">
        ///     A function that determines whether the tile at the given X and Y coordinates blocks the passage of light.
        ///     The function also returns the index of the tile if it's within valid range.
        /// </param>
        /// <param name="visibleTerrain"> The output array that will have all visible cells values populated. </param>
        /// <param name="visibleTerrainList"> The output list the will contain the visible cells ordered by distance. </param>
        private void Compute(
            int octant, Point origin,
            int range, byte linearFalloff, DirectionFlags marginalCellsToDouble,
            int x, Slope top, Slope bottom,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility,
            byte[] visibleTerrain,
            List<(int, byte)> visibleTerrainList)
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

            Span<(float NextTopIntersectionY, float NextBottomIntersectionY, Slope Top, Slope Bottom)> sectors =
                stackalloc (float, float, Slope, Slope)[1];

            sectors[0].Top = top;
            sectors[0].Bottom = bottom;
            if (top.X == 1)
            {
                sectors[0].NextTopIntersectionY = x - 0.5f;
            }
            else
            {
                sectors[0].NextTopIntersectionY = ((x << 1) - 1) * top.Y / (float)(top.X << 1);
            }

            if (bottom.Y == 0)
            {
                sectors[0].NextBottomIntersectionY = 0;
            }
            else
            {
                sectors[0].NextBottomIntersectionY = ((x << 1) - 1) * bottom.Y / (float)(bottom.X << 1);
            }

            for (; x <= range; x++)
            {
                var rangeFalloff = linearFalloff * x;
                if (NormalizedMaxVisibility <= rangeFalloff)
                {
                    return;
                }

                for (var sectorIndex = 0; sectorIndex < sectors.Length; sectorIndex++)
                {
                    top = sectors[sectorIndex].Top;
                    if (top.Y == 0)
                    {
                        // If top == 0/? this isn't a valid sector
                        continue;
                    }

                    // Compute the Y coordinate of the top tile for the current column of the sector,
                    // this is the same Y as the tile from the next column where the top vector enters from the left.
                    // If top == ?/1 then it must be 1/1 because 0/1 < top <= 1/1. This is special-cased because top
                    // starts at 1/1 and remains 1/1 as long as it doesn't hit anything, so it's common
                    var topIntersectionY = sectors[sectorIndex].NextTopIntersectionY;
                    int topY;
                    if (top.X == 1)
                    {
                        sectors[sectorIndex].NextTopIntersectionY = x + 0.5f;
                        topY = x;
                    }
                    else
                    {
                        // Since our coordinates refer to the center of the
                        // tile, this is (x-0.5)*top, which can be computed as (x-0.5)*top = (2(x-0.5)*top)/2 =
                        // ((2x-1)*top)/2. Since top == a/b, this is ((2x-1)*a)/2b.
                        sectors[sectorIndex].NextTopIntersectionY = ((x << 1) + 1) * top.Y / (float)(top.X << 1);
                        // Since decimal part is truncated we need to add 0.5 to emulate rounding
                        var topYFloat = sectors[sectorIndex].NextTopIntersectionY + 0.5f;
                        topY = (int)topYFloat;
                        if (topY == topYFloat)
                        {
                            // Top slope intersects exactly in the corner
                            topY--;
                        }
                    }

                    bottom = sectors[sectorIndex].Bottom;
                    // Compute the Y coordinate of the bottom tile for the first column of the sector,
                    // where the bottom vectors enters from the left.
                    // If bottom == 0/?, then it's hitting the tile at Y=0 dead center. This is special-cased because
                    // bottom.Y starts at 0 and remains 0 as long as it doesn't hit anything, so it's common
                    var bottomIntersectionY = sectors[sectorIndex].NextBottomIntersectionY;
                    int bottomY;
                    if (bottom.Y == 0)
                    {
                        sectors[sectorIndex].NextBottomIntersectionY = 0;
                        bottomY = 0;
                    }
                    else
                    {
                        sectors[sectorIndex].NextBottomIntersectionY = ((x << 1) + 1) * bottom.Y / (float)(bottom.X << 1);
                        // Since decimal part is truncated we need to add 0.5 to emulate rounding
                        bottomY = (int)(bottomIntersectionY + 0.5f);
                    }

                    // Go through the tiles in the current column of the choosen sector
                    var wasOpaque = -1; // 0:false, 1:true, -1:not applicable
                    for (var y = bottomY; y <= topY; y++)
                    {
                        var (lightBlocked, index) = BlocksVisibility(x, y, octant, origin, blocksVisibility);
                        if (index == -1)
                        {
                            continue;
                        }

                        if (lightBlocked)
                        {
                            // There's something completely blocking the light, so we treat it as fully visible
                            var visibility = (byte)(MaxVisibility - rangeFalloff);
                            if (((marginalCellsToDouble & DirectionFlags.East) == 0 && y == 0)
                                || (marginalCellsToDouble & DirectionFlags.Northeast) == 0 && x == y)
                            {
                                visibility >>= 1;
                            }

                            if (wasOpaque == 1
                                && top.Less(new Slope((y << 1) - 1, (x << 1) - 1)))
                            {
                                // The tile below blocks visibility completely
                                // But don't block visibility for the corner tiles when seen diagonally
                                visibility = 0;
                            }

                            if (visibleTerrain != null)
                            {
                                visibleTerrain[index] += visibility;
                            }
                            else
                            {
                                visibleTerrainList.Add((index, visibility));
                            }

                            if (wasOpaque == 0)
                            {
                                // If we found a transition from clear to opaque, this sector is done in this column,
                                // so adjust the top vector downwards. If the opaque tile
                                // has a beveled bottom-right corner, move the top vector down to the bottom center. Otherwise, move it
                                // down to the bottom right midpoint. The corner is beveled if the tiles below and to the right are clear.
                                var newTopY = (y << 2) - 2;
                                var newTopX = x << 2;
                                if (!BottomRightBeveled(x, y, octant, origin))
                                {
                                    // Bottom right midpoint since the corner is not beveled
                                    newTopX++;
                                }

                                var newTop = new Slope(newTopY, newTopX);
                                if (top.Less(newTop))
                                {
                                    newTop = top;
                                }

                                if (newTop.Greater(bottom))
                                {
                                    // We have to maintain the invariant that top > bottom, so the new sector
                                    // created by adjusting the top is only valid if that's the case.
                                    if (y == topY)
                                    {
                                        // We're at the top of the column, then just adjust the current sector
                                        // since there's no chance that this sector can be split in two by a later transition back to clear
                                        sectors[sectorIndex].Top = newTop;
                                        sectors[sectorIndex].NextTopIntersectionY = ((x << 1) + 1) * newTop.Y / (float)(newTop.X << 1);
                                        break;
                                    }

                                    if (sectorIndex > 0
                                        && sectors[sectorIndex - 1].Top.Y == 0)
                                    {
                                        // Previous sector is empty, so reuse it
                                        sectorIndex--;
                                    }
                                    else if (sectorIndex + 1 < sectors.Length
                                        && sectors[sectorIndex + 1].Top.Y == 0)
                                    {
                                        // Next sector is empty, so reuse it
                                        sectors[sectorIndex + 1] = sectors[sectorIndex];
                                    }
                                    else
                                    {
                                        Span<(float NextTopIntersectionY, float NextBottomIntersectionY, Slope Top, Slope Bottom)> newSectors =
#pragma warning disable CA2014 // Do not use stackalloc in loops
                                            stackalloc (float, float, Slope, Slope)[sectors.Length + 1];
#pragma warning restore CA2014 // Do not use stackalloc in loops

                                        var j = 0;
                                        for (var i = 0; i < sectors.Length; i++)
                                        {
                                            if (i == sectorIndex)
                                            {
                                                j++;
                                            }

                                            newSectors[j++] = sectors[i];
                                        }

                                        sectors = newSectors;
                                    }

                                    sectors[sectorIndex].Top = newTop;
                                    sectors[sectorIndex].Bottom = bottom;
                                    sectors[sectorIndex].NextTopIntersectionY = newTop.X == 1
                                        ? x + 0.5f
                                        : ((x << 1) + 1) * newTop.Y / (float)(newTop.X << 1);
                                    sectors[sectorIndex].NextBottomIntersectionY = bottom.Y == 0
                                        ? 0
                                        : ((x << 1) + 1) * bottom.Y / (float)(bottom.X << 1);

                                    sectorIndex++;
                                }
                                else
                                {
                                    // The bottom is greater than or equal to the new top, so the new sector is empty and we'll ignore
                                    // it. If we're at the top of the column, we'd normally adjust the current sector rather than
                                    // recursing, so that invalidates the current sector.
                                    if (y == topY)
                                    {
                                        sectors[sectorIndex].Top = new Slope(0, 1);
                                        break;
                                    }
                                }
                            }

                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque == 1)
                            {
                                // We found a transition from opaque to clear, so adjust the bottom vector upward.
                                // If the opaque tile has a beveled top-left corner, move the bottom vector up to the bottom center.
                                // Otherwise, move it up to the bottom left midpoint.
                                // The corner is beveled if the tiles above and to the left are clear.
                                var newBottomY = (y << 2) - 2;
                                var newBottomX = x << 2;
                                if (!TopLeftBeveled(x, y - 1, octant, origin))
                                {
                                    // Bottom left midpoint since the corner is not beveled to allow peaking through corner doors
                                    newBottomX--;
                                }

                                var newBottom = new Slope(newBottomY, newBottomX);
                                if (bottom.Less(newBottom))
                                {
                                    bottom = newBottom;
                                    sectors[sectorIndex].Bottom = newBottom;

                                    // We have to maintain the invariant that top > bottom. If not, the sector is empty and we're done.
                                    if (top.LessOrEqual(bottom))
                                    {
                                        sectors[sectorIndex].Top = new Slope(0, 1);
                                        break;
                                    }

                                    bottomIntersectionY = (x << 1) * bottom.Y / (float)(bottom.X << 1);
                                    sectors[sectorIndex].NextBottomIntersectionY = ((x << 1) + 1) * bottom.Y / (float)(bottom.X << 1);
                                    bottomY = (int)(bottomIntersectionY + 0.5f);
                                }
                            }

                            wasOpaque = 0;

                            // Calculate how much of the tile is visible. It can only be partially visible for
                            // the first two and the last two rows in the sector.
                            var adjustedMaxVisibility = (byte)(NormalizedMaxVisibility - rangeFalloff);
                            var visibility = adjustedMaxVisibility;

                            if (y == bottomY)
                            {
                                float visibleArea;
                                if (bottom.Y == 0)
                                {
                                    // Bottom vector is horizontal

                                    visibleArea = 0.5f;
                                }
                                else if (bottomY != (int)(sectors[sectorIndex].NextBottomIntersectionY + 0.5f))
                                {
                                    // Bottom vector intersects bottom and right side

                                    var bottomSideIntersectionY = y + 0.5f;
                                    var bottomSideIntersectionX = bottomSideIntersectionY * bottom.X / bottom.Y;

                                    // The length of the left side that is above the bottom slope
                                    var lT = bottomSideIntersectionY - bottomIntersectionY;

                                    // The length of the top side that is to the left of the bottom slope
                                    var tL = bottomSideIntersectionX - x + 0.5f;

                                    visibleArea = lT * tL * 0.5f;
                                }
                                else
                                {
                                    // Bottom vector intersects left and right side

                                    var topSideY = y + 0.5f;

                                    // The length of the left side that is above the bottom slope
                                    var lT = topSideY - bottomIntersectionY;

                                    // The length of the right side that is above the bottom slope
                                    var rT = topSideY - sectors[sectorIndex].NextBottomIntersectionY;

                                    visibleArea = (lT + rT) * 0.5f;
                                }

                                visibility = (byte)(visibility * visibleArea);
                            }
                            else if (y == bottomY + 1 && bottomY != (int)(sectors[sectorIndex].NextBottomIntersectionY + 0.5f))
                            {
                                var bottomSideIntersectionY = y - 0.5f;
                                var bottomSideIntersectionX = bottomSideIntersectionY * bottom.X / bottom.Y;

                                // The length of the right side that is below the bottom slope
                                var rB = sectors[sectorIndex].NextBottomIntersectionY - bottomSideIntersectionY;

                                // The length of the bottom side that is to the right of the bottom slope
                                var bR = x + 0.5f - bottomSideIntersectionX;
                                var blockedArea = rB * bR * 0.5f;

                                visibility = (byte)(visibility - adjustedMaxVisibility * blockedArea);
                            }

                            if (y == topY - 1 && topY != (int)(topIntersectionY + 0.5f))
                            {
                                var bottomSideIntersectionY = y + 0.5f;
                                var bottomSideIntersectionX = bottomSideIntersectionY * top.X / top.Y;

                                // The length of the left side that is above the top slope
                                var lT = bottomSideIntersectionY - topIntersectionY;

                                // The length of the top side that is to the left of the top slope
                                var tL = bottomSideIntersectionX - x + 0.5f;
                                var blockedArea = lT * tL * 0.5f;

                                visibility = (byte)(visibility - adjustedMaxVisibility * blockedArea);
                            }
                            else if (y == topY)
                            {
                                float visibleArea;
                                if (top.X == 1)
                                {
                                    // Top vector is diagonal

                                    visibleArea = 0.5f;
                                }
                                else if (topY != (int)(topIntersectionY + 0.5f))
                                {
                                    // Top vector intersects bottom and right side

                                    var bottomSideIntersectionY = y - 0.5f;
                                    var bottomSideIntersectionX = bottomSideIntersectionY * top.X / top.Y;

                                    // The length of the right side that is below the top slope
                                    var rB = sectors[sectorIndex].NextTopIntersectionY - bottomSideIntersectionY;

                                    // The length of the bottom side that is to the right of the top slope
                                    var bR = x + 0.5f - bottomSideIntersectionX;
                                    visibleArea = rB * bR * 0.5f;
                                }
                                else
                                {
                                    // Top vector intersects left and right side

                                    var bottomSideY = y - 0.5f;

                                    // The length of the left side that is below the top slope
                                    var lB = topIntersectionY - bottomSideY;

                                    // The length of the right side that is below the top slope
                                    var rB = sectors[sectorIndex].NextTopIntersectionY - bottomSideY;
                                    visibleArea = (lB + rB) * 0.5f;
                                }

                                visibility = (byte)(visibility -
                                                    (adjustedMaxVisibility - (byte)(adjustedMaxVisibility * visibleArea)));
                            }

                            visibility += MinVisibility;

                            if (visibleTerrain != null)
                            {
                                visibleTerrain[index] += visibility;
                            }
                            else
                            {
                                visibleTerrainList.Add((index, visibility));
                            }
                        }
                    }

                    // If the column didn't end in a clear tile, then there's no reason to continue processing the current sector
                    // because that means either 1) wasOpaque == -1, implying that the sector is empty or at its range limit, or
                    // 2) wasOpaque == 1, implying that we found a transition from clear to opaque and added a new sector, but never found
                    // a transition back to clear, so there's nothing else for us to do here.
                    // (if we didn't add a new sector (because y == bottomY), it would have exited the loop, leaving wasOpaque equal to 0.)
                    if (wasOpaque != 0)
                    {
                        sectors[sectorIndex].Top = new Slope(0, 1);
                        continue;
                    }
                }
            }
        }

        // Switch statements repeated below for perf
        private (bool, int) BlocksVisibility(int x, int y, int octant, Point origin,
            Func<byte, byte, LevelComponent, (bool, int)> blocksVisibility)
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

            return blocksVisibility((byte)nx, (byte)ny, _level);
        }

        private bool TopLeftBeveled(int x, int y, int octant, Point origin)
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

        private bool BottomRightBeveled(int x, int y, int octant, Point origin)
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

        private int GetCardinallyAdjacentOctant(int octant)
        {
            switch (octant)
            {
                case 0:
                    return 7;
                case 1:
                    return 2;
                case 2:
                    return 1;
                case 3:
                    return 4;
                case 4:
                    return 3;
                case 5:
                    return 6;
                case 6:
                    return 5;
                case 7:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetDiagonallyAdjacentOctant(int octant)
        {
            switch (octant)
            {
                case 0:
                    return 1;
                case 1:
                    return 0;
                case 2:
                    return 3;
                case 3:
                    return 2;
                case 4:
                    return 5;
                case 5:
                    return 4;
                case 6:
                    return 7;
                case 7:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerDisplay("{Y}/{X}")]
        private struct Slope // represents the slope Y/X as a rational number
        {
            public readonly int X, Y;

            public Slope(int y, int x)
            {
                Y = y;
                X = x;
            }

            public bool Greater(Slope other) => Y * other.X > X * other.Y;

            public bool GreaterOrEqual(Slope other) => Y * other.X >= X * other.Y;

            public bool Less(Slope other) => Y * other.X < X * other.Y;

            public bool LessOrEqual(Slope other) => Y * other.X <= X * other.Y;

            public int CompareTo(Slope other) => Y * other.X - X * other.Y;
        }
    }
}
