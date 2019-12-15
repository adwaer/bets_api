using System.Linq;

namespace Bets.Configuration
{
    public static class StartupHelpers
    {
        public static string GetRunUrl(string[] args)
        {
            const string argTag = "--server.urls=";

            var found = args.FirstOrDefault(arg => arg.StartsWith(argTag));
            return found?.Substring(argTag.Length, found.Length - argTag.Length);
        }
    }
}