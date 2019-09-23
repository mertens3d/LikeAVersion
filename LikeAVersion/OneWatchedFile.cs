namespace LikeAVersion
{
    public class OneWatchedFile
    {
        public string Filter { get; set; }
        public string WatchRoot { get; set; }

        public OneWatchedFile(string pathToWatch, string filter)
        {
            this.WatchRoot = pathToWatch;
            this.Filter = filter;
        }
    }
}