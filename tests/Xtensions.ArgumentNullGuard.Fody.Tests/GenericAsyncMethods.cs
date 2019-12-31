﻿namespace Xtensions.ArgumentNullGuard.Fody.Tests
{
    using System;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Xtensions.ArgumentNullGuard.Fody.Tests.Helpers;
    using Xunit;

    public class GenericAsyncMethods : BaseModuleWeaverTests
    {
        [Theory]
        [OptimizationLevelData]
        public void ThrowsArgumentNullExceptionForNonNullableParameter(OptimizationLevel optimizationLevel)
        {
            string sourceCode = @"
                using System.Threading.Tasks;

                public static class Target
                {
                    public static async Task<T> TestMethod<T>(T value)
                        where T : class
                    {
                        await Task.Delay(0);
                        return value;
                    }
                }";

            Assembly assembly = this.WeaveAssembly(sourceCode, optimizationLevel);
            MethodInfo testMethod = assembly.GetType("Target")!.GetMethod("TestMethod")!.MakeGenericMethod(typeof(string));

            Assert.Throws<ArgumentNullException>(
                paramName: "value",
                testCode: () => InvokeMethod(method: testMethod, parameters: new object?[] { null }));
        }

        [Theory]
        [OptimizationLevelData]
        public void DoesNotThrowArgumentNullExceptionForNotNullableParameterWhenNonNullValueIsPassed(OptimizationLevel optimizationLevel)
        {
            string sourceCode = @"
                using System.Threading.Tasks;

                public static class Target
                {
                    public static async Task<T?> TestMethod<T>(T? value)
                        where T : class
                    {
                        await Task.Delay(0);
                        return value;
                    }
                }";

            Assembly assembly = this.WeaveAssembly(sourceCode, optimizationLevel);
            MethodInfo testMethod = assembly.GetType("Target")!.GetMethod("TestMethod")!.MakeGenericMethod(typeof(string));

            Assert.Null(Record.Exception(() => InvokeMethod(method: testMethod, parameters: new object?[] { "test-value" })));
        }

        [Theory]
        [OptimizationLevelData]
        public void DoesNotThrowArgumentNullExceptionForNullableParameter(OptimizationLevel optimizationLevel)
        {
            string sourceCode = @"
                using System.Threading.Tasks;

                public static class Target
                {
                    public static async Task<T?> TestMethod<T>(T? value)
                        where T : class
                    {
                        await Task.Delay(0);
                        return value;
                    }
                }";

            Assembly assembly = this.WeaveAssembly(sourceCode, optimizationLevel);
            MethodInfo testMethod = assembly.GetType("Target")!.GetMethod("TestMethod")!.MakeGenericMethod(typeof(string));

            Assert.Null(Record.Exception(() => InvokeMethod(method: testMethod, parameters: new object?[] { null })));
        }
    }
}
