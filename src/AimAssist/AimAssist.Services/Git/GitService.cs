using AimAssist.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Services.Git
{
    public class GitService : IGitService
    {
        private readonly string _baseSearchPath;
        private readonly HashSet<string> _manualRepositories;
        private readonly string _settingsFilePath;

        public GitService()
        {
            _baseSearchPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _manualRepositories = new HashSet<string>();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "git-repositories.txt");
            LoadManualRepositories();
        }

        public async Task<IEnumerable<string>> GetRepositoriesAsync()
        {
            var repositories = new HashSet<string>();
            
            // Add manually added repositories first
            foreach (var repo in _manualRepositories)
            {
                if (await IsValidGitRepositoryAsync(repo))
                {
                    repositories.Add(repo);
                }
            }
            
            // Search for repositories automatically
            await Task.Run(() =>
            {
                FindGitRepositories(_baseSearchPath, repositories, 3);
            });

            return repositories.OrderBy(r => r);
        }

        private void FindGitRepositories(string directory, HashSet<string> repositories, int maxDepth)
        {
            if (maxDepth <= 0) return;

            try
            {
                if (Directory.Exists(Path.Combine(directory, ".git")))
                {
                    repositories.Add(directory);
                    return;
                }

                foreach (var subdirectory in Directory.GetDirectories(directory))
                {
                    try
                    {
                        FindGitRepositories(subdirectory, repositories, maxDepth - 1);
                    }
                    catch
                    {
                        // Skip directories that can't be accessed
                    }
                }
            }
            catch
            {
                // Skip directories that can't be accessed
            }
        }

        public async Task<string> GetCurrentBranchAsync(string repositoryPath)
        {
            var result = await ExecuteGitCommandAsync(repositoryPath, "branch --show-current");
            return result?.Trim();
        }

        public async Task<IEnumerable<string>> GetBranchesAsync(string repositoryPath)
        {
            var result = await ExecuteGitCommandAsync(repositoryPath, "branch -a");
            if (string.IsNullOrEmpty(result)) return new List<string>();

            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(b => b.Trim().TrimStart('*').Trim())
                         .Where(b => !string.IsNullOrEmpty(b))
                         .OrderBy(b => b);
        }

        public async Task<bool> CheckoutBranchAsync(string repositoryPath, string branchName)
        {
            var result = await ExecuteGitCommandAsync(repositoryPath, $"checkout {branchName}");
            return !string.IsNullOrEmpty(result);
        }

        public async Task<IEnumerable<GitCommit>> GetCommitHistoryAsync(string repositoryPath, int maxCount = 100)
        {
            var format = "--pretty=format:%H|%h|%s|%an|%ad";
            var dateFormat = "--date=iso";
            var command = $"log {format} {dateFormat} -n {maxCount}";
            
            var result = await ExecuteGitCommandAsync(repositoryPath, command);
            if (string.IsNullOrEmpty(result)) return new List<GitCommit>();

            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(ParseCommitLine)
                         .Where(c => c != null);
        }

        private GitCommit ParseCommitLine(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 5) return null;

            if (DateTime.TryParse(parts[4], out var date))
            {
                return new GitCommit
                {
                    Hash = parts[0],
                    ShortHash = parts[1],
                    Message = parts[2],
                    Author = parts[3],
                    Date = date
                };
            }

            return null;
        }

        
public async Task<bool> ExportFilesDiffAsync(string repositoryPath, string fromCommit, string toCommit, string outputDirectory)
{
    try
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Get list of changed files
        var diffCommand = $"diff --name-only {fromCommit} {toCommit}";
        var changedFiles = await ExecuteGitCommandAsync(repositoryPath, diffCommand);

        if (string.IsNullOrEmpty(changedFiles)) return true;

        var files = changedFiles.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Create before and after directories
        var beforeDir = Path.Combine(outputDirectory, "before");
        var afterDir = Path.Combine(outputDirectory, "after");
        Directory.CreateDirectory(beforeDir);
        Directory.CreateDirectory(afterDir);

        foreach (var file in files)
        {
            var fileName = file.Trim();
            if (string.IsNullOrEmpty(fileName)) continue;

            // --- before (fromCommit) ---
            var beforeContent = await ExecuteGitCommandAsync(repositoryPath, $"show {fromCommit}:{fileName}");
            if (!string.IsNullOrEmpty(beforeContent))
            {
                var beforePath = Path.Combine(beforeDir, fileName.Replace('/', Path.DirectorySeparatorChar));
                var beforeFileDir = Path.GetDirectoryName(beforePath);
                if (!Directory.Exists(beforeFileDir))
                    Directory.CreateDirectory(beforeFileDir);

                await File.WriteAllTextAsync(beforePath, beforeContent);
            }

            // --- after (toCommit) ---
            var afterContent = await ExecuteGitCommandAsync(repositoryPath, $"show {toCommit}:{fileName}");
            if (!string.IsNullOrEmpty(afterContent))
            {
                var afterPath = Path.Combine(afterDir, fileName.Replace('/', Path.DirectorySeparatorChar));
                var afterFileDir = Path.GetDirectoryName(afterPath);
                if (!Directory.Exists(afterFileDir))
                    Directory.CreateDirectory(afterFileDir);

                await File.WriteAllTextAsync(afterPath, afterContent);
            }
        }

        return true;
    }
    catch
    {
        return false;
    }
}


        private async Task<string> ExecuteGitCommandAsync(string workingDirectory, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                return process.ExitCode == 0 ? output : error;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddRepositoryAsync(string repositoryPath)
        {
            try
            {
                if (!await IsValidGitRepositoryAsync(repositoryPath))
                    return false;

                _manualRepositories.Add(repositoryPath);
                await SaveManualRepositoriesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveRepositoryAsync(string repositoryPath)
        {
            try
            {
                var removed = _manualRepositories.Remove(repositoryPath);
                if (removed)
                {
                    await SaveManualRepositoriesAsync();
                }
                return removed;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsValidGitRepositoryAsync(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;

                var gitDir = Path.Combine(path, ".git");
                if (Directory.Exists(gitDir) || File.Exists(gitDir))
                    return true;

                // Try git command to verify
                var result = await ExecuteGitCommandAsync(path, "rev-parse --git-dir");
                return !string.IsNullOrEmpty(result) && !result.Contains("not a git repository");
            }
            catch
            {
                return false;
            }
        }

        private void LoadManualRepositories()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var lines = File.ReadAllLines(_settingsFilePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _manualRepositories.Add(line.Trim());
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors when loading settings
            }
        }

        private async Task SaveManualRepositoriesAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                await File.WriteAllLinesAsync(_settingsFilePath, _manualRepositories);
            }
            catch
            {
                // Ignore errors when saving settings
            }
        }
    }
}