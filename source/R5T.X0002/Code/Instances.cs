using System;

using R5T.T0044;
using R5T.T0113;


namespace R5T.X0002
{
    public static class Instances
    {
        public static IFileSystemOperator FileSystemOperator { get; } = T0044.FileSystemOperator.Instance;
        public static ISolutionOperator SolutionOperator { get; } = T0113.SolutionOperator.Instance;
    }
}
