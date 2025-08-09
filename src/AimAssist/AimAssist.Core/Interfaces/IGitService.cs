using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AimAssist.Core.Interfaces
{
    public interface IGitService
    {
        Task<IEnumerable<string>> GetRepositoriesAsync();
        Task<string> GetCurrentBranchAsync(string repositoryPath);
        Task<IEnumerable<string>> GetBranchesAsync(string repositoryPath);
        Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName);
        Task<IEnumerable<GitCommit>> GetCommitHistoryAsync(string repositoryPath, int maxCount = 100);
        Task<bool> ExportFilesDiffAsync(string repositoryPath, string fromCommit, string toCommit, string outputDirectory);
        Task<bool> AddRepositoryAsync(string repositoryPath);
        Task<bool> RemoveRepositoryAsync(string repositoryPath);
        Task<bool> IsValidGitRepositoryAsync(string path);
    }

    public class GitCommit
    {
        public string Hash { get; set; }
        public string ShortHash { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
    }

    public class GitRepository
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string CurrentBranch { get; set; }
    }
}