﻿using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("LsmvParsers")]
	public class LsmvTests : BaseParserTests
	{
		private Lsmv _lsmvParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.LsmvSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_lsmvParser = new Lsmv();
		}

		[TestMethod]
		public void Errors()
		{
			var result = _lsmvParser.Parse(Embedded("noinputlog.lsmv"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void Frames_WithSubFrames()
		{
			var result = _lsmvParser.Parse(Embedded("2frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(2, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var result = _lsmvParser.Parse(Embedded("0frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void NoRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("norerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void EmptyRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("emptyrerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void InvalidRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("invalidrerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void ValidRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("2frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void MissingGameType_Error()
		{
			var result = _lsmvParser.Parse(Embedded("gametype-missing.lsmv"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void InvalidGameType_DefaultsSnesNtsc()
		{
			var result = _lsmvParser.Parse(Embedded("gametype-empty.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Snes, result.SystemCode);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(2, result.Warnings.Count());
			AssertNoErrors(result);
		}

		[TestMethod]
		[DataRow("gametype-snesntsc.lsmv", SystemCodes.Snes, RegionType.Ntsc)]
		[DataRow("gametype-snespal.lsmv", SystemCodes.Snes, RegionType.Pal)]
		[DataRow("gametype-bsx.lsmv", SystemCodes.Snes, RegionType.Ntsc)]
		[DataRow("gametype-bsxslotted.lsmv", SystemCodes.Snes, RegionType.Ntsc)]
		[DataRow("gametype-sufamiturbo.lsmv", SystemCodes.Snes, RegionType.Ntsc)]
		[DataRow("gametype-sgb_ntsc.lsmv", SystemCodes.Sgb, RegionType.Ntsc)]
		[DataRow("gametype-sgb_pal.lsmv", SystemCodes.Sgb, RegionType.Pal)]
		[DataRow("gametype-gdmg.lsmv", SystemCodes.GameBoy, RegionType.Ntsc)]
		[DataRow("gametype-ggbc.lsmv", SystemCodes.Gbc, RegionType.Ntsc)]
		[DataRow("gametype-ggbca.lsmv", SystemCodes.Gbc, RegionType.Ntsc)]
		public void SystemAndRegion(string file, string expectedSystem, RegionType expectedRegion)
		{
			var result = _lsmvParser.Parse(Embedded(file));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(expectedSystem, result.SystemCode);
			Assert.AreEqual(expectedRegion, result.Region);
			AssertNoWarningsOrErrors(result);
		}
	}
}
