using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Scrapper.Common;

namespace Scrapper.Runner
{
    class Program
    {        
        private static string DefaultOutput = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        static void Main(string[] args)
        {
            IScrapper scrapper = null;
            string entryUrl = null;
            string outputDirectory = null;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>{
                    switch(o.Type)
                    {
                        case PodcastType.HowToSaveMoney:
                            scrapper = new HowToSaveMoney.Scrapper.Scrapper();
                            entryUrl = "https://jakoszczedzacpieniadze.pl/podcast";
                            break;

                        case PodcastType.ForeignDailyNews:
                            scrapper = new ForeignDailyNews.Scrapper.Scrapper();
                            entryUrl = "https://www.dzialzagraniczny.pl/category/podcasty/";
                            break;
                        default:
                            throw new System.ArgumentNullException(nameof(Options.Type));
                    }
                
                    outputDirectory = string.IsNullOrEmpty(o.OutputDirectory) ? DefaultOutput : o.OutputDirectory;
                });

            if(scrapper!= null) 
            {
                var downloader = new Downloader(scrapper, outputDirectory, entryUrl);
                downloader.Run().GetAwaiter().GetResult();            
            }
        }
    }
}
