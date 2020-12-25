namespace ytsearch
{
    public class InputArgument
    {
        public string SearchTerm { get; set; }

        public SearchType SearchType { get; set; }

        public int ChannelNumber { get; set; }

        public int VideoNumber { get; set; }

        public int QueryLimit { get; set; } = 10;
    }

    public enum SearchType
    {
        VideoSearch,
        ChannelSearch,
        VideoUnderChannelSearch,
    };
}
