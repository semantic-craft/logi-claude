namespace Loupedeck.CodexActionRingPlugin.Feedback.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public sealed class HapticAssetsTests
    {
        private static readonly String AssetsDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "haptics");

        private static readonly IReadOnlyDictionary<String, String> ExpectedDefaultWaveforms =
            new Dictionary<String, String>(StringComparer.Ordinal)
            {
                [HapticEventNames.DispatchRequested] = "sharp_state_change",
                [HapticEventNames.DispatchFailed] = "knock",
                [HapticEventNames.SelectionRejected] = "subtle_collision",
            };

        [Fact]
        public void EventSourceAndMappingAreValidYamlWithExactOneToOneNames()
        {
            var eventSource = Deserialize<EventSource>(Path.Combine(AssetsDirectory, "DefaultEventSource.yaml"));
            var mapping = Deserialize<EventMapping>(Path.Combine(AssetsDirectory, "extra", "eventMapping.yaml"));

            var sourceNames = eventSource.Events.Select(definition => definition.Name).ToArray();
            var mappingNames = mapping.Haptics.Keys.ToArray();

            Assert.Equal(HapticEventNames.All, sourceNames, StringComparer.Ordinal);
            Assert.Equal(HapticEventNames.All.OrderBy(name => name, StringComparer.Ordinal), mappingNames.OrderBy(name => name, StringComparer.Ordinal), StringComparer.Ordinal);
            Assert.Equal(sourceNames.Length, sourceNames.Distinct(StringComparer.Ordinal).Count());
            Assert.All(sourceNames, name => Assert.Matches(new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant), name));
            Assert.All(eventSource.Events, definition =>
            {
                Assert.False(String.IsNullOrWhiteSpace(definition.DisplayName));
                Assert.False(String.IsNullOrWhiteSpace(definition.Description));
            });
        }

        [Fact]
        public void EveryMappingHasAnHonestDefaultAndOnlySupportedKeys()
        {
            var mapping = Deserialize<EventMapping>(Path.Combine(AssetsDirectory, "extra", "eventMapping.yaml"));

            Assert.Equal(ExpectedDefaultWaveforms.Count, mapping.Haptics.Count);

            foreach (var (eventName, expectedWaveform) in ExpectedDefaultWaveforms)
            {
                var deviceMappings = Assert.Contains(eventName, mapping.Haptics);
                Assert.Equal(expectedWaveform, Assert.Contains("DEFAULT", deviceMappings));
                Assert.All(deviceMappings.Keys, key => Assert.Contains(key, new[] { "DEFAULT", "MX Master 4" }));
            }
        }

        [Fact]
        public void MappingsDoNotUseSuccessOrAggressiveAlertWaveforms()
        {
            var mapping = Deserialize<EventMapping>(Path.Combine(AssetsDirectory, "extra", "eventMapping.yaml"));
            var forbidden = new[] { "happy_alert", "completed", "angry_alert" };

            Assert.DoesNotContain(
                mapping.Haptics.Values.SelectMany(deviceMappings => deviceMappings.Values),
                waveform => forbidden.Contains(waveform, StringComparer.Ordinal));
        }

        private static T Deserialize<T>(String path)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using var reader = File.OpenText(path);
            return deserializer.Deserialize<T>(reader);
        }

        private sealed class EventSource
        {
            public String DisplayName { get; set; } = String.Empty;

            public String Description { get; set; } = String.Empty;

            public List<EventDefinition> Events { get; set; } = new();
        }

        private sealed class EventDefinition
        {
            public String Name { get; set; } = String.Empty;

            public String DisplayName { get; set; } = String.Empty;

            public String Description { get; set; } = String.Empty;
        }

        private sealed class EventMapping
        {
            public Dictionary<String, Dictionary<String, String>> Haptics { get; set; } = new(StringComparer.Ordinal);
        }
    }
}
