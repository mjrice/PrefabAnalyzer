using System;
using System.Collections.Generic;
using System.Xml;

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
            List<itemType> namesList = new List<itemType>();            
            bool isknown;

            string inputfile="prefabs.xml";
            int counts=0;
            Console.WriteLine("Starting up, reading " + inputfile);
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(inputfile);

            int []typecounts = new int[12];
            string [] typenames = new string[12];
            for(int i=0;i<12;i++) typecounts[i]=0;
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

            Console.WriteLine("processing input file...\n");
            foreach(XmlNode node in xdoc.DocumentElement.ChildNodes)
            {
                counts++;
                string nextName = node.Attributes["name"].Value;                
                isknown=false;
                foreach(itemType item in namesList)
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
                    for(int k=1;k<12;k++)
                    {
                        if(typenames[k].Length>0)
                        if(nextName.Contains(typenames[k])) newitem.type=k;
                    }
                    namesList.Add(newitem);                    
                }
            }

            namesList.Sort((x,y)=>x.count.CompareTo(y.count));
           
            foreach(itemType item in namesList)
            {
                Console.WriteLine(item.name + ", " + item.count );
                typecounts[item.type]+= item.count;
            }
            
            Console.WriteLine("there were " + counts + " prefab instances of " + namesList.Count + " types.");        

            Console.WriteLine("\nSummary of prefab types:");
            for(int i=0;i<12;i++)
            {
                if(typecounts[i]>0)
                {
                    float pct = (float)typecounts[i]/counts * 10000.0f;
                    int pcti = (int)pct;
                    pct = pcti * .01f;

                    Console.WriteLine("\t" + typenames[i].PadRight(13) + " = " + typecounts[i].ToString().PadLeft(3) + " (" + pct + "%)");
                }
            }
        }
    }
}
