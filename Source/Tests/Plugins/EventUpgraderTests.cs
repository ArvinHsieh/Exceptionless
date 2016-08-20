﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests.Reporters;
using Exceptionless.Api.Tests.Utility;
using Exceptionless.Core.Plugins.EventParser;
using Exceptionless.Core.Plugins.EventUpgrader;
using Xunit;
using Xunit.Abstractions;

namespace Exceptionless.Api.Tests.Plugins {
    [UseReporter(typeof(DiffReporter))]
    public class EventUpgraderTests : TestBase {
        private readonly EventUpgraderPluginManager _upgrader;
        private readonly EventParserPluginManager _parser;

        public EventUpgraderTests(ITestOutputHelper output) : base(output) {
            _upgrader = GetService<EventUpgraderPluginManager>();
            _parser = GetService<EventParserPluginManager>();
        }

        [Theory]
        [MemberData("Errors")]
        public void ParseErrors(string errorFilePath) {
            var json = File.ReadAllText(errorFilePath);
            var ctx = new EventUpgraderContext(json);

            _upgrader.Upgrade(ctx);
            ApprovalsUtility.VerifyFile(Path.ChangeExtension(errorFilePath, ".expected.json"), ctx.Documents.First.ToString());

            var events = _parser.ParseEvents(ctx.Documents.ToString(), 2, "exceptionless/2.0.0.0");
            Assert.Equal(1, events.Count);
        }

        public static IEnumerable<object[]> Errors {
            get {
                var result = new List<object[]>();
                foreach (var file in Directory.GetFiles(@"..\..\ErrorData\", "*.json", SearchOption.AllDirectories).Where(f => !f.EndsWith(".expected.json")))
                    result.Add(new object[] { Path.GetFullPath(file) });

                return result.ToArray();
            }
        }
    }
}