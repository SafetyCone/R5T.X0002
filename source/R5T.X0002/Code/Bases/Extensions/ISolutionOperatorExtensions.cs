using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R5T.Lombardy;

using R5T.D0078;
using R5T.D0083;
using R5T.T0113;

using Instances = R5T.X0002.Instances;


namespace System
{
    public static class ISolutionOperatorExtensions
    {
        public static async Task AddDependencyProjectReferencesAndRecursiveDependencies(this ISolutionOperator _,
            string solutionFilePath,
            IEnumerable<string> projectReferenceFilePaths,
            IStringlyTypedPathOperator stringlyTypedPathOperator,
            IVisualStudioProjectFileReferencesProvider visualStudioProjectFileReferencesProvider,
            IVisualStudioSolutionFileOperator visualStudioSolutionFileOperator)
        {
            // Get all recursive project references to add. Order alphabetically and evaluate now to aide in debugging.
            var allProjectReferenceFilePaths = await Instances.ProjectOperator.GetAllRecursiveProjectReferencesInclusive(
                projectReferenceFilePaths,
                visualStudioProjectFileReferencesProvider);

            await _.AddDependencyProjectReferences(
                EnumerableHelper.From(solutionFilePath),
                allProjectReferenceFilePaths,
                stringlyTypedPathOperator,
                visualStudioSolutionFileOperator);
        }

        public static async Task<string[]> GetSolutionFilePathsContainingProject(this ISolutionOperator _,
            string projectFilePath,
            IStringlyTypedPathOperator stringlyTypedPathOperator,
            IVisualStudioSolutionFileOperator visualStudioSolutionFileOperator)
        {
            var solutionFilePaths = Instances.FileSystemOperator.FindSolutionFilesInFileDirectoryOrDirectParentDirectories(
                projectFilePath);

            var output = await _.GetSolutionFilePathsContainingProject(
                solutionFilePaths,
                projectFilePath,
                stringlyTypedPathOperator,
                visualStudioSolutionFileOperator);

            return output;
        }

        public static async Task<string[]> GetSolutionFilePathsContainingProject(this ISolutionOperator _,
            IEnumerable<string> solutionFilePaths,
            string projectFilePath,
            IStringlyTypedPathOperator stringlyTypedPathOperator,
            IVisualStudioSolutionFileOperator visualStudioSolutionFileOperator)
        {
            var outputSolutionFilePaths = new List<string>();

            foreach (var solutionFilePath in solutionFilePaths)
            {
                var includesProjectToModify = await visualStudioSolutionFileOperator.HasProjectReference(
                    solutionFilePath,
                    projectFilePath,
                    stringlyTypedPathOperator);

                if (includesProjectToModify)
                {
                    outputSolutionFilePaths.Add(solutionFilePath);
                }
            }

            return outputSolutionFilePaths.ToArray();
        }

        public static async Task AddDependencyProjectReferences(this ISolutionOperator _,
            IEnumerable<string> solutionFilePaths,
            IList<string> projectFilePaths,
            IStringlyTypedPathOperator stringlyTypedPathOperator,
            IVisualStudioSolutionFileOperator visualStudioSolutionFileOperator)
        {
            // Foreach solution file.
            foreach (var solutionFilePath in solutionFilePaths)
            {
                // Get all project references of the solution file.
                var solutionProjectReferences = await visualStudioSolutionFileOperator.ListProjectReferencePaths(
                    solutionFilePath,
                    stringlyTypedPathOperator);

                // Determine what project references need to be added.
                var missingProjectReferences = _.DetermineMissingProjectReferences(
                    solutionProjectReferences,
                    projectFilePaths)
                    .ToArray(); // Evaluate now, aids in debugging.

                // Add all required project references as dependency projects.
                await visualStudioSolutionFileOperator.AddDependencyProjectReferences(
                    solutionFilePath,
                    missingProjectReferences);
            }
        }
    }
}
