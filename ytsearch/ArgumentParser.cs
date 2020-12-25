using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ytsearch
{
    public static class ArgumentParser
    {
        public static InputArgument Parse(string[] args)
        {
            var result = new InputArgument 
            {
                SearchType = SearchType.VideoSearch
            };

            var argList = args.ToList();

            // Search query limit.
            int idx = argList.FindIndex(x => x.Equals("-l"));
            if (idx != -1)
            {
                result.QueryLimit = int.Parse(argList[idx + 1]) + 1; // Increasing one as sometimes channel also comes in result
                argList.RemoveAt(idx); // Remove the flag
                argList.RemoveAt(idx); // Remove the value
            }

            idx = argList.FindIndex(x => x.Equals("-c"));
            if (idx != -1)
            {
                // Search Channel number
                if (idx + 1 < argList.Count && int.TryParse(argList[idx + 1], out int channelNumber))
                {
                    result.ChannelNumber = channelNumber;
                    result.SearchType = SearchType.VideoUnderChannelSearch;
                    argList.RemoveAt(idx); // Remove the flag
                    argList.RemoveAt(idx); // Remove the value
                }
                else
                {
                    result.SearchType = SearchType.ChannelSearch;
                    argList.RemoveAt(idx); // Remove the flag
                }
            }

            Assert.AreEqual(1, argList.Count);
            result.SearchTerm = argList[0];

            return result;
        }
    }
}
