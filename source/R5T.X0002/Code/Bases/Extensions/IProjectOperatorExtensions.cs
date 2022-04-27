using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R5T.Lombardy;

using R5T.D0078;
using R5T.D0079;
using R5T.D0083;
using R5T.D0101;
using R5T.T0113;

using Instances = R5T.X0002.Instances;


namespace System
{
    public static class IProjectOperatorExtensions
    {
        /// <summary>
        /// Adds project references by identity string to a project, and adds all recursive project reference dependencies to any enclosing solution files found in direct parent directories.
        /// </summary>
        public static async Task AddProjectReferencesToProject(this IProjectOperator _,
            string projectToModifyFilePath,
            IList<string> projectReferenceToAddIdentityStrings,
            IProjectRepository projectRepository,
            IStringlyTypedPathOperator stringlyTypedPathOperator,
            IVisualStudioProjectFileOperator visualStudioProjectFileOperator,
            IVisualStudioProjectFileReferencesProvider visualStudioProjectFileReferencesProvider,
            IVisualStudioSolutionFileOperator visualStudioSolutionFileOperator)
        {
            // Get file paths for all project reference identity strings.
            var projectReferenceToAddFilePaths = await _.GetFilePathsForProjectIdentityStrings(
                projectReferenceToAddIdentityStrings,
                projectRepository);

            // Add all project references to the project.
            await visualStudioProjectFileOperator.AddProjectReferencesOkIfAlreadyAdded(
                    projectToModifyFilePath,
                    projectReferenceToAddFilePaths);

            // Now modify solution files that include the project to modify.
            // Get all recursive project references to add. Order alphabetically and evaluate now to aide in debugging.
            var allProjectReferencesToAdd = await _.GetAllRecursiveProjectReferencesInclusive(
                projectReferenceToAddFilePaths,
                visualStudioProjectFileReferencesProvider);

            // Get all solution files in parent directories of the project to modify.

            // Get all solution files that contain a reference to the project to modify (are of interest).
            var solutionFilePaths = await Instances.SolutionOperator.GetSolutionFilePathsContainingProject(
                projectToModifyFilePath,
                stringlyTypedPathOperator,
                visualStudioSolutionFileOperator);

            await Instances.SolutionOperator.AddDependencyProjectReferences(
                solutionFilePaths,
                allProjectReferencesToAdd,
                stringlyTypedPathOperator,
                visualStudioSolutionFileOperator);
        }

        public static async Task<string[]> GetAllRecursiveProjectReferencesInclusive(this IProjectOperator _,
            IEnumerable<string> projectFilePaths,
            IVisualStudioProjectFileReferencesProvider visualStudioProjectFileReferencesProvider)
        {
            var projectFilePathsHash = new HashSet<string>();

            foreach (var projectFilePath in projectFilePaths)
            {
                // Get all project references, recursively, inclusively of the project reference to add.
                // Use inclusive, to ensure the actually specified project reference (and not just its dependencies) get added.
                var projectReferencesToAdd = await visualStudioProjectFileReferencesProvider.GetAllRecursiveProjectReferenceDependenciesInclusive(
                    projectFilePath);

                projectFilePathsHash.AddRange(projectReferencesToAdd);
            }

            // Order alphabetically and evaluate now to aide in debugging.
            var output = projectFilePathsHash
                .OrderAlphabetically()
                .ToArray();

            return output;
        }

        public static async Task<string[]> GetAllRecursiveProjectReferencesInclusive(this IProjectOperator _,
            string projectFilePath,
            IVisualStudioProjectFileReferencesProvider visualStudioProjectFileReferencesProvider)
        {
            var output = await _.GetAllRecursiveProjectReferencesInclusive(
                EnumerableHelper.From(projectFilePath),
                visualStudioProjectFileReferencesProvider);

            return output;
        }

        public static async Task<string> GetFilePathForProjectIdentity(this IProjectOperator _,
            Guid projectIdentity,
            IProjectRepository projectRepository)
        {
            var projectFilePathsByIdentity = await projectRepository.GetProjectFilePaths(
                EnumerableHelper.From(projectIdentity));

            var output = projectFilePathsByIdentity.Values.Single();
            return output;
        }

        public static async Task<string[]> GetFilePathsForProjectIdentityStrings(this IProjectOperator _,
            IEnumerable<string> projectIdentityStrings,
            IProjectRepository projectRepository)
        {
            var projectIdentities = _.GetProjectIdentities(projectIdentityStrings);

            var output = await projectRepository.GetFilePathsForProjectIdentities(
                projectIdentities);

            return output;
        }

        public static async Task<string> GetFilePathForProjectIdentityString(this IProjectOperator _,
            string projectIdentityString,
            IProjectRepository projectRepository)
        {
            var projectIdentity = _.GetProjectIdentity(projectIdentityString);

            var output = await _.GetFilePathForProjectIdentity(
                projectIdentity,
                projectRepository);

            return output;
        }
    }
}
