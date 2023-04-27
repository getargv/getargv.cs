using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;

RootCommand rootCommand = new RootCommand(description: "prints the arguments of a process.");
rootCommand.TreatUnmatchedTokensAsErrors = true;

var skipOption = new Option<uint>(aliases: new string[] { "--skip", "-s" }, description: "The number of leading arguments to skip before printing.", getDefaultValue: () => 0);
rootCommand.AddOption(skipOption);

var nulsOption = new Option<bool>(aliases: new string[] { "--nuls", "-0" }, description: "Print the arguments NUL separated.");
nulsOption.Arity = ArgumentArity.ZeroOrOne;
nulsOption.IsRequired = false;
rootCommand.AddOption(nulsOption);

var pidArgument = new Argument<uint>(name: "pid", description: "The pid of the process to target.");
rootCommand.Add(pidArgument);

rootCommand.SetHandler<uint,bool,uint>(
    (skip, nuls, pid) => {
        try{
        string res = Getargv.Getargv.asString(Environment.ProcessId, Encoding.UTF8, nuls, skip);
        Console.WriteLine(res);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    },
    skipOption, nulsOption, pidArgument);

return rootCommand.Invoke(args);
