using CommandLine;

namespace Scrapper.Runner
{
    public class Options
    {
        [Option('o', "output", Required=false, HelpText="Set output directory")]
        public string OutputDirectory { get; set; }

        [Option('t', "type", Required=true, HelpText="Set podcast type to download from HowToSaveMoney, ForeignDailyNews")]
        public PodcastType Type { get; set; }
    }

    public enum PodcastType
    {
        HowToSaveMoney = 0,
        ForeignDailyNews =1
    }
}