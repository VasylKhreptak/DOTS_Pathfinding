using System;
using NUnit.Framework;
using Plugins.Timer.Reactive;

namespace Plugins.Timer.Tests.TimerEditModeTests
{
    public class ReactiveTimeSpanTests
    {
        [Test]
        public void Constructor_InitializesToZero()
        {
            // Arrange & Act
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan();

            // Assert
            Assert.AreEqual(0, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Hours.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Minutes.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Seconds.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Milliseconds.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Ticks.Value);
            Assert.AreEqual(0, reactiveTimeSpan.TotalDays.Value);
            Assert.AreEqual(0, reactiveTimeSpan.TotalHours.Value);
            Assert.AreEqual(0, reactiveTimeSpan.TotalMinutes.Value);
            Assert.AreEqual(0, reactiveTimeSpan.TotalSeconds.Value);
            Assert.AreEqual(0, reactiveTimeSpan.TotalMilliseconds.Value);
        }

        [Test]
        public void Constructor_WithTimeSpan_SetsInitialValues()
        {
            // Arrange
            TimeSpan timeSpan = new TimeSpan(1, 2, 3, 4, 500);

            // Act
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(timeSpan);

            // Assert
            Assert.AreEqual(1, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(2, reactiveTimeSpan.Hours.Value);
            Assert.AreEqual(3, reactiveTimeSpan.Minutes.Value);
            Assert.AreEqual(4, reactiveTimeSpan.Seconds.Value);
            Assert.AreEqual(500, reactiveTimeSpan.Milliseconds.Value);
        }

        [Test]
        public void Add_AddsTimeSpanCorrectly()
        {
            // Arrange
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(new TimeSpan(1, 2, 3, 4, 500));
            TimeSpan timeToAdd = new TimeSpan(0, 1, 30, 30, 250); // 0d, 1h, 30m, 30s, 250ms

            // Act
            reactiveTimeSpan.Add(timeToAdd);

            // Assert
            Assert.AreEqual(1, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(3, reactiveTimeSpan.Hours.Value);
            Assert.AreEqual(33, reactiveTimeSpan.Minutes.Value);
            Assert.AreEqual(34, reactiveTimeSpan.Seconds.Value);
            Assert.AreEqual(750, reactiveTimeSpan.Milliseconds.Value);
        }

        [Test]
        public void Subtract_SubtractsTimeSpanCorrectly()
        {
            // Arrange
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(new TimeSpan(1, 2, 3, 4, 500));
            TimeSpan timeToSubtract = new TimeSpan(0, 1, 2, 3, 400); // 0d, 1h, 2m, 3s, 400ms

            // Act
            reactiveTimeSpan.Subtract(timeToSubtract);

            // Assert
            Assert.AreEqual(1, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(1, reactiveTimeSpan.Hours.Value);
            Assert.AreEqual(1, reactiveTimeSpan.Minutes.Value);
            Assert.AreEqual(1, reactiveTimeSpan.Seconds.Value);
            Assert.AreEqual(100, reactiveTimeSpan.Milliseconds.Value);
        }

        [Test]
        public void Multiply_MultipliesTimeSpanCorrectly()
        {
            // Arrange
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(new TimeSpan(1, 0, 0, 0)); // 1 day

            // Act
            reactiveTimeSpan.Multiply(2);

            // Assert
            Assert.AreEqual(2, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Hours.Value);
        }

        [Test]
        public void Divide_DividesTimeSpanCorrectly()
        {
            // Arrange
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(new TimeSpan(2, 0, 0, 0)); // 2 days

            // Act
            reactiveTimeSpan.Divide(2);

            // Assert
            Assert.AreEqual(1, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(0, reactiveTimeSpan.Hours.Value);
        }

        [Test]
        public void Negate_NegatesTimeSpan()
        {
            // Arrange
            ReactiveTimeSpan reactiveTimeSpan = new ReactiveTimeSpan(new TimeSpan(1, 2, 3, 4, 500));

            // Act
            reactiveTimeSpan.Negate();

            // Assert
            Assert.AreEqual(-1, reactiveTimeSpan.Days.Value);
            Assert.AreEqual(-2, reactiveTimeSpan.Hours.Value);
            Assert.AreEqual(-3, reactiveTimeSpan.Minutes.Value);
            Assert.AreEqual(-4, reactiveTimeSpan.Seconds.Value);
            Assert.AreEqual(-500, reactiveTimeSpan.Milliseconds.Value);
        }
    }
}