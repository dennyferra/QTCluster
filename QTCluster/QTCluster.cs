using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Clustering
{
    /// <summary>
    /// Quality Threshold Clustering algorithm
    /// </summary>
    /// <remarks>
    /// Original code from http://www.c-sharpcorner.com/uploadfile/b942f9/implementing-the-qt-algorithm-using-C-Sharp/
    /// Changes were applied to include the average point in the cluster however the algorithm remains mostly unchanged.
    /// </remarks>
    public static class QualityThresholdCluster
    {
        /// <summary>
        /// Creates a Quality Threshold cluster from the specified points and maximum diameter.
        /// </summary>
        /// <param name="points">The points to be clustered</param>
        /// <param name="maxDiameter">The maximum diameter to cluster points</param>
        /// <returns>
        /// A Lookup where the Key is the average point in the cluster and the Value is the
        /// collection of points in the cluster.
        /// </returns>
        /// <remarks>
        /// The returned Lookup Key is a calculated value (the average point in the cluster)
        /// and therefore is not representative of a point in the original set of data.
        /// </remarks>
        public static ILookup<Point, List<Point>> GetClusters(List<Point> points, double maxDiameter)
        {
            if (points == null) return null;
            
            // Leave original list unaltered
            List<Point> pointsCopy = new List<Point>(points);

            // Will contain the dictionary of clusters where:
            // Key = average point from the list of candidates
            // Value = list of candidates
            Dictionary<Point, List<Point>> clusters = new Dictionary<Point, List<Point>>();
            
            while (pointsCopy.Count > 0)
            {
                List<Point> bestCandidate = GetBestCandidate(pointsCopy, maxDiameter);

                // Determine the average x/y coordinate from the cluster candidates
                var minx = bestCandidate.Min(m => m.X);
                var maxx = bestCandidate.Max(m => m.X);

                var midx = minx + ((maxx - minx) / 2);
                var midy = bestCandidate.Min(m => m.Y) + ((bestCandidate.Max(m => m.Y) - bestCandidate.Min(m => m.Y)) / 2);

                Point averagePoint = new Point(midx, midy);

                clusters.Add(averagePoint, bestCandidate);
            }

            return clusters.ToLookup(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Returns first candidate cluster encountered if there is more than one with the maximum number of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="maxDiameter"></param>
        private static List<Point> GetBestCandidate(List<Point> points, double maxDiameter)
        {
            double maxDiameterSquared = maxDiameter * maxDiameter; // square maximum diameter       
            List<List<Point>> candidates = new List<List<Point>>(); // stores all candidate clusters
            List<Point> currentCandidate = null; // stores current candidate cluster
            int[] candidateNumbers = new int[points.Count]; // keeps track of candidate numbers to which points have been allocated
            int totalPointsAllocated = 0; // total number of points already allocated to candidates
            int currentCandidateNumber = 0; // current candidate number

            for (int i = 0; i < points.Count; i++)
            {
                if (totalPointsAllocated == points.Count) break; // no need to continue further
                if (candidateNumbers[i] > 0) continue; // point already allocated to a candidate
                currentCandidateNumber++;
                currentCandidate = new List<Point> { points[i] }; // create a new candidate cluster
                candidateNumbers[i] = currentCandidateNumber;
                totalPointsAllocated++;

                Point latestPoint = points[i]; // latest point added to current cluster
                int[] diametersSquared = new int[points.Count]; // diameters squared of each point when added to current cluster

                // iterate through any remaining points
                // successively selecting the point closest to the group until the threshold is exceeded
                while (true)
                {
                    if (totalPointsAllocated == points.Count) break; // no need to continue further               
                    int closest = -1; // index of closest point to current candidate cluster
                    int minDiameterSquared = Int32.MaxValue; // minimum diameter squared, initialized to impossible value 

                    for (int j = i + 1; j < points.Count; j++)
                    {
                        if (candidateNumbers[j] > 0) continue; // point already allocated to a candidate           

                        // update diameters squared to allow for latest point added to current cluster
                        int distSquared = latestPoint.DistanceSquared(points[j]);
                        if (distSquared > diametersSquared[j]) diametersSquared[j] = distSquared;

                        // check if closer than previous closest point
                        if (diametersSquared[j] < minDiameterSquared)
                        {
                            minDiameterSquared = diametersSquared[j];
                            closest = j;
                        }
                    }

                    // if closest point is within maxDiameter, add it to the current candidate and mark it accordingly
                    if (minDiameterSquared <= maxDiameterSquared)
                    {
                        currentCandidate.Add(points[closest]);
                        candidateNumbers[closest] = currentCandidateNumber;
                        totalPointsAllocated++;
                    }
                    else // otherwise finished with current candidate
                    {
                        break;
                    }
                }

                // add current candidate to candidates list
                candidates.Add(currentCandidate);
            }

            // now find the candidate cluster with the largest number of points
            int maxPoints = -1; // impossibly small value
            int bestCandidateNumber = 0; // ditto
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Count > maxPoints)
                {
                    maxPoints = candidates[i].Count;
                    bestCandidateNumber = i + 1; // counting from 1 rather than 0
                }
            }

            // iterating backwards to avoid indexing problems, remove points in best candidate from points list
            // this will automatically be persisted to caller as List<Point> is a reference type
            for (int i = candidateNumbers.Length - 1; i >= 0; i--)
            {
                if (candidateNumbers[i] == bestCandidateNumber) points.RemoveAt(i);
            }

            // return best candidate to caller                
            return candidates[bestCandidateNumber - 1];
        }
    }
}