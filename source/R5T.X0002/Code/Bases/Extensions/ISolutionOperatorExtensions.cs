using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R5T.Lombardy;

using R5T.D0078;
using R5T.T0113;


namespace System
{
    public static class ISolutionOperatorExtensions
    {
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

        public static async Task UpdateSolutionsToIncludeProjectReferences(this ISolutionOperator _,
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
