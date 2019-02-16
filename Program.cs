using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace PrefabAnalyzer
{
    class Program
    {
        class itemType {
                public string name;
                public int count;
                public int type;
        };

        static void Main(string[] args)
        {
            string fp="e:\\Steam\\steamapps\\common\\7 Days To Die\\Data\\Worlds"; // <==== put your worlds folder location here
            string worldname="Wukumo Territory";
            // "situtation19" 
            // "8" Wukumo Territory
            // "7" Xihutu Territory
            // "5" Maviku Mountains
            // "4" Gozana Mountains 
            // "3" Bireka Valley                                       // <==== this is the world to be analyzed
            // "2" South Nafege County
            // "1" Old Fenaha Valley

            List<itemType> namesList = new List<itemType>();            
            List<string>   traderList = new List<string>();

            bool isknown;
            string inputfile="prefabs.xml";
            int counts=0;
            string fpfn = fp + "\\" + worldname + "\\" + inputfile;
            char[] charSeparators = new char[] {','};            
            string[] result;

            Console.WriteLine("Starting up, reading " + fpfn);
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(fpfn);
            
            FileInfo fi = new FileInfo(fpfn);
            System.DateTime created = fi.CreationTime;

            int []typecounts = new int[12];
            string [] typenames = new string[12];
            typenames[0] = "other";
            typenames[1] = "trader";
            typenames[2] = "survivor_site";
            typenames[3] = "skyscraper";
            typenames[4] = "junkyard";
            typenames[5] = "house";
            typenames[6] = "utility";
            typenames[7] = "store";
            typenames[8] = "cabin";
            typenames[9] = "waste_bldg";
            typenames[10]= "cave";
            typenames[11]= "factory";

            string[] typenames_ignore = new string[2];
            typenames_ignore[0] = "sign";
            typenames_ignore[1] = "street_light";

            Console.WriteLine("processing input file...\n");

            int maxXLocation = -100;
            int minXLocation = 100;
            int maxZLocation = -100;
            int minZLocation = 100;
            int maxYLocation = -100;
            int minYLocation = 100;

            // on the first pass, just determining how many of each type of prefab exist
            counts=0;
            foreach(XmlNode node in xdoc.DocumentElement.ChildNodes)
            {
                // each node should appear like this:  <decoration type="model" name="fastfood_01" position="214,49,-2856" rotation="1" />
                counts++;
                string nextName = node.Attributes["name"].Value;                
                string location = node.Attributes["position"].Value;

                isknown=false;

                foreach(string dontcare in typenames_ignore)
                {
                    if(nextName.Contains(dontcare))
                        isknown=true;
                }

                if(isknown==false) foreach(itemType item in namesList)
                {                    
                    if(item.name.Equals(nextName))
                    {
                        isknown=true;
                        item.count++;
                    }
                }
                if(isknown==false)
                {
                    itemType newitem=new itemType();
                    newitem.name = nextName;
                    newitem.count= 1;
                    newitem.type = 0;
                    for(int k=1;k<typenames.Length;k++)
                    {
                        if(typenames[k].Length>0)
                        if(nextName.Contains(typenames[k])) newitem.type=k;
                    }
                    namesList.Add(newitem);                    
                }

                result = location.Split(charSeparators, StringSplitOptions.None);
                int xLocation = Int16.Parse(result[0]);
                int yLocation = Int16.Parse(result[1]);
                int zLocation = Int16.Parse(result[2]);

                if(nextName.Contains("trader"))
                {
                    traderList.Add(location);
                }

                if(xLocation>maxXLocation) maxXLocation=xLocation;
                if(xLocation<minXLocation) minXLocation=xLocation;

                if(yLocation>maxYLocation) maxYLocation=yLocation;
                if(yLocation<minYLocation) minYLocation=yLocation;

                if(zLocation>maxZLocation) maxZLocation=zLocation;
                if(zLocation<minZLocation) minZLocation=zLocation;
            }

            
            // divide the world into a matrix of 10 by 10 blocks and then tally which block each prefab is in, in order to see how spread out or clumpy it is
            float widthX = (float)(maxXLocation-minXLocation+1) * .1f;
            float widthZ = (float)(maxZLocation-minZLocation+1) * .1f;
            int [,] bins = new int [10,10];
            foreach(XmlNode node in xdoc.DocumentElement.ChildNodes)
            {
                string location = node.Attributes["position"].Value;
                result = location.Split(charSeparators, StringSplitOptions.None);
                int xLocation = Int16.Parse(result[0]);
                int zLocation = Int16.Parse(result[2]);

                float xbin = (float)(xLocation - minXLocation) / widthX;
                int xbinInt = (int)(xbin);

                float zbin = (float)(zLocation - minZLocation) / widthZ;
                int zbinInt = (int)(zbin);

                bins[xbinInt,zbinInt] += 1;
            }

            Console.WriteLine("\nThere were " + counts + " prefab instances of " + namesList.Count + " types found in the world \"" + worldname + "\" which was created on " + created.ToLongDateString() + " at " + created.ToShortTimeString());
            
            Console.WriteLine("Prefabs will spawn between coordinates (" + minXLocation + "," + minYLocation + "," + minZLocation + ") and (" + maxXLocation + "," + maxYLocation + "," + maxZLocation + ")");
            Console.WriteLine("In a grid with each block " + widthX + " by " + widthZ + " meters, here is the number of prefabs in each block:");
            for(int z=0;z<10;z++)
            {
                Console.Write("\t");
                for(int x=0;x<10;x++)
                {
                    Console.Write(bins[x,z] + "\t");
                }
                Console.Write("\n");
            }
            
            // sort the list of prefabs (most common ones first)
            namesList.Sort((x,y)=>y.count.CompareTo(x.count));           

            for(int i=0;i<12;i++) typecounts[i]=0;
            // tally count of each prefab type found
            foreach(itemType item in namesList)
            {             
                typecounts[item.type]+= item.count;
            }

            int mostcommoncount=0;
            for(int i=0;i<10;i++)
            {
                mostcommoncount += namesList[i].count;
            }

            float pct;
            pct = (float)mostcommoncount/counts * 10000.0f;
            int pcti = (int)pct;
            pct = pcti * .01f;

            Console.WriteLine("\nThe 10 most commonly duplicated prefabs account for " + pct + "% of the prefabs and are: ");
            Console.WriteLine("\tPrefab:              \tOccurances:");
            for(int i=0;i<10;i++) {
                Console.WriteLine("\t" + namesList[i].name.PadRight(21) + "\t" + namesList[i].count );
            }

            Console.WriteLine("\nSummary of prefab types:");
            for(int i=0;i<12;i++)
            {                                
                pct = (float)typecounts[i]/counts * 10000.0f;
                pcti = (int)pct;
                pct = pcti * .01f;
                Console.WriteLine("\t" + typenames[i].PadRight(13) + " = " + typecounts[i].ToString().PadLeft(3) + " (" + pct + "%)");                
            }

            // analyze trader locations
            double aveDistance=0.0;
            int countdistances=0;            
            foreach(string position in traderList)
            {
                result = position.Split(charSeparators, StringSplitOptions.None);
                int x1 = Int16.Parse(result[0]);                
                int z1 = Int16.Parse(result[2]);
                double nearestNeighborDistance = 10000.0;
                
                foreach(string position2 in traderList)
                {
                    if(position.Equals(position2)==false)
                    {
                        result = position2.Split(charSeparators, StringSplitOptions.None);
                        int x2 = Int16.Parse(result[0]);                    
                        int z2 = Int16.Parse(result[2]);
                    
                        double distance = Math.Sqrt((x2-x1)*(x2-x1) + (z2-z1)*(z2-z1));            
                        if(distance < nearestNeighborDistance) nearestNeighborDistance=distance;
                    }
                }                
                aveDistance += nearestNeighborDistance;
                countdistances++;
            }
            aveDistance /= (double)countdistances;
            aveDistance = (int)(aveDistance+.5);
            Console.WriteLine("\nOn average, each trader is " + aveDistance + " meters away from another trader.");
        }
    }
}
