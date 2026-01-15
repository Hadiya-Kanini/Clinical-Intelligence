using System.Text;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Security;

public sealed class SqlInjectionGuardrailTests
{
    [Fact]
    public void ProductionCode_ShouldNotIntroduceRiskyRawSqlApisOrDynamicSqlConstruction()
    {
        var apiProjectRoot = ResolveApiProjectRoot();

        var excludedSegments = new[]
        {
            "\\bin\\",
            "\\obj\\",
            "\\Migrations\\"
        };

        var bannedTokens = new[]
        {
            "FromSqlRaw(",
            "ExecuteSqlRaw(",
            "SqlQueryRaw(",
            "FromSqlInterpolated(",
            "ExecuteSqlInterpolated(",
            "SqlQueryInterpolated(",
            "migrationBuilder.Sql(",
            "CommandText = $\"",
            "CommandText=$\"",
            "CommandText = string.Format",
            "CommandText=string.Format",
            "new NpgsqlCommand($\"",
            "new NpgsqlCommand(string.Format"
        };

        var findings = new List<(string FilePath, int LineNumber, string Token, string Line)>();

        foreach (var file in Directory.EnumerateFiles(apiProjectRoot, "*.cs", SearchOption.AllDirectories))
        {
            var normalizedPath = file.Replace('/', '\\');
            if (excludedSegments.Any(seg => normalizedPath.Contains(seg, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                foreach (var token in bannedTokens)
                {
                    if (line.Contains(token, StringComparison.Ordinal))
                    {
                        findings.Add((file, i + 1, token, line.Trim()));
                    }
                }
            }
        }

        if (findings.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SQL INJECTION GUARDRAIL VIOLATION");
            sb.AppendLine("Detected risky raw SQL usage or dynamic SQL construction in production code.");
            sb.AppendLine("If raw SQL is truly required, use parameterized APIs and document security review.");
            sb.AppendLine();

            foreach (var f in findings.Take(30))
            {
                sb.AppendLine($"- {f.FilePath}:{f.LineNumber} [{f.Token}] {f.Line}");
            }

            if (findings.Count > 30)
            {
                sb.AppendLine($"...and {findings.Count - 30} more");
            }

            Assert.Fail(sb.ToString());
        }
    }

    private static string ResolveApiProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var serverDirCandidate = dir.FullName;
            var apiDir = Path.Combine(serverDirCandidate, "ClinicalIntelligence.Api");
            var apiCsproj = Path.Combine(apiDir, "ClinicalIntelligence.Api.csproj");

            if (Directory.Exists(apiDir) && File.Exists(apiCsproj))
            {
                return apiDir;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not resolve ClinicalIntelligence.Api project root from test base directory.");
    }
}
