using System.ComponentModel.DataAnnotations;

namespace BachelorProject.Server.Helpers
{
    public static class Constants
    {
        public static int MaxWeight { get; private set; } = int.MaxValue;
        public static int MinWeight { get; private set;} = int.MinValue;

        public static string ColorBase { get; private set; } = "#3498db";
        public static string ColorProcessing { get; private set; } = "orange";
        public static string ColorProcessed { get; private set; } = "blue";
        public static string ColorResult { get; private set; } = "green";
        public static string ColorDiscarded { get; private set; } = "red";
    }
}
