using System.Collections.Concurrent;
using FluentAssertions;
using ProtoBufJsonConverter.Extensions;

namespace ProtoBufJsonConverterTests;

public class DictionaryExtensionsTests
{
    [Fact]
    public async Task GetOrAddAsync_CalledConcurrently_InvokesFactoryOnce()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<int, Lazy<Task<int>>>();
        var callCount = 0;

        async Task<int> ValueFactory(int key)
        {
            Interlocked.Increment(ref callCount);

            await Task.Delay(100); // Simulate some asynchronous work
            return 42;
        }

        var tasks = Enumerable.Range(0, 100)
            .Select(_ => dictionary.GetOrAddAsync(1, ValueFactory))
            .ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        callCount.Should().Be(1);
        results.Should().OnlyContain(r => r == 42);
    }

    [Fact]
    public async Task GetOrAddAsync_WhenFactoryFails_RemovesEntryAndAllowsRetry()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<int, Lazy<Task<int>>>();
        var callCount = 0;

        Task<int> ValueFactory(int key)
        {
            var current = Interlocked.Increment(ref callCount);
            return current == 1
                ? Task.FromException<int>(new InvalidOperationException("boom"))
                : Task.FromResult(99);
        }

        // Act 1: first attempt fails
        var act = async () => await dictionary.GetOrAddAsync(1, ValueFactory);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Act 2: second attempt should retry and succeed
        var result = await dictionary.GetOrAddAsync(1, ValueFactory);

        // Assert
        result.Should().Be(99);
        callCount.Should().Be(2);
    }
}