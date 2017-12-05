        public static IPolygon CreateLargestRectangleSeatingBoundary2(IPolygon drwBdryPoly,
            IEnumerable<IPolygon> nonSeatingAreas, int? cnt = null)
        {
            IPolygon largestOne = new Polygon();
            IBoundingBox drwBndrRect = drwBdryPoly.GetBoundingRectangle();
            const double tolerance = 1; // .01;
            long areaMatrixWidth = (long)Math.Ceiling(drwBndrRect.Width / tolerance);
            long areaMatrixLength = (long)Math.Ceiling(drwBndrRect.Length / tolerance);

            var p = new long[areaMatrixLength, areaMatrixWidth]; 

            IEnumerable<IBoundingBox> obstaclesRects = nonSeatingAreas.Select(nsArea => nsArea.GetBoundingRectangle())
                .OrderBy(rect => rect.TopLeft.X)
                .ToList();

            int obstaclesCount = obstaclesRects.Count();

            Point[] obstacleTopLefts = obstaclesRects.Select(bb => bb.TopLeft).ToArray();
            Point[] obstacleTopRights = obstaclesRects.Select(bb => bb.TopRight).ToArray();
            Point[] obstacleBottomLefts = obstaclesRects.Select(bb => bb.BottomLeft).ToArray();

            double topLeftX = drwBndrRect.TopLeft.X;
            double topLeftY = drwBndrRect.TopLeft.Y;

            long maxRectTopLeftIndexW = 0;
            long maxRectTopLeftIndexL = 0;
            long maxRectArea = 0;
            double maxRectWidth = 0;
            double maxRectLenght = 0;

            for (int rowIndex = 0; rowIndex < areaMatrixLength; rowIndex++)
            {
                //p[rowIndex] = new long[areaMatrixWidth];

                for (int colIndex = 0; colIndex < areaMatrixWidth; colIndex++)
                {
                    p[rowIndex, colIndex] = (rowIndex > 0 ? p[rowIndex - 1, colIndex] : 0) + 1;

                    for (int k = 0; k < obstaclesCount; k++)
                    {
                        if (obstacleTopLefts[k].X > topLeftX + tolerance)
                            break;

                        if (obstacleTopRights[k].X < topLeftX)
                            continue;

                        if (
                            !(
                                obstacleTopLefts[k].Y > topLeftY + tolerance
                                ||
                                obstacleBottomLefts[k].Y < topLeftY
                            )
                        )
                        {

                            p[rowIndex, colIndex] = 0;
                        }
                    }

                    topLeftX += tolerance;
                }
                topLeftY += tolerance;
                topLeftX = drwBndrRect.TopLeft.X;
            }

            for (long i = 0; i < areaMatrixLength; i++)
            {
                var S = new long[areaMatrixWidth+1];
                var R = new long[areaMatrixWidth];
                var L = new long[areaMatrixWidth];

                S[0] = 0;
                //S[S[0]] = 0;

                for (long j = 0; j < areaMatrixWidth; j++)
                {
                    while (S[0] > 0 && p[i, j] < p[i, S[S[0]]])
                    {
                        R[S[S[0]]] = j - 1;
                        S[0] = S[0] - 1;
                    }

                    S[0] = S[0] + 1;
                    S[S[0]] = j;
                }

                for (long j = 1; j <= S[0]; j++)
                {
                    R[S[j]] = areaMatrixWidth - 1;
                }

                //S[0] = 1;
                S[0] = 0;

                for (long j = areaMatrixWidth - 1; j >= 0; j--)
                {
                    while (S[0] > 0 && p[i, j] < p[i, S[S[0]]])
                    {
                        L[S[S[0]]] = j + 1;
                        S[0] = S[0] - 1;
                    }

                    S[0] = S[0] + 1;
                    S[S[0]] = j;
                }

                for (long j = 1; j <= S[0]; j++)
                {
                    L[S[j]] = 0;
                }

                for (long j = 0; j < areaMatrixWidth; j++)
                {
                    long currArea = p[i, j] * (R[j] - L[j] + 1);

                    if (currArea > maxRectArea)
                    {
                        maxRectArea = currArea;
                        maxRectTopLeftIndexL = i - p[i, j];
                        maxRectTopLeftIndexW = L[j];
                        maxRectWidth = (R[j] - L[j] + 1) * tolerance;
                        maxRectLenght = p[i, j] * tolerance;
                    }
                }
            }

            if (maxRectArea > 0)
            {
                double topLeftRectX = drwBndrRect.TopLeft.X + (maxRectTopLeftIndexW * tolerance);
                double topLeftRectY = drwBndrRect.TopLeft.Y + (maxRectTopLeftIndexL * tolerance);

                largestOne.Vertices.Add(new Point(topLeftRectX,                topLeftRectY));
                largestOne.Vertices.Add(new Point(topLeftRectX + maxRectWidth, topLeftRectY));
                largestOne.Vertices.Add(new Point(topLeftRectX + maxRectWidth, topLeftRectY + maxRectLenght));
                largestOne.Vertices.Add(new Point(topLeftRectX,                topLeftRectY + maxRectLenght));
            }

            return largestOne;
        }
