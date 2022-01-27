using System;
using Common;
using NSubstitute;
using NUnit.Framework;

namespace Tests.ShellRunnerTests
{
    [TestFixture]
    [Platform("Windows10")]
    public class TestRunInDirectory
    {
private IShellRunner runner;
        private ReadLineEvent outputChangedEvent; 
        private ReadLineEvent errorChangedEvent;
        
        [SetUp]        
        public void Setup()
        {
            outputChangedEvent = Substitute.For<ReadLineEvent>();
            errorChangedEvent = Substitute.For<ReadLineEvent>();
            runner = ShellRunnerFactory.Create();
            runner.OnOutputChange += outputChangedEvent;
            runner.OnErrorsChange += errorChangedEvent;

            ShellRunnerStaticInfo.LastOutput = null;
        }
        
        [Test]
        public void TestExecution()
        {
            runner.RunInDirectory("", "echo current", TimeSpan.FromSeconds(4));
            
            Assert.That(runner.Output, Is.EqualTo($"current{Environment.NewLine}"), "Output is wrong");
            Assert.That(runner.Errors, Is.Empty, "Errors is wrong");
            Assert.That(runner.HasTimeout, Is.False, "HasTimeout is wrong");
            Assert.That(ShellRunnerStaticInfo.LastOutput, Is.EqualTo($"current{Environment.NewLine}"), "LastOutput is wrong");
            outputChangedEvent.Received(1).Invoke("current");
            errorChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
        }
        
        [Test]
        [Explicit("The test requires the path 'C:\\' to exists")]
        public void TestWorkingDirectoryC()
        {
            runner.RunInDirectory("C:\\", "echo %cd%", TimeSpan.FromSeconds(4));
            
            Assert.That(runner.Output, Is.EqualTo($"C:\\{Environment.NewLine}"), "Output is wrong");
            Assert.That(runner.Errors, Is.Empty, "Errors is wrong");
            Assert.That(runner.HasTimeout, Is.False, "HasTimeout is wrong");
            Assert.That(ShellRunnerStaticInfo.LastOutput, Is.EqualTo($"C:\\{Environment.NewLine}"), "LastOutput is wrong");
            outputChangedEvent.Received(1).Invoke($"C:\\");
            errorChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
        }
        
        [Test]
        [Explicit("The test requires the path 'C:\\Users' to exists")]
        public void TestWorkingDirectoryUsersOnC()
        {
            runner.RunInDirectory("C:\\Users", "echo %cd%", TimeSpan.FromSeconds(4));
            
            Assert.That(runner.Output, Is.EqualTo($"C:\\Users{Environment.NewLine}"), "Output is wrong");
            Assert.That(runner.Errors, Is.Empty, "Errors is wrong");
            Assert.That(runner.HasTimeout, Is.False, "HasTimeout is wrong");
            Assert.That(ShellRunnerStaticInfo.LastOutput, Is.EqualTo($"C:\\Users{Environment.NewLine}"), "LastOutput is wrong");
            outputChangedEvent.Received(1).Invoke("C:\\Users");
            errorChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
        }

        [Test]
        public void TestWrongCommand()
        {
            runner.RunInDirectory("", "wrong_command", TimeSpan.FromSeconds(4));

            Assert.That(runner.Output, Is.Empty, "Output is wrong");
            Assert.That(runner.Errors, Does.StartWith("\"wrong_command\""), "Errors is wrong");
            Assert.That(runner.HasTimeout, Is.False, "HasTimeout is wrong");
            Assert.That(ShellRunnerStaticInfo.LastOutput, Is.Empty, "LastOutput is wrong");
            outputChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
            errorChangedEvent.Received(1).Invoke(Arg.Is<string>(line => line.StartsWith("\"wrong_command\"")));
        }

        [Test]
        [Ignore("The test works incorrect for CliWrap and pause")]
        [Explicit("The test is too slow because default timeout increasing in IShellRunner implementations is too big")]
        public void TestTimeout()
        {
            runner.RunInDirectory("", "pause", TimeSpan.Zero);
            
            Assert.That(runner.Output, Is.Empty, "Output is wrong");
            Assert.That(runner.Errors, Does.StartWith("Running timeout"), "Errors is wrong");
            Assert.That(runner.HasTimeout, Is.True, "HasTimeout is wrong");
            Assert.That(ShellRunnerStaticInfo.LastOutput, Is.Null, "LastOutput is wrong");
            outputChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
            errorChangedEvent.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<string>());
        }
    }
}