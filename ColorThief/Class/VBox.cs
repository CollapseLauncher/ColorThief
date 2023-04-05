﻿using System;
using System.Collections.Generic;

namespace ColorThiefDotNet
{
    /// <summary>
    ///     3D color space box.
    /// </summary>
#if NETCOREAPP && NET7_0_OR_GREATER
    internal struct VBox
#else
    internal class VBox
#endif
    {
        private readonly int[] histo;
        private int[] avg;
        public int B1;
        public int B2;
        private int? count;
        public int G1;
        public int G2;
        public int R1;
        public int R2;
        private int? volume;
#if NETCOREAPP && NET7_0_OR_GREATER
        public bool isDummy = true;
#endif

        public VBox(int r1, int r2, int g1, int g2, int b1, int b2, int[] histo)
        {
            R1 = r1;
            R2 = r2;
            G1 = g1;
            G2 = g2;
            B1 = b1;
            B2 = b2;

            this.histo = histo;
#if NETCOREAPP && NET7_0_OR_GREATER
            this.isDummy = false;
#endif
        }

        public int Volume(bool force)
        {
            if (volume == null || force)
            {
                volume = (R2 - R1 + 1) * (G2 - G1 + 1) * (B2 - B1 + 1);
            }

            return volume.Value;
        }

        public int Count(bool force)
        {
            if (count == null || force)
            {
                int npix = 0;
                int i;

                for (i = R1; i <= R2; i++)
                {
                    int j;
                    for (j = G1; j <= G2; j++)
                    {
                        int k;
                        for (k = B1; k <= B2; k++)
                        {
                            int index = Mmcq.GetColorIndex(i, j, k);
                            npix += histo[index];
                        }
                    }
                }

                count = npix;
            }

            return count.Value;
        }

        public VBox Clone() => new VBox(R1, R2, G1, G2, B1, B2, histo);

        public int[] Avg(bool force)
        {
            if (avg == null || force)
            {
                int ntot = 0;

                int rsum = 0;
                int gsum = 0;
                int bsum = 0;

                int i;

                for (i = R1; i <= R2; i++)
                {
                    int j;
                    for (j = G1; j <= G2; j++)
                    {
                        int k;
                        for (k = B1; k <= B2; k++)
                        {
                            int histoindex = Mmcq.GetColorIndex(i, j, k);
                            int hval = histo[histoindex];
                            ntot += hval;
                            rsum += (int)(hval * (i + 0.5) * Mmcq.Mult);
                            gsum += (int)(hval * (j + 0.5) * Mmcq.Mult);
                            bsum += (int)(hval * (k + 0.5) * Mmcq.Mult);
                        }
                    }
                }

                if (ntot > 0)
                {
                    avg = new[]
                    {
                        Math.Abs(rsum / ntot), Math.Abs(gsum / ntot),
                        Math.Abs(bsum / ntot)
                    };
                }
                else
                {
                    avg = new[]
                    {
                        Math.Abs(Mmcq.Mult * (R1 + R2 + 1) / 2),
                        Math.Abs(Mmcq.Mult * (G1 + G2 + 1) / 2),
                        Math.Abs(Mmcq.Mult * (B1 + B2 + 1) / 2)
                    };
                }
            }

            return avg;
        }
    }

    internal class VBoxCountComparer : IComparer<VBox>
    {
        public int Compare(VBox x, VBox y)
        {
            int a = x.Count(false);
            int b = y.Count(false);
            return a < b ? -1 : (a > b ? 1 : 0);
        }
    }

    internal class VBoxComparer : IComparer<VBox>
    {
        public int Compare(VBox x, VBox y)
        {
            int aCount = x.Count(false);
            int bCount = y.Count(false);
            int aVolume = x.Volume(false);
            int bVolume = y.Volume(false);

            // Otherwise sort by products
            int a = aCount * aVolume;
            int b = bCount * bVolume;
            return a < b ? -1 : (a > b ? 1 : 0);
        }
    }
}