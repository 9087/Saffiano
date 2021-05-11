using Dotnet.Script.Core;
using Dotnet.Script.Core.Commands;
using Dotnet.Script.DependencyModel.Context;
using Dotnet.Script.DependencyModel.Logging;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saffiano.Console
{
    internal class FrameActionScheduler : Behaviour
    {
        private List<Action> actions = new List<Action>();

        public void Run(Action action)
        {
            actions.Add(action);
        }

        void LateUpdate()
        {
            foreach (var action in actions)
            {
                action.Invoke();
            }
            actions.Clear();
        }
    }

    internal class CommandLineTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;

        public CommandLine commandLine { get; private set; }

        public CommandLineTextWriter(CommandLine commandLine)
        {
            this.commandLine = commandLine;
        }

        public override void WriteLine(string value)
        {
            commandLine.WriteLine(value);
        }
    }

    public class Console : CommandLine
    {
        private ScriptContext context { get; set; }

        private ScriptCompiler compiler { get; set; }

        private LogFactory logFactory { get; set; }

        private TextWriter @out { get; set; }

        private InteractiveScriptGlobals globals { get; set; }

        private ScriptState<object> state { get; set; }

        private ScriptOptions options { get; set; }

        private FrameActionScheduler frameActionScheduler { get; set; }

        public Console()
        {
            frameActionScheduler = AddComponent<FrameActionScheduler>();
            this.TextEntered += OnCommandLineTextEntered;
            this.WriteLine("Console initializing...");
            Execute(() => Initialize());
            textField.header = ">>>";
        }

        void Execute(Action action)
        {
            this.SetInputActive(false);
            Task.Run(() => {
                action.Invoke();
                RunFrameAction(() => this.SetInputActive(true));
            });
        }

        void RunFrameAction(Action action)
        {
            frameActionScheduler.Run(action);
        }

        void Initialize()
        {
            logFactory = new LogFactory(type =>
            {
                return (level, message, exception) =>
                {
                    switch (level)
                    {
                        case LogLevel.Warning:
                        case LogLevel.Error:
                        case LogLevel.Critical:
                            Debug.LogWarning(message);
                            break;
                    }
                };
            });

            context = new ScriptContext(
                SourceText.From(string.Empty),
                Directory.GetCurrentDirectory(),
                Enumerable.Empty<string>(),
                scriptMode: ScriptMode.REPL
            );

            compiler = new ScriptCompiler(logFactory, useRestoreCache: false);

            @out = new CommandLineTextWriter(this);

            globals = new InteractiveScriptGlobals(@out, CSharpObjectFormatter.Instance);

            var compilation = compiler.CreateCompilationContext<object, InteractiveScriptGlobals>(context);
            compilation.Warnings.ToList().ForEach((x) => Debug.LogFormat(x.ToString()));
            compilation.Errors.ToList().ForEach((x) => Debug.LogFormat(x.ToString()));

            var task = compilation.Script.RunAsync(globals, ex => true);
            task.Wait();
            state = task.Result;
            options = compilation.ScriptOptions;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }
                    Debug.LogFormat("Add reference: {0}", assembly.FullName);
                    options = options.AddReferences(assembly);
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.ToString());
                }
            }
        }

        private void OnCommandLineTextEntered(string source)
        {
            Debug.Log(source);
            Execute(() => {
                try
                {
                    var task = state.ContinueWithAsync(source, options, ex => true);
                    task.Wait();
                    state = task.Result;
                    if (state.Exception != null)
                    {
                        RunFrameAction(() => { this.WriteLine(state.Exception.ToString()); });
                    }
                    if (state.ReturnValue != null)
                    {
                        RunFrameAction(() => { this.WriteLine(state.ReturnValue.ToString()); });
                    }
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.ToString());
                    WriteLine(exception.Message);
                }
            });
        }
    }
}
