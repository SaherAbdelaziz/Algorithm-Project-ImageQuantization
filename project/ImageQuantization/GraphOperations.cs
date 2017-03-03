using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
namespace ImageQuantization
{
    class GraphOperations
    { 
        public class Edgee 
        {
            public int from { get; set; }
            public int to { get; set; }
            public double w { get; set; }
            public Edgee(int from, int to, double w)    
            {
                this.from = from;
                this.to = to;
                this.w = w;

            }
        }
        // Data uesed in all/some functions 
        public static class GlobalData
        {
            public static int height;
            public static int width;

            public static void setData()
            {
                height = ImageOperations.GetHeight(MainForm.ImageMatrix);
                width = ImageOperations.GetWidth(MainForm.ImageMatrix);
                arrMP = new bool[256, 256, 256];
                arrStore = new RGBPixel[256, 256, 256];
                
            }

            public static bool[, ,] arrMP;
            public static RGBPixel[, ,] arrStore;
            public static double mstcost = 0;
            public static long howmmanyclusters = 0;
            public static long DC = 0;
        }

        public static List<RGBPixel> getD(List<RGBPixel> DistinctColors)
        {
            GlobalData.DC = 0;
            for (int i = 0; i < GlobalData.height; i++)// O(Height*Width)
            {
                for (int j = 0; j < GlobalData.width; j++)
                {
                    if (GlobalData.arrMP[MainForm.ImageMatrix[i, j].red, MainForm.ImageMatrix[i, j].green, MainForm.ImageMatrix[i, j].blue] == false)
                    {
                        GlobalData.arrMP[MainForm.ImageMatrix[i, j].red, MainForm.ImageMatrix[i, j].green, MainForm.ImageMatrix[i, j].blue] = true;
                        GlobalData.arrStore[MainForm.ImageMatrix[i, j].red, MainForm.ImageMatrix[i, j].green, MainForm.ImageMatrix[i, j].blue] 
                            = MainForm.ImageMatrix[i, j];
                        DistinctColors.Add(MainForm.ImageMatrix[i, j]);
                    }
                }
            }
            return DistinctColors ;
        }

        public static List<Edgee> GetMST(List<RGBPixel> DistinctColors , List<Edgee> mst) // O(distinctColors.Count*SetForMST.size)
        {
            List<double> costs = Enumerable.Repeat(double.MaxValue, DistinctColors.Count).ToList();
            List<int> parent; parent = Enumerable.Repeat(-1, DistinctColors.Count).ToList();
            costs[0] = 0;
            int nextnode = 0;
            bool[] MSTvisted = new bool[DistinctColors.Count];
            RGBPixel f ;
            RGBPixel s ;
            double cost=0 ;
            for (int i = 0; i < DistinctColors.Count - 1; i++)  // o(n)
            {
                int idx = nextnode;
                f = DistinctColors[idx];
                MSTvisted[idx] = true;
                double mininode = double.MaxValue;
                for (int j = 0; j < DistinctColors.Count; j++) // o(N)
                {
                    if (!MSTvisted[j])
                    {
                        s = DistinctColors[j];
                        cost = (Math.Abs(f.red - s.red) * Math.Abs(f.red - s.red))
                            + Math.Abs((f.green - s.green) * Math.Abs(f.green - s.green))
                            + (Math.Abs(f.blue - s.blue) * Math.Abs(f.blue - s.blue));
                        cost = Math.Sqrt(cost);

                        if (cost < costs[j])
                        {
                            parent[j] = idx;
                            costs[j] = cost;
                        }

                        if (costs[j] < mininode)
                        {
                            mininode = costs[j];
                            nextnode = j;
                        }       
                    }
                }
                GlobalData.mstcost += mininode;
            }
            GlobalData.mstcost = 0;
            for (int i = 1; i < parent.Count; i++)
            {
                mst.Add(new Edgee(parent[i], i, costs[i]));
                GlobalData.mstcost += costs[i];
            }

                GlobalData.DC = DistinctColors.Count;
            return mst;

        }

        public static void Kcluster(int k, List<RGBPixel> DistinctColors , List<Edgee> mymst)
        {
            bool [] visForClusters = new bool[DistinctColors.Count + 10];
            List< List<RGBPixel> > clusters = new List< List<RGBPixel> >() ;
            List<Edgee> []  adjl = new List<Edgee>[DistinctColors.Count + 10];
            //mst = mst.OrderBy(Edge => Edge.w).ToList();
            double[] costsofmst = new double[mymst.Count + 10];
           

            for(int i=0 ; i<DistinctColors.Count ; i++)
            {
                adjl[i] = new List<Edgee>();
            }

            List<int> NodesToCut = new List<int>();
            
            for (int i = 0; i < k - 1; i++)
            {
                double max = -1;
                int index = 0;
                for (int g = 0; g < mymst.Count; g++)
                {
                    if (max < mymst[g].w)
                    {
                        max = mymst[g].w;
                        index = g;
                    }
                }
                costsofmst[index] = mymst[index].w;
                mymst[index].w = (double)-1;
                
                NodesToCut.Add(mymst[index].from);
                NodesToCut.Add(mymst[index].to);   
            }

            for (int i = 0; i < mymst.Count; i++)
            {
                Edgee tmpp = mymst[i];
                if (tmpp.w != -1)
                {
                    adjl[tmpp.from].Add(new Edgee(tmpp.from, tmpp.to, tmpp.w));
                    adjl[tmpp.to].Add(new Edgee(tmpp.to, tmpp.from, tmpp.w));
                }
            }

            foreach(int i in NodesToCut)
            {
                if (visForClusters[i] == false)
                {
                    GetKcluster(i, adjl, visForClusters, DistinctColors, clusters);
                    // BFS
                }   
            }
              
            if (k == 1)
            {
                for (int i = 0; i < mymst.Count; i++) // O(SetForMST2.size)
                {

                    if (visForClusters[mymst[i].to] == false)
                    {
                        GetKcluster(mymst[i].to, adjl, visForClusters, DistinctColors, clusters);
                    }
                }
            }

            for (int i = 0; i < mymst.Count; i++ )
            {
                if(mymst[i].w==-1)
                {
                    mymst[i].w = costsofmst[i];
                }
            }


                // Get colors
                platte(clusters); // o(height * width)
        }
        // Using BFS
        public static void GetKcluster(int from, List<Edgee>[] adjl, bool[] visForClusters,List<RGBPixel> DistinctColors, List< List<RGBPixel> > clusters) // O(E+V)
        {
            List<RGBPixel> clu = new List<RGBPixel>();
            clu.Add(DistinctColors[from]);
            Queue<int> QueueforCluster = new Queue<int>();
            QueueforCluster.Enqueue(from);
         
            int helper , helper1;
            // BFS
            while (QueueforCluster.Count != 0)
            {
                helper = QueueforCluster.Dequeue();

                if (visForClusters[helper] == false)
                {
                    visForClusters[helper] = true;
                    for (int i = 0; i < adjl[helper].Count; i++) 
                    {
                        helper1 = adjl[helper][i].to;
                        if (visForClusters[helper1] == false)
                        {
                            clu.Add(DistinctColors[helper1]);
                            QueueforCluster.Enqueue(helper1);
                        }
                    }
                }
            }
            // clusters.Add(clu);
            // Calculate average color for cluster  and change color int specific cluster
            Calculate_average(clu);// o(d)
        }

        // Calculate average color for cluster 
        public static void Calculate_average(List<RGBPixel> clue) // o(d)
        {
            long rr=0, gg=0, bb=0;
            foreach(RGBPixel i in clue)
            {
                rr += i.red;
                gg += i.green;
                bb += i.blue;

            }

            RGBPixel tmp=new RGBPixel(0 , 0, 0);
            if (clue.Count != 0)
            {
                rr = rr / clue.Count;
                gg = gg / clue.Count;
                bb = bb / clue.Count;

                tmp.red = (byte)rr;
                tmp.green = (byte)gg;
                tmp.blue = (byte)bb;
            }
            for (int i = 0; i < clue.Count; i++) 
            {
                GlobalData.arrStore[ clue[i].red, clue[i].green, clue[i].blue] = tmp;
            }
        }

        // change original colors with clusters colors to get the output 
        public static void platte(List<List<RGBPixel>> clusters) 
        {
            for (int i = 0; i < GlobalData.height; i++)
            {
                for (int j = 0; j < GlobalData.width; j++)
                {
                    MainForm.Result[i, j] = 
                        GlobalData.arrStore[MainForm.ImageMatrix[i, j].red, MainForm.ImageMatrix[i, j].green, MainForm.ImageMatrix[i, j].blue];
                }
            }   
        }
    }
}