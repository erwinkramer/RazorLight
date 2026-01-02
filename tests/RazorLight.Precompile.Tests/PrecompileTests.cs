using NUnit.Framework;
using System.Diagnostics;

namespace RazorLight.Precompile.Tests
{
	public class PrecompileTests : TestWithCulture
	{
		[TestCaseSource(typeof(PrecompileTestCases), nameof(PrecompileTestCases.TestCases))]
		public void PrecompileFromScratch(string templateFilePath, TestScenario scenario)
		{
			scenario.Cleanup();

			var expectedPrecompiledFilePath = GetExpectedPrecompiledFilePath(templateFilePath, scenario);
			Assert.That(expectedPrecompiledFilePath, Does.Not.Exist);

			Precompile(templateFilePath, scenario, expectedPrecompiledFilePath);

			scenario.Cleanup();
			Assert.That(expectedPrecompiledFilePath, Does.Not.Exist);
		}

		[TestCaseSource(typeof(PrecompileTestCases), nameof(PrecompileTestCases.TestCases))]
		public void PrecompileCached(string templateFilePath, TestScenario scenario)
		{
			scenario.Cleanup();

			var expectedPrecompiledFilePath = GetExpectedPrecompiledFilePath(templateFilePath, scenario);
			Assert.That(expectedPrecompiledFilePath, Does.Not.Exist);

			var sw1 = Stopwatch.StartNew();
			Precompile(templateFilePath, scenario, expectedPrecompiledFilePath);
			sw1.Stop();

			Assert.That(expectedPrecompiledFilePath, Does.Exist);

			var sw2 = Stopwatch.StartNew();
			Precompile(templateFilePath, scenario, expectedPrecompiledFilePath);
			sw2.Stop();

			TestContext.WriteLine($"TS1 = {sw1.Elapsed}, TS2 = {sw2.Elapsed}");

			scenario.Cleanup();
			Assert.That(expectedPrecompiledFilePath, Does.Not.Exist);
		}

		public static string GetExpectedPrecompiledFilePath(string templateFilePath, TestScenario scenario)
		{
			var cacheFileInfo = scenario.ExpectedCachingStrategy.GetCachedFileInfo(scenario.GetTemplateKey(templateFilePath), templateFilePath, scenario.GetExpectedCacheDirectory(templateFilePath));
			return scenario.GetExpectedPrecompiledFilePath(cacheFileInfo.AssemblyFilePath);
		}

		public static void Precompile(string templateFilePath, TestScenario scenario, string? expectedPrecompiledFilePath)
		{
			var commandLineArgs = new List<string>
			{
				"precompile",
				"-t",
				templateFilePath
			};
			commandLineArgs.AddRange(scenario.ExtraCommandLineArgs);

			var precompiledFilePath = Helper.RunCommandTrimNewline(commandLineArgs.ToArray());
			Assert.That(precompiledFilePath, Is.EqualTo(expectedPrecompiledFilePath));
			Assert.That(precompiledFilePath, Does.Exist);
		}
	}
}