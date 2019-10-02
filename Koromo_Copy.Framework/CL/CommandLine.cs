// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Koromo_Copy.Framework.CL
{
    public enum CommandType
    {
        OPTION,
        ARGUMENTS,
        EQUAL,
    }

    /// <summary>
    /// Attribute model for the command line parser.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandLine : Attribute
    {
        public CommandType CType { get; private set; }
        public string Option { get; private set; }

        /// <summary>
        /// Usage information of the command.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Default argument to insert if the argument does not match.
        /// </summary>
        public bool DefaultArgument { get; set; }

        /// <summary>
        /// Indicates that the variable will use a pipe.
        /// </summary>
        public bool Pipe { get; set; }

        /// <summary>
        /// This message is displayed when the command syntax is incorrect.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// This value is set to true when nothing is entered in the pipe.
        /// </summary>
        public bool PipeDefault { get; set; } = false;

        /// <summary>
        /// This value is set to true when nothing is entered.
        /// </summary>
        public bool Default { get; set; } = false;

        /// <summary>
        /// For ARGUMENTS type, specifies the number of arguments.
        /// </summary>
        public int ArgumentsCount { get; set; } = 1;

        /// <summary>
        /// One character option
        /// </summary>
        public string ShortOption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option">Option token.</param>
        /// <param name="type"></param>
        public CommandLine(string option, CommandType type)
        {
            Option = option;
            CType = type;
        }
    }

    /// <summary>
    /// Tools for organizing the command line.
    /// </summary>
    public class CommandLineUtil
    {
        /// <summary>
        /// Checks whether there is an option in the argument array.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool AnyOption(string[] args)
        {
            return args.ToList().Any(x => x[0] == '-');
        }

        /// <summary>
        /// Checks whether the argument array contains a string.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool AnyStrings(string[] args)
        {
            return args.ToList().Any(x => x[0] != '-');
        }

        /// <summary>
        /// Gets whether a specific argument is included.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool AnyArgument(string[] args, string arg)
        {
            return args.ToList().Any(x => x == arg);
        }

        /// <summary>
        /// Delete a specific argument.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="arg"></param>
        public static string[] DeleteArgument(string[] args, string arg)
        {
            var list = args.ToList();
            list.Remove(arg);
            return list.ToArray();
        }

        /// <summary>
        /// Put specific options at the beginning.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] PushFront(string[] args, string option)
        {
            var list = args.ToList();
            list.Insert(0, option);
            return list.ToArray();
        }

        /// <summary>
        /// Put specific options in specific locations.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] Insert(string[] args, string option, int index)
        {
            var list = args.ToList();
            list.Insert(index, option);
            return list.ToArray();
        }

        /// <summary>
        /// If there is a mismatched argument, get it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argv"></param>
        /// <returns></returns>
        public static List<int> GetWeirdArguments<T>(string[] argv)
            where T : IConsoleOption, new()
        {
            var field = CommandLineParser<T>.GetFields();
            List<int> result = new List<int>();

            for (int i = 0; i < argv.Length; i++)
            {
                string token = argv[i].Split('=')[0];
                if (field.ContainsKey(token))
                {
                    var cl = field[token];
                    if (cl.Item2.CType == CommandType.ARGUMENTS)
                        i += cl.Item2.ArgumentsCount;
                }
                else
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Insert default arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pipe"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string[] InsertWeirdArguments<T>(string[] args, bool pipe, string option)
            where T : IConsoleOption, new()
        {
            var weird = GetWeirdArguments<T>(args);

            if (weird.Count > 0 && pipe)
                args = Insert(args, option, weird[0]);

            return args;
        }

        /// <summary>
        /// Separate the combined factors.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string[] SplitCombinedOptions(string[] args)
        {
            List<string> result = new List<string>();
            foreach (var arg in args)
            {
                if (arg.Length > 1 && arg.StartsWith("-") && !arg.StartsWith("--") && !arg.Contains("="))
                {
                    for (int i = 1; i < arg.Length; i++)
                        result.Add($"-{arg[i]}");
                }
                else
                {
                    result.Add(arg);
                }
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// The command line parser.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandLineParser<T>
        where T : IConsoleOption, new()
    {
        /// <summary>
        /// Get field information that contains Attribute information.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Tuple<string, CommandLine>> GetFields()
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields();
            var field = new Dictionary<string, Tuple<string, CommandLine>>();

            foreach (FieldInfo m in fields)
            {
                object[] attrs = m.GetCustomAttributes(false);

                foreach (var cl in attrs)
                {
                    var clcast = cl as CommandLine;
                    if (clcast != null)
                    {
                        field.Add(clcast.Option, Tuple.Create(m.Name, clcast));
                        if (clcast.ShortOption != "")
                            field.Add(clcast.ShortOption, Tuple.Create(m.Name, clcast));
                    }
                }
            }

            return field;
        }

        /// <summary>
        /// Parse command lines based on attributes.
        /// </summary>
        /// <param name="argv"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static T Parse(string[] argv, bool pipe = false, string contents = "")
        {
            var field = GetFields();
            T result = new T();

            //
            // This flag is enabled if there is no option
            //
            bool any_option = true;

            for (int i = 0; i < argv.Length; i++)
            {
                string token = argv[i].Split('=')[0];
                if (field.ContainsKey(token))
                {
                    var cl = field[token];
                    if (cl.Item2.CType == CommandType.OPTION)
                    {
                        //
                        // In the case of the OPTION type, the variable must be set to true.
                        //
                        typeof(T).GetField(cl.Item1).SetValue(result, true);
                    }
                    else if (cl.Item2.CType == CommandType.ARGUMENTS)
                    {
                        List<string> sub_args = new List<string>();

                        int arguments_count = cl.Item2.ArgumentsCount;

                        if (cl.Item2.Pipe == true && pipe == true)
                        {
                            arguments_count--;
                            sub_args.Add(contents);
                        }

                        for (int j = 1; j <= arguments_count; j++)
                        {
                            if (i + j == argv.Length)
                            {
                                typeof(T).GetField("Error").SetValue(result, true);
                                typeof(T).GetField("ErrorMessage").SetValue(result, $"'{argv[i]}' require {arguments_count - j + 1} more sub arguments.");
                                typeof(T).GetField("HelpMessage").SetValue(result, cl.Item2.Help);
                                return result;
                            }

                            sub_args.Add(argv[i + j]);
                        }

                        i += cl.Item2.ArgumentsCount;

                        typeof(T).GetField(cl.Item1).SetValue(result, sub_args.ToArray());
                    }
                    else if (cl.Item2.CType == CommandType.EQUAL)
                    {
                        string[] split = argv[i].Split('=');

                        if (split.Length == 1)
                        {
                            typeof(T).GetField("Error").SetValue(result, true);
                            typeof(T).GetField("ErrorMessage").SetValue(result, $"'{split[0]}' must have equal delimiter.");
                            typeof(T).GetField("HelpMessage").SetValue(result, cl.Item2.Help);
                            return result;
                        }

                        typeof(T).GetField(cl.Item1).SetValue(result, split[1]);
                    }
                    any_option = false;
                }
                else
                {
                    typeof(T).GetField("Error").SetValue(result, true);
                    typeof(T).GetField("ErrorMessage").SetValue(result, $"'{argv[i]}' is not correct arguments.");
                    return result;
                }
            }

            if (any_option)
            {
                //
                // Find and activate the first Default
                //
                foreach (var kv in field)
                {
                    if (!pipe && kv.Value.Item2.Default)
                    {
                        typeof(T).GetField(kv.Value.Item1).SetValue(result, true);
                        break;
                    }
                    else if (pipe && kv.Value.Item2.PipeDefault)
                    {
                        typeof(T).GetField(kv.Value.Item1).SetValue(result, new[] { contents });
                        break;
                    }
                }
            }

            return result;
        }
    }
}
